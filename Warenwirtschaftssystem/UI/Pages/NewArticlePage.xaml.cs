using Microsoft.EntityFrameworkCore;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Warenwirtschaftssystem.Model;
using Warenwirtschaftssystem.Model.Db;
using Warenwirtschaftssystem.UI.Behaviors;
using Warenwirtschaftssystem.UI.Controls;
using Warenwirtschaftssystem.UI.Windows;
using Color = Warenwirtschaftssystem.Model.Db.Color;

namespace Warenwirtschaftssystem.UI.Pages
{
    public partial class NewArticlePage : Page
    {
        // Attribute
        private DataModel Data;
        private ToolWindow OwnerWindow;
        public Article Article;
        private Article OriginalArticle;
        private Supplier Supplier;
        private DbModel MainDb;
        private ArticlePage ArticlePage;
        private NewArticlesPage NewArticlesPage;
        private CollectionViewSource GenderCVS;
        private CollectionViewSource BrandsCVS;
        private CollectionViewSource SizesCVS;
        private CollectionViewSource MaterialsCVS;
        private CollectionViewSource PartsCVS;
        private CollectionViewSource DefectsCVS;
        private CollectionViewSource CategoriesCVS;
        private CollectionViewSource ColorsCVS;
        private CollectionViewSource SubCategoryCVS;
        private FilterableDataGrid[] DataGrids;

        private bool IsNewArticle = false;
        private bool Editable = true;

        #region Initialisierung

        /// <summary>
        /// Neuen Artikel von NewArticlesPage aus anlegen
        /// </summary>
        public NewArticlePage(DataModel data, ToolWindow ownerWindow, Supplier supplier, DbModel mainDb, NewArticlesPage newArticlesPage)
        {
            Data = data;
            OwnerWindow = ownerWindow;
            Supplier = supplier;
            MainDb = mainDb;
            NewArticlesPage = newArticlesPage;
            IsNewArticle = true;

            // Artikel mit Standardwerten erzeugen
            Article = new Article
            {
                Supplier = Supplier,
                Status = Status.Sortiment,
                AsNew = false,
                AddedToSortiment = DateTime.Now
            };

            OwnerWindow.Title = "Neuer Artikel L-Nr " + Article.Supplier.Id + " " + Article.Supplier.Name;

            if (supplier.PickUp > -1)
                Article.PickUp = DateTime.Now.Date.AddDays(Supplier.PickUp * 7);

            InitializeEverything();
        }

        /// <summary>
        /// Neuen Artikel von NewArticlesPage aus anlegen, bei Nichterfolg Rückkehr zu ArticlePage
        /// </summary>
        public NewArticlePage(DataModel data, ToolWindow ownerWindow, Supplier supplier, DbModel mainDb, NewArticlesPage newArticlesPage, ArticlePage articlePage)
        {
            Data = data;
            OwnerWindow = ownerWindow;
            Supplier = supplier;
            MainDb = mainDb;
            NewArticlesPage = newArticlesPage;
            ArticlePage = articlePage;
            IsNewArticle = true;

            #region Artikel + Standardwerte erzeugen

            Article = new Article
            {
                Supplier = Supplier,
                Status = Status.Sortiment,
                AsNew = false,
                AddedToSortiment = DateTime.Now
            };

            #endregion

            OwnerWindow.Title = "Neuer Artikel L-Nr " + Article.Supplier.Id + " " + Article.Supplier.Name;

            if (supplier.PickUp > -1)
                Article.PickUp = DateTime.Now.Date.AddDays(Supplier.PickUp * 7);

            InitializeEverything();
        }

