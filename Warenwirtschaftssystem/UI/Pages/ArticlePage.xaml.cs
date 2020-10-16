using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Warenwirtschaftssystem.Model;
using Warenwirtschaftssystem.Model.Db;
using Warenwirtschaftssystem.Model.Documents;
using Warenwirtschaftssystem.UI.Windows;

namespace Warenwirtschaftssystem.UI.Pages
{
    public partial class ArticlePage : Page
    {
        // Attribute
        private DataModel Data;
        private ToolWindow OwnerWindow;
        private DbModel MainDb;
        private CollectionViewSource ArticlesCVS;
        public Article SelectedArticle;
        public bool ArticlesAdded = false;

        private Documents Documents;
        private CancellationTokenSource CancelLoadingToken;
        private IInputElement FocusedElementBeforeLoading = null;

        #region Initialisierung

        // Konstruktor
        public ArticlePage(DataModel data, ToolWindow ownerWindow)
        {
            Data = data;
            OwnerWindow = ownerWindow;
            MainDb = new DbModel(data.MainConnectionString);

            InitializeComponent();

            #region Daten aus Db laden

            ArticlesCVS = (CollectionViewSource)FindResource("ArticlesCVS");

            #endregion

            OwnerWindow.PreviewKeyUp += OwnerWindow_PreviewKeyUp;
        }

        public ArticlePage(DataModel data, ToolWindow ownerWindow, Supplier filterSupplier)
        {
            Data = data;
            OwnerWindow = ownerWindow;
            MainDb = new DbModel(data.MainConnectionString);

            InitializeComponent();

            #region Daten aus Db laden

            ArticlesCVS = (CollectionViewSource)FindResource("ArticlesCVS");

            FilterSupplierTb.Text = filterSupplier.Id.ToString();

            if (filterSupplier != null)
            {
                CancelLoadingToken = new CancellationTokenSource();
                _ = ApplyFilterAsync(CancelLoadingToken.Token);
            }

            #endregion

            OwnerWindow.PreviewKeyUp += OwnerWindow_PreviewKeyUp;
        }

        public ArticlePage(DataModel data, ToolWindow ownerWindow, Supplier filterSupplier, Status status)
        {
            Data = data;
            OwnerWindow = ownerWindow;
            MainDb = new DbModel(data.MainConnectionString);

            InitializeComponent();

            #region Daten aus Db laden

            ArticlesCVS = (CollectionViewSource)FindResource("ArticlesCVS");

            FilterSupplierTb.Text = filterSupplier.Id.ToString();
            FilterStatusCB.SelectedValue = status;

            if (filterSupplier != null)
            {
                CancelLoadingToken = new CancellationTokenSource();
                _ = ApplyFilterAsync(CancelLoadingToken.Token);
            }

            #endregion

            OwnerWindow.PreviewKeyUp += OwnerWindow_PreviewKeyUp;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (SelectedArticle != null)
            {
                ArticlesDG_SelectionChanged(null, null);

                //Rows von ArticlesDG anpassen
                ArticlesDG.Columns[1].Width = 0;
                ArticlesDG.UpdateLayout();
                ArticlesDG.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            }
            else if (ArticlesAdded)
            {
                FilterStatusCB.SelectedItem = Status.Sortiment;

                CancelLoadingToken = new CancellationTokenSource();
                _ = ApplyFilterAsync(CancelLoadingToken.Token);
                ArticlesAdded = false;
            }

            FilterSupplierTb.Focus();
        }

        #endregion

        #region Filter

