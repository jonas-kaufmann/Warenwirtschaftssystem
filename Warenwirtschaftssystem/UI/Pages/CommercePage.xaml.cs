using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Warenwirtschaftssystem.Model;
using Warenwirtschaftssystem.Model.Db;
using Warenwirtschaftssystem.UI.Windows;
using System.Windows.Input;
using System;
using Warenwirtschaftssystem.Model.Documents;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media;

namespace Warenwirtschaftssystem.UI.Pages
{
    public partial class CommercePage : Page
    {
        // Attribute
        private DataModel Data;
        private Window OwnerWindow;
        private DbModel MainDb;
        private ObservableCollection<Article> Articles = new ObservableCollection<Article>();
        private decimal Sum = 0;
        private List<Defect> EmptyDefectsList = new List<Defect> { new Defect { Title = "" } };

        //Preisänderung
        private bool IsSupplierProportionFixed = false;
        private List<Article> ArticlesPriceChanged = new List<Article>();

        #region Initialisierung

        // Konstruktor
        public CommercePage(Window ownerWindow, DataModel data)
        {
            Data = data;
            OwnerWindow = ownerWindow;
            MainDb = new DbModel(data.MainConnectionString);

            InitializeComponent();

            (FindResource("ArticlesCVS") as CollectionViewSource).Source = Articles;
            DefectsDG.ItemsSource = EmptyDefectsList;
            Articles.CollectionChanged += Articles_CollectionChanged;

            ArticleIdTB.Focus();
            OwnerWindow.KeyDown += OwnerWindow_KeyDown;
        }

        #endregion

        private void AddArticle()
        {
            string articleId = ArticleIdTB.Text;
            Article article;

            if (int.TryParse(articleId, out int id))
            {
                id -= 100000;
                if ((article = Articles.Where(a => a.Id == id).FirstOrDefault()) != null)
                    ArticlesDG.SelectedItem = article;
                else
                {
                    if ((article = MainDb.Articles.Where(a => a.Id == id).SingleOrDefault()) != null && !(article.Status == Status.ClosedOut || article.Status == Status.PayedOut || article.Status == Status.Returned))
                    {
                        MainDb.Entry(article).Reload();

                        if (article.Status == Status.Reserved)
                        {
                            MessageBoxResult result;

                            if (article.Reservation != null)
                                result = MessageBox.Show("Dieser Artikel ist auf den Lieferanten " + article.Reservation.Supplier.Name + " (" + article.Reservation.Supplier.Id + ") reserviert. Soll der Artikel wieder für den Verkauf freigegeben werden?", "Artikel ist reserviert", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);
                            else
                                result = MessageBox.Show("Dieser Artikel ist reserviert. Soll der Artikel wieder für den Verkauf freigegeben werden?", "Artikel ist reserviert", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);

                            if (result == MessageBoxResult.Yes)
                            {
                                article.Status = Status.Sortiment;
                                if (article.Reservation != null)
                                {
                                    MainDb.ArticleReservations.Remove(article.Reservation);
                                    article.Reservation = null;
                                }
                            }
                            else return;
                        }

                        if (article.Status == Status.Sold)
                        {
                            article.Price *= -1;
                            article.SupplierProportion *= -1;

                            Document document = MainDb.Documents.Where(d => d.Articles.Where(a => a.Id == article.Id).FirstOrDefault() != null).OrderByDescending(d => d.Id).FirstOrDefault();
                            if (document != null)
                            {
                                article.Sold = document.DateTime;
                            }
                        }

                        Articles.Add(article);

                        ArticleIdTB.Text = "";
                        ArticleIdTB.Focus();

                        article.PropertyChanged += Article_PropertyChanged;

                        //Column-Width anpassen
                        ArticlesDG.Columns[1].Width = 0;
                        ArticlesDG.UpdateLayout();
                        ArticlesDG.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Star);

                        // Scroll to end
                        if (ArticlesDG.Items.Count > 0)
                        {
                            if (VisualTreeHelper.GetChild(ArticlesDG, 0) is Decorator border)
                            {
                                if (border.Child is ScrollViewer scroll) scroll.ScrollToEnd();
                            }
                        }
                    }
                }
            }
        }