        /// <summary>
        /// Neuen Artikel von NewArticlesPage aus anlegen, der die gleichen Eigenschaften wie 'articleToCopy' haben soll
        /// </summary>
        public NewArticlePage(DataModel data, ToolWindow ownerWindow, Supplier supplier, DbModel mainDb, NewArticlesPage newArticlesPage, Article articleToCopy)
        {
            Data = data;
            OwnerWindow = ownerWindow;
            Supplier = supplier;
            MainDb = mainDb;
            NewArticlesPage = newArticlesPage;
            IsNewArticle = true;

            // Artikel mit Standardwerten erzeugen
            Article = new Article
            {
                Supplier = Supplier,
                Status = Status.Sortiment,
                AsNew = false,
                Brand = articleToCopy.Brand,
                Category = articleToCopy.Category,
                Colors = articleToCopy.Colors,
                Gender = articleToCopy.Gender,
                Materials = articleToCopy.Materials,
                Parts = articleToCopy.Parts,
                PickUp = articleToCopy.PickUp,
                Price = articleToCopy.Price,
                Size = articleToCopy.Size,
                SupplierProportion = articleToCopy.SupplierProportion,
                SubCategory = articleToCopy.SubCategory,
                AddedToSortiment = DateTime.Now
            };

            OwnerWindow.Title = "Neuer Artikel L-Nr " + Article.Supplier.Id + " " + Article.Supplier.Name;

            if (supplier.PickUp > -1)
                Article.PickUp = DateTime.Now.Date.AddDays(Supplier.PickUp * 7);

            InitializeEverything();
        }

        /// <summary>
        /// Bestehenden Artikel von NewArticlesPage aus bearbeiten
        /// </summary>
        public NewArticlePage(DataModel data, ToolWindow ownerWindow, Article articleToEdit, NewArticlesPage newArticlesPage, DbModel mainDb)
        {
            Data = data;
            OwnerWindow = ownerWindow;
            Article = articleToEdit;
            OriginalArticle = Article.clone();
            Supplier = Article.Supplier;
            NewArticlesPage = newArticlesPage;
            MainDb = mainDb;

            OwnerWindow.Title = "Artikel " + Article.Description + " bearbeiten";

            OwnerWindow.Title = "Artikel bearbeiten L-Nr " + Article.Supplier.Id + " " + Article.Supplier.Name;

            InitializeEverything();
        }

        /// <summary>
        /// Bestehenden Artikel von ArticlePage aus bearbeiten
        /// </summary>
        public NewArticlePage(DataModel data, DbModel mainDb, ToolWindow ownerWindow, Article articleToEdit, ArticlePage articlePage, bool editable)
        {
            Data = data;
            MainDb = mainDb;
            OwnerWindow = ownerWindow;
            Article = articleToEdit;
            OriginalArticle = Article.clone();
            Editable = editable;

            OwnerWindow.Title = "A-Nr " + Article.ConvertedId + " L-Nr " + Article.Supplier.Id + " " + Article.Supplier.Name;

            Supplier = Article.Supplier;
            Article = articleToEdit;
            ArticlePage = articlePage;

            InitializeEverything();
        }

        private void InitializeEverything()
        {
            InitializeComponent();

            ThirdColumnGrid.DataContext = Article;

            DataGrids = new FilterableDataGrid[]
            {
                GenderDG,
                CategoriesDG,
                TypesDG,
                BrandsDG,
                SizesDG,
                ColorsDG,
                MaterialsDG,
                PartsDG,
                DefectsDG
            };

            if (!Editable)
            {
                NotesTB.IsEnabled = false;
                AsNewCB.IsEnabled = false;
                PickUpDP.IsEnabled = false;
                PriceTB.IsEnabled = false;
                SupplierProportionTB.IsEnabled = false;

                foreach (var dg in DataGrids)
                {
                    dg.IsEnabled = false;
                }


                SaveBtn.IsEnabled = false;
            }

            LoadDataFromDbIntoView();

            Article.PropertyChanged += Article_PropertyChanged;
        }

