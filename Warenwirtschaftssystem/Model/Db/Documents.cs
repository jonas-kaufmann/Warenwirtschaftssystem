using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;

namespace Warenwirtschaftssystem.Model.Db
{
    public class Documents
    {
        public static void NotifyArticlePropertiesChanged(DbModel MainDb, DataModel Data, int articleId)
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
                if (document.SavedArticleAttributes == null)
                    document.SavedArticleAttributes = new ObservableCollection<SavedArticleAttributes>();
                document.SavedArticleAttributes.Add(savedArticleAttributes);
            }
        }

        public static Document CreateDocument(DbModel dbContext, DocumentType documentType, List<Article> articles, List<SavedArticleAttributes> changedArticleAttributes, Supplier supplier)
        {
            Document document = new Document
            {
                DateTime = DateTime.Now,
                Articles = new ObservableCollection<Article>(articles),
                Supplier = supplier,
                DocumentType = documentType
            };

            if (changedArticleAttributes != null && changedArticleAttributes.Count != 0)
            {
                document.SavedArticleAttributes = new ObservableCollection<SavedArticleAttributes>();
                foreach (var changedArticleAttribute in changedArticleAttributes)
                {
                    var sameElement = dbContext.SalesSavedArticleAttributes.Where(c => c.ArticleId == changedArticleAttribute.ArticleId
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

        public static Document AddDocumentAndSave(DbModel dbContext, DocumentType documentType, List<Article> articles, List<SavedArticleAttributes> changedArticleAttributes, Supplier supplier)
        {
            Document doc = CreateDocument(dbContext, documentType, articles, changedArticleAttributes, supplier);
            dbContext.Documents.Add(doc);
            dbContext.SaveChanges();
            return doc;
        }
    }
}