        private async Task ApplyFilterAsync(CancellationToken cancellationToken)
        {
            ShowArticlesLoadingElement();

            #region Artikel laden

            bool artIdSet = int.TryParse(FilterIdTb.Text, out int artId);
            if (artIdSet)
                artId -= 100000;

            Status? status = FilterStatusCB.SelectedItem as Status?;
            bool statusSet = status.HasValue;
            bool suppIdSet = int.TryParse(FilterSupplierTb.Text, out int suppId);

            string gender = FilterGenderTb.Text.ToLower();
            bool genderSet = !string.IsNullOrWhiteSpace(gender);

            string category = FilterCategoryTb.Text.ToLower();
            bool categorySet = !string.IsNullOrWhiteSpace(category);

            string type = FilterTypeTb.Text.ToLower();
            bool typeSet = !string.IsNullOrWhiteSpace(type);

            string brand = FilterBrandTb.Text.ToLower();
            bool brandSet = !string.IsNullOrWhiteSpace(brand);

            string size = FilterSizeTb.Text.ToLower();
            bool sizeSet = !string.IsNullOrWhiteSpace(size);

            string material = FilterMaterialTb.Text.ToLower();
            bool materialSet = !string.IsNullOrWhiteSpace(material);

            List<Article> articles = null;

            try
            {
                articles = await MainDb.Articles.Where(a => (!artIdSet || a.Id == artId)
                    && (!statusSet || a.Status == status.Value)
                    && (!suppIdSet || a.Supplier.Id == suppId)
                    && (!genderSet || a.Gender.Description.ToLower().Contains(gender))
                    && (!categorySet || a.Category.Title.ToLower().Contains(category))
                    && (!typeSet || a.Type.Title.ToLower().Contains(type))
                    && (!brandSet || a.Brand.Title.ToLower().Contains(brand))
                    && (!sizeSet || a.Size.Value.ToLower().Contains(size))
                    && (!materialSet || a.Materials.Where(m => m.Title.ToLower().Contains(material)).FirstOrDefault() != null))
                    .OrderByDescending(a => a.Id)
                    .ToListAsync(cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return;

                foreach (Article article in articles)
                {
                    if (article.Status == Status.Sold || article.Status == Status.PayedOut)
                    {
                        Document document = await MainDb.Documents.Where(d => d.Articles.Where(a => a.Id == article.Id).FirstOrDefault() != null).OrderByDescending(d => d.Id).FirstOrDefaultAsync(cancellationToken);
                        if (document != null)
                        {
                            article.Sold = document.DateTime;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return;
            }

            ArticlesCVS.Source = articles;

            #endregion

            HideArticlesLoadingElement();

            //Rows von ArticlesDG anpassen
            ArticlesDG.Columns[1].Width = 0;
            ArticlesDG.UpdateLayout();
            ArticlesDG.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
        }

        private void FilterTb_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CancelLoadingToken = new CancellationTokenSource();
                _ = ApplyFilterAsync(CancelLoadingToken.Token);
            }
        }

        private void ResetFilterBtn_Click(object sender, RoutedEventArgs e)
        {
            FilterIdTb.Clear();
            FilterStatusCB.SelectedValue = null;
            FilterSupplierTb.Clear();
            FilterGenderTb.Clear();
            FilterCategoryTb.Clear();
            FilterTypeTb.Clear();
            FilterBrandTb.Clear();
            FilterSizeTb.Clear();
            FilterMaterialTb.Clear();
        }

        #endregion

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            OwnerWindow.DisableClosingPrompt = true;
            OwnerWindow.RootWindow.RemoveToolWindow(OwnerWindow);

            if (Documents != null)
                Documents.DiscardChanges();

            OwnerWindow.Close();

            if (Documents != null)
                Documents.DiscardChanges();

            MainDb.Dispose();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            OwnerWindow.DisableClosingPrompt = true;
            OwnerWindow.RootWindow.RemoveToolWindow(OwnerWindow);
            OwnerWindow.Close();

            if (Documents != null)
                Documents.PrepareDocumentsToBeSaved();

            MainDb.SaveChanges();
            MainDb.Dispose();
        }

        private void NewArticlesBtn_Click(object sender, RoutedEventArgs e)
        {
            bool isSelectionClosedOutOrReturned = true;

            if (ArticlesDG.SelectedItems.Count == 0)
                isSelectionClosedOutOrReturned = false;
            else
                foreach (object selectedItem in ArticlesDG.SelectedItems)
                {
                    if (!(selectedItem is Article article && (article.Status == Status.ClosedOut || article.Status == Status.Returned)))
                    {
                        isSelectionClosedOutOrReturned = false;
                        break;
                    }
                }

            if (isSelectionClosedOutOrReturned)
            {
                MessageBoxResult result = MessageBox.Show("Ausgewählte Artikel wieder ins Sortiment aufnhemen?", "Artikel wieder aufnehmen?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);

                if (result == MessageBoxResult.Yes)
                {
                    foreach (Article article in ArticlesDG.SelectedItems)
                    {
                        article.AddedToSortiment = DateTime.Now;
                        article.Status = Status.Sortiment;
                        article.EnteredFinalState = null;
                    }
                }
            }
            else
            {
                DbModel mainDb = new DbModel(Data.MainConnectionString);
                Supplier supplier = null;
                if (!(int.TryParse(FilterSupplierTb.Text, out int id) && (supplier = MainDb.Suppliers.Where(s => s.Id == id).SingleOrDefault()) != null))
                {
                    // Fenster zum auswählen eines Lieferanten öffnen
                    PopupWindow popupWindow = new PopupWindow(OwnerWindow, Data)
                    {
                        Owner = OwnerWindow,
                        Title = "Lieferanten auswählen"
                    };

                    PickSupplierPage pickSupplierPage = new PickSupplierPage(popupWindow, Data, mainDb);

                    popupWindow.Content = pickSupplierPage;
                    popupWindow.Initialize();
                    popupWindow.ShowDialog();

                    FilterStatusCB.SelectedItem = Status.Sortiment;

                    if (pickSupplierPage.SelectedSupplier == null)
                        return;
                    supplier = pickSupplierPage.SelectedSupplier;
                }

                //Keine Artikelannahmen auf Supplier mit PickUp = 0 möglich
                if (supplier.PickUp != 0)
                {
                    // NewArticlesPage Toolwindow zuweisen
                    NewArticlesPage newArticlesPage = new NewArticlesPage(Data, mainDb, OwnerWindow, supplier.Id, this);
                    NewArticlePage newArticlePage = new NewArticlePage(Data, OwnerWindow, newArticlesPage.Supplier, mainDb, newArticlesPage, this);
                    OwnerWindow.Content = newArticlePage;
                }
            }
        }

        private void EditArticleBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ArticlesDG.SelectedItems.Count == 1 && (SelectedArticle = ArticlesDG.SelectedItem as Article) != null)
            {
                NewArticlePage newArticlePage;

                newArticlePage = new NewArticlePage(Data, MainDb, OwnerWindow, SelectedArticle, this, SelectedArticle.Status != Status.PayedOut && SelectedArticle.Status != Status.Sold);

                OwnerWindow.Content = newArticlePage;
            }
        }