        private void LoadDataFromDbIntoView()
        {
            // Gender
            GenderCVS = (CollectionViewSource)FindResource("GenderCVS");
            MainDb.Genders.Load();
            GenderCVS.SortDescriptions.Clear();
            GenderCVS.SortDescriptions.Add(new SortDescription(nameof(Gender.Name), ListSortDirection.Ascending));
            GenderCVS.Source = MainDb.Genders.Local.ToObservableCollection();
            // Brands
            BrandsCVS = (CollectionViewSource)FindResource("BrandsCVS");
            MainDb.Brands.Load();
            BrandsCVS.SortDescriptions.Clear();
            BrandsCVS.SortDescriptions.Add(new SortDescription(nameof(ArticleAttributeBase.Name), ListSortDirection.Ascending));
            BrandsCVS.Source = MainDb.Brands.Local.ToObservableCollection();
            // Sizes
            SizesCVS = (CollectionViewSource)FindResource("SizesCVS");
            MainDb.Sizes.Load();
            SizesCVS.SortDescriptions.Clear();
            SizesCVS.SortDescriptions.Add(new SortDescription(nameof(ArticleAttributeBase.Name), ListSortDirection.Ascending));
            SizesCVS.Source = MainDb.Sizes.Local.ToObservableCollection();
            // Materials
            MaterialsCVS = (CollectionViewSource)FindResource("MaterialsCVS");
            MainDb.Materials.Load();
            MaterialsCVS.SortDescriptions.Clear();
            MaterialsCVS.SortDescriptions.Add(new SortDescription(nameof(ArticleAttributeBase.Name), ListSortDirection.Ascending));
            MaterialsCVS.Source = MainDb.Materials.Local.ToObservableCollection();
            // Parts
            PartsCVS = (CollectionViewSource)FindResource("PartsCVS");
            MainDb.Parts.Load();
            PartsCVS.SortDescriptions.Clear();
            PartsCVS.SortDescriptions.Add(new SortDescription(nameof(ArticleAttributeBase.Name), ListSortDirection.Ascending));
            PartsCVS.Source = MainDb.Parts.Local.ToObservableCollection();
            // Defects
            DefectsCVS = (CollectionViewSource)FindResource("DefectsCVS");
            MainDb.Defects.Load();
            DefectsCVS.SortDescriptions.Clear();
            DefectsCVS.SortDescriptions.Add(new SortDescription(nameof(ArticleAttributeBase.Name), ListSortDirection.Ascending));
            DefectsCVS.Source = MainDb.Defects.Local.ToObservableCollection();
            // Categories
            CategoriesCVS = (CollectionViewSource)FindResource("CategoriesCVS");
            MainDb.Categories.Include(c => c.SubCategories).Load();
            CategoriesCVS.SortDescriptions.Clear();
            CategoriesCVS.SortDescriptions.Add(new SortDescription(nameof(ArticleAttributeBase.Name), ListSortDirection.Ascending));
            CategoriesCVS.Source = MainDb.Categories.Local.ToObservableCollection();

            // Colors
            ColorsCVS = (CollectionViewSource)FindResource("ColorsCVS");
            MainDb.Colors.Load();
            ColorsCVS.SortDescriptions.Clear();
            ColorsCVS.SortDescriptions.Add(new SortDescription(nameof(ArticleAttributeBase.Name), ListSortDirection.Ascending));
            ColorsCVS.Source = MainDb.Colors.Local.ToObservableCollection();

            // Types
            SubCategoryCVS = (CollectionViewSource)FindResource("TypesCVS");
            SubCategoryCVS.SortDescriptions.Clear();
            SubCategoryCVS.SortDescriptions.Add(new SortDescription(nameof(Type.Name), ListSortDirection.Ascending));
            SubCategoryCVS.Source = Article == null || Article.Category == null ? null : Article.Category.SubCategories;

            SelectArticleAttributes();
        }

