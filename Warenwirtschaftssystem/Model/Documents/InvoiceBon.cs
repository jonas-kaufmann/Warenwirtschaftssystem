using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using Warenwirtschaftssystem.Model.Db;
using System.Printing;
using System.Collections.Generic;

namespace Warenwirtschaftssystem.Model.Documents
{
    public class InvoiceBon
    {
        private FixedDocument Bon = new FixedDocument();

        private const double CmToPx = 96d / 2.54;
        private const double PtToPx = 96d / 72d;
        private const double FontSize = 8 * PtToPx;
        private FontFamily FontFamily = new FontFamily("Arial");
        private Thickness PageMargin = new Thickness(20, 0, 12, 12);
        private double PageWidth;

        private DataModel Data;
        private DbModel MainDb;
        private Supplier Customer;
        private List<Article> Articles = new List<Article>();
        private readonly int DocumentId;

        public InvoiceBon(DataModel data, Document sale)
        {
            Data = data;
            MainDb = new DbModel(data.MainConnectionString);
            DocumentId = sale.Id;
            Customer = sale.Supplier;

            foreach (Article a in sale.Articles)
            {
                SavedArticleAttributes savedArticleAttributes = null;
                if (sale.SavedArticleAttributes != null && sale.SavedArticleAttributes.Count != 0)
                {
                    savedArticleAttributes = sale.SavedArticleAttributes.Where(s => s.ArticleId == a.Id).FirstOrDefault();
                }

                if (savedArticleAttributes == null)
                {
                    Article article = new Article
                    {
                        Id = a.Id,
                        Price = a.Price,
                        Defects = a.Defects
                    };

                    article.SetDescriptionExplicitly(a.Description);
                    Articles.Add(article);
                }
                else
                {
                    Article article = new Article
                    {
                        Id = a.Id,
                        Price = savedArticleAttributes.Price,
                        Defects = a.Defects
                    };

                    article.SetDescriptionExplicitly(a.Description);
                    Articles.Add(article);
                }
            }
        }

        public bool? CreateAndPrint()
        {
            PrintQueue pQ = null;

            foreach (PrintQueue p in new LocalPrintServer().GetPrintQueues(new[] { EnumeratedPrintQueueTypes.Local, EnumeratedPrintQueueTypes.Connections }))
            {
                if (p.FullName == Data.StandardPrinters.BonPrinter)
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

            PageWidth = pD.PrintableAreaWidth;

            FixedPage fP = new FixedPage
            {
                Width = PageWidth,
                Margin = PageMargin
            };
            PageContent pC = new PageContent { Child = fP };
            Bon.Pages.Add(pC);

            StackPanel pageContent = new StackPanel { Width = PageWidth - PageMargin.Left - PageMargin.Right };
            fP.Children.Add(pageContent);

            #region Kopf

            #region Geschäftsdaten

            Grid grid = new Grid();
            pageContent.Children.Add(grid);

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(FontSize) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(FontSize) });

