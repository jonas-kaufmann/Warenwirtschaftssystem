using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Warenwirtschaftssystem.Model;
using Warenwirtschaftssystem.Model.Db;
using Warenwirtschaftssystem.Model.Documents;
using Warenwirtschaftssystem.Model.Printing;
using Warenwirtschaftssystem.UI.Windows;

namespace Warenwirtschaftssystem.UI.Pages
{
    public partial class NewArticlesPage : Page
    {
        // Attribute
        private DataModel Data;
        private ToolWindow OwnerWindow;
        private CollectionViewSource ArticlesCVS;
        public ObservableCollection<Article> Articles = new ObservableCollection<Article>();
        public Article NewArticle = null;
        public Supplier Supplier;
        private ArticlePage ArticlePage;
        public DbModel MainDb;
        private bool DisableClosedEvent = false;

        #region Initialisierung

        // Konstruktor
        public NewArticlesPage(DataModel data, DbModel mainDb, ToolWindow ownerWindow, int supplierId, ArticlePage articlePage)
        {
            Data = data;
            OwnerWindow = ownerWindow;
            ArticlePage = articlePage;
            MainDb = mainDb;
            Supplier = MainDb.Suppliers.Where(s => s.Id == supplierId).First();

            InitializeComponent();

            ArticlesCVS = (CollectionViewSource)FindResource("ArticlesCVS");
            ArticlesCVS.Source = Articles;

            OwnerWindow.Closed += OwnerWindow_Closed;
        }

        private void OwnerWindow_Closed(object sender, System.EventArgs e)
        {
            if (!DisableClosedEvent)
            {
                MainDb.Articles.RemoveRange(Articles);
                MainDb.SaveChangesRetryOnUserInput();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            OwnerWindow.Title += " L-Nr: " + Supplier.Id;
            if (NewArticle != null && !Articles.Contains(NewArticle) )
            {
                Articles.Add(NewArticle);

                ArticlesDG.SelectedItem = NewArticle;
                ArticlesDG.ScrollIntoView(NewArticle);

                NewArticle = null;
            }
        }

        #endregion

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            MainDb.Articles.RemoveRange(Articles);
            MainDb.SaveChangesRetryOnUserInput();

            OwnerWindow.Title = "Artikel";
            OwnerWindow.Content = ArticlePage;

            DisableClosedEvent = true;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
                using (var document = new SubmissionDocument(MainDb))
                {
                    foreach (var article in Articles)
                    {
                        try
                        {
                            document.AddArticle(article);
                        }
                        catch (ArgumentException exception)
                        {
                            MessageBox.Show(exception.Message, "Artikel konnten Dokument nicht hinzugefügt werden", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }

                    MessageBoxResult result = MessageBox.Show("Soll ein Abgabebeleg gedruckt werden?", "Abgabebeleg drucken?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            document.Save();
                        new SubmissionDoc(Data, document.Document).CreateAndPrintDocument();
                            break;
                        case MessageBoxResult.No:
                            document.Save();
                            break;
                        default:
                            return;
                    }
                }

            OwnerWindow.Title = "Artikel";
            ArticlePage.ArticlesAdded = true;
            OwnerWindow.Content = ArticlePage;

            DisableClosedEvent = true;
        }
        private void NewArticleBtn_Click(object sender, RoutedEventArgs e)
        {
            // NewArticlePage Toolwindow zuweisen
            NewArticlePage newArticlePage = new NewArticlePage(Data, OwnerWindow, Supplier, MainDb, this);
            OwnerWindow.Content = newArticlePage;
        }

        private void NewSimilarArticleBtn_Click(object sender, RoutedEventArgs e)
        {
            // NewArticlePage Toolwindow zuweisen
            OwnerWindow.Title = "Artikel anlegen";
            NewArticlePage newArticlePage = new NewArticlePage(Data, OwnerWindow, Supplier, MainDb, this, ArticlesDG.SelectedItem as Article);
            OwnerWindow.Content = newArticlePage;
        }

        private void EditArticleBtn_Click(object sender, RoutedEventArgs e)
        {
            // NewArticlePage Toolwindow zuweisen
            NewArticlePage newArticlePage = new NewArticlePage(Data, OwnerWindow, ArticlesDG.SelectedItem as Article, this, MainDb);
            OwnerWindow.Content = newArticlePage;
        }

        #region Drucken

        private void PrintLabelsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ArticlesDG.Items.Count > 0)
            {
                MainDb.SaveChangesRetryOnUserInput();

                List<Article> articles = Articles.ToList();

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

        #endregion

        private void ArticlesDG_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditArticleBtn_Click(null, null);
        }
    }
}
