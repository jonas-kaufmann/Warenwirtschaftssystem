using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Warenwirtschaftssystem.Model.Db;

namespace Warenwirtschaftssystem.Model.Documents
{
    public class ArticlesInStockDoc
    {
        private readonly IEnumerable<Article> Articles;
        private Supplier Supplier;
        private DataModel Data;
        private DbModel MainDb;

        private DocumentWithPageIndex Document;
        private StackPanel PageContent;
        private double PageWidth;
        private double PageHeight;
        private Thickness PageMargin = new Thickness(2.5 * CmToPx, 1.69 * CmToPx, CmToPx, 1.69 * CmToPx);

        private FontFamily FontFamily = new FontFamily("Arial");
        private const double FontSize = 10 * PtToPx;

        private const double CmToPx = 96d / 2.54;
        private const double PtToPx = 96d / 72d;

        private readonly bool PrintPriceInsteadOfPayout;

        public ArticlesInStockDoc(DataModel data, Supplier supplier, bool printPriceInsteadOfPayout = false)
        {
            Data = data;
            MainDb = new DbModel(data.MainConnectionString);
            Articles = supplier.Articles.Where(a => a.Status == Status.Sortiment || a.Status == Status.InStock);
            Supplier = supplier;

            PrintPriceInsteadOfPayout = printPriceInsteadOfPayout;
        }