        private void ArticlesDG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditArticleBtn_Click(null, null);
        }

        private void AttributesEP_Expanded(object sender, RoutedEventArgs e)
        {
            ActionsEP.IsExpanded = false;
            StatusChangeEP.IsExpanded = false;
        }

        private void ActionsEP_Expanded(object sender, RoutedEventArgs e)
        {
            AttributesEP.IsExpanded = false;
        }

        private void StatusChangeEP_Expanded(object sender, RoutedEventArgs e)
        {
            AttributesEP.IsExpanded = false;
        }

        #region Drucken

        private void PrintLabels_Click(object sender, RoutedEventArgs e)
        {
            if (ArticlesDG.SelectedItems.Count > 0)
            {
                List<Article> articles = new List<Article>();

                for (int i = 0; i < ArticlesDG.SelectedItems.Count; i++)
                    if (ArticlesDG.SelectedItems[i] is Article article && (article.Status == Status.Sortiment || article.Status == Status.InStock))
                        articles.Add(article);

                PopupWindow pW = new PopupWindow(OwnerWindow, Data)
                {
                    Title = "Etikettendruck"
                };
                PrintLabelsPage pLP = new PrintLabelsPage(Data, pW, articles);
                pW.Content = pLP;
                pW.Initialize();
                pW.ShowDialog();
            }
        }

