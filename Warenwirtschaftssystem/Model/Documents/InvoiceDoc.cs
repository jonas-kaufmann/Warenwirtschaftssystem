﻿using System;
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
    public class InvoiceDoc
    {
        private Supplier Customer;
        private DataModel Data;
        private DbModel MainDb;
        private List<Article> Articles = new List<Article>();
        private readonly int DocumentId;

        private DocumentWithPageIndex Document;
        private StackPanel PageContent;
        private double PageWidth;
        private double PageHeight;
        private Thickness PageMargin = new Thickness(2.5 * CmToPx, 1.69 * CmToPx, CmToPx, 1.69 * CmToPx);

        private FontFamily FontFamily = new FontFamily("Arial");
        private const double FontSize = 10 * PtToPx;

        private const double CmToPx = 96d / 2.54;
        private const double PtToPx = 96d / 72d;

        public InvoiceDoc(DataModel data, Document sale)
        {
            Data = data;
            MainDb = Data.CreateDbConnection();
            Customer = sale.Supplier;
            DocumentId = sale.Id;

            foreach (Article a in sale.Articles)
            {
                SavedArticleAttributes savedArticleAttributes = null;
                if (sale.SavedArticleAttributes != null && sale.SavedArticleAttributes.Count != 0)
                {
                    savedArticleAttributes = sale.SavedArticleAttributes.Where(s => s.Article.Id == a.Id).FirstOrDefault();
                }

                if (savedArticleAttributes == null)
                {
                    Article article = new Article
                    {
                        Id = a.Id,
                        Price = a.Price,
                        Defects = a.Defects
                    };

                    article.Description = a.Description;
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

                    article.Description = a.Description;
                    Articles.Add(article);
                }
            }
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
            else if (!pD.ShowDialog().GetValueOrDefault(false))
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

            #region Anschrift

            TextBlock tb;

            if (string.IsNullOrWhiteSpace(Customer.Company))
            {
                tb = new TextBlock
                {
                    Text = "\n\n\n"
                        + Customer.Title.GetDescription() + " " + Customer.Name
                        + "\n" + Customer.Street
                        + "\n" + Customer.Postcode + " " + Customer.Place,
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
                        + Customer.Title.GetDescription() + " " + Customer.Name
                        + "\n" + Customer.Company
                        + "\n" + Customer.Street
                        + "\n" + Customer.Postcode + " " + Customer.Place,
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

            #region Datum & BelegNr

            tb = new TextBlock
            {
                Text = MainDb.Settings.Where(s => s.Key == "Shopplace").Single().Value + ", " + DateTime.Now.ToShortDateString()
                    + "\nBelegnr. " + DocumentId,
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
                Text = "\n\nKaufbeleg\n\n",
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
                sum += article.Price;

                AddArticleRowToGrid(grid, article);

                //Seite voll
                if (Document.IsPageOverfilled(Document.PageCount - 1))
                {
                    UIElement[] elements = new UIElement[]
                    {
                        grid.Children[grid.Children.Count - 3],
                        grid.Children[grid.Children.Count - 2],
                        grid.Children[grid.Children.Count - 1]
                    };

                    grid.Children.RemoveRange(grid.Children.Count - 3, 3);
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
            Grid.SetColumn(tb1, 2);
            Grid.SetColumn(tb2, 4);

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

            pD.PrintDocument(Document.GetPrintable(), "Warenwirtschaftssystem - Abgabebeleg für Lieferant " + Customer.Id);

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


            tb = new TextBlock { Text = "Preis", FontFamily = new FontFamily("Arial"), FontSize = 10 * PtToPx };
            Grid.SetColumn(tb, 4);
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

            TextBlock tb = new TextBlock { Text = article.ConvertedId.ToString(), FontFamily = new FontFamily("Arial"), FontSize = FontSize };
            Grid.SetRow(tb, grid.RowDefinitions.Count - 1);
            grid.Children.Add(tb);

            tb = new TextBlock { Text = article.Description, FontFamily = new FontFamily("Arial"), FontSize = FontSize };
            Grid.SetColumn(tb, 2);
            Grid.SetRow(tb, grid.RowDefinitions.Count - 1);
            grid.Children.Add(tb);

            tb = new TextBlock { Text = article.Price.ToString("C"), FontFamily = new FontFamily("Arial"), FontSize = FontSize, HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetColumn(tb, 4);
            Grid.SetRow(tb, grid.RowDefinitions.Count - 1);
            grid.Children.Add(tb);

            //Mängel hinzufügen
            if (article.Defects != null && article.Defects.Count != 0)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                string defects = "*" + ArticleAttributes.ToRepresentation(article.Defects);

                tb = new TextBlock
                {
                    Text = defects,
                    FontFamily = FontFamily,
                    FontSize = FontSize - 2 * PtToPx
                };

                Grid.SetColumn(tb, 2);
                Grid.SetColumnSpan(tb, 3);
                Grid.SetRow(tb, grid.RowDefinitions.Count - 1);
                grid.Children.Add(tb);
            }
        }
    }
}
