using Microsoft.Xaml.Behaviors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Warenwirtschaftssystem.Model;
using Warenwirtschaftssystem.Model.Db;
using Warenwirtschaftssystem.UI.Behaviors;
using Warenwirtschaftssystem.UI.Controls;
using Warenwirtschaftssystem.UI.Windows;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.Toolkit;
using Color = Warenwirtschaftssystem.Model.Db.Color;
using MessageBox = System.Windows.MessageBox;
using Type = Warenwirtschaftssystem.Model.Db.Type;

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
        private CollectionViewSource TypesCVS;
        private FilterableDataGrid[] DataGrids;

        //Überprüfung auf Datenänderung für Belege
        private decimal? OldPrice;
        private decimal? OldSupplierProportion;

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
                AsNew = false
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
                AsNew = false
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
                Type = articleToCopy.Type
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

            OldPrice = Article.Price;
            OldSupplierProportion = Article.SupplierProportion;

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
            MainDb.ArticleGender.Load();
            GenderCVS.SortDescriptions.Clear();
            GenderCVS.SortDescriptions.Add(new SortDescription(nameof(Gender.Description), ListSortDirection.Ascending));
            GenderCVS.Source = MainDb.ArticleGender.Local;
            // Brands
            BrandsCVS = (CollectionViewSource)FindResource("BrandsCVS");
            MainDb.ArticleBrands.Load();
            BrandsCVS.SortDescriptions.Clear();
            BrandsCVS.SortDescriptions.Add(new SortDescription(nameof(Brand.Title), ListSortDirection.Ascending));
            BrandsCVS.Source = MainDb.ArticleBrands.Local;
            // Sizes
            SizesCVS = (CollectionViewSource)FindResource("SizesCVS");
            MainDb.ArticleSizes.Load();
            SizesCVS.SortDescriptions.Clear();
            SizesCVS.SortDescriptions.Add(new SortDescription(nameof(Model.Db.Size.Value), ListSortDirection.Ascending));
            SizesCVS.Source = MainDb.ArticleSizes.Local;
            // Materials
            MaterialsCVS = (CollectionViewSource)FindResource("MaterialsCVS");
            MainDb.ArticleMaterials.Load();
            MaterialsCVS.SortDescriptions.Clear();
            MaterialsCVS.SortDescriptions.Add(new SortDescription(nameof(Material.Title), ListSortDirection.Ascending));
            MaterialsCVS.Source = MainDb.ArticleMaterials.Local;
            // Parts
            PartsCVS = (CollectionViewSource)FindResource("PartsCVS");
            MainDb.ArticleParts.Load();
            PartsCVS.SortDescriptions.Clear();
            PartsCVS.SortDescriptions.Add(new SortDescription(nameof(Part.Title), ListSortDirection.Ascending));
            PartsCVS.Source = MainDb.ArticleParts.Local;
            // Defects
            DefectsCVS = (CollectionViewSource)FindResource("DefectsCVS");
            MainDb.ArticleDefects.Load();
            DefectsCVS.SortDescriptions.Clear();
            DefectsCVS.SortDescriptions.Add(new SortDescription(nameof(Defect.Title), ListSortDirection.Ascending));
            DefectsCVS.Source = MainDb.ArticleDefects.Local;
            // Categories
            CategoriesCVS = (CollectionViewSource)FindResource("CategoriesCVS");
            MainDb.ArticleCategories.Load();
            CategoriesCVS.SortDescriptions.Clear();
            CategoriesCVS.SortDescriptions.Add(new SortDescription(nameof(Category.Title), ListSortDirection.Ascending));
            CategoriesCVS.Source = MainDb.ArticleCategories.Local;

            // Colors
            ColorsCVS = (CollectionViewSource)FindResource("ColorsCVS");
            MainDb.ArticleColors.Load();
            ColorsCVS.SortDescriptions.Clear();
            ColorsCVS.SortDescriptions.Add(new SortDescription(nameof(Color.Description), ListSortDirection.Ascending));
            ColorsCVS.Source = MainDb.ArticleColors.Local;

            // Types
            TypesCVS = (CollectionViewSource)FindResource("TypesCVS");
            TypesCVS.SortDescriptions.Clear();
            TypesCVS.SortDescriptions.Add(new SortDescription(nameof(Type.Title), ListSortDirection.Ascending));
            TypesCVS.Source = Article == null || Article.Category == null ? null : Article.Category.Types;

            SelectArticleAttributes();
        }

        private void SelectArticleAttributes()
        {
            // Gender
            GenderDG.DataGrid.SelectedItem = Article.Gender;
            // Categories
            CategoriesDG.DataGrid.SelectedItem = Article.Category;
            // Type
            TypesDG.DataGrid.SelectedItem = Article.Type;
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
                MaterialsDG.DataGrid.SelectedItem = Article.Materials[0];

                for (int i = 1; i < Article.Materials.Count; i++)
                    MaterialsDG.DataGrid.SelectedItems.Add(Article.Materials[i]);
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
                DefectsDG.DataGrid.SelectedItem = Article.Defects[0];

                for (int i = 1; i < Article.Defects.Count; i++)
                    DefectsDG.DataGrid.SelectedItems.Add(Article.Defects[i]);
            }
            // Colors
            if (Article.Colors == null || Article.Colors.Count == 0)
            {
                ColorsDG.DataGrid.SelectedItem = null;
            }
            else
            {
                ColorsDG.DataGrid.SelectedItem = Article.Colors[0];

                for (int i = 1; i < Article.Colors.Count; i++)
                    ColorsDG.DataGrid.SelectedItems.Add(Article.Colors[i]);
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
                            GraduationSupplierProportion supplierGraduationProportion = MainDb.GraduationSupplierProportion.Where(sGP => sGP.FromPrice <= Article.Price).OrderByDescending(sGP => sGP.FromPrice).First();
                            Article.SupplierProportion = Article.Price * supplierGraduationProportion.SupplierProportion / 100;
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

            if (Article.AddedToSortiment == default)
                Article.AddedToSortiment = DateTime.Now;

            if (!IsNewArticle && OldPrice != null && OldSupplierProportion != null
                       && (Article.Price != OldPrice || Article.SupplierProportion != OldSupplierProportion))
                Documents.NotifyArticlePropertiesChanged(MainDb, Data, Article.Id);

            if (IsNewArticle)
            {
                Article.AddedToSortiment = DateTime.Now;
                MainDb.Articles.Add(Article);
            }

            if (Editable)
                MainDb.SaveChanges();

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
                Types = new ObservableCollection<Type>()
            };
            e.NewItem = c;
        }

        private void CategoriesDG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoriesDG.DataGrid.SelectedItem is Category category)
            {
                TypesCVS.Source = category.Types;
                TypesCVS.SortDescriptions.Clear();
                TypesCVS.SortDescriptions.Add(new SortDescription(nameof(Type.Title), ListSortDirection.Ascending));
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
            if (TypesDG.DataGrid.SelectedItem is Model.Db.Type type)
                Article.Type = type;
            else
                Article.Type = null;
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
            if (PartsDG.DataGrid.SelectedItem is Part part && !string.IsNullOrWhiteSpace(part.Title))
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
                    if (gender.Id == 0 && gender.Description == null && gender.Short == null)
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
                    if (category.Id == 0 && category.Title == null && (category.Types == null || category.Types.Count == 0))
                    {
                        categories.Remove(category);
                        continue;
                    }

                    i++;
                }
            }

            i = 0;
            ObservableCollection<Type> types = TypesCVS.Source as ObservableCollection<Type>;
            if (types != null)
            {
                while (i < types.Count)
                {
                    Type type = types[i];
                    if (type.Id == 0 && type.Title == null)
                    {
                        types.Remove(type);
                        continue;
                    }

                    i++;
                }
            }

            i = 0;
            ObservableCollection<Brand> brands = BrandsCVS.Source as ObservableCollection<Brand>;
            if (brands != null)
            {
                while (i < brands.Count)
                {
                    Brand brand = brands[i];
                    if (brand.Id == 0 && brand.Title == null && (brand.Articles == null || brand.Articles.Count == 0))
                    {
                        brands.Remove(brand);
                        continue;
                    }

                    i++;
                }
            }

            i = 0;
            ObservableCollection<Model.Db.Size> sizes = SizesCVS.Source as ObservableCollection<Model.Db.Size>;
            if (sizes != null)
            {
                while (i < sizes.Count)
                {
                    Model.Db.Size size = sizes[i];
                    if (size.Id == 0 && size.Value == null)
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
                    if (color.Id == 0 && color.ColorCode == null && color.Description == null && (color.Articles == null || color.Articles.Count == 0))
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
                    Material material = materials[i];
                    if (material.Id == 0 && material.Title == null && (material.Articles == null || material.Articles.Count == 0))
                    {
                        materials.Remove(material);
                        continue;
                    }

                    i++;
                }
            }

            i = 0;
            var parts = PartsCVS.Source as ObservableCollection<Part>;
            if (parts != null)
            {
                while (i < parts.Count)
                {
                    Part part = parts[i];
                    if (part.Id == 0 && part.Title == null)
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
                    Defect defect = defects[i];
                    if (defect.Id == 0 && defect.Title == null && (defect.Articles == null || defect.Articles.Count == 0))
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

            if (Article.Category == null || Article.Type == null || Article.Brand == null)
                return;

            List<decimal> results;
            if (Article.Gender == null)
                results = MainDb.Articles.Where(a => a.Category != null && a.Category.Id == Article.Category.Id && a.Type != null && a.Type.Id == Article.Type.Id && a.Brand != null && a.Brand.Id == Article.Brand.Id)
                    .Select(a => a.Price)
                    .Distinct()
                    .OrderByDescending(p => p)
                    .Take(20)
                    .ToList();
            else
                results = MainDb.Articles.Where(a => a.Gender != null && a.Gender.Id == Article.Gender.Id && a.Category != null && a.Category.Id == Article.Category.Id && a.Type != null && a.Type.Id == Article.Type.Id && a.Brand != null && a.Brand.Id == Article.Brand.Id)
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