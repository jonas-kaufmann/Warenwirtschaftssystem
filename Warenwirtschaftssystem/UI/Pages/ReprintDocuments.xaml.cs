using System;
using System.Collections.Generic;
using System.Linq;
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

            MainDb = Data.CreateDbConnection();
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
                // replace price and payout values of article by saved values
                foreach (SavedArticleAttributes sAA in document.SavedArticleAttributes)
                {
                    Article article = document.Articles.Where(a => a.Id == sAA.Article.Id).FirstOrDefault();

                    if (article != null)
                    {
                        article.Price = sAA.Price;
                        article.SupplierProportion = sAA.Payout;
                    }
                }

                //fill Sum and SupplierSum properties of document
                if (document.Sum == null && (document.DocumentType == DocumentType.Bill || document.DocumentType == DocumentType.Return || document.DocumentType == DocumentType.Reservation || document.DocumentType == DocumentType.Payout))
                {
                    if (document.Articles != null && document.Articles.Count > 0)
                    {
                        document.Sum = 0;
                        document.SupplierSum = 0;
                        foreach (Article article in document.Articles)
                        {
                            document.Sum += article.Price;
                            document.SupplierSum += article.SupplierProportion;
                        }
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
                            CustomMessageBox cMB = new CustomMessageBox(OwnerWindow, "Format?", "Welches Format soll gedruckt werden?", "Bon", "Kaufbeleg (A4)", "Abbrechen");
                            cMB.ShowDialog();

                            if (cMB.Result == PressedButton.One)
                                new InvoiceBon(Data, document).CreateAndPrint();
                            else if (cMB.Result == PressedButton.Two)
                                new InvoiceDoc(Data, document).CreateAndPrintDocument();
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