        public void CreateAndPrintDocument()
        {
            PrintQueue pQ = null;

            foreach (PrintQueue p in new LocalPrintServer().GetPrintQueues(new[] { EnumeratedPrintQueueTypes.Local, EnumeratedPrintQueueTypes.Connections }))
            {
                if (p.FullName == Data.StandardPrinters.DocumentPrinter)
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

            if (pQ != null)
                pD.PrintQueue = pQ;

            if (!pD.ShowDialog().GetValueOrDefault(false))
                return;

            PageWidth = pD.PrintableAreaWidth;
            PageHeight = pD.PrintableAreaHeight;

            if (PageWidth <= 0 && PageHeight <= 0)
            {
                PageWidth = 21 * CmToPx;
                PageHeight = 29.7 * CmToPx;
            }

            Document = new DocumentWithPageIndex(PageWidth, PageHeight, PageMargin, FontFamily, FontSize);
            PageContent = Document.AddNewPage();

            #region Kopf

            Grid grid = new Grid { Width = PageContent.Width };
            PageContent.Children.Add(grid);

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Grid gLeft = new Grid();
            Grid gRight = new Grid();
            grid.Children.Add(gLeft);
            Grid.SetColumn(gRight, 2);
            grid.Children.Add(gRight);

            gRight.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            gRight.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            gRight.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            #region Lieferanteninfos & Anschrift

            gLeft.Children.Add(new TextBlock
            {
                Text = "Lieferanten-Nr. " + Supplier.Id
                    + "\nTelefon " + Supplier.Phone
                    + "\nE-Mail " + Supplier.EMail,
                FontFamily = FontFamily,
                FontSize = FontSize
            });

            TextBlock tb;

            if (string.IsNullOrWhiteSpace(Supplier.Company))
            {
                tb = new TextBlock
                {
                    Text = "\n\n\n"
                        + Supplier.Title.GetDescription() + " " + Supplier.Name
                        + "\n" + Supplier.Street
                        + "\n" + Supplier.Postcode + " " + Supplier.Place,
                    FontFamily = FontFamily,
                    FontSize = FontSize,
                    Width = 8 * CmToPx
                };
            }
            else
            {
                tb = new TextBlock
                {
                    Text = "\n\n\n"
                        + Supplier.Title.GetDescription() + " " + Supplier.Name
                        + "\n" + Supplier.Company
                        + "\n" + Supplier.Street
                        + "\n" + Supplier.Postcode + " " + Supplier.Place,
                    FontFamily = FontFamily,
                    FontSize = FontSize,
                    Width = 8 * CmToPx
                };
            }
            Thickness margin = tb.Margin;
            margin.Top = 4.5 * CmToPx - PageMargin.Top;
            tb.Margin = margin;

            gLeft.Children.Add(tb);

            #endregion

            #region Shopinfos

            StackPanel sP = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Right };
            gRight.Children.Add(sP);

            sP.Children.Add(new TextBlock
            {
                Text = MainDb.Settings.Where(s => s.Key == "Shopdescription").Single().Value,
                FontFamily = FontFamily,
                FontSize = 12 * PtToPx,
                HorizontalAlignment = HorizontalAlignment.Left
            });

            sP.Children.Add(new TextBlock
            {
                Text = MainDb.Settings.Where(s => s.Key == "Shopinformation").Single().Value,
                FontFamily = FontFamily,
                FontSize = FontSize,
                HorizontalAlignment = HorizontalAlignment.Left
            });

            #endregion

            #region Datum

            tb = new TextBlock
            {
                Text = MainDb.Settings.Where(s => s.Key == "Shopplace").Single().Value + ", " + DateTime.Now.ToShortDateString(),
                HorizontalAlignment = HorizontalAlignment.Right,
                FontFamily = FontFamily,
                FontSize = FontSize
            };

            Grid.SetRow(tb, 2);

            gRight.Children.Add(tb);

            #endregion

            #region Betreff

            PageContent.Children.Add(new TextBlock
            {
                Text = "\n\nArtikel im Bestand\n\n",
                FontWeight = FontWeights.Bold,
                FontFamily = FontFamily,
                FontSize = FontSize + 1 * PtToPx
            });

            #endregion

            #endregion

            #region Artikel

            grid = GenerateArticlesGrid();
            PageContent.Children.Add(grid);

            decimal sum = 0;
            foreach (Article article in Articles)
            {
                if (PrintPriceInsteadOfPayout)
                    sum += article.Price;
                else
                    sum += article.SupplierProportion;

                AddArticleRowToGrid(grid, article);

                //Seite voll
                if (Document.IsPageOverfilled(Document.PageCount - 1))
                {
                    UIElement[] elements = new UIElement[]
                    {
                        grid.Children[grid.Children.Count - 4],
                        grid.Children[grid.Children.Count - 3],
                        grid.Children[grid.Children.Count - 2],
                        grid.Children[grid.Children.Count - 1]
                    };

                    grid.Children.RemoveRange(grid.Children.Count - 4, 4);
                    grid.RowDefinitions.RemoveAt(grid.RowDefinitions.Count - 1);

                    PageContent = Document.AddNewPage();

                    grid = GenerateArticlesGrid();
                    PageContent.Children.Add(grid);
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    foreach (UIElement element in elements)
                    {
                        Grid.SetRow(element, grid.RowDefinitions.Count - 1);
                        grid.Children.Add(element);
                    }
                }
            }

            #endregion

            #region Summe

            TextBlock tb1 = new TextBlock
            {
                Text = "Summe",
                HorizontalAlignment = HorizontalAlignment.Right,
                FontWeight = FontWeights.Bold,
                FontFamily = FontFamily,
                FontSize = FontSize
            };
            TextBlock tb2 = new TextBlock
            {
                Text = sum.ToString("C"),
                FontWeight = FontWeights.Bold,
                FontFamily = FontFamily,
                FontSize = FontSize,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            RowDefinition rd1 = new RowDefinition { Height = new GridLength(4) };
            RowDefinition rd2 = new RowDefinition { Height = GridLength.Auto };
            grid.RowDefinitions.Add(rd1);
            grid.RowDefinitions.Add(rd2);

            Grid.SetRow(tb1, grid.RowDefinitions.Count - 1);
            Grid.SetRow(tb2, grid.RowDefinitions.Count - 1);
            Grid.SetColumn(tb1, 4);
            Grid.SetColumn(tb2, 6);

            grid.Children.Add(tb1);
            grid.Children.Add(tb2);

            //Seite voll
            if (Document.IsPageOverfilled(Document.PageCount - 1))
            {
                grid.RowDefinitions.RemoveRange(grid.RowDefinitions.Count - 2, 2);
                grid.Children.RemoveRange(grid.Children.Count - 2, 2);

                PageContent = Document.AddNewPage();
                grid = GenerateArticlesGrid();
                PageContent.Children.Add(grid);

                grid.RowDefinitions.Add(rd1);
                grid.RowDefinitions.Add(rd2);

                Grid.SetRow(tb1, grid.RowDefinitions.Count - 1);
                Grid.SetRow(tb2, grid.RowDefinitions.Count - 1);

                grid.Children.Add(tb1);
                grid.Children.Add(tb2);
            }

            #endregion

            #region Drucken

            pD.PrintDocument(Document.GetPrintable(), "Warenwirtschaftssystem - Abgabebeleg für Lieferant " + Supplier.Id);

            #endregion
        }

        private Grid GenerateArticlesGrid()
        {
            Grid grid = new Grid { Width = PageContent.Width };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(4) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(4) });

