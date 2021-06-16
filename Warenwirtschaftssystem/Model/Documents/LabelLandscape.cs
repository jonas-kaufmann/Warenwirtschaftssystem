﻿using System.Collections.Generic;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Warenwirtschaftssystem.Model.Db;
using BarcodeLib;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;

namespace Warenwirtschaftssystem.Model.Documents
{
    public class LabelLandscape
    {
        private const double CmToPx = 96d / 2.54;
        private const double PtToPx = 96d / 72d;
        private double PageWidth = 5.7 * CmToPx;
        private double PageHeight = 3.1 * CmToPx;
        private const double FontSize = 8 * PtToPx;

        private Thickness PageMargin = new Thickness(0.175 * CmToPx, 0.25 * CmToPx, 0.1 * CmToPx, 0.1 * CmToPx);
        private System.Windows.Media.FontFamily FontFamily = new System.Windows.Media.FontFamily("Arial");
        private readonly List<Article> Articles;
        private FixedDocument Labels = new FixedDocument();
        private DataModel Data;

        public LabelLandscape(DataModel data, List<Article> articles)
        {
            Data = data;
            Articles = articles;
        }

        public bool? CreateAndPrint()
        {
            PrintQueue pQ = null;

            foreach (PrintQueue p in new LocalPrintServer().GetPrintQueues(new[] { EnumeratedPrintQueueTypes.Local, EnumeratedPrintQueueTypes.Connections }))
            {
                if (p.FullName == Data.StandardPrinters.TagLandscapePrinter)
                {
                    pQ = p;
                    break;
                }
            }

            PrintDialog pD = new PrintDialog
            {
                PageRangeSelection = PageRangeSelection.AllPages,
                UserPageRangeEnabled = false
            };

            if (pQ == null)
            {
                bool? result = pD.ShowDialog();

                if (result != true)
                    return result;
            }
            else
            {
                pD.PrintQueue = pQ;
            }

            PageHeight = pD.PrintableAreaHeight;
            PageWidth = pD.PrintableAreaWidth;

            foreach (Article article in Articles)
            {
                #region Neue Seite hinzufügen

                PageContent pC = new PageContent();
                FixedPage fP = new FixedPage
                {
                    Height = PageHeight,
                    Width = PageWidth
                };
                pC.Child = fP;
                Labels.Pages.Add(pC);

                Grid grid = new Grid
                {
                    Width = PageWidth - PageMargin.Left - PageMargin.Right,
                    Height = PageHeight - PageMargin.Top - PageMargin.Bottom,
                    Margin = new Thickness(PageMargin.Left, PageMargin.Top, 0, 0)
                };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.25 * CmToPx, GridUnitType.Pixel) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                #endregion

                AddAttributesPart(grid, article);
                AddBarcodePart(grid, article);

                fP.Children.Add(grid);
            }

            #region Drucken

            pD.PrintDocument(Labels.DocumentPaginator, "Warenwirtschaftssystem - Etikettendruck");
            return true;

            #endregion
        }