        private void PrintSubmissionDocBtn_Click(object sender, RoutedEventArgs e)
        {
            var articles = new List<Article>();
            Supplier supplier = null;

            foreach (var item in ArticlesDG.SelectedItems)
            {
                if (item is Article article)
                {
                    if (supplier == null)
                    {
                        supplier = article.Supplier;
                    }

                    if (article.Supplier != supplier)
                    {
                        MessageBox.Show("Kann keinen Annahmebeleg erstellen, da die Lieferanten der ausgewählten Artikel unterschiedlich sind.", "Kann keinen Annahmebeleg erstellen", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    switch (article.Status)
                    {
                        case Status.Sortiment:
                        case Status.InStock:
                            break;

                        default:
                            MessageBox.Show("Kann keinen Annahmebeleg erstellen, da mindestens einer der ausgewählten Artikel einen ungültigen Status besitzt.", "Kann keinen Annahmebeleg erstellen", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                    }

                    articles.Add(article);
                }
            }

            if (Documents == null)
            {
                Documents = new Documents(Data, MainDb);
            }

            Document document = Documents.AddDocument(DocumentType.Submission, articles, null, supplier, true);
            new SubmissionDoc(Data, document).CreateAndPrintDocument();
        }

        #endregion

        private void PayoutBtn_Click(object sender, RoutedEventArgs e)
        {
            List<Article> articles = new List<Article>();
            Supplier supplier = null;

            foreach (Article article in ArticlesDG.SelectedItems)
            {
                if (article.Status == Status.Sold && (supplier == null || supplier == article.Supplier))
                {
                    if (supplier == null)
                        supplier = article.Supplier;
                    else if (supplier != article.Supplier)
                        return;
                    articles.Add(article);
                }
                else
                    return;
            }

            MessageBoxResult result = MessageBox.Show("Soll ein Auszahlungsbeleg gedruckt werden?", "Auszahlungsbeleg drucken?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);
            if (result == MessageBoxResult.Cancel)
                return;

            foreach (Article article in articles)
            {
                article.Status = Status.PayedOut;
                article.EnteredFinalState = DateTime.Now;
                article.OnPropertyChanged("Status");
            }

            if (result == MessageBoxResult.Yes || result == MessageBoxResult.No)
            {
                if (Documents == null)
                    Documents = new Documents(Data, MainDb);

                if (result == MessageBoxResult.No)
                    Documents.AddDocument(DocumentType.Payout, articles, null, supplier, false);
                else
                {
                    Document document = Documents.AddDocument(DocumentType.Payout, articles, null, supplier, true);
                    new PayoutDoc(Data, document).CreateAndPrintDocument();
                }
            }
        }

        private void CloseOutBtn_Click(object sender, RoutedEventArgs e)
        {
            List<Article> articlesToDelete = new List<Article>();

            foreach (object selectedItem in ArticlesDG.SelectedItems)
                if (selectedItem is Article article)
                    if (article.Status == Status.Sortiment || article.Status == Status.InStock)
                        articlesToDelete.Add(article);
                    else
                    {
                        MessageBox.Show("Artikel können nicht ausgebucht werden", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

            MessageBoxResult result = MessageBox.Show("Wirklich die ausgewählten Artikel ausbuchen?", "Ausbuchen bestätigen", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
            {
                foreach (Article article in articlesToDelete)
                {
                    article.Status = Status.ClosedOut;
                    article.EnteredFinalState = DateTime.Now;
                }
            }
        }

        private void ChangeStatusBtn_Click(object sender, RoutedEventArgs e)
        {
            Status articleStatus;
            if (ArticlesDG.SelectedItem is Article selectedArticle)
                articleStatus = selectedArticle.Status;
            else
                return;

            foreach (Article article in ArticlesDG.SelectedItems)
            {
                if (article.Status != articleStatus)
                {
                    MessageBoxResult mBR = MessageBox.Show("Die Artikel in der Auswahl haben nicht den gleichen Status. Fortfahren?", "Unterschiedliche Status", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

                    if (mBR == MessageBoxResult.No)
                        return;
                    else
                        break;
                }
            }

            foreach (Article article in ArticlesDG.SelectedItems)
            {
                if (article.Status == Status.Sortiment)
                    article.Status = Status.InStock;
                else if (article.Status == Status.InStock)
                    article.Status = Status.Sortiment;
            }
        }

        private void ChangeNotesBtn_Click(object sender, RoutedEventArgs e)
        {
            Status articleStatus = (ArticlesDG.SelectedItem as Article).Status;

            foreach (Article article in ArticlesDG.SelectedItems)
            {
                if (article.Status != articleStatus)
                {
                    MessageBoxResult mBR = MessageBox.Show("Die Artikel in der Auswahl haben nicht den gleichen Status. Fortfahren?", "Unterschiedliche Status", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

                    if (mBR == MessageBoxResult.No)
                        return;
                    else
                        break;
                }
            }

            PopupWindow pW = new PopupWindow(OwnerWindow, Data);
            NewValuePage nVP = new NewValuePage(pW);
            pW.Content = nVP;
            pW.Initialize();
            pW.ShowDialog();

            if (nVP.NewValue != null)
            {
                foreach (Article article in ArticlesDG.SelectedItems)
                {
                    article.Notes = nVP.NewValue;
                }
            }
        }

        private void ArticlesDG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            decimal sum = 0;
            decimal payoutSum = 0;

            foreach (Article article in ArticlesDG.SelectedItems)
            {
                sum += article.Price;
                payoutSum += article.SupplierProportion;
            }

            SumTb.Text = "Summe: " + sum.ToString("C");
            PayoutSumTb.Text = "Auszahlung: " + payoutSum.ToString("C");
        }

        private void ReturnBtn_Click(object sender, RoutedEventArgs e)
        {
            List<Article> articlesToReturn = new List<Article>();

            Supplier supplier = null;
            foreach (Article article in ArticlesDG.SelectedItems)
            {
                if (supplier == null)
                    supplier = article.Supplier;
                else if (article.Supplier != supplier || (article.Status != Status.Sortiment && article.Status != Status.InStock))
                    return;

                articlesToReturn.Add(article);
            }

            MessageBoxResult result = MessageBox.Show("Soll ein Rückgabebeleg gedruckt werden?", "Rückgabebeleg drucken?", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);

            if (result == MessageBoxResult.Yes || result == MessageBoxResult.No)
            {
                foreach (Article article in articlesToReturn)
                {
                    article.Status = Status.Returned;
                    article.EnteredFinalState = DateTime.Now;
                }

                if (Documents == null)
                    Documents = new Documents(Data, MainDb);

                if (result == MessageBoxResult.No)
                    Documents.AddDocument(DocumentType.Return, articlesToReturn, null, supplier, false);
                else
                {
                    Document document = Documents.AddDocument(DocumentType.Return, articlesToReturn, null, supplier, false);
                    MainDb.SaveChanges();
                    new ReturnDoc(Data, document).CreateAndPrintDocument();
                }
            }
        }

        #region ArticlesLoading Element

        private void ShowArticlesLoadingElement()
        {
            FocusedElementBeforeLoading = FocusManager.GetFocusedElement(OwnerWindow);

            ArticlesLoadingElement.Visibility = Visibility.Visible;

            ArticlesDG.IsEnabled = false;

            InteractionOptionsSP.IsEnabled = false;
            SaveBtn.IsEnabled = false;
            CancelBtn.IsEnabled = false;

            NotesTB.IsEnabled = false;

            OwnerWindow.Closable = false;
        }

        private void HideArticlesLoadingElement()
        {
            ArticlesLoadingElement.Visibility = Visibility.Collapsed;

            ArticlesDG.IsEnabled = true;

            InteractionOptionsSP.IsEnabled = true;
            SaveBtn.IsEnabled = true;
            CancelBtn.IsEnabled = true;

            NotesTB.IsEnabled = true;

            OwnerWindow.Closable = true;

            if (FocusedElementBeforeLoading != null)
                FocusedElementBeforeLoading.Focus();
        }

        private void CancelLoadingBtn_Click(object sender, RoutedEventArgs e)
        {
            CancelLoadingToken.Cancel();

            HideArticlesLoadingElement();
        }

        #endregion

        private void OwnerWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && ArticlesLoadingElement.IsVisible)
            {
                CancelLoadingBtn_Click(null, null);
                e.Handled = true;
            }
        }

        private void ChangePickUp_Click(object sender, RoutedEventArgs e)
        {
            if (ArticlesDG.SelectedItems != null && ArticlesDG.SelectedItems.Count != 0)
            {
                foreach (Article article in ArticlesDG.SelectedItems)
                {
                    if (article.Status != Status.Sortiment && article.Status != Status.InStock)
                        return;
                }

                PopupWindow pW = new PopupWindow(OwnerWindow, Data) { Title = "Neues Abholdatum" };
                DatePickerPage dPP = new DatePickerPage(pW,
                    DateTime.Now.Date.AddDays(int.Parse(MainDb.Settings.Where(s => s.Key == "DefaultPickUp").First().Value) * 7)
                    );
                pW.Content = dPP;
                pW.Initialize();

                pW.ShowDialog();

                if (dPP.SelectedDate != null)
                {
                    foreach (Article article in ArticlesDG.SelectedItems)
                    {
                        if (article.PickUp != null && article.PickUp < dPP.SelectedDate)
                            article.PickUp = dPP.SelectedDate;
                    }
                }
            }
        }

        private void ArticlesDG_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void ChangeSupplierBtn_Click(object sender, RoutedEventArgs e)
        {
            Supplier supplier = null;
            bool differentSuppliers = false;
            foreach (Article article in ArticlesDG.SelectedItems)
            {
                if (article.Status != Status.Sortiment && article.Status != Status.InStock && article.Status != Status.Reserved && article.Status != Status.Sold)
                    return;
                if (supplier == null)
                    supplier = article.Supplier;
                else if (supplier != article.Supplier)
                    differentSuppliers = true;
            }

            if (differentSuppliers)
            {
                MessageBoxResult result = MessageBox.Show("Es sind Artikel, die auf verschiedene Lieferanten gebucht sind, ausgewählt. Sollen diese wirklich auf einen anderen Lieferanten gebucht werden?", "Ausgewählte Artikel haben unterschiedliche Lieferanten", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            PopupWindow pW = new PopupWindow(OwnerWindow, Data) { Title = "Neuen Lieferanten für Artikel auswählen" };
            PickSupplierPage pSP = new PickSupplierPage(pW, Data, MainDb);

            pW.Content = pSP;
            pW.Initialize();
            pW.ShowDialog();

            if (pSP.SelectedSupplier == null)
                return;
            else
            {
                foreach (Article article in ArticlesDG.SelectedItems)
                {
                    article.Supplier = pSP.SelectedSupplier;
                }

                ArticlesDG.Items.Refresh();
            }
        }
    }
}
