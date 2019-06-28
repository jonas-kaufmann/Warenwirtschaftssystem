using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;

namespace Warenwirtschaftssystem.Model.Db
{
    public class Documents
    {
        private DataModel Data;
        private DbModel MainDb;
        private List<Document> DocumentsWithId;
        private DbModel ContextForGeneratingIds;

        public Documents(DataModel data, DbModel mainDb)
        {
            Data = data;
            MainDb = mainDb;
        }

        public Document AddDocument(DocumentType documentType, List<Article> articles, List<SavedArticleAttributes> changedArticleAttributes, bool pregenerateId)
        {
            Document document = CreateDocument(documentType, articles, changedArticleAttributes);

            if (pregenerateId)
            {
                if (ContextForGeneratingIds == null)
                    ContextForGeneratingIds = new DbModel(Data.MainConnectionString);
                if (DocumentsWithId == null)
                    DocumentsWithId = new List<Document>();

                Document pregeneratedId = new Document()
                {
                    DateTime = DateTime.Now
                };
                ContextForGeneratingIds.Documents.Add(pregeneratedId);
                ContextForGeneratingIds.SaveChanges();

                document.Id = pregeneratedId.Id;

                DocumentsWithId.Add(document);
            }
            else
            {
                MainDb.Documents.Add(document);
            }

            return document;
        }

        public Document AddDocument(DocumentType documentType, List<Article> articles, List<SavedArticleAttributes> changedArticleAttributes, Supplier supplier, bool pregenerateId)
        {
            Document document = CreateDocument(documentType, articles, changedArticleAttributes);
            document.Supplier = supplier;

            if (pregenerateId)
            {
                if (ContextForGeneratingIds == null)
                    ContextForGeneratingIds = new DbModel(Data.MainConnectionString);
                if (DocumentsWithId == null)
                    DocumentsWithId = new List<Document>();

                Document pregeneratedId = new Document()
                {
                    DateTime = DateTime.Now
                };
                ContextForGeneratingIds.Documents.Add(pregeneratedId);
                ContextForGeneratingIds.SaveChanges();

                document.Id = pregeneratedId.Id;

                DocumentsWithId.Add(document);
            }
            else
            {
                MainDb.Documents.Add(document);
            }

            return document;
        }

        public void NotifyArticlePropertiesChanged(int articleId)
        {
            DbModel oldValueDb = new DbModel(Data.MainConnectionString);
            Article oldArticle = oldValueDb.Articles.Where(a => a.Id == articleId).Single();

            var savedArticleAttributes = MainDb.SalesSavedArticleAttributes.Where(c => c.ArticleId == articleId
                && c.Price == oldArticle.Price
                && c.Payout == oldArticle.SupplierProportion).FirstOrDefault();

            if (savedArticleAttributes == null)
                savedArticleAttributes = new SavedArticleAttributes
                {
                    ArticleId = articleId,
                    Payout = oldArticle.SupplierProportion,
                    Price = oldArticle.Price
                };

            oldValueDb.Dispose();

            var affectedDocuments = MainDb.Documents.Where(d => d.Articles.Where(a => a.Id == articleId).FirstOrDefault() != null
                && d.SavedArticleAttributes.Where(s => s.ArticleId == articleId).FirstOrDefault() == null).Include(d => d.SavedArticleAttributes).ToList();

            foreach (var document in affectedDocuments)
            {
                if (document.SavedArticleAttributes == null) document.SavedArticleAttributes = new ObservableCollection<SavedArticleAttributes>();
                document.SavedArticleAttributes.Add(savedArticleAttributes);
            }
        }

        private Document CreateDocument(DocumentType documentType, List<Article> articles, List<SavedArticleAttributes> changedArticleAttributes)
        {
            Document document = new Document
            {
                DateTime = DateTime.Now,
                Articles = new ObservableCollection<Article>(articles),
                DocumentType = documentType
            };

            if (changedArticleAttributes != null && changedArticleAttributes.Count != 0)
            {
                document.SavedArticleAttributes = new ObservableCollection<SavedArticleAttributes>();
                foreach (var changedArticleAttribute in changedArticleAttributes)
                {
                    var sameElement = MainDb.SalesSavedArticleAttributes.Where(c => c.ArticleId == changedArticleAttribute.ArticleId
                        && c.Price == changedArticleAttribute.Price
                        && c.Payout == changedArticleAttribute.Payout
                    ).FirstOrDefault();

                    if (sameElement == null)
                    {
                        document.SavedArticleAttributes.Add(changedArticleAttribute);
                    }
                    else document.SavedArticleAttributes.Add(sameElement);
                }
            }

            return document;
        }

        public void PrepareDocumentsToBeSaved()
        {
            foreach (Document document in DocumentsWithId)
            {
                MainDb.Entry(MainDb.Documents.Where(d => d.Id == document.Id).First()).Reload();
                Document existingDoc = MainDb.Documents.Where(d => d.Id == document.Id).First();

                if (existingDoc != null)
                {
                    existingDoc.DateTime = document.DateTime;
                    existingDoc.Articles = document.Articles;
                    existingDoc.DocumentType = document.DocumentType;
                    existingDoc.SavedArticleAttributes = document.SavedArticleAttributes;
                    existingDoc.Supplier = document.Supplier;                      
                }
            }
        }

        public void DiscardChanges()
        {
            if (DocumentsWithId != null)
                foreach (Document document in DocumentsWithId)
                {
                    ContextForGeneratingIds.Documents.Remove(ContextForGeneratingIds.Documents.Where(d => d.Id == document.Id).First());
                }
            if (ContextForGeneratingIds != null)
                ContextForGeneratingIds.SaveChanges();
        }
    }
}
