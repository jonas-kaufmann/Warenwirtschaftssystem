using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Warenwirtschaftssystem.Model;
using Warenwirtschaftssystem.Model.Db;
using Warenwirtschaftssystem.UI.Windows;
using System.Windows.Input;
using Warenwirtschaftssystem.Model.Printing;
using System.Collections.Generic;
using System.Windows.Media;
using System;
using Warenwirtschaftssystem.Model.Documents;
using Microsoft.EntityFrameworkCore;

namespace Warenwirtschaftssystem.UI.Pages
{
    public partial class CommercePage : Page
    {
        // Attribute
        private DataModel Data;
        private Window OwnerWindow;
        private DbModel MainDb;
        private decimal Sum = 0;
        private List<Defect> EmptyDefectsList = new List<Defect> { new Defect { Name = "" } };
        private DocumentBase document;
        private readonly CollectionViewSource ArticlesCVS;

        #region Initialisierung

        // Konstruktor
        public CommercePage(Window ownerWindow, DataModel data)
        {
            Data = data;
            OwnerWindow = ownerWindow;
            MainDb = Data.CreateDbConnection();

            InitializeComponent();

            ArticlesCVS = FindResource("ArticlesCVS") as CollectionViewSource;
            DefectsDG.ItemsSource = EmptyDefectsList;

            ArticleIdTB.Focus();
            OwnerWindow.KeyDown += OwnerWindow_KeyDown;
        }

        #endregion

        private void NewDocument()
        {
            if (document != null)
                document.Dispose();

            document = new InvoiceDocument(MainDb);

            ArticlesCVS.Source = document.Document.DisplayArticles;
            document.Document.PropertyChanged += Document_PropertyChanged;
        }

        private void AddArticle()
        {
            if (document == null)
                NewDocument();

            string articleId = ArticleIdTB.Text;

            if (int.TryParse(articleId, out int id))
            {

                id -= 100000;
                if (MainDb.Articles.FirstOrDefault(a => a.Id == id) is Article article)
                {
                    MainDb.Entry(article).Reload();
                    if (article.Status == Status.Reserved)
                    {
                        MessageBoxResult result;

                        if (article.ReservingSupplier != null)
                            result = MessageBox.Show("Dieser Artikel ist auf den Lieferanten " + article.ReservingSupplier.Name + " (" + article.ReservingSupplier.Id + ") reserviert. Soll der Artikel wieder für den Verkauf freigegeben werden?", "Artikel ist reserviert", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                        else
                            result = MessageBox.Show("Dieser Artikel ist reserviert. Soll der Artikel wieder für den Verkauf freigegeben werden?", "Artikel ist reserviert", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);

                        if (result == MessageBoxResult.Yes)
                        {
                            article.Status = Status.Sortiment;
                            article.ReservingSupplier = null;
                            article.ReservedFrom = null;
                            article.ReservedUntil = null;
                        }
                        else
                        {
                            return;
                        }
                    }

                    try
                    {
                        document.AddArticle(article);
                    }
                    catch (InvalidOperationException e)
                    {
                        _ = MessageBox.Show(e.ToString(), "Artikel konnte nicht hinzugefügt werden", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    var displayArticle = document.Document.DisplayArticles.Single(a => a.Id == article.Id);
                    ArticlesDG.SelectedItem = displayArticle;
                    ArticlesDG.ScrollIntoView(displayArticle);
                }

            }

            ArticleIdTB.Text = "";
            _ = ArticleIdTB.Focus();
        }

        private void Document_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(document.Document.DisplayArticles))
                return;

            // update ArticlesDG's column width
            ArticlesDG.Columns[1].Width = 0;
            ArticlesDG.UpdateLayout();
            ArticlesDG.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Star);

            // calculate sum
            Sum = 0;
            var articles = document.Document.DisplayArticles;
            if (articles.Count > 0)
            {
                foreach (Article a in articles)
                {
                    Sum += a.Price;
                }
            }
            SumTB.Text = "Anzahl " + articles.Count + " - Summe " + Sum.ToString("C");
        }

        private void ClearArticlesBtn_Click(object sender, RoutedEventArgs e)
        {
            if (document != null)
            {
                document.Dispose();
                document = null;
            }
        }