            #region Tabellenkopf

            TextBlock tb = new TextBlock { Text = "Artikelnr.", FontFamily = new FontFamily("Arial"), FontSize = 10 * PtToPx };
            grid.Children.Add(tb);

            tb = new TextBlock { Text = "Artikelbezeichnung", FontFamily = new FontFamily("Arial"), FontSize = 10 * PtToPx };
            Grid.SetColumn(tb, 2);
            grid.Children.Add(tb);

            tb = new TextBlock { Text = "Fälligkeit", FontFamily = new FontFamily("Arial"), FontSize = 10 * PtToPx };
            Grid.SetColumn(tb, 4);
            grid.Children.Add(tb);

            if (PrintPriceInsteadOfPayout)
                tb = new TextBlock { Text = "Preis", FontFamily = new FontFamily("Arial"), FontSize = 10 * PtToPx };
            else
                tb = new TextBlock { Text = "Auszahlung", FontFamily = new FontFamily("Arial"), FontSize = 10 * PtToPx };

            Grid.SetColumn(tb, 6);
            grid.Children.Add(tb);


            Rectangle rectangle = new Rectangle
            {
                StrokeThickness = 0,
                Fill = Brushes.Black,
                Height = 1
            };
            Grid.SetColumnSpan(rectangle, 7);
            Grid.SetRow(rectangle, 2);
            grid.Children.Add(rectangle);

            #endregion

            return grid;
        }

        private void AddArticleRowToGrid(Grid grid, Article article)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            TextBlock tb = new TextBlock { Text = article.ConvertedId.ToString(), FontFamily = new FontFamily("Arial"), FontSize = 10 * PtToPx };
            Grid.SetRow(tb, grid.RowDefinitions.Count - 1);
            grid.Children.Add(tb);

            //Bezeichnung
            tb = new TextBlock { Text = article.Description, FontFamily = FontFamily, FontSize = FontSize };
            Grid.SetColumn(tb, 2);
            Grid.SetRow(tb, grid.RowDefinitions.Count - 1);
            grid.Children.Add(tb);

            string pickup;
            if (!article.PickUp.HasValue)
                pickup = "∞";
            else
                pickup = article.PickUp.Value.ToShortDateString();
            tb = new TextBlock { Text = pickup, FontFamily = new FontFamily("Arial"), FontSize = 10 * PtToPx };
            Grid.SetColumn(tb, 4);
            Grid.SetRow(tb, grid.RowDefinitions.Count - 1);
            grid.Children.Add(tb);

            //Lieferantenanteil/Preis

            if (PrintPriceInsteadOfPayout)
                tb = new TextBlock { Text = article.Price.ToString("C"), FontFamily = new FontFamily("Arial"), FontSize = 10 * PtToPx, HorizontalAlignment = HorizontalAlignment.Right };
            else
                tb = new TextBlock { Text = article.SupplierProportion.ToString("C"), FontFamily = new FontFamily("Arial"), FontSize = 10 * PtToPx, HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetColumn(tb, 6);
            Grid.SetRow(tb, grid.RowDefinitions.Count - 1);
            grid.Children.Add(tb);

            //Mängel
            if (article.Defects != null && article.Defects.Count > 0)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                string defects = "*";
                defects = article.Defects[0].Title;
                for (int i = 1; i < article.Defects.Count; i++)
                {
                    defects += ", " + article.Defects[i].Title;
                }
                tb = new TextBlock { Text = defects, FontFamily = FontFamily, FontSize = FontSize - 2 * PtToPx };
                Grid.SetColumn(tb, 2);
                Grid.SetRow(tb, grid.RowDefinitions.Count - 1);
                grid.Children.Add(tb);
            }

            //Bemerkungen
            if (!string.IsNullOrWhiteSpace(article.Notes))
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                tb = new TextBlock { Text = "*" + article.Notes.Trim(), FontFamily = FontFamily, FontSize = FontSize - 2 * PtToPx };
                Grid.SetColumn(tb, 2);
                Grid.SetRow(tb, grid.RowDefinitions.Count - 1);
                grid.Children.Add(tb);
            }
        }
    }
}
