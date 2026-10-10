using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Forms = System.Windows.Forms;

namespace Warenwirtschaftssystem.Model.Documents
{
    // Prints via GDI instead of WPF/XPS, so no PrintTicket is needed
    // (under Wine, creating one fails for non-standard paper sizes).
    // Each page is rendered as a black-and-white bitmap at the printer's resolution.
    public class BitmapPrintDialog
    {
        private const double HundredthInchToPx = 96d / 100d;

        private readonly PrintDocument PrintDoc = new PrintDocument
        {
            PrintController = new StandardPrintController()
        };

        public bool IsPrinterSelected { get; }

        // Like WPF's PrintDialog, these return the full paper size, with the origin at the paper's corner
        public double PrintableAreaWidth => PrintDoc.DefaultPageSettings.Bounds.Width * HundredthInchToPx;
        public double PrintableAreaHeight => PrintDoc.DefaultPageSettings.Bounds.Height * HundredthInchToPx;

        public short CopyCount
        {
            get => PrintDoc.PrinterSettings.Copies;
            set => PrintDoc.PrinterSettings.Copies = value;
        }

        public BitmapPrintDialog(string printerName)
        {
            if (string.IsNullOrWhiteSpace(printerName))
                return;

            foreach (string installedPrinter in PrinterSettings.InstalledPrinters)
            {
                if (installedPrinter == printerName)
                {
                    PrintDoc.PrinterSettings.PrinterName = printerName;
                    IsPrinterSelected = PrintDoc.PrinterSettings.IsValid;
                    break;
                }
            }
        }

        public bool? ShowDialog()
        {
            using Forms.PrintDialog dialog = new Forms.PrintDialog
            {
                Document = PrintDoc,
                UseEXDialog = true,
                AllowSomePages = false
            };

            return dialog.ShowDialog() == Forms.DialogResult.OK;
        }

        public void PrintDocument(FixedDocument document, string description)
        {
            List<FixedPage> pages = new List<FixedPage>();
            List<Size> pageSizes = new List<Size>();

            foreach (PageContent pC in document.Pages)
            {
                FixedPage fP = pC.Child;
                pages.Add(fP);
                pageSizes.Add(LayoutPage(fP));
            }

            int pageIndex = 0;
            int stripTop = 0;
            System.Drawing.Bitmap bitmap = null;

            PrintDoc.DocumentName = description;
            PrintDoc.PrintPage += (sender, e) =>
            {
                if (bitmap == null)
                    bitmap = RenderPage(pages[pageIndex], pageSizes[pageIndex], e.Graphics.DpiX, e.Graphics.DpiY);

                // Split content taller than one page (e.g. long receipts) across multiple pages
                int pageHeight = (int)Math.Round(e.PageBounds.Height / 100d * e.Graphics.DpiY);
                int stripHeight = pageHeight > 0 ? Math.Min(pageHeight, bitmap.Height - stripTop) : bitmap.Height;

                e.Graphics.PageUnit = System.Drawing.GraphicsUnit.Pixel;
                // GDI's origin is the corner of the printable area, so shift it back to the paper's corner
                e.Graphics.TranslateTransform(
                    -(float)(e.PageSettings.HardMarginX / 100d * e.Graphics.DpiX),
                    -(float)(e.PageSettings.HardMarginY / 100d * e.Graphics.DpiY));
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                e.Graphics.DrawImage(bitmap,
                    new System.Drawing.Rectangle(0, 0, bitmap.Width, stripHeight),
                    new System.Drawing.Rectangle(0, stripTop, bitmap.Width, stripHeight),
                    System.Drawing.GraphicsUnit.Pixel);

                stripTop += stripHeight;

                // A rounding difference of one pixel does not start a new page
                if (bitmap.Height - stripTop <= 1)
                {
                    bitmap.Dispose();
                    bitmap = null;
                    stripTop = 0;
                    pageIndex++;
                }

                e.HasMorePages = pageIndex < pages.Count;
            };

            PrintDoc.Print();
        }

        private Size LayoutPage(FixedPage fP)
        {
            fP.Background = Brushes.White;
            RenderOptions.SetEdgeMode(fP, EdgeMode.Aliased);
            RenderOptions.SetBitmapScalingMode(fP, BitmapScalingMode.NearestNeighbor);
            TextOptions.SetTextRenderingMode(fP, TextRenderingMode.Aliased);

            double width = double.IsNaN(fP.Width) ? PrintableAreaWidth : fP.Width;
            fP.Measure(new Size(width, double.PositiveInfinity));

            // FixedPage reports no DesiredSize of its own, so without a fixed height (receipts) derive it from the children
            double height = fP.Height;
            if (double.IsNaN(height))
            {
                height = 0;
                foreach (UIElement child in fP.Children)
                {
                    double top = FixedPage.GetTop(child);
                    height = Math.Max(height, (double.IsNaN(top) ? 0 : top) + child.DesiredSize.Height);
                }
            }

            Size size = new Size(width, height);
            fP.Arrange(new Rect(size));
            fP.UpdateLayout();

            return size;
        }

        private static System.Drawing.Bitmap RenderPage(FixedPage fP, Size size, double dpiX, double dpiY)
        {
            int width = (int)Math.Round(size.Width / 96d * dpiX);
            int height = (int)Math.Round(size.Height / 96d * dpiY);

            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, dpiX, dpiY, PixelFormats.Pbgra32);
            rtb.Render(fP);

            int stride = width * 4;
            byte[] pixels = new byte[stride * height];
            rtb.CopyPixels(pixels, stride, 0);

            // Convert to pure black and white, as thermal printers cannot print grayscale
            for (int i = 0; i < pixels.Length; i += 4)
            {
                int luma = (pixels[i] * 114 + pixels[i + 1] * 587 + pixels[i + 2] * 299) / 1000;
                byte value = luma < 128 ? (byte)0 : (byte)255;

                pixels[i] = value;
                pixels[i + 1] = value;
                pixels[i + 2] = value;
                pixels[i + 3] = 255;
            }

            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int y = 0; y < height; y++)
                Marshal.Copy(pixels, y * stride, data.Scan0 + y * data.Stride, stride);

            bitmap.UnlockBits(data);
            bitmap.SetResolution((float)dpiX, (float)dpiY);

            return bitmap;
        }
    }
}
