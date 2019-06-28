using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Warenwirtschaftssystem.Model;
using Warenwirtschaftssystem.Model.Db;
using Warenwirtschaftssystem.UI.Windows;
using System.Linq;
using System;
using System.Windows.Input;
using Warenwirtschaftssystem.Model.Documents;

namespace Warenwirtschaftssystem.UI.Pages
{
    public partial class SuppliersPage : Page
    {
        // Attribute
        private DataModel Data;
        private ToolWindow OwnerWindow;
        private DbModel MainDb;
        private CollectionViewSource SuppliersCVS;

        #region Initialisierung

        // Konstruktor
        public SuppliersPage(DataModel data, ToolWindow ownerWindow)
        {
            Data = data;
            OwnerWindow = ownerWindow;
            MainDb = new DbModel(data.MainConnectionString);

            InitializeComponent();

            #region Daten aus Db laden

            SuppliersCVS = FindResource("SuppliersCVS") as CollectionViewSource;

            MainDb.Suppliers.Load();
            SuppliersCVS.Source = MainDb.Suppliers.Local;

            #endregion

            FilterNameTb.Focus();

            //Column-Width anpassen
            SuppliersDG.Columns[2].Width = 0;
            SuppliersDG.UpdateLayout();
            SuppliersDG.Columns[2].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
        }

        #endregion

        #region Filter

