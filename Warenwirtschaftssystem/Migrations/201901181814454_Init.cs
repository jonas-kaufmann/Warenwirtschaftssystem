namespace Warenwirtschaftssystem.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Brands",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Articles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PickUp = c.DateTime(),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SupplierProportion = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Status = c.Int(nullable: false),
                        AddedToSortiment = c.DateTime(nullable: false),
                        EnteredFinalState = c.DateTime(),
                        AsNew = c.Boolean(nullable: false),
                        Notes = c.String(),
                        Brand_Id = c.Int(),
                        Category_Id = c.Int(),
                        Supplier_Id = c.Int(),
                        Gender_Id = c.Int(),
                        Parts_Id = c.Int(),
                        Reservation_ArticleId = c.Int(),
                        Size_Id = c.Int(),
                        Type_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Brands", t => t.Brand_Id)
                .ForeignKey("dbo.Categories", t => t.Category_Id)
                .ForeignKey("dbo.Suppliers", t => t.Supplier_Id)
                .ForeignKey("dbo.Gender", t => t.Gender_Id)
                .ForeignKey("dbo.Parts", t => t.Parts_Id)
                .ForeignKey("dbo.Reservations", t => t.Reservation_ArticleId)
                .ForeignKey("dbo.Sizes", t => t.Size_Id)
                .ForeignKey("dbo.Types", t => t.Type_Id)
                .Index(t => t.Brand_Id)
                .Index(t => t.Category_Id)
                .Index(t => t.Supplier_Id)
                .Index(t => t.Gender_Id)
                .Index(t => t.Parts_Id)
                .Index(t => t.Reservation_ArticleId)
                .Index(t => t.Size_Id)
                .Index(t => t.Type_Id);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Types",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Category_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.Category_Id)
                .Index(t => t.Category_Id);
            
            CreateTable(
                "dbo.Colors",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        ColorCode = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Defects",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SavedDocuments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DateTime = c.DateTime(nullable: false),
                        DocumentType = c.Int(nullable: false),
                        Supplier_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Suppliers", t => t.Supplier_Id)
                .Index(t => t.Supplier_Id);
            
            CreateTable(
                "dbo.SavedArticleAttributes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ArticleId = c.Int(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Payout = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Suppliers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Place = c.String(),
                        EMail = c.String(),
                        Phone = c.String(),
                        Street = c.String(),
                        Postcode = c.String(),
                        SupplierProportion = c.Decimal(precision: 5, scale: 2),
                        PickUp = c.Int(nullable: false),
                        CreationDate = c.DateTime(nullable: false),
                        Notes = c.String(),
                        Title = c.Int(nullable: false),
                        Company = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Reservations",
                c => new
                    {
                        ArticleId = c.Int(nullable: false),
                        From = c.DateTime(nullable: false),
                        Until = c.DateTime(nullable: false),
                        Supplier_Id = c.Int(),
                    })
                .PrimaryKey(t => t.ArticleId)
                .ForeignKey("dbo.Suppliers", t => t.Supplier_Id)
                .Index(t => t.Supplier_Id);
            
            CreateTable(
                "dbo.Gender",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        Short = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Materials",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Parts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Sizes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Value = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.GraduationSupplierProportion",
                c => new
                    {
                        FromPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SupplierProportion = c.Decimal(nullable: false, precision: 5, scale: 2),
                    })
                .PrimaryKey(t => t.FromPrice);
            
            CreateTable(
                "dbo.Settings",
                c => new
                    {
                        Key = c.String(nullable: false, maxLength: 128),
                        Value = c.String(),
                    })
                .PrimaryKey(t => t.Key);
            
            CreateTable(
                "dbo.ColorArticles",
                c => new
                    {
                        Color_Id = c.Int(nullable: false),
                        Article_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Color_Id, t.Article_Id })
                .ForeignKey("dbo.Colors", t => t.Color_Id, cascadeDelete: true)
                .ForeignKey("dbo.Articles", t => t.Article_Id, cascadeDelete: true)
                .Index(t => t.Color_Id)
                .Index(t => t.Article_Id);
            
            CreateTable(
                "dbo.DefectArticles",
                c => new
                    {
                        Defect_Id = c.Int(nullable: false),
                        Article_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Defect_Id, t.Article_Id })
                .ForeignKey("dbo.Defects", t => t.Defect_Id, cascadeDelete: true)
                .ForeignKey("dbo.Articles", t => t.Article_Id, cascadeDelete: true)
                .Index(t => t.Defect_Id)
                .Index(t => t.Article_Id);
            
            CreateTable(
                "dbo.DocumentArticles",
                c => new
                    {
                        Document_Id = c.Int(nullable: false),
                        Article_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Document_Id, t.Article_Id })
                .ForeignKey("dbo.SavedDocuments", t => t.Document_Id, cascadeDelete: true)
                .ForeignKey("dbo.Articles", t => t.Article_Id, cascadeDelete: true)
                .Index(t => t.Document_Id)
                .Index(t => t.Article_Id);
            
            CreateTable(
                "dbo.SavedArticleAttributesDocuments",
                c => new
                    {
                        SavedArticleAttributes_Id = c.Int(nullable: false),
                        Document_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.SavedArticleAttributes_Id, t.Document_Id })
                .ForeignKey("dbo.SavedArticleAttributes", t => t.SavedArticleAttributes_Id, cascadeDelete: true)
                .ForeignKey("dbo.SavedDocuments", t => t.Document_Id, cascadeDelete: true)
                .Index(t => t.SavedArticleAttributes_Id)
                .Index(t => t.Document_Id);
            
            CreateTable(
                "dbo.MaterialArticles",
                c => new
                    {
                        Material_Id = c.Int(nullable: false),
                        Article_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Material_Id, t.Article_Id })
                .ForeignKey("dbo.Materials", t => t.Material_Id, cascadeDelete: true)
                .ForeignKey("dbo.Articles", t => t.Article_Id, cascadeDelete: true)
                .Index(t => t.Material_Id)
                .Index(t => t.Article_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Articles", "Type_Id", "dbo.Types");
            DropForeignKey("dbo.Articles", "Size_Id", "dbo.Sizes");
            DropForeignKey("dbo.Articles", "Reservation_ArticleId", "dbo.Reservations");
            DropForeignKey("dbo.Articles", "Parts_Id", "dbo.Parts");
            DropForeignKey("dbo.MaterialArticles", "Article_Id", "dbo.Articles");
            DropForeignKey("dbo.MaterialArticles", "Material_Id", "dbo.Materials");
            DropForeignKey("dbo.Articles", "Gender_Id", "dbo.Gender");
            DropForeignKey("dbo.SavedDocuments", "Supplier_Id", "dbo.Suppliers");
            DropForeignKey("dbo.Reservations", "Supplier_Id", "dbo.Suppliers");
            DropForeignKey("dbo.Articles", "Supplier_Id", "dbo.Suppliers");
            DropForeignKey("dbo.SavedArticleAttributesDocuments", "Document_Id", "dbo.SavedDocuments");
            DropForeignKey("dbo.SavedArticleAttributesDocuments", "SavedArticleAttributes_Id", "dbo.SavedArticleAttributes");
            DropForeignKey("dbo.DocumentArticles", "Article_Id", "dbo.Articles");
            DropForeignKey("dbo.DocumentArticles", "Document_Id", "dbo.SavedDocuments");
            DropForeignKey("dbo.DefectArticles", "Article_Id", "dbo.Articles");
            DropForeignKey("dbo.DefectArticles", "Defect_Id", "dbo.Defects");
            DropForeignKey("dbo.ColorArticles", "Article_Id", "dbo.Articles");
            DropForeignKey("dbo.ColorArticles", "Color_Id", "dbo.Colors");
            DropForeignKey("dbo.Articles", "Category_Id", "dbo.Categories");
            DropForeignKey("dbo.Types", "Category_Id", "dbo.Categories");
            DropForeignKey("dbo.Articles", "Brand_Id", "dbo.Brands");
            DropIndex("dbo.MaterialArticles", new[] { "Article_Id" });
            DropIndex("dbo.MaterialArticles", new[] { "Material_Id" });
            DropIndex("dbo.SavedArticleAttributesDocuments", new[] { "Document_Id" });
            DropIndex("dbo.SavedArticleAttributesDocuments", new[] { "SavedArticleAttributes_Id" });
            DropIndex("dbo.DocumentArticles", new[] { "Article_Id" });
            DropIndex("dbo.DocumentArticles", new[] { "Document_Id" });
            DropIndex("dbo.DefectArticles", new[] { "Article_Id" });
            DropIndex("dbo.DefectArticles", new[] { "Defect_Id" });
            DropIndex("dbo.ColorArticles", new[] { "Article_Id" });
            DropIndex("dbo.ColorArticles", new[] { "Color_Id" });
            DropIndex("dbo.Reservations", new[] { "Supplier_Id" });
            DropIndex("dbo.SavedDocuments", new[] { "Supplier_Id" });
            DropIndex("dbo.Types", new[] { "Category_Id" });
            DropIndex("dbo.Articles", new[] { "Type_Id" });
            DropIndex("dbo.Articles", new[] { "Size_Id" });
            DropIndex("dbo.Articles", new[] { "Reservation_ArticleId" });
            DropIndex("dbo.Articles", new[] { "Parts_Id" });
            DropIndex("dbo.Articles", new[] { "Gender_Id" });
            DropIndex("dbo.Articles", new[] { "Supplier_Id" });
            DropIndex("dbo.Articles", new[] { "Category_Id" });
            DropIndex("dbo.Articles", new[] { "Brand_Id" });
            DropTable("dbo.MaterialArticles");
            DropTable("dbo.SavedArticleAttributesDocuments");
            DropTable("dbo.DocumentArticles");
            DropTable("dbo.DefectArticles");
            DropTable("dbo.ColorArticles");
            DropTable("dbo.Settings");
            DropTable("dbo.GraduationSupplierProportion");
            DropTable("dbo.Sizes");
            DropTable("dbo.Parts");
            DropTable("dbo.Materials");
            DropTable("dbo.Gender");
            DropTable("dbo.Reservations");
            DropTable("dbo.Suppliers");
            DropTable("dbo.SavedArticleAttributes");
            DropTable("dbo.SavedDocuments");
            DropTable("dbo.Defects");
            DropTable("dbo.Colors");
            DropTable("dbo.Types");
            DropTable("dbo.Categories");
            DropTable("dbo.Articles");
            DropTable("dbo.Brands");
        }
    }
}
