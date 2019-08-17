using System;
using System.Collections;
using System.Collections.ObjectModel;
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
        private string FilterInput = "";
        private Timer FilterTimer = new Timer(500);
        private Key[] FilterLetterKeys = { Key.A, Key.B, Key.C, Key.D, Key.E, Key.F, Key.G, Key.H, Key.I, Key.J, Key.K, Key.L, Key.M, Key.N, Key.O, Key.P, Key.Q, Key.R, Key.S, Key.T, Key.U, Key.V, Key.W, Key.X, Key.Y, Key.Z };
        private Key[] FilterNumberKeys = { Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9 };
        private Key[] FilterNumPadKeys = { Key.NumPad0, Key.NumPad1, Key.NumPad2, Key.NumPad3, Key.NumPad4, Key.NumPad5, Key.NumPad6, Key.NumPad7, Key.NumPad8, Key.NumPad9 };
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

            if (supplier.PickUp > -1) Article.PickUp = DateTime.Now.Date.AddDays(Supplier.PickUp * 7);

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

            if (supplier.PickUp > -1) Article.PickUp = DateTime.Now.Date.AddDays(Supplier.PickUp * 7);

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

            if (supplier.PickUp > -1) Article.PickUp = DateTime.Now.Date.AddDays(Supplier.PickUp * 7);

            ThirdColumnGrid.DataContext = Article;
            LoadDataFromDbIntoView();

            #region Artikeleigenschaften in DGs auswählen

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

            #endregion
        }

        /// <summary>
        /// Bestehenden Artikel von NewArticlesPage aus bearbeiten
        /// </summary>
        public NewArticlePage(DataModel data, ToolWindow ownerWindow, Article articleToEdit, NewArticlesPage newArticlesPage, DbModel mainDb)
        {
            Data = data;
            OwnerWindow = ownerWindow;
            Article = articleToEdit;
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

            #region Artikeleigenschaften in DGs auswählen

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

            #endregion

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

                SaveBtn.IsEnabled = false;
            }

            MainDb = mainDb;

            Article referencedArticle = articleToEdit;

            Supplier = MainDb.Suppliers.Where(s => s.Id == referencedArticle.Supplier.Id).Single();
            Article = MainDb.Articles.Where(a => a.Id == referencedArticle.Id).Single();
            ArticlePage = articlePage;

            ThirdColumnGrid.DataContext = Article;

            LoadDataFromDbIntoView();

            #region Artikeleigenschaften in DGs auswählen

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

            #endregion

            Article.PropertyChanged += Article_PropertyChanged;
        }

        private void LoadDataFromDbIntoView()
        {
            // Gender
            GenderCVS = (CollectionViewSource)FindResource("GenderCVS");
            MainDb.ArticleGender.OrderBy(g => g.Description).Load();
            GenderCVS.Source = MainDb.ArticleGender.Local;
            // Brands
            BrandsCVS = (CollectionViewSource)FindResource("BrandsCVS");
            MainDb.ArticleBrands.OrderBy(b => b.Title).Load();
            BrandsCVS.Source = MainDb.ArticleBrands.Local;
            BrandsDG.SelectedItem = null;
            // Sizes
            SizesCVS = (CollectionViewSource)FindResource("SizesCVS");
            MainDb.ArticleSizes.OrderBy(s => s.Value).Load();
            SizesCVS.Source = MainDb.ArticleSizes.Local;
            SizesDG.SelectedItem = null;
            // Materials
            MaterialsCVS = (CollectionViewSource)FindResource("MaterialsCVS");
            MainDb.ArticleMaterials.OrderBy(m => m.Title).Load();
            MaterialsCVS.Source = MainDb.ArticleMaterials.Local;
            MaterialsDG.SelectedItem = null;
            // Parts
            PartsCVS = (CollectionViewSource)FindResource("PartsCVS");
            MainDb.ArticleParts.OrderBy(p => p.Title).Load();
            PartsCVS.Source = MainDb.ArticleParts.Local;
            PartsDG.SelectedItem = null;
            // Defects
            DefectsCVS = (CollectionViewSource)FindResource("DefectsCVS");
            MainDb.ArticleDefects.OrderBy(d => d.Title).Load();
            DefectsCVS.Source = MainDb.ArticleDefects.Local;
            DefectsDG.SelectedItem = null;
            // Categories
            CategoriesCVS = (CollectionViewSource)FindResource("CategoriesCVS");
            MainDb.ArticleCategories.OrderBy(c => c.Title).Include(c => c.Types).Load();
            CategoriesCVS.Source = MainDb.ArticleCategories.Local;

            //Types sortieren
            foreach (Category category in MainDb.ArticleCategories.Local)
                category.Types = new ObservableCollection<Type>(category.Types.OrderBy(t => t.Title));

            CategoriesDG.SelectedItem = null;
            // Colors
            ColorsCVS = (CollectionViewSource)FindResource("ColorsCVS");
            MainDb.ArticleColors.OrderBy(c => c.Description).Load();
            ColorsCVS.Source = MainDb.ArticleColors.Local;
            ColorsDG.SelectedItem = null;
            // Types
            TypesCVS = (CollectionViewSource)FindResource("TypesCVS");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            GenderDG.Focus();
            FilterTimer.Elapsed += FilterTimerOver;
            SaveBtn.IsEnabled = false;

            #region UI-Events registrieren

            CategoriesDG.AddingNewItem += CategoriesDG_AddingNewItem;
            CategoriesDG.SelectionChanged += CategoriesDG_SelectionChanged;
            PriceTB.TextChanged += CurrencyTBs_TextChanged;
            SupplierProportionTB.TextChanged += CurrencyTBs_TextChanged;

            foreach (DataGrid dg in DataGrids)
            {
                dg.PreviewKeyDown += DataGrids_PreviewKeyDown;
                dg.SelectionChanged += DGs_SelectionChanged;
            }

            if (!Editable)
            {
                foreach (DataGrid dg in DataGrids)
                {
                    ScrollContentPresenter sCP = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(dg, 0), 0), 0), 2) as ScrollContentPresenter;
                    sCP.PreviewMouseLeftButtonDown += DataGridsDisableUserSelection_PreviewMouseButton;
                    sCP.PreviewMouseRightButtonUp += DataGridsDisableUserSelection_PreviewMouseButton;
                }
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

        private void DataGridsDisableUserSelection_PreviewMouseButton(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
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

                        SaveBtn.IsEnabled = true;
                    }

                    break;
            }
        }

        #endregion

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
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

            if (NewArticlesPage == null)
            {
                Article.notifyAllPropertiesChanged();

                OwnerWindow.Title = "Artikel";
                OwnerWindow.Content = ArticlePage;

                MainDb.SaveChangesAsync();
            }
            else
            {
                if (IsNewArticle)
                {
                    Article.AddedToSortiment = DateTime.Now;
                    NewArticlesPage.NewArticle = Article;
                }
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
                SaveBtn.IsEnabled = true;
            }
            else
            {
                SaveBtn.IsEnabled = false;
            }
        }

        private void CategoriesDG_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            Category c = new Category
            {
                Types = new ObservableCollection<Model.Db.Type>()
            };
            e.NewItem = c;
        }

        private void CategoriesDG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoriesDG.SelectedItem as Category == null)
            {
                TypesDG.DataContext = null;
                TypesDG.IsEnabled = false;
            }
            else
            {
                TypesDG.DataContext = TypesCVS;
                TypesDG.IsEnabled = true;
            }
        }

        private void DataGrids_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DGInEditingMode)
                return;

            FocusedDG = sender as DataGrid;

            string key;
            if (FocusedDG != null && !(key = ParseKey(e.Key)).Equals(""))
            {
                if (FilterTimer.Enabled)
                {
                    FilterTimer.Stop();
                    FilterTimer.Start();
                }
                else FilterTimer.Start();
                FilterInput += key;
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
            else if (key == Key.Oem1) return "ü";
            else if (key == Key.Oem7) return "ä";
            else if (key == Key.Oem3) return "ö";
            else return "";
        }

        private void FilterTimerOver(object source, ElapsedEventArgs e)
        {
            FilterTimer.Stop();
            FilterDG();
        }

        private void FilterDG()
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
                                var gender = (GenderCVS.Source as ObservableCollection<Gender>).Where(i => i.Description.IndexOf(FilterInput, 0, StringComparison.CurrentCultureIgnoreCase) == 0).FirstOrDefault();
                                if (gender != null)
                                {
                                    GenderDG.SelectedItem = gender;
                                    GenderDG.ScrollIntoView(gender);
                                }
                                break;
                            case "CategoriesDG":
                                var category = (CategoriesCVS.Source as ObservableCollection<Category>).Where(i => i.Title.IndexOf(FilterInput, 0, StringComparison.CurrentCultureIgnoreCase) == 0).FirstOrDefault();
                                if (category != null)
                                {
                                    CategoriesDG.SelectedItem = category;
                                    CategoriesDG.ScrollIntoView(category);
                                }
                                break;
                            case "BrandsDG":
                                var brand = (BrandsCVS.Source as ObservableCollection<Brand>).Where(i => i.Title.IndexOf(FilterInput, 0, StringComparison.CurrentCultureIgnoreCase) == 0).FirstOrDefault();
                                if (brand != null)
                                {
                                    BrandsDG.SelectedItem = brand;
                                    BrandsDG.ScrollIntoView(brand);
                                }
                                break;
                            case "SizesDG":
                                var size = (SizesCVS.Source as ObservableCollection<Model.Db.Size>).Where(i => i.Value.IndexOf(FilterInput, 0, StringComparison.CurrentCultureIgnoreCase) == 0).FirstOrDefault();
                                if (size != null)
                                {
                                    SizesDG.SelectedItem = size;
                                    SizesDG.ScrollIntoView(size);
                                }
                                break;
                            case "ColorsDG":
                                var color = (ColorsCVS.Source as ObservableCollection<Model.Db.Color>).Where(i => i.Description.IndexOf(FilterInput, 0, StringComparison.CurrentCultureIgnoreCase) == 0).FirstOrDefault();
                                if (color != null)
                                {
                                    ColorsDG.SelectedItem = color;
                                    ColorsDG.ScrollIntoView(color);
                                }
                                break;
                            case "MaterialsDG":
                                var material = (MaterialsCVS.Source as ObservableCollection<Material>).Where(i => i.Title.IndexOf(FilterInput, 0, StringComparison.CurrentCultureIgnoreCase) == 0).FirstOrDefault();
                                if (material != null)
                                {
                                    MaterialsDG.SelectedItem = material;
                                    MaterialsDG.ScrollIntoView(material);
                                }
                                break;
                            case "PartsDG":
                                var parts = (PartsCVS.Source as ObservableCollection<Part>).Where(i => i.Title.IndexOf(FilterInput, 0, StringComparison.CurrentCultureIgnoreCase) == 0).FirstOrDefault();
                                if (parts != null)
                                {
                                    PartsDG.SelectedItem = parts;
                                    PartsDG.ScrollIntoView(parts);
                                }
                                break;
                            case "DefectsDG":
                                var defect = (DefectsCVS.Source as ObservableCollection<Defect>).Where(i => i.Title.IndexOf(FilterInput, 0, StringComparison.CurrentCultureIgnoreCase) == 0).FirstOrDefault();
                                if (defect != null)
                                {
                                    DefectsDG.SelectedItem = defect;
                                    DefectsDG.ScrollIntoView(defect);
                                }
                                break;
                            case "TypesDG":
                                var type = (TypesCVS.Source as ObservableCollection<Model.Db.Type>).Where(i => i.Title.IndexOf(FilterInput, 0, StringComparison.CurrentCultureIgnoreCase) == 0).FirstOrDefault();
                                if (type != null)
                                {
                                    TypesDG.SelectedItem = type;
                                    TypesDG.ScrollIntoView(type);
                                }
                                break;
                        }
                    }
                    catch { }
                    FilterInput = "";
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
            else Article.Gender = null;
            // Categories
            if (CategoriesDG.SelectedItem is Category category)
                Article.Category = category;
            else Article.Category = null;
            // Type
            if (TypesDG.SelectedItem is Model.Db.Type type)
                Article.Type = type;
            else Article.Type = null;
            // Brand
            if (BrandsDG.SelectedItem is Brand brand)
                Article.Brand = brand;
            else Article.Brand = null;
            // Size
            if (SizesDG.SelectedItem is Model.Db.Size size)
                Article.Size = size;
            else Article.Size = null;
            // Colors
            if (ColorsDG.SelectedItems.Count > 0) Article.Colors = RemoveNamedObject<Color>(ColorsDG.SelectedItems);
            else Article.Colors = null;
            // Materials
            if (MaterialsDG.SelectedItems.Count > 0) Article.Materials = RemoveNamedObject<Material>(MaterialsDG.SelectedItems);
            else Article.Materials = null;
            // Parts
            if (PartsDG.SelectedItem is Part part && !string.IsNullOrWhiteSpace(part.Title))
                Article.Parts = part;
            else Article.Parts = null;
            // Defects
            if (DefectsDG.SelectedItems.Count > 0) Article.Defects = RemoveNamedObject<Defect>(DefectsDG.SelectedItems);
            else Article.Defects = null;

            Article.RegenerateDescription();
            Article.OnPropertyChanged("Description");
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
            t.Elapsed += Timer_Elapsed; ;
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
    }
}
