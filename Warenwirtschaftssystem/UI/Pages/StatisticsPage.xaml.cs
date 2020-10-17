using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Warenwirtschaftssystem.Model;
using Warenwirtschaftssystem.Model.Db;
using Warenwirtschaftssystem.Model.Documents;
using Warenwirtschaftssystem.UI.Windows;

namespace Warenwirtschaftssystem.UI.Pages
{
    public partial class StatisticsPage : Page
    {
        // Attribute
        private DataModel Data;
        private Window OwnerWindow;
        private DbModel MainDb;
        private CollectionViewSource ArticlesCVS;
        private ObservableCollection<SaleArticle> Articles = new ObservableCollection<SaleArticle>();
        private DateTime? FromDateTime;
        private DateTime? ToDateTime;

        #region Initialisierung

        // Konstruktor
        public StatisticsPage(Window ownerWindow, DataModel data)
        {
            Data = data;
            OwnerWindow = ownerWindow;
            InitializeComponent();

            DateRangeFromDTP.Value = DateTime.Now.Date;
            DateRangeToDTP.Value = DateTime.Now.Date.AddMilliseconds(86400000 - 1);

            MainDb = new DbModel(data.MainConnectionString);
            ArticlesCVS = (CollectionViewSource)FindResource("ArticlesCVS");
            ArticlesCVS.Source = Articles;
            Articles.CollectionChanged += Articles_CollectionChanged;

            ApplyDateRangeBtn_Click(null, null);
        }

        private void Articles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            recalculateSums();

            #region Spaltenbreite aktualisieren

            ArticlesDG.Columns[3].Width = 0;
            ArticlesDG.UpdateLayout();
            ArticlesDG.Columns[3].Width = new DataGridLength(1, DataGridLengthUnitType.Star);

            #endregion
        }

        private void recalculateSums()
        {
            #region Summe & Einträge aktualisieren

            decimal sumSupplierProportion = 0;
            decimal sum = 0;
            int count = 0;

            if (ArticlesDG.SelectedItems.Count == 0)
            {
                foreach (SaleArticle a in Articles)
                {
                    sumSupplierProportion += a.Payout;
                    sum += a.Price;
                    count++;
                }
            } else
            {
                foreach (var a in ArticlesDG.SelectedItems)
                {
                    if (a is SaleArticle article)
                    {
                        sumSupplierProportion += article.Payout;
                        sum += article.Price;
                        count++;
                    }
                }
            }

            SumTB.Text = "Eigenanteil " + (sum - sumSupplierProportion).ToString("C") + " - Lieferantenanteil " + sumSupplierProportion.ToString("C") + " - Summe " + sum.ToString("C") + " - Einträge " + count;

            #endregion
        }

        #endregion

        private void ApplyDateRangeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DateRangeFromDTP.Value.HasValue && DateRangeToDTP.Value.HasValue && DateRangeFromDTP.Value.Value > DateRangeToDTP.Value.Value)
            {
                return;
            }


            if (DateRangeFromDTP.Value != null && DateRangeToDTP.Value != null && (FromDateTime == null || DateRangeFromDTP.Value != FromDateTime) && (ToDateTime == null || DateRangeToDTP.Value != ToDateTime))
            {
                Articles.Clear();
                List<Document> sales = MainDb.Documents.Where(b => b.DateTime >= DateRangeFromDTP.Value && b.DateTime <= DateRangeToDTP.Value && b.DocumentType == DocumentType.Bill).ToList();
                AddSales(sales);
                FromDateTime = DateRangeFromDTP.Value;
                ToDateTime = DateRangeToDTP.Value;
            }
            else if (DateRangeFromDTP.Value != null && (FromDateTime == null || DateRangeFromDTP.Value != FromDateTime))
            {
                Articles.Clear();
                List<Document> sales = MainDb.Documents.Where(b => b.DateTime >= DateRangeFromDTP.Value && b.DocumentType == DocumentType.Bill).ToList();
                AddSales(sales);
                FromDateTime = DateRangeFromDTP.Value;
                ToDateTime = null;
            }
            else if (DateRangeToDTP.Value != null && (ToDateTime == null || DateRangeToDTP.Value != ToDateTime))
            {
                Articles.Clear();
                List<Document> sales = MainDb.Documents.Where(b => b.DateTime <= DateRangeToDTP.Value && b.DocumentType == DocumentType.Bill).ToList();
                AddSales(sales);
                FromDateTime = null;
                ToDateTime = DateRangeToDTP.Value;
            }
        }

        private void AddSales(List<Document> sales)
        {
            foreach (Document s in sales)
            {
                foreach (Article a in s.Articles)
                {
                    SavedArticleAttributes savedArticleAttributes = null;
                    if (s.SavedArticleAttributes != null && s.SavedArticleAttributes.Count != 0)
                    {
                        savedArticleAttributes = s.SavedArticleAttributes.Where(sa => sa.ArticleId == a.Id).FirstOrDefault();
                    }

                    if (savedArticleAttributes == null)
                        Articles.Add(new SaleArticle()
                        {
                            Id = s.Id,
                            DateTime = s.DateTime,
                            Description = a.Description,
                            ArticleConvertedId = a.ConvertedId,
                            Payout = a.SupplierProportion,
                            Price = a.Price,
                            Supplier = a.Supplier,
                            Sale = s
                        });
                    else
                        Articles.Add(new SaleArticle()
                        {
                            Id = s.Id,
                            DateTime = s.DateTime,
                            Description = a.Description,
                            ArticleConvertedId = a.ConvertedId,
                            Payout = savedArticleAttributes.Payout,
                            Price = savedArticleAttributes.Price,
                            Supplier = a.Supplier,
                            Sale = s
                        });
                }
            }
        }

        private void DateRangeDTP_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                ApplyDateRangeBtn_Click(null, null);
        }

        private void PrintBonBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ArticlesDG.SelectedItem is SaleArticle article)
            {
                new InvoiceBon(Data, article.Sale).CreateAndPrint();
            }
        }

        private void PrintDocBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ArticlesDG.SelectedItem is SaleArticle article)
            {
                if (article.Sale.Supplier != null)
                {
                    new InvoiceDoc(Data, article.Sale).CreateAndPrintDocument();
                }
            }
        }

        private void ArticlesDG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            recalculateSums();
        }
    }

    public class SaleArticle
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public int ArticleConvertedId { get; set; }
        public string Description { get; set; }
        public Supplier Supplier { get; set; }
        public decimal Payout { get; set; }
        public decimal Price { get; set; }
        public Document Sale { get; set; }
    }
}