        private void SelectArticleAttributes()
        {
            // Gender
            GenderDG.DataGrid.SelectedItem = Article.Gender;
            // Categories
            CategoriesDG.DataGrid.SelectedItem = Article.Category;
            // Type
            TypesDG.DataGrid.SelectedItem = Article.SubCategory;
            // Brand
            BrandsDG.DataGrid.SelectedItem = Article.Brand;
            // Size
            SizesDG.DataGrid.SelectedItem = Article.Size;
            // Materials
            if (Article.Materials == null || Article.Materials.Count == 0)
            {
                MaterialsDG.DataGrid.SelectedItem = null;
            }
            else
            {
                bool firstIteration = true;

                foreach (var material in Article.Materials)
                {
                    if (firstIteration)
                    {
                        MaterialsDG.DataGrid.SelectedItem = material;
                        firstIteration = false;
                    }
                    else
                    {
                        MaterialsDG.DataGrid.SelectedItems.Add(material);
                    }
                }
            }
            // Parts
            PartsDG.DataGrid.SelectedItem = Article.Parts;
            // Defects
            if (Article.Defects == null || Article.Defects.Count == 0)
            {
                DefectsDG.DataGrid.SelectedItem = null;
            }
            else
            {
                bool firstIteration = true;

                foreach (var defect in Article.Defects)
                {
                    if (firstIteration)
                    {
                        DefectsDG.DataGrid.SelectedItem = defect;
                        firstIteration = false;
                    }
                    else
                    {
                        DefectsDG.DataGrid.SelectedItems.Add(defect);
                    }
                }
            }
            // Colors
            if (Article.Colors == null || Article.Colors.Count == 0)
            {
                ColorsDG.DataGrid.SelectedItem = null;
            }
            else
            {
                bool firstIteration = true;

                foreach (var color in Article.Colors)
                {
                    if (firstIteration)
                    {
                        ColorsDG.DataGrid.SelectedItem = color;
                        firstIteration = false;
                    }
                    else
                    {
                        ColorsDG.DataGrid.SelectedItems.Add(color);
                    }
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // PriceTB
            ((AutoCompleteBehavior)Interaction.GetBehaviors(PriceTB)[0]).SuggestionsProvider = SuggestionsProvider;
            PriceTB.Focus();

            if (Editable)
                SaveBtn.IsEnabled = false;

            CategoriesDG.DataGrid.AddingNewItem += CategoriesDG_AddingNewItem;
            CategoriesDG.DataGrid.SelectionChanged += CategoriesDG_SelectionChanged;
            SupplierProportionTB.TextChanged += CurrencyTBs_TextChanged;

            foreach (var dg in DataGrids)
            {
                dg.DataGrid.SelectionChanged += DGs_SelectionChanged;

                // Auf SelectedItem scrollen
                if (dg.DataGrid.SelectedItem != null)
                {
                    dg.DataGrid.ScrollIntoView(dg.DataGrid.SelectedItem);
                }
            }

            //Enable SaveBtn if PriceTb.Text & SupplierProportion.Text are valid
            CurrencyTBs_TextChanged(null, null);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Article.PropertyChanged -= Article_PropertyChanged;
        }

        //Tracken von Attributsänderungen, damit Auszahlungsbetrag angepasst werden kann
        private void Article_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Price":
                    if (Article.Price >= 0)
                    {
                        if (Article.Supplier.SupplierProportion.HasValue)
                        {
                            Article.SupplierProportion = Article.Price * Article.Supplier.SupplierProportion.Value / 100;
                        }
                        else
                        {
                            SupplierProportion supplierGraduationProportion = MainDb.SupplierProportions.Where(sGP => sGP.FromPrice <= Article.Price).OrderByDescending(sGP => sGP.FromPrice).First();
                            Article.SupplierProportion = Article.Price * supplierGraduationProportion.Proportion / 100;
                        }

                        if (Editable)
                            SaveBtn.IsEnabled = true;
                    }

                    break;
            }
        }

        #endregion

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            // restore original article
            if (Editable && OriginalArticle != null)
                Article.takePropertiesFrom(OriginalArticle);

            DeleteEmptyItemsInDGs();

            if (NewArticlesPage == null)
            {
                OwnerWindow.Title = "Artikel";
                OwnerWindow.Content = ArticlePage;
            }
            else
            {
                if (ArticlePage == null)
                {
                    OwnerWindow.Title = "Mehrere Artikel anlegen";
                    OwnerWindow.Content = NewArticlesPage;
                }
                else
                {
                    OwnerWindow.Title = "Artikel";
                    OwnerWindow.Content = ArticlePage;
                }
            }
        }

        private ObservableCollection<T> RemoveNamedObject<T>(IList selectedItems) where T : class
        {
            ObservableCollection<T> attributes = new ObservableCollection<T>();
            T attribute;
            foreach (var item in selectedItems)
            {
                if ((attribute = item as T) != null)
                {
                    attributes.Add(attribute);
                }
            }
            return attributes;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            DeleteEmptyItemsInDGs();

            if (!IsNewArticle)
                new Documents(Data, MainDb).NotifyArticlePropertiesChanged(Article.Id);

            if (IsNewArticle)
            {
                Article.AddedToSortiment = DateTime.Now;
                MainDb.Articles.Add(Article);
            }

            if (Editable)
                MainDb.SaveChangesRetryOnUserInput();

            if (NewArticlesPage == null)
            {
                OwnerWindow.Title = "Artikel";
                OwnerWindow.Content = ArticlePage;
            }
            else
            {
                if (IsNewArticle)
                    NewArticlesPage.NewArticle = Article;

                OwnerWindow.Title = "Artikel anlegen";
                OwnerWindow.Content = NewArticlesPage;
            }
        }