        private void AddBarcodePart(Grid contentGrid, Article article)
        {
            Grid grid = new Grid();

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            TextBlock tb = new TextBlock
            {
                Text = article.Price.ToString("C"),
                FontFamily = FontFamily,
                FontSize = FontSize + 6 * PtToPx,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            grid.Children.Add(tb);

            string id = article.ConvertedId.ToString("d8");
            if (id.Length % 2 == 1)
                id.Insert(0, "0");

            Bitmap bmp = new Bitmap(new Barcode().Encode(TYPE.Interleaved2of5, id));
            BitmapImage bI;

            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;

                bI = new BitmapImage();
                bI.BeginInit();
                bI.CacheOption = BitmapCacheOption.OnLoad;
                bI.StreamSource = ms;
                bI.EndInit();
            }

            System.Windows.Controls.Image image = new System.Windows.Controls.Image
            {
                Height = 0.75 * CmToPx,
                Source = bI,
                Stretch = Stretch.Fill
            };

            Grid.SetRow(image, 2);
            grid.Children.Add(image);

            tb = new TextBlock
            {
                Text = article.Supplier.Id.ToString(),
                FontFamily = FontFamily,
                FontSize = FontSize
            };
            Grid.SetRow(tb, 4);
            grid.Children.Add(tb);

            tb = new TextBlock
            {
                Text = article.ConvertedId.ToString(),
                FontFamily = FontFamily,
                FontSize = FontSize,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetRow(tb, 4);
            grid.Children.Add(tb);

            grid.LayoutTransform = new RotateTransform(-90);

            Grid.SetColumn(grid, 2);
            contentGrid.Children.Add(grid);
        }

        private void AddAttributesPart(Grid contentGrid, Article article)
        {
            Grid grid = new Grid();

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            TextBlock tB;

            string headerText = "";

            if (article.SubCategory != null && !string.IsNullOrWhiteSpace(article.SubCategory.Name))
                headerText += article.SubCategory.Name;
            else if (article.Category != null && !string.IsNullOrWhiteSpace(article.Category.Name))
                headerText += article.Category.Name;

            if (article.Gender != null && !string.IsNullOrWhiteSpace(article.Gender.Short))
            {
                if (headerText == "")
                    headerText += article.Gender.Short;
                else
                    headerText += " - " + article.Gender.Short;
            }

            tB = new TextBlock
            {
                Text = headerText,
                FontFamily = FontFamily,
                FontSize = FontSize,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
            };
            grid.Children.Add(tB);

            System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle
            {
                Height = 0.4,
                StrokeThickness = 0,
                Fill = System.Windows.Media.Brushes.Black
            };
            Grid.SetRow(rect, 2);
            grid.Children.Add(rect);

            int lastRowIndex = 2;

            #region Größe

            if (article.Size != null)
            {
                tB = new TextBlock
                {
                    Text = article.Size.Name,
                    FontFamily = FontFamily,
                    FontSize = FontSize + 2 * PtToPx,
                    FontWeight = FontWeights.Bold,
                    TextWrapping = TextWrapping.Wrap,
                };
                Grid.SetRow(tB, lastRowIndex + 2);
                grid.Children.Add(tB);

                lastRowIndex += 2;
            }

            #endregion

            #region Neuware

            if (article.AsNew)
            {
                tB = new TextBlock
                {
                    Text = "Neuware",
                    FontFamily = FontFamily,
                    FontSize = FontSize,
                    FontWeight = FontWeights.Bold,
                    TextWrapping = TextWrapping.Wrap,
                };
                Grid.SetRow(tB, lastRowIndex + 2);
                grid.Children.Add(tB);

                lastRowIndex += 2;
            }

            #endregion

            #region Teile

            if (article.Parts != null)
            {

                tB = new TextBlock
                {
                    Text = article.Parts.Name,
                    FontFamily = FontFamily,
                    FontSize = FontSize,
                    TextWrapping = TextWrapping.Wrap
                };
                Grid.SetRow(tB, lastRowIndex + 2);
                grid.Children.Add(tB);

                lastRowIndex += 2;
            }

            #endregion

            #region Material

            if (article.Materials != null && article.Materials.Count > 0)
            {

                string material = ArticleAttributes.ToRepresentation(article.Materials);

                tB = new TextBlock
                {
                    Text = material,
                    FontFamily = FontFamily,
                    FontSize = FontSize,
                    TextWrapping = TextWrapping.Wrap,
                };
                Grid.SetRow(tB, lastRowIndex + 2);
                grid.Children.Add(tB);

                lastRowIndex += 2;
            }

            #endregion

            #region Marke

            if (article.Brand != null)
            {
                tB = new TextBlock
                {
                    Text = article.Brand.Name,
                    FontFamily = FontFamily,
                    FontSize = FontSize,
                    TextWrapping = TextWrapping.Wrap,
                };
                Grid.SetRow(tB, lastRowIndex + 2);
                grid.Children.Add(tB);

                lastRowIndex += 2;
            }

            #endregion

            #region Farbe

            if (article.Colors != null && article.Colors.Count > 0)
            {
                string colors = ArticleAttributes.ToRepresentation(article.Colors);

                tB = new TextBlock
                {
                    Text = colors,
                    TextWrapping = TextWrapping.Wrap,
                    FontFamily = FontFamily,
                    FontSize = FontSize
                };
                Grid.SetRow(tB, lastRowIndex + 2);
                grid.Children.Add(tB);

                lastRowIndex += 2;
            }

            #endregion

            contentGrid.Children.Add(grid);
        }

    }
}
