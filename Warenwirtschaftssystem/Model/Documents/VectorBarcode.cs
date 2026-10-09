using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using BarcodeStandard;

namespace Warenwirtschaftssystem.Model.Documents
{
    // Draws an encoded barcode as vector graphics instead of a bitmap,
    // so that it also prints correctly via the GDI print path (e.g. under Wine)
    public static class VectorBarcode
    {
        public static Viewbox Create(Barcode barcode, double height)
        {
            string encoded = barcode.EncodedValue;

            // Same geometry BarcodeLib uses when generating its bitmap (centered)
            int barWidth = barcode.Width / encoded.Length;
            int shift = (barcode.Width % encoded.Length) / 2;

            GeometryGroup bars = new GeometryGroup();

            for (int pos = 0; pos < encoded.Length; pos++)
            {
                if (encoded[pos] != '1')
                    continue;

                // Merge adjacent bars
                int start = pos;
                while (pos + 1 < encoded.Length && encoded[pos + 1] == '1')
                    pos++;

                bars.Children.Add(new RectangleGeometry(new Rect(start * barWidth + shift, 0, (pos - start + 1) * barWidth, barcode.Height)));
            }

            bars.Freeze();

            // Canvas with the bitmap's size, so the Viewbox lays out like the previous Image
            Canvas canvas = new Canvas
            {
                Width = barcode.Width,
                Height = barcode.Height
            };
            canvas.Children.Add(new Path
            {
                Data = bars,
                Fill = Brushes.Black
            });

            return new Viewbox
            {
                Height = height,
                Stretch = Stretch.Fill,
                Child = canvas
            };
        }
    }
}