        private void SuppliersCVS_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is Supplier supplier)
            {
                #region Id/Name

                if (SupplierSet && ((FilterId != 0 && FilterId != supplier.Id) || (FilterId == 0 && (supplier.Name == null || supplier.Name.IndexOf(FilterNameTb.Text, 0, StringComparison.CurrentCultureIgnoreCase) == -1))))
                {
                    e.Accepted = false;
                    return;
                }

                #endregion

                #region Place

                if (PlaceSet && (supplier.Place == null || supplier.Place.IndexOf(FilterPlaceTb.Text, 0, StringComparison.CurrentCultureIgnoreCase) == -1))
                {
                    e.Accepted = false;
                    return;
                }

                #endregion

                #region Company

                if (CompanySet && (supplier.Company == null || supplier.Company.IndexOf(FilterCompanyTb.Text, 0, StringComparison.CurrentCultureIgnoreCase) == -1))
                {
                    e.Accepted = false;
                    return;
                }

                #endregion
            }
            else e.Accepted = false;
        }

        private void FilterTb_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplyFilter();
            }
        }

        private void ResetFilterBtn_Click(object sender, RoutedEventArgs e)
        {
            FilterNameTb.Clear();
            FilterPlaceTb.Clear();
            FilterCompanyTb.Clear();

            ApplyFilter();
        }

        private bool SupplierSet;
        private int FilterId;
        private bool PlaceSet;
        private bool CompanySet;

        private void ApplyFilter()
        {
            SupplierSet = !string.IsNullOrEmpty(FilterNameTb.Text);
            int.TryParse(FilterNameTb.Text, out FilterId);
            PlaceSet = !string.IsNullOrEmpty(FilterPlaceTb.Text);
            CompanySet = !string.IsNullOrEmpty(FilterCompanyTb.Text);

            SuppliersCVS.Filter += SuppliersCVS_Filter;

            //Column-Width anpassen
            SuppliersDG.Columns[2].Width = 0;
            SuppliersDG.UpdateLayout();
            SuppliersDG.Columns[2].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
        }

        #endregion

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            OwnerWindow.DisableClosingPrompt = true;
            OwnerWindow.RootWindow.RemoveToolWindow(OwnerWindow);
            OwnerWindow.Close();
            MainDb.Dispose();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            OwnerWindow.DisableClosingPrompt = true;
            OwnerWindow.RootWindow.RemoveToolWindow(OwnerWindow);
            OwnerWindow.Close();
            MainDb.SaveChanges();
            MainDb.Dispose();
        }

        private void NewSupplierBtn_Click(object sender, RoutedEventArgs e)
        {
            Supplier supplier = new Supplier();
            AddStandardValuesToSupplier(supplier);
            (SuppliersCVS.Source as ObservableCollection<Supplier>).Add(supplier);
            SuppliersDG.SelectedItem = supplier;
            SuppliersDG.ScrollIntoView(supplier);
            SuppliersDG.CurrentCell = new DataGridCellInfo(supplier, SuppliersDG.Columns[1]);
            SuppliersDG.BeginEdit();
        }

        private void ShowArticlesBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SuppliersDG.SelectedItem != null)
            {
                ToolWindow toolWindow = new ToolWindow(OwnerWindow.RootWindow, Data)
                {
                    Owner = OwnerWindow.RootWindow,
                    Width = 1200,
                    Height = 700,
                    MinWidth = 1200,
                    MinHeight = 700
                };
                toolWindow.Content = new ArticlePage(Data, toolWindow, (Supplier)SuppliersDG.SelectedItem);
                toolWindow.Title = "Artikel";
                OwnerWindow.RootWindow.AddToolWindow(toolWindow);
                toolWindow.Show();
            }
        }

        private void SuppliersDG_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            Supplier supplier = new Supplier();
            AddStandardValuesToSupplier(supplier);
            e.NewItem = supplier;
        }

        private void AddStandardValuesToSupplier(Supplier supplier)
        {
            supplier.CreationDate = DateTime.Now;
            supplier.Title = Model.Db.Title.Mrs;
            if (MainDb.Settings.Where(s => s.Key == "DefaultPickUp").SingleOrDefault() is Setting defaultPickUp)
                supplier.PickUp = short.Parse(defaultPickUp.Value);
        }

        private void PickUpTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PickUpTb.Value == null) PickUpBorder.BorderThickness = new Thickness(1);
            else PickUpBorder.BorderThickness = new Thickness(0);
        }

        private void SupplierProportionTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SupplierProportionTb.Text) && (!decimal.TryParse(SupplierProportionTb.Text, out decimal supplierProportion) || supplierProportion < 0 || supplierProportion > 100)) SupplierProportionBorder.BorderThickness = new Thickness(1);
            else SupplierProportionBorder.BorderThickness = new Thickness(0);
        }

        private void SuppliersDG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(SuppliersDG.SelectedItem is Supplier selectedSupllier))
            {
                SupplierCharacteristicsG.DataContext = null;
                PickUpTb.Text = "0";
                SupplierCharacteristicsG.IsEnabled = false;
                ShowArticlesBtn.IsEnabled = false;
            }
            else
            {
                SupplierCharacteristicsG.DataContext = SuppliersCVS;
                SupplierCharacteristicsG.IsEnabled = true;
                if (selectedSupllier.Id == 0) ShowArticlesBtn.IsEnabled = false;
                else ShowArticlesBtn.IsEnabled = true;
            }
        }

        private void PayoutBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SuppliersDG.SelectedItem is Supplier supplier)
            {
                ToolWindow tW = new ToolWindow(OwnerWindow.RootWindow, Data)
                {
                    Title = "Artikel"
                };
                ArticlePage aP = new ArticlePage(Data, tW, supplier, Status.Sold);
                tW.Content = aP;
                tW.Show();
            }
        }

        private void PrintStockList_Click(object sender, RoutedEventArgs e)
        {
            if (SuppliersDG.SelectedItem is Supplier supplier && supplier.Articles != null && supplier.Articles.Count > 0)
            {
                new ArticlesInStockDoc(Data, supplier).CreateAndPrintDocument();
            }
        }

        private void PrintStockListWithPrice_Click(object sender, RoutedEventArgs e)
        {
            if (SuppliersDG.SelectedItem is Supplier supplier && supplier.Articles != null && supplier.Articles.Count > 0)
            {
                new ArticlesInStockDoc(Data, supplier, true).CreateAndPrintDocument();
            }
        }

        private void SuppliersDG_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            #region Nach Supplier-Row in Textboxen tabben

            if (e.Key == Key.Tab && SuppliersDG.CurrentColumn.DisplayIndex == 6)
            {
                StreetTB.Focus();
                e.Handled = true;
            }

            #endregion
        }
    }
}