        private void CurrencyTBs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(PriceTB.Text, NumberStyles.Currency, CultureInfo.CreateSpecificCulture("de-DE"), out decimal price)
                && decimal.TryParse(SupplierProportionTB.Text, NumberStyles.Currency, CultureInfo.CreateSpecificCulture("de-DE"), out decimal supplierProportion)
                && price > 0
                && supplierProportion >= 0
                && price >= supplierProportion)
            {
                if (Editable)
                    SaveBtn.IsEnabled = true;
            }
            else
            {
                if (Editable)
                    SaveBtn.IsEnabled = false;
            }
        }

        private void CategoriesDG_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            Category c = new Category
            {
                SubCategories = new ObservableCollection<SubCategory>()
            };
            e.NewItem = c;
        }

        private void CategoriesDG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoriesDG.DataGrid.SelectedItem is Category category)
            {
                SubCategoryCVS.Source = category.SubCategories;
                SubCategoryCVS.SortDescriptions.Clear();
                SubCategoryCVS.SortDescriptions.Add(new SortDescription(nameof(Type.Name), ListSortDirection.Ascending));
                TypesDG.IsEnabled = true;
            }
            else
            {
                TypesDG.IsEnabled = false;
            }

            TypesDG.DataGrid.SelectedItem = null;
        }

        private void DGs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Gender
            if (GenderDG.DataGrid.SelectedItem is Gender gender)
                Article.Gender = gender;
            else
                Article.Gender = null;
            // Categories
            if (CategoriesDG.DataGrid.SelectedItem is Category category)
                Article.Category = category;
            else
                Article.Category = null;
            // Type
            if (TypesDG.DataGrid.SelectedItem is SubCategory type)
                Article.SubCategory = type;
            else
                Article.SubCategory = null;
            // Brand
            if (BrandsDG.DataGrid.SelectedItem is Brand brand)
                Article.Brand = brand;
            else
                Article.Brand = null;
            // Size
            if (SizesDG.DataGrid.SelectedItem is Model.Db.Size size)
                Article.Size = size;
            else
                Article.Size = null;
            // Colors
            if (ColorsDG.DataGrid.SelectedItems.Count > 0)
                Article.Colors = RemoveNamedObject<Color>(ColorsDG.DataGrid.SelectedItems);
            else
                Article.Colors = null;
            // Materials
            if (MaterialsDG.DataGrid.SelectedItems.Count > 0)
                Article.Materials = RemoveNamedObject<Material>(MaterialsDG.DataGrid.SelectedItems);
            else
                Article.Materials = null;
            // Parts
            if (PartsDG.DataGrid.SelectedItem is Parts part && !string.IsNullOrWhiteSpace(part.Name))
                Article.Parts = part;
            else
                Article.Parts = null;
            // Defects
            if (DefectsDG.DataGrid.SelectedItems.Count > 0)
                Article.Defects = RemoveNamedObject<Defect>(DefectsDG.DataGrid.SelectedItems);
            else
                Article.Defects = null;

            Article.GenerateDescription();
            Article.OnPropertyChanged(nameof(Article.Description));

            PopulateAutoCompleteEntries();
        }

        private void DeleteEmptyItemsInDGs()
        {
            int i = 0;
            ObservableCollection<Gender> genders = GenderCVS.Source as ObservableCollection<Gender>;
            if (genders != null)
            {
                while (i < genders.Count)
                {
                    Gender gender = genders[i];
                    if (gender.Id == 0 && gender.Name == null && gender.Short == null)
                    {
                        genders.Remove(gender);
                        continue;
                    }

                    i++;
                }
            }

            i = 0;
            ObservableCollection<Category> categories = CategoriesCVS.Source as ObservableCollection<Category>;
            if (categories != null)
            {
                while (i < categories.Count)
                {
                    Category category = categories[i];
                    if (category.Id == 0 && category.Name == null && (category.SubCategories == null || category.SubCategories.Count == 0))
                    {
                        categories.Remove(category);
                        continue;
                    }

                    i++;
                }
            }

            i = 0;
            var types = SubCategoryCVS.Source as ObservableCollection<SubCategory>;
            if (types != null)
            {
                while (i < types.Count)
                {
                    var type = types[i];
                    if (type.Id == 0 && type.Name == null)
                    {
                        types.Remove(type);
                        continue;
                    }

                    i++;
                }
            }

            i = 0;
            var brands = BrandsCVS.Source as ObservableCollection<Brand>;
            if (brands != null)
            {
                while (i < brands.Count)
                {
                    var brand = brands[i];
                    if (brand.Id == 0 && brand.Name == null && (brand.Articles == null || brand.Articles.Count == 0))
                    {
                        brands.Remove(brand);
                        continue;
                    }

                    i++;
                }
            }

            i = 0;
            var sizes = SizesCVS.Source as ObservableCollection<Model.Db.Size>;
            if (sizes != null)
            {
                while (i < sizes.Count)
                {
                    var size = sizes[i];
                    if (size.Id == 0 && size.Name == null)
                    {
                        sizes.Remove(size);
                        continue;
                    }

                    i++;
                }
            }

            i = 0;
            var colors = ColorsCVS.Source as ObservableCollection<Color>;
            if (colors != null)
            {
                while (i < colors.Count)
                {

                    Color color = colors[i];
                    if (color.Id == 0 && color.ColorCode == null && color.Name == null && (color.Articles == null || color.Articles.Count == 0))
                    {
                        colors.Remove(color);
                        continue;
                    }

                    i++;
                }
            }

            i = 0;
            var materials = MaterialsCVS.Source as ObservableCollection<Material>;
            if (materials != null)
            {
                while (i < materials.Count)
                {
                    var material = materials[i];
                    if (material.Id == 0 && material.Name == null && (material.Articles == null || material.Articles.Count == 0))
                    {
                        materials.Remove(material);
                        continue;
                    }

                    i++;
                }
            }

            i = 0;
            var parts = PartsCVS.Source as ObservableCollection<Parts>;
            if (parts != null)
            {
                while (i < parts.Count)
                {
                    var part = parts[i];
                    if (part.Id == 0 && part.Name == null)
                    {
                        parts.Remove(part);
                        continue;
                    }

                    i++;
                }
            }

            i = 0;
            var defects = DefectsCVS.Source as ObservableCollection<Defect>;
            if (defects != null)
            {
                while (i < defects.Count)
                {
                    var defect = defects[i];
                    if (defect.Id == 0 && defect.Name == null && (defect.Articles == null || defect.Articles.Count == 0))
                    {
                        defects.Remove(defect);
                        continue;
                    }

                    i++;
                }

            }
        }

        #region PriceTB auto completion
        private PricesProvider SuggestionsProvider = new PricesProvider();

        private void PopulateAutoCompleteEntries()
        {
            SuggestionsProvider.Suggestions.Clear();

            if (Article.Category == null || Article.SubCategory == null || Article.Brand == null)
                return;

            List<decimal> results;
            if (Article.Gender == null)
                results = MainDb.Articles.Where(a => a.Category != null && a.Category.Id == Article.Category.Id && a.SubCategory != null && a.SubCategory.Id == Article.SubCategory.Id && a.Brand != null && a.Brand.Id == Article.Brand.Id)
                    .Select(a => a.Price)
                    .Distinct()
                    .OrderByDescending(p => p)
                    .Take(20)
                    .ToList();
            else
                results = MainDb.Articles.Where(a => a.Gender != null && a.Gender.Id == Article.Gender.Id && a.Category != null && a.Category.Id == Article.Category.Id && a.SubCategory != null && a.SubCategory.Id == Article.SubCategory.Id && a.Brand != null && a.Brand.Id == Article.Brand.Id)
                    .Select(a => a.Price)
                    .Distinct()
                    .OrderByDescending(p => p)
                    .Take(20)
                    .ToList();

            foreach (var result in results)
            {
                SuggestionsProvider.Suggestions.Add(result);
            }
        }
        #endregion
    }
}