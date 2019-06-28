using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using Warenwirtschaftssystem.Model;
using Warenwirtschaftssystem.Model.Db;
using Warenwirtschaftssystem.Model.Documents;

namespace Warenwirtschaftssystem.UI.Pages
{
    public partial class ReprintDocuments : Page
    {
        private Window OwnerWindow;
        private DataModel Data;
        private DbModel MainDb;
        private CollectionViewSource DocumentsCVS;

        public ReprintDocuments(Window ownerWindow, DataModel data)
        {
            InitializeComponent();

            OwnerWindow = ownerWindow;
            Data = data;

            MainDb = new DbModel(Data.MainConnectionString);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DocumentsCVS = FindResource("DocumentsCVS") as CollectionViewSource;
            ResetFilterBtn_Click(null, null);
        }

        #region Filter

        private void ApplyFilter()
        {
            bool idSet = int.TryParse(FilterIdTb.Text, out int id);
            bool supplierIdSet = int.TryParse(SupplierIdTb.Text, out int supplierId);

            bool documentTypeSet = FilterDocumentTypeCb.SelectedIndex != -1;
            DocumentType documentType = DocumentType.Bill;
            if (documentTypeSet)
                documentType = (FilterDocumentTypeCb.SelectedItem as DocumentType?).Value;

            bool dateFromSet = FilterFromDTP.Value.HasValue;
            DateTime dateFrom = DateTime.Now;
            if (dateFromSet)
                dateFrom = FilterFromDTP.Value.Value;

            bool dateUntilSet = FilterUntilDTP.Value.HasValue;
            DateTime dateUntil = DateTime.Now;
            if (dateUntilSet)
                dateUntil = FilterUntilDTP.Value.Value;

            List<Document> documents;

            if (idSet)
            {

                documents = new List<Document>();
                Document document = MainDb.Documents.Where(d => d.Id == id).FirstOrDefault();

                if (document != null)
                    documents.Add(document);
            }
            else
            {
                documents = MainDb.Documents.Where(d => (!documentTypeSet || d.DocumentType == documentType)
                    && (!supplierIdSet || d.Supplier.Id == supplierId)
                    && (!dateFromSet || d.DateTime >= dateFrom)
                    && (!dateUntilSet || d.DateTime <= dateUntil)).ToList();
            }

            foreach (Document document in documents)
            {
                foreach (SavedArticleAttributes sAA in document.SavedArticleAttributes)
                {
                    Article article = document.Articles.Where(a => a.Id == sAA.ArticleId).FirstOrDefault();

                    if (article != null)
                    {
                        Article articleToAdd = new Article
                        {
                            Id = article.Id,
                            Defects = article.Defects,
                            PickUp = article.PickUp,
                            Price = sAA.Price,
                            SupplierProportion = sAA.Payout
                        };

                        articleToAdd.SetDescriptionExplicitly(article.Description);

                        int index = document.Articles.IndexOf(article);

                        document.Articles.RemoveAt(index);
                        document.Articles.Insert(index, articleToAdd);
                    }
                }

                //Fill Sum attribute of Document
                if (document.Sum == null && (document.DocumentType == DocumentType.Bill || document.DocumentType == DocumentType.Return || document.DocumentType == DocumentType.Reservation || document.DocumentType == DocumentType.Payout))
                {
                    if (document.Articles != null && document.Articles.Count > 0)
                    {
                        document.Sum = 0;
                        foreach (Article article in document.Articles)
                            document.Sum += article.Price;
                    }
                }
            }

            DocumentsCVS.Source = documents;
            DocumentsDG.Items.Refresh();

            DocumentsDG.SelectedItem = null;
        }

        private void FilterSection_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ApplyFilter();
        }

        private void ResetFilterBtn_Click(object sender, RoutedEventArgs e)
        {
            FilterIdTb.Text = "";
            FilterDocumentTypeCb.SelectedIndex = -1;
            FilterFromDTP.Value = DateTime.Now.Date;
            FilterUntilDTP.Value = DateTime.Now.Date.AddSeconds(86399);

            ApplyFilter();
        }

        #endregion

        private void PrintBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DocumentsDG.SelectedItem is Document document)
            {
                switch (document.DocumentType)
                {
                    case DocumentType.Submission:
                        new SubmissionDoc(Data, document).CreateAndPrintDocument();
                        break;
                    case DocumentType.Reservation:
                        new ReserveBon(Data, document).CreateAndPrint(1);
                        break;
                    case DocumentType.Bill:
                        if (document.Supplier == null)
                        {
                            new InvoiceBon(Data, document).CreateAndPrint();
                        }
                        else
                        {
                            MessageBoxManager.Yes = "Bon";
                            MessageBoxManager.No = "Rechnung";
                            MessageBoxManager.Cancel = "Abbrechen";
                            MessageBoxManager.Register();

                            MessageBoxResult result = MessageBox.Show("Soll ein Bon oder eine Rechnung gedruckt werden?", "Drucken", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                            if (result == MessageBoxResult.Yes)
                                new InvoiceBon(Data, document).CreateAndPrint();
                            else if (result == MessageBoxResult.No)
                                new InvoiceDoc(Data, document).CreateAndPrintDocument();

                            MessageBoxManager.Unregister();
                        }
                        break;
                    case DocumentType.Payout:
                        new PayoutDoc(Data, document).CreateAndPrintDocument();
                        break;
                    case DocumentType.Return:
                        new ReturnDoc(Data, document).CreateAndPrintDocument();
                        break;
                }

            }
        }
    }
}
