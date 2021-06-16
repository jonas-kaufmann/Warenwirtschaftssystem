using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.Linq;

namespace Warenwirtschaftssystem.Model.Db
{
    public class Documents
    {
        private DataModel Data;
        private DbModel MainDb;
        private List<Document> DocumentsWithId = new List<Document>();
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
                    ContextForGeneratingIds = Data.CreateDbConnection();

                Document pregeneratedId = new Document()
                {
                    DateTime = SqlDateTime.MinValue.Value
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
                    ContextForGeneratingIds = Data.CreateDbConnection();

                Document pregeneratedId = new Document()
                {
                    DateTime = SqlDateTime.MinValue.Value
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
            // create or find an existing save of the values
            var oldValueDb = Data.CreateDbConnection();
            var oldArticle = oldValueDb.Articles.Where(a => a.Id == articleId).Single();
            var article = MainDb.Articles.First(a => a.Id == articleId);

            // no changes
            if (oldArticle.Price == article.Price && oldArticle.SupplierProportion == article.SupplierProportion)
            {
                return;
            }

            var savedArticleAttributes = MainDb.SavedArticleAttributes.Where(c => c.Article.Id == articleId
                && c.Price == oldArticle.Price
                && c.Payout == oldArticle.SupplierProportion).FirstOrDefault();

            if (savedArticleAttributes == null)
                savedArticleAttributes = new SavedArticleAttributes
                {
                    Article = article,
                    Payout = oldArticle.SupplierProportion,
                    Price = oldArticle.Price
                };

            oldValueDb.Dispose();

            // condition: document contains this article && hasn't already saved its attributes
            var affectedDocuments = MainDb.Documents
                .Include(d => d.SavedArticleAttributes)
                .Where(d => d.Articles.FirstOrDefault(a => a.Id == articleId) != null && d.SavedArticleAttributes.FirstOrDefault(s => s.Article.Id == articleId) == null);

            foreach (var document in affectedDocuments)
            {
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
                    var sameElement = MainDb.SavedArticleAttributes.Where(c => c.Article.Id == changedArticleAttribute.Article.Id
                        && c.Price == changedArticleAttribute.Price
                        && c.Payout == changedArticleAttribute.Payout
                    ).FirstOrDefault();

                    if (sameElement == null)
                    {
                        document.SavedArticleAttributes.Add(changedArticleAttribute);
                    }
                    else
                        document.SavedArticleAttributes.Add(sameElement);
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
            if (ContextForGeneratingIds != null)
            {
                // clear empty Documents
                var documentsToRemove = ContextForGeneratingIds.Documents.Where(d => d.DateTime == SqlDateTime.MinValue.Value);
                ContextForGeneratingIds.Documents.RemoveRange(documentsToRemove);
                ContextForGeneratingIds.SaveChanges();
            }
        }
    }
}
