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
using Warenwirtschaftssystem.UI.Windows;
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
        private DataGrid FocusedDG;
        private Key[] FilterLetterKeys = { Key.A, Key.B, Key.C, Key.D, Key.E, Key.F, Key.G, Key.H, Key.I, Key.J, Key.K, Key.L, Key.M, Key.N, Key.O, Key.P, Key.Q, Key.R, Key.S, Key.T, Key.U, Key.V, Key.W, Key.X, Key.Y, Key.Z };
        private readonly Key[] FilterNumPadKeys = { Key.NumPad0, Key.NumPad1, Key.NumPad2, Key.NumPad3, Key.NumPad4, Key.NumPad5, Key.NumPad6, Key.NumPad7, Key.NumPad8, Key.NumPad9 };
        private DataGrid[] DataGrids;
        private bool DGInEditingMode = false;
        private bool DisableEnteringEditingMode = true;

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

            #region Artikel + Standardwerte erzeugen

            Article = new Article
            {
                Supplier = Supplier,
                Status = Status.Sortiment,
                AsNew = false
            };

            Article.PropertyChanged += Article_PropertyChanged;

            #endregion

            OwnerWindow.Title = "Neuer Artikel L-Nr " + Article.Supplier.Id + " " + Article.Supplier.Name;

            InitializeComponent();

            DataGrids = new DataGrid[]
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

            if (supplier.PickUp > -1)
                Article.PickUp = DateTime.Now.Date.AddDays(Supplier.PickUp * 7);

            ThirdColumnGrid.DataContext = Article;
            LoadDataFromDbIntoView();
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

            Article.PropertyChanged += Article_PropertyChanged;

            #endregion

            OwnerWindow.Title = "Neuer Artikel L-Nr " + Article.Supplier.Id + " " + Article.Supplier.Name;
            InitializeComponent();

            DataGrids = new DataGrid[]
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

            if (supplier.PickUp > -1)
                Article.PickUp = DateTime.Now.Date.AddDays(Supplier.PickUp * 7);

            ThirdColumnGrid.DataContext = Article;
            LoadDataFromDbIntoView();
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

            #region Artikel + Standardwerte erzeugen

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

            Article.PropertyChanged += Article_PropertyChanged;

            #endregion

            OwnerWindow.Title = "Neuer Artikel L-Nr " + Article.Supplier.Id + " " + Article.Supplier.Name;
            InitializeComponent();

            DataGrids = new DataGrid[]
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

            if (supplier.PickUp > -1)
                Article.PickUp = DateTime.Now.Date.AddDays(Supplier.PickUp * 7);

            ThirdColumnGrid.DataContext = Article;
            LoadDataFromDbIntoView();
            SelectArticleAttributes();
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
            InitializeComponent();

            DataGrids = new DataGrid[]
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

            ThirdColumnGrid.DataContext = Article;

            LoadDataFromDbIntoView();
            SelectArticleAttributes();

            Article.PropertyChanged += Article_PropertyChanged;
        }

        /// <summary>
        /// Bestehenden Artikel von ArticlePage aus bearbeiten
        /// </summary>
        public NewArticlePage(DataModel data, DbModel mainDb, ToolWindow ownerWindow, Article articleToEdit, ArticlePage articlePage, bool editable)
        {
            Data = data;
            OwnerWindow = ownerWindow;
            Article = articleToEdit;
            OriginalArticle = Article.clone();
            Editable = editable;

            OldPrice = Article.Price;
            OldSupplierProportion = Article.SupplierProportion;

            OwnerWindow.Title = "A-Nr " + Article.ConvertedId + " L-Nr " + Article.Supplier.Id + " " + Article.Supplier.Name;

            InitializeComponent();

            DataGrids = new DataGrid[]
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

            if (!editable)
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

            MainDb = mainDb;

            Supplier = Article.Supplier;
            Article = articleToEdit;
            ArticlePage = articlePage;

            ThirdColumnGrid.DataContext = Article;

            LoadDataFromDbIntoView();
            SelectArticleAttributes();

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
            BrandsDG.SelectedItem = null;
            // Sizes
            SizesCVS = (CollectionViewSource)FindResource("SizesCVS");
            MainDb.ArticleSizes.Load();
            SizesCVS.SortDescriptions.Clear();
            SizesCVS.SortDescriptions.Add(new SortDescription(nameof(Model.Db.Size.Value), ListSortDirection.Ascending));
            SizesCVS.Source = MainDb.ArticleSizes.Local;
            SizesDG.SelectedItem = null;
            // Materials
            MaterialsCVS = (CollectionViewSource)FindResource("MaterialsCVS");
            MainDb.ArticleMaterials.Load();
            MaterialsCVS.SortDescriptions.Clear();
            MaterialsCVS.SortDescriptions.Add(new SortDescription(nameof(Material.Title), ListSortDirection.Ascending));
            MaterialsCVS.Source = MainDb.ArticleMaterials.Local;
            MaterialsDG.SelectedItem = null;
            // Parts
            PartsCVS = (CollectionViewSource)FindResource("PartsCVS");
            MainDb.ArticleParts.Load();
            PartsCVS.SortDescriptions.Clear();
            PartsCVS.SortDescriptions.Add(new SortDescription(nameof(Part.Title), ListSortDirection.Ascending));
            PartsCVS.Source = MainDb.ArticleParts.Local;
            PartsDG.SelectedItem = null;
            // Defects
            DefectsCVS = (CollectionViewSource)FindResource("DefectsCVS");
            MainDb.ArticleDefects.Load();
            DefectsCVS.SortDescriptions.Clear();
            DefectsCVS.SortDescriptions.Add(new SortDescription(nameof(Defect.Title), ListSortDirection.Ascending));
            DefectsCVS.Source = MainDb.ArticleDefects.Local;
            DefectsDG.SelectedItem = null;
            // Categories
            CategoriesCVS = (CollectionViewSource)FindResource("CategoriesCVS");
            MainDb.ArticleCategories.Load();
            CategoriesCVS.SortDescriptions.Clear();
            CategoriesCVS.SortDescriptions.Add(new SortDescription(nameof(Category.Title), ListSortDirection.Ascending));
            CategoriesCVS.Source = MainDb.ArticleCategories.Local;
            CategoriesDG.SelectedItem = null;

            // Colors
            ColorsCVS = (CollectionViewSource)FindResource("ColorsCVS");
            MainDb.ArticleColors.Load();
            ColorsCVS.SortDescriptions.Clear();
            ColorsCVS.SortDescriptions.Add(new SortDescription(nameof(Color.Description), ListSortDirection.Ascending));
            ColorsCVS.Source = MainDb.ArticleColors.Local;
            ColorsDG.SelectedItem = null;
            ColorsDG.Columns[1].SortDirection = ListSortDirection.Ascending;

            // Types
            TypesCVS = (CollectionViewSource)FindResource("TypesCVS");
            TypesCVS.SortDescriptions.Clear();
            TypesCVS.SortDescriptions.Add(new SortDescription(nameof(Type.Title), ListSortDirection.Ascending));
            TypesCVS.Source = Article == null || Article.Category == null ? null : Article.Category.Types;
            TypesDG.IsEnabled = true;
        }

        private void SelectArticleAttributes()
        {
            // Gender
            GenderDG.SelectedItem = Article.Gender;
            // Categories
            CategoriesDG.SelectedItem = Article.Category;
            // Type
            TypesDG.SelectedItem = Article.Type;
            // Brand
            BrandsDG.SelectedItem = Article.Brand;
            // Size
            SizesDG.SelectedItem = Article.Size;
            // Materials
            if (Article.Materials != null)
            {
                foreach (Material m in Article.Materials)
                {
                    MaterialsDG.SelectedItems.Add(m);
                }
            }
            // Parts
            PartsDG.SelectedItem = Article.Parts;
            // Defects
            if (Article.Defects != null)
            {
                foreach (Defect d in Article.Defects)
                {
                    DefectsDG.SelectedItems.Add(d);
                }
            }
            // Colors
            if (Article.Colors != null)
            {
                foreach (Color c in Article.Colors)
                {
                    ColorsDG.SelectedItems.Add(c);
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

            #region UI-Events registrieren

            CategoriesDG.AddingNewItem += CategoriesDG_AddingNewItem;
            CategoriesDG.SelectionChanged += CategoriesDG_SelectionChanged;
            SupplierProportionTB.TextChanged += CurrencyTBs_TextChanged;

            foreach (DataGrid dg in DataGrids)
            {
                dg.PreviewKeyDown += DataGrids_PreviewKeyDown;
                dg.SelectionChanged += DGs_SelectionChanged;
            }

            #endregion

            #region Auf ausgewählte Eigenschaften scrollen

            if (GenderDG.SelectedItem != null)
                GenderDG.ScrollIntoView(GenderDG.SelectedItem);

            if (CategoriesDG.SelectedItem != null)
                CategoriesDG.ScrollIntoView(CategoriesDG.SelectedItem);

            if (TypesDG.SelectedItem != null)
                TypesDG.ScrollIntoView(TypesDG.SelectedItem);

            if (BrandsDG.SelectedItem != null)
                BrandsDG.ScrollIntoView(BrandsDG.SelectedItem);

            if (SizesDG.SelectedItem != null)
                SizesDG.ScrollIntoView(SizesDG.SelectedItem);

            if (ColorsDG.SelectedItem != null)
                ColorsDG.ScrollIntoView(ColorsDG.SelectedItem);

            if (MaterialsDG.SelectedItem != null)
                MaterialsDG.ScrollIntoView(MaterialsDG.SelectedItem);

            if (PartsDG.SelectedItem != null)
                PartsDG.ScrollIntoView(PartsDG.SelectedItem);

            if (DefectsDG.SelectedItem != null)
                DefectsDG.ScrollIntoView(DefectsDG.SelectedItem);

            #endregion

            //Enable SaveBtn if PriceTb.Text & SupplierProportion.Text are valid
            CurrencyTBs_TextChanged(null, null);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Article.PropertyChanged -= Article_PropertyChanged;
        }

        //Tracken von Attributsänderungen, damit Auszahlungsbetrag angepasst werden kann
        private void Article_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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

            if (Article.AddedToSortiment == null)
                Article.AddedToSortiment = DateTime.Now;

            if (!IsNewArticle && OldPrice != null && OldSupplierProportion != null
                       && (Article.Price != OldPrice || Article.SupplierProportion != OldSupplierProportion))
                new Documents(Data, MainDb).NotifyArticlePropertiesChanged(Article.Id);

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
            if (CategoriesDG.SelectedItem is Category category)
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

            TypesDG.SelectedItem = null;
        }

        private void DataGrids_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DGInEditingMode)
                return;

            FocusedDG = sender as DataGrid;

            string key;
            if (FocusedDG != null && !(key = ParseKey(e.Key)).Equals(""))
            {
                FilterCurrentDG(key);
                e.Handled = true;
            }
        }

        private string ParseKey(Key key)
        {
            if (FilterLetterKeys.Contains(key))
                return key.ToString();
            else if (FilterLetterKeys.Contains(key))
            {
                return "" + key.ToString()[1];
            }
            else if (FilterNumPadKeys.Contains(key))
            {
                return "" + key.ToString().Last();
            }
            else if (key == Key.Oem1)
                return "ü";
            else if (key == Key.Oem7)
                return "ä";
            else if (key == Key.Oem3)
                return "ö";
            else
                return "";
        }

        private void FilterCurrentDG(string filter)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (FocusedDG != null)
                {
                    try
                    {
                        switch (FocusedDG.Name)
                        {
                            case "GenderDG":
                                var gender = (GenderCVS.Source as ObservableCollection<Gender>).Where(i => i.Description.StartsWith(filter, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                                if (gender != null)
                                {
                                    GenderDG.SelectedItem = gender;
                                    GenderDG.ScrollIntoView(gender);
                                }
                                break;
                            case "CategoriesDG":
                                var category = (CategoriesCVS.Source as ObservableCollection<Category>).Where(i => i.Title.StartsWith(filter, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                                if (category != null)
                                {
                                    CategoriesDG.SelectedItem = category;
                                    CategoriesDG.ScrollIntoView(category);
                                }
                                break;
                            case "BrandsDG":
                                var brand = (BrandsCVS.Source as ObservableCollection<Brand>).Where(i => i.Title.StartsWith(filter, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                                if (brand != null)
                                {
                                    BrandsDG.SelectedItem = brand;
                                    BrandsDG.ScrollIntoView(brand);
                                }
                                break;
                            case "SizesDG":
                                var size = (SizesCVS.Source as ObservableCollection<Model.Db.Size>).Where(i => i.Value.StartsWith(filter, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                                if (size != null)
                                {
                                    SizesDG.SelectedItem = size;
                                    SizesDG.ScrollIntoView(size);
                                }
                                break;
                            case "ColorsDG":
                                var color = (ColorsCVS.Source as ObservableCollection<Model.Db.Color>).Where(i => i.Description.StartsWith(filter, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                                if (color != null)
                                {
                                    ColorsDG.SelectedItem = color;
                                    ColorsDG.ScrollIntoView(color);
                                }
                                break;
                            case "MaterialsDG":
                                var material = (MaterialsCVS.Source as ObservableCollection<Material>).Where(i => i.Title.StartsWith(filter, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                                if (material != null)
                                {
                                    MaterialsDG.SelectedItem = material;
                                    MaterialsDG.ScrollIntoView(material);
                                }
                                break;
                            case "PartsDG":
                                var parts = (PartsCVS.Source as ObservableCollection<Part>).Where(i => i.Title.StartsWith(filter, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                                if (parts != null)
                                {
                                    PartsDG.SelectedItem = parts;
                                    PartsDG.ScrollIntoView(parts);
                                }
                                break;
                            case "DefectsDG":
                                var defect = (DefectsCVS.Source as ObservableCollection<Defect>).Where(i => i.Title.StartsWith(filter, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                                if (defect != null)
                                {
                                    DefectsDG.SelectedItem = defect;
                                    DefectsDG.ScrollIntoView(defect);
                                }
                                break;
                            case "TypesDG":
                                var type = (TypesCVS.Source as ObservableCollection<Model.Db.Type>).Where(i => i.Title.StartsWith(filter, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                                if (type != null)
                                {
                                    TypesDG.SelectedItem = type;
                                    TypesDG.ScrollIntoView(type);
                                }
                                break;
                        }
                    }
                    catch { }
                }
            }));
        }

        private void DGs_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Cancel)
            {
                DGInEditingMode = false;
                return;
            }

            MessageBoxResult result = MessageBox.Show("Sollen die Änderungen an den Merkmalen gespeichert werden?", "Merkmale wurden geändert", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.No);

            if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            else if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
                DataGrid dg = sender as DataGrid;
                dg.CancelEdit();
            }

            DGInEditingMode = false;
        }

        private void DGs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Gender
            if (GenderDG.SelectedItem is Gender gender)
                Article.Gender = gender;
            else
                Article.Gender = null;
            // Categories
            if (CategoriesDG.SelectedItem is Category category)
                Article.Category = category;
            else
                Article.Category = null;
            // Type
            if (TypesDG.SelectedItem is Model.Db.Type type)
                Article.Type = type;
            else
                Article.Type = null;
            // Brand
            if (BrandsDG.SelectedItem is Brand brand)
                Article.Brand = brand;
            else
                Article.Brand = null;
            // Size
            if (SizesDG.SelectedItem is Model.Db.Size size)
                Article.Size = size;
            else
                Article.Size = null;
            // Colors
            if (ColorsDG.SelectedItems.Count > 0)
                Article.Colors = RemoveNamedObject<Color>(ColorsDG.SelectedItems);
            else
                Article.Colors = null;
            // Materials
            if (MaterialsDG.SelectedItems.Count > 0)
                Article.Materials = RemoveNamedObject<Material>(MaterialsDG.SelectedItems);
            else
                Article.Materials = null;
            // Parts
            if (PartsDG.SelectedItem is Part part && !string.IsNullOrWhiteSpace(part.Title))
                Article.Parts = part;
            else
                Article.Parts = null;
            // Defects
            if (DefectsDG.SelectedItems.Count > 0)
                Article.Defects = RemoveNamedObject<Defect>(DefectsDG.SelectedItems);
            else
                Article.Defects = null;

            Article.GenerateDescription();
            Article.OnPropertyChanged("Description");

            PopulateAutoCompleteEntries();
        }

        private void DGs_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (DisableEnteringEditingMode)
            {
                DisableEnteringEditingMode = true;
                e.Cancel = true;
            }
            else
                DGInEditingMode = true;
        }

        private void DGs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DisableEnteringEditingMode = false;

            Timer t = new Timer(100);
            t.Elapsed += Timer_Elapsed;
            ;
            t.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DisableEnteringEditingMode = true;

            Timer timer = sender as Timer;
            timer.Stop();
            timer.Dispose();
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

        private async void PopulateAutoCompleteEntries()
        {
            SuggestionsProvider.Suggestions.Clear();

            if (Article.Category == null || Article.Type == null || Article.Brand == null)
                return;

            List<decimal> results;
            if (Article.Gender == null)
                results = await MainDb.Articles.Where(a => a.Category != null && a.Category.Id == Article.Category.Id && a.Type != null && a.Type.Id == Article.Type.Id && a.Brand != null && a.Brand.Id == Article.Brand.Id)
                    .Select(a => a.Price)
                    .Distinct()
                    .OrderByDescending(p => p)
                    .Take(20)
                    .ToListAsync();
            else
                results = await MainDb.Articles.Where(a => a.Gender != null && a.Gender.Id == Article.Gender.Id && a.Category != null && a.Category.Id == Article.Category.Id && a.Type != null && a.Type.Id == Article.Type.Id && a.Brand != null && a.Brand.Id == Article.Brand.Id)
                    .Select(a => a.Price)
                    .Distinct()
                    .OrderByDescending(p => p)
                    .Take(20)
                    .ToListAsync();

            foreach (var result in results)
            {
                SuggestionsProvider.Suggestions.Add(result);
            }
        }
        #endregion
    }
}