        private void Article_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Price")
                Articles_CollectionChanged(null, null);
        }

        private void ClearArticlesBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (Article article in Articles)
            {
                article.Price = Math.Abs(article.Price);
                article.SupplierProportion = Math.Abs(article.SupplierProportion);
            }
            Articles.Clear();
            ArticleIdTB.Focus();

            Documents documents = new Documents(Data, MainDb);
            foreach (Article article in ArticlesPriceChanged)
            {
                documents.NotifyArticlePropertiesChanged(article.Id);
            }

            MainDb.SaveChanges();
        }

        private void SellBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Articles.Count > 0)
            {
                CustomMessageBox cMB = new CustomMessageBox(OwnerWindow, "Bon drucken?", "Soll ein Bon gedruckt werden?", "Bon", "Kaufbeleg (A4)", "Kein Beleg");
                cMB.ShowDialog();

                if (cMB.Result == PressedButton.None)
                    return;

                Documents documents = new Documents(Data, MainDb);

                List<SavedArticleAttributes> savedArticleAttributes = new List<SavedArticleAttributes>();

                foreach (Article a in Articles)
                {
                    if (a.Status == Status.Sold)
                    {
                        savedArticleAttributes.Add(new SavedArticleAttributes
                        {
                            ArticleId = a.Id,
                            Payout = a.SupplierProportion,
                            Price = a.Price
                        });
                        a.Status = Status.Sortiment;
                    }
                    else
                        a.Status = Status.Sold;
                }

                if (cMB.Result == PressedButton.One)
                {
                    Document document = documents.AddDocument(DocumentType.Bill, Articles.ToList(), savedArticleAttributes, null, false);
                    MainDb.SaveChanges();
                    new InvoiceBon(Data, document).CreateAndPrint();
                }
                else if (cMB.Result == PressedButton.Two)
                {
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
                    else
                    {
                        Document document = documents.AddDocument(DocumentType.Bill, Articles.ToList(), savedArticleAttributes, pSP.SelectedSupplier, false);
                        MainDb.SaveChanges();
                        new InvoiceDoc(Data, document).CreateAndPrintDocument();
                    }
                }
                else if (cMB.Result == PressedButton.Three)
                {
                    documents.AddDocument(DocumentType.Bill, Articles.ToList(), savedArticleAttributes, null, false);
                }

                ClearArticlesBtn_Click(null, null);
            }
        }

        private void ReserveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Articles.Count > 0 && Articles.Where(a => a.Status != Status.Sortiment && a.Status != Status.InStock).SingleOrDefault() == null)
            {
                #region Supplier, auf den reserviert werden soll, auswählen

                PopupWindow popup = new PopupWindow(OwnerWindow, Data);
                PickSupplierPage pickSupplierPage = new PickSupplierPage(popup, Data, MainDb);
                popup.Content = pickSupplierPage;
                popup.Initialize();
                popup.ShowDialog();

                Supplier reservingSupplier = pickSupplierPage.SelectedSupplier;

                if (reservingSupplier == null)
                    return;

                #endregion

                #region Daten ermitteln

                PopupWindow pW = new PopupWindow(OwnerWindow, Data)
                {
                    Title = "Reservierung abschließen"
                };
                DateFromUntilPage dFUP = new DateFromUntilPage(pW);
                pW.Content = dFUP;
                pW.Initialize();
                pW.ShowDialog();

                if (!dFUP.Result)
                    return;

                #endregion

                if (reservingSupplier != null)
                {
                    MessageBoxResult result = MessageBox.Show("Soll ein Bon gedruckt werden?", "Bon drucken?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);

                    foreach (Article a in Articles)
                    {
                        a.Status = Status.Reserved;
                        a.Reservation = new ArticleReservation
                        {
                            Supplier = reservingSupplier,
                            From = dFUP.DateFrom,
                            Until = dFUP.DateUntil,
                            ArticleId = a.Id
                        };
                    }

                    if (result == MessageBoxResult.Yes || result == MessageBoxResult.No)
                    {
                        Documents documents = new Documents(Data, MainDb);
                        Document document = documents.AddDocument(DocumentType.Reservation, Articles.ToList(), null, reservingSupplier, false);
                        MainDb.SaveChanges();
                        ClearArticlesBtn_Click(null, null);

                        if (result == MessageBoxResult.Yes)
                            new ReserveBon(Data, document).CreateAndPrint(1);
                    }
                }
            }
        }

        private void Articles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e != null && (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove))
            {
                foreach (Article a in e.OldItems)
                {
                    a.Price = Math.Abs(a.Price);
                }
            }

            #region ArticlesDG Spaltenbreite aktualisieren

            ArticlesDG.Columns[1].Width = 0;
            ArticlesDG.UpdateLayout();
            ArticlesDG.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Star);

            #endregion

            #region Summe berechnen

            Sum = 0;
            if (Articles.Count > 0)
            {
                foreach (Article a in Articles)
                {
                    Sum += a.Price;
                }
            }
            SumTB.Text = "Anzahl " + Articles.Count + " - Summe " + Sum.ToString("C");

            #endregion
        }

        private void ArticleIdTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) AddArticle();
        }

        private void ArticlesDG_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            //Auf gültige Eingabe überprüfen
            string text = (e.EditingElement as TextBox).Text;

            Article article = ArticlesDG.SelectedItem as Article;

            if (decimal.TryParse(text, NumberStyles.Currency, CultureInfo.CreateSpecificCulture("de-DE"), out decimal newPrice) && newPrice >= 0)
            {
                if (newPrice != article.Price)
                {
                    if (IsSupplierProportionFixed)
                    {
                        if (newPrice < article.SupplierProportion)
                        {
                            e.Cancel = true;
                            SellBtn.IsEnabled = false;
                            ReserveBtn.IsEnabled = false;
                            ClearArticlesBtn.IsEnabled = false;
                            return;
                        }
                    }
                    else
                    {
                        if (article.Supplier.SupplierProportion.HasValue)
                            article.SupplierProportion = newPrice * article.Supplier.SupplierProportion.Value / 100;
                        else
                            article.SupplierProportion = newPrice * MainDb.GraduationSupplierProportion.Where(g => g.FromPrice <= newPrice).OrderByDescending(g => g.FromPrice).First().SupplierProportion / 100;
                    }

                    if (newPrice != article.Price && !ArticlesPriceChanged.Contains(article))
                        ArticlesPriceChanged.Add(article);
                }
            }
            else
            {
                e.Cancel = true;
                SellBtn.IsEnabled = false;
                ReserveBtn.IsEnabled = false;
                ClearArticlesBtn.IsEnabled = false;
                return;
            }

            SellBtn.IsEnabled = true;
            ReserveBtn.IsEnabled = true;
            ClearArticlesBtn.IsEnabled = true;
        }

        private void ArticlesDG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(ArticlesDG.SelectedItem is Article article) || article.Defects == null || article.Defects.Count == 0)
            {
                DefectsDG.ItemsSource = EmptyDefectsList;
            }
            else
            {
                List<Defect> defectsList = new List<Defect>();

                foreach (Defect defect in article.Defects)
                {
                    defectsList.Add(defect);
                }

                DefectsDG.ItemsSource = defectsList;
            }
        }

        private void ArticlesDG_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (ArticlesDG.SelectedItem is Article article)
            {
                if (article.Price < 0)
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    decimal supplierProportion;
                    if (article.Supplier.SupplierProportion.HasValue)
                        supplierProportion = article.Supplier.SupplierProportion.Value / 100 * article.Price;
                    else
                    {
                        GraduationSupplierProportion supplierGraduationProportion = MainDb.GraduationSupplierProportion.Where(sGP => article.Price >= sGP.FromPrice).OrderByDescending(sGP => sGP.FromPrice).First();
                        supplierProportion = article.Price * supplierGraduationProportion.SupplierProportion / 100;
                    }

                    if (supplierProportion != article.SupplierProportion)
                        IsSupplierProportionFixed = true;
                    else
                        IsSupplierProportionFixed = false;
                }
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
            PopupWindow pW = new PopupWindow(OwnerWindow, Data);
            ReservedArticlesPage rAP = new ReservedArticlesPage(pW, MainDb);
            pW.Content = rAP;
            pW.Initialize();
            pW.Closing += rAP.OwnerWindow_Closing;
            pW.ShowDialog();
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