            StackPanel sP = new StackPanel() { HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetColumn(sP, 2);
            grid.Children.Add(sP);

            sP.Children.Add(new TextBlock
            {
                Text = MainDb.Settings.Where(s => s.Key == "Shopdescription").Single().Value,
                FontFamily = FontFamily,
                FontSize = FontSize + PtToPx,
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

            #region Kundendaten

            if (Customer != null)
            {
                if (string.IsNullOrWhiteSpace(Customer.Company))
                {

                    grid.Children.Add(new TextBlock
                    {
                        Text = Customer.Name
                            + "\n" + Customer.Street
                            + "\n" + Customer.Postcode + " " + Customer.Place,
                        FontFamily = FontFamily,
                        FontSize = FontSize
                    });
                }
                else
                {
                    grid.Children.Add(new TextBlock
                    {
                        Text = Customer.Name
                            + "\n" + Customer.Company
                            + "\n" + Customer.Street
                            + "\n" + Customer.Postcode + " " + Customer.Place,
                        FontFamily = FontFamily,
                        FontSize = FontSize
                    });
                }
            }

            #endregion

            #region Rechnungsnr und Datum

            TextBlock tB = new TextBlock
            {
                Text = "Kaufbeleg Nr. " + DocumentId,
                FontFamily = FontFamily,
                FontSize = FontSize,
                FontWeight = FontWeights.Bold
            };

            Grid.SetRow(tB, 2);
            grid.Children.Add(tB);

            tB = new TextBlock
            {
                Text =  DateTime.Now.ToShortDateString(),
                FontFamily = FontFamily,
                FontSize = FontSize,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Grid.SetRow(tB, 2);
            Grid.SetColumn(tB, 2);
            grid.Children.Add(tB);

            #endregion

            #endregion

            #region Artikel

            grid = new Grid();
            pageContent.Children.Add(grid);

            #region Header

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(4) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            grid.Children.Add(new TextBlock
            {
                Text = "Artikelnr.",
                FontFamily = FontFamily,
                FontSize = FontSize
            });

            tB = new TextBlock
            {
                Text = "Artikelbezeichnung",
                FontFamily = FontFamily,
                FontSize = FontSize
            };
            Grid.SetColumn(tB, 2);
            grid.Children.Add(tB);

            tB = new TextBlock
            {
                Text = "Preis",
                FontFamily = FontFamily,
                FontSize = FontSize
            };
            Grid.SetColumn(tB, 4);
            grid.Children.Add(tB);

            Rectangle rect = new Rectangle
            {
                StrokeThickness = 0,
                Height = 1,
                Fill = Brushes.Black,
                Width = pageContent.Width,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(rect, 1);
            Grid.SetColumnSpan(rect, 5);
            grid.Children.Add(rect);

            #endregion

            #region Zeilen

            decimal sum = 0;

            foreach (var article in Articles)
            {
                sum += article.Price; 

                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                tB = new TextBlock
                {
                    Text = article.ConvertedId.ToString(),
                    FontFamily = FontFamily,
                    FontSize = FontSize
                };
                Grid.SetRow(tB, grid.RowDefinitions.Count - 1);
                grid.Children.Add(tB);

                tB = new TextBlock
                {
                    Text = article.Description,
                    FontFamily = FontFamily,
                    FontSize = FontSize,
                    TextWrapping = TextWrapping.Wrap
                };
                Grid.SetColumn(tB, 2);
                Grid.SetRow(tB, grid.RowDefinitions.Count - 1);
                grid.Children.Add(tB);

                tB = new TextBlock
                {
                    Text = article.Price.ToString("C"),
                    FontFamily = FontFamily,
                    FontSize = FontSize,
                    HorizontalAlignment =  HorizontalAlignment.Right
                };
                Grid.SetColumn(tB, 4);
                Grid.SetRow(tB, grid.RowDefinitions.Count - 1);
                grid.Children.Add(tB);

                //Mängel hinzufügen
                if (article.Defects != null && article.Defects.Count != 0)
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    string defects = "*" + article.Defects[0].Title;

                    for (int i = 1; i < article.Defects.Count; i++)
                        defects += ", " + article.Defects[i].Title;

                    tB = new TextBlock
                    {
                        Text = defects,
                        FontFamily = FontFamily,
                        FontSize = FontSize - 2 * PtToPx
                    };

                    Grid.SetColumn(tB, 2);
                    Grid.SetColumnSpan(tB, 3);
                    Grid.SetRow(tB, grid.RowDefinitions.Count - 1);
                    grid.Children.Add(tB);
                }

            }

            #endregion

            #endregion

            #region Summe

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(4) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            tB = new TextBlock
            {
                Text = "Summe",
                HorizontalAlignment = HorizontalAlignment.Right,
                FontFamily = FontFamily,
                FontSize = FontSize,
                FontWeight = FontWeights.Bold
            };
            Grid.SetColumn(tB, 2);
            Grid.SetRow(tB, grid.RowDefinitions.Count - 1);
            grid.Children.Add(tB);

            tB = new TextBlock
            {
                Text = sum.ToString("C"),
                HorizontalAlignment = HorizontalAlignment.Right,
                FontFamily = FontFamily,
                FontSize = FontSize,
                FontWeight = FontWeights.Bold
            };
            Grid.SetColumn(tB, 4);
            Grid.SetRow(tB, grid.RowDefinitions.Count - 1);
            grid.Children.Add(tB);

            #endregion

            #region Footer

            Setting bonFooter = MainDb.Settings.Where(setting => setting.Key == "Bon-Footer").FirstOrDefault();

            if (bonFooter != null && !string.IsNullOrWhiteSpace(bonFooter.Value))
            {
                pageContent.Children.Add(new TextBlock
                {
                    Text = "\n\n" + bonFooter.Value,
                    FontFamily = FontFamily,
                    FontSize = FontSize,
                    HorizontalAlignment = HorizontalAlignment.Left
                });
            }

            #endregion

            #region Drucken

            pD.PrintDocument(Bon.DocumentPaginator, "Warenwirtschaftssystem - Rechnungsbon");
            return true;

            #endregion
        }
    }
}
