namespace Warenwirtschaftssystem.Model.Documents
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;

    public class DocumentWithPageIndex
    {
        private const double CmToPx = 96d / 2.54;
        private const double PtToPx = 96d / 72d;

        private double PageWidth;
        private double PageHeight;
        private double MaxPageContentHeight;
        private Thickness Margin;
        private readonly double MarginPageIndex;
        private FontFamily FontFamily;
        private double FontSize;

        public int PageCount { get => Document.Pages.Count; }

        public FixedDocument Document = new FixedDocument();

        public DocumentWithPageIndex(double pageWidth, double pageHeight, Thickness margin, FontFamily pageIndexFontFamily, double pageIndexFontSize)
        {
            PageWidth = pageWidth;
            PageHeight = pageHeight;
            Margin = margin;
            FontFamily = pageIndexFontFamily;
            FontSize = pageIndexFontSize;

            MarginPageIndex = FontSize * 2.5;

            MaxPageContentHeight = PageHeight - Margin.Top - Margin.Bottom;
        }

        public StackPanel AddNewPage()
        {
            PageContent pC = new PageContent();
            FixedPage fP = new FixedPage { Width = PageWidth, Height = PageHeight, Margin = Margin };

            StackPanel sP = new StackPanel { Width = PageWidth - Margin.Left - Margin.Right };
            fP.Children.Add(sP);

            pC.Child = fP;
            Document.Pages.Add(pC);

            return sP;
        }

        public bool IsPageOverfilled(int pageIndex)
        {
            StackPanel sP = (Document.Pages[pageIndex].Child as FixedPage).Children[0] as StackPanel;
            sP.UpdateLayout();
            sP.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            if (sP.DesiredSize.Height > MaxPageContentHeight - MarginPageIndex)
                return true;
            else return false;
        }

        public DocumentPaginator GetPrintable()
        {
            #region PageIndex hinzufügen

            for (int i = 0; i < Document.Pages.Count; i++)
            {
                Grid grid = new Grid
                {
                    Width = PageWidth - Margin.Left - Margin.Right,
                    Height = MaxPageContentHeight
                };

                grid.Children.Add(new TextBlock
                {
                    Text = "Seite " + (i + 1) + " von " + Document.Pages.Count,
                    FontFamily = FontFamily,
                    FontSize = FontSize,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom
                });

                Document.Pages[i].Child.Children.Add(grid);
            }

            #endregion

            return Document.DocumentPaginator;
        }
    }
}
