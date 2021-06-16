set xact_abort on
BEGIN TRANSACTION

	--SupplierProportions
	INSERT INTO WWS.dbo.SupplierProportions
	SELECT FromPrice, SupplierProportion
	FROM WWS_old.dbo.GraduationSupplierProportion


	--Settings
	INSERT INTO WWS.dbo.Settings
	SELECT [Key], Value
	FROM WWS_old.dbo.Settings


	--Suppliers
	SET IDENTITY_INSERT WWS.dbo.Articles OFF
	SET IDENTITY_INSERT WWS.dbo.Suppliers ON

	INSERT INTO WWS.dbo.Suppliers (Id, Name, Place, EMail, Phone, Street, Postcode, SupplierProportion, PickUp, CreationDate, Notes, Title, Company)
	SELECT Id, Name, Place, EMail, Phone, Street, Postcode, SupplierProportion, PickUp, CreationDate, Notes, Title, Company
	FROM WWS_old.dbo.Suppliers

	SET IDENTITY_INSERT WWS.dbo.Suppliers OFF


	--Colors
	SET IDENTITY_INSERT WWS.dbo.Colors ON

	INSERT INTO WWS.dbo.Colors (Id, ColorCode, Name)
	SELECT Id, ColorCode, Description
	FROM WWS_old.dbo.Colors

	SET IDENTITY_INSERT WWS.dbo.Colors OFF


	--Genders
	SET IDENTITY_INSERT WWS.dbo.Genders ON

	INSERT INTO WWS.dbo.Genders (Id, Short, Name)
	SELECT Id, Short, Description
	FROM WWS_old.dbo.Gender

	SET IDENTITY_INSERT WWS.dbo.Genders OFF


	--Categories
	SET IDENTITY_INSERT WWS.dbo.Categories ON

	INSERT INTO WWS.dbo.Categories (Id, Name)
	SELECT Id, Title
	FROM WWS_old.dbo.Categories

	SET IDENTITY_INSERT WWS.dbo.Categories OFF


	--SubCategories
	SET IDENTITY_INSERT WWS.dbo.SubCategories ON

	INSERT INTO WWS.dbo.SubCategories (Id, CategoryId, Name)
	SELECT Id, Category_Id, Title
	FROM WWS_old.dbo.Types

	SET IDENTITY_INSERT WWS.dbo.SubCategories OFF


	--Sizes
	SET IDENTITY_INSERT WWS.dbo.Sizes ON

	INSERT INTO WWS.dbo.Sizes (Id, Name)
	SELECT Id, Value
	FROM WWS_old.dbo.Sizes

	SET IDENTITY_INSERT WWS.dbo.Sizes OFF


	--Materials
	SET IDENTITY_INSERT WWS.dbo.Materials ON

	INSERT INTO WWS.dbo.Materials (Id, Name)
	SELECT Id, Title
	FROM WWS_old.dbo.Materials

	SET IDENTITY_INSERT WWS.dbo.Materials OFF


	--Parts
	SET IDENTITY_INSERT WWS.dbo.Parts ON

	INSERT INTO WWS.dbo.Parts (Id, Name)
	SELECT Id, Title
	FROM WWS_old.dbo.Parts

	SET IDENTITY_INSERT WWS.dbo.Parts OFF


	--Brands
	SET IDENTITY_INSERT WWS.dbo.Brands ON

	INSERT INTO WWS.dbo.Brands (Id, Name)
	SELECT Id, Title
	FROM WWS_old.dbo.Brands

	SET IDENTITY_INSERT WWS.dbo.Brands OFF


	--Defects
	SET IDENTITY_INSERT WWS.dbo.Defects ON

	INSERT INTO WWS.dbo.Defects (Id, Name)
	SELECT Id, Title
	FROM WWS_old.dbo.Defects

	SET IDENTITY_INSERT WWS.dbo.Defects OFF


	--Articles
	SET IDENTITY_INSERT WWS.dbo.Articles ON

	INSERT INTO WWS.dbo.Articles (Id, PickUp, Description, Price, SupplierProportion, Status, AddedToSortiment, EnteredFinalState, GenderId, CategoryId, SubCategoryId, SizeId, PartsId, BrandId, AsNew, Notes, SupplierId)
	SELECT Id, PickUp, Description, Price, SupplierProportion, Status, AddedToSortiment, EnteredFinalState, Gender_Id, Category_Id, Type_Id, Size_Id, Parts_Id, Brand_Id, AsNew, Notes, Supplier_Id
	FROM WWS_old.dbo.Articles

	SET IDENTITY_INSERT WWS.dbo.Articles OFF


	--ArticleDefect
	INSERT INTO WWS.dbo.ArticleDefect
	SELECT Article_Id, Defect_Id
	FROM WWS_old.dbo.DefectArticles


	--ArticleMaterial
	INSERT INTO WWS.dbo.ArticleMaterial
	SELECT Article_Id, Material_Id
	FROM WWS_old.dbo.MaterialArticles


	--ArticleReservations
	INSERT INTO WWS.dbo.ArticleReservations
	SELECT ArticleId, Supplier_Id, [From], Until 
	FROM WWS_old.dbo.Reservations


	--ArticleColor
	INSERT INTO WWS.dbo.ArticleColor
	SELECT Article_Id, Color_Id
	FROM WWS_old.dbo.ColorArticles


	--SavedArticleAttributes
	SET IDENTITY_INSERT WWS.dbo.SavedArticleAttributes ON

	INSERT INTO WWS.dbo.SavedArticleAttributes (Id, ArticleId, Price, Payout)
	SELECT Id, ArticleId, Price, Payout
	FROM WWS_old.dbo.SavedArticleAttributes

	SET IDENTITY_INSERT WWS.dbo.SavedArticleAttributes OFF


	--Documents
	SET IDENTITY_INSERT WWS.dbo.Documents ON

	INSERT INTO WWS.dbo.Documents (Id, DateTime, DocumentType, SupplierId)
	SELECT Id, DateTime, DocumentType, Supplier_Id
	FROM WWS_old.dbo.SavedDocuments

	SET IDENTITY_INSERT WWS.dbo.Documents OFF


	--ArticleDocument
	ALTER TABLE WWS.dbo.ArticleDocument NOCHECK CONSTRAINT ALL

	INSERT INTO WWS.dbo.ArticleDocument
	SELECT Article_Id, Document_Id
	FROM WWS_old.dbo.DocumentArticles

	ALTER TABLE WWS.dbo.ArticleDocument CHECK CONSTRAINT ALL


	--DocumentSavedArticleAttributes
	ALTER TABLE WWS.dbo.DocumentSavedArticleAttributes NOCHECK CONSTRAINT ALL

	INSERT INTO WWS.dbo.DocumentSavedArticleAttributes
	SELECT Document_Id, SavedArticleAttributes_Id
	FROM WWS_old.dbo.SavedArticleAttributesDocuments

	ALTER TABLE WWS.dbo.DocumentSavedArticleAttributes CHECK CONSTRAINT ALL


COMMIT