        private void SellBtn_Click(object sender, RoutedEventArgs e)
        {
            if (document != null && document.Document.Articles.Count > 0)
            {
                CustomMessageBox cMB = new CustomMessageBox(OwnerWindow, "Bon drucken?", "Soll ein Bon gedruckt werden?", "Bon", "Kaufbeleg (A4)", "Kein Beleg");
                cMB.ShowDialog();

                if (cMB.Result == PressedButton.None)
                    return;

                switch (cMB.Result)
                {
                    case PressedButton.First:
                        new InvoiceBon(Data, document.Document).CreateAndPrint();
                        break;
                    case PressedButton.Second:
                        PopupWindow pW = new PopupWindow(OwnerWindow, Data)
                        {
                            Title = "Kunde auswählen"
                        };

                        PickSupplierPage pSP = new PickSupplierPage(pW, Data, MainDb);
                        pW.Content = pSP;
                        pW.Initialize();
                        pW.ShowDialog();

                        if (pSP.SelectedSupplier == null)
                            return;

                        document.Save();
                        new InvoiceDoc(Data, document.Document).CreateAndPrintDocument();
                        break;
                    case PressedButton.Third:
                        document.Save();
                        break;

                }

                ClearArticlesBtn_Click(null, null);
            }
        }

        private void ReserveBtn_Click(object sender, RoutedEventArgs e)
        {
            //if (document.Document.Articles.Count > 0)
            //{
            //    #region Supplier, auf den reserviert werden soll, auswählen

            //    PopupWindow popup = new PopupWindow(OwnerWindow, Data);
            //    PickSupplierPage pickSupplierPage = new PickSupplierPage(popup, Data, MainDb);
            //    popup.Content = pickSupplierPage;
            //    popup.Initialize();
            //    popup.ShowDialog();

            //    Supplier reservingSupplier = pickSupplierPage.SelectedSupplier;

            //    if (reservingSupplier == null)
            //        return;

            //    #endregion

            //    #region Daten ermitteln

            //    PopupWindow pW = new PopupWindow(OwnerWindow, Data)
            //    {
            //        Title = "Reservierung abschließen"
            //    };
            //    DateFromUntilPage dFUP = new DateFromUntilPage(pW);
            //    pW.Content = dFUP;
            //    pW.Initialize();
            //    pW.ShowDialog();

            //    if (!dFUP.Result)
            //        return;

            //    #endregion

            //    if (reservingSupplier != null)
            //    {
            //        MessageBoxResult result = MessageBox.Show("Soll ein Bon gedruckt werden?", "Bon drucken?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);

            //        foreach (Article a in Articles)
            //        {
            //            a.Status = Status.Reserved;
            //            a.Reservation = new ArticleReservation
            //            {
            //                Supplier = reservingSupplier,
            //                From = dFUP.DateFrom,
            //                Until = dFUP.DateUntil,
            //                Article = a
            //            };
            //        }

            //        if (result == MessageBoxResult.Yes || result == MessageBoxResult.No)
            //        {
            //            DocumentManager documents = new DocumentManager(Data, MainDb);
            //            Document document = documents.AddDocument(DocumentType.Reservation, Articles.ToList(), null, reservingSupplier, false);
            //            MainDb.SaveChangesRetryOnUserInput();
            //            ClearArticlesBtn_Click(null, null);

            //            if (result == MessageBoxResult.Yes)
            //                new ReserveBon(Data, document).CreateAndPrint(1);
            //        }
            //    }
            //}
        }

        private void ArticleIdTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) AddArticle();
        }

        private void ArticlesDG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(ArticlesDG.SelectedItem is Article article) || article.Defects == null || article.Defects.Count == 0)
            {
                DefectsDG.ItemsSource = EmptyDefectsList;
            }
            else
            {
                var defectsList = new List<Defect>();

                foreach (var defect in article.Defects)
                {
                    defectsList.Add(defect);
                }

                DefectsDG.ItemsSource = defectsList;
            }
        }

        private void OwnerWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F12 || e.SystemKey == Key.F12)
            {
                SellBtn_Click(null, null);
            }
            else if (e.Key == Key.F10 || e.SystemKey == Key.F10)
            {
                ReserveBtn_Click(null, null);
            }
        }

        private void ReservedArticlesBtn_Click(object sender, RoutedEventArgs e)
        {
            //PopupWindow pW = new PopupWindow(OwnerWindow, Data);
            //ReservedArticlesPage rAP = new ReservedArticlesPage(pW, MainDb);
            //pW.Content = rAP;
            //pW.Initialize();
            //pW.Closing += rAP.OwnerWindow_Closing;
            //pW.ShowDialog();
        }

        // deletion of articles
        private void ArticlesDG_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            foreach (Article article in ArticlesDG.SelectedItems)
            {
                document.RemoveArticle(article.Id);
                MainDb.Entry(article).State = EntityState.Unchanged;
                e.Handled = true;
            }
        }

        private void ChangePriceBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ArticlesDG.SelectedItem is Article article)
            {
                PopupWindow pW = new PopupWindow(OwnerWindow, Data) { Title = "Preis/Lieferantenanteil von Artikel Nr " + article.ConvertedId + " ändern" };
                ChangePriceAndPayoutPage cPAPP = new ChangePriceAndPayoutPage(Data, pW, MainDb, article);
                pW.Content = cPAPP;
                pW.Initialize();
                pW.ShowDialog();
            }
        }
    }
}