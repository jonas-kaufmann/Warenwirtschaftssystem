using System;
using System.Data.Entity;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Warenwirtschaftssystem.Model;
using Warenwirtschaftssystem.Model.Db;
using Warenwirtschaftssystem.UI.Windows;

namespace Warenwirtschaftssystem.UI.Pages
{
    public partial class PickSupplierPage : Page
    {
        // Attribute
        private DbModel MainDb;
        private CollectionViewSource SuppliersCVS;
        private Window OwnerWindow;
        private DataModel Data;

        public Supplier SelectedSupplier = null;

        #region Initialisierung

        // Konstruktor
        public PickSupplierPage(Window ownerWindow, DataModel data, DbModel mainDb) 
        {
            OwnerWindow = ownerWindow;
            OwnerWindow.Title = "Lieferanten auswählen";
            Data = data;
            MainDb = mainDb;

            InitializeComponent();

            FilterNameTb.Focus();

            #region Daten aus Db laden

            SuppliersCVS = FindResource("SuppliersCVS") as CollectionViewSource;

            MainDb.Suppliers.Load();
            SuppliersCVS.Source = MainDb.Suppliers.Local;

            #endregion
        }

        #endregion

        #region Supplier übergeben

        private void SelectBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenNewArticleWindow();
        }

        private void SupplierDG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenNewArticleWindow();
        }

        private void OpenNewArticleWindow()
        {
            if((SelectedSupplier = SuppliersDG.SelectedItem as Supplier) != null) OwnerWindow.Close();
        }

        #endregion

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            OwnerWindow.Close();
        }

        private void NewSupplierBtn_Click(object sender, RoutedEventArgs e)
        {
            // Fenster zum Erstellen eines Lieferanten öffnen
            PopupWindow popupWindow = new PopupWindow(OwnerWindow, Data)
            {
                Owner = OwnerWindow,
                Title = "Lieferant anlegen"
            };

            NewSupplierPage newSupplierPage = new NewSupplierPage(popupWindow, Data, MainDb);

            popupWindow.Content = newSupplierPage;
            popupWindow.Initialize();
            popupWindow.ShowDialog();

            if (newSupplierPage.Supplier != null)
            {
                MainDb.Suppliers.Add(newSupplierPage.Supplier);
                MainDb.SaveChanges();
                SelectedSupplier = newSupplierPage.Supplier;
                OwnerWindow.Close();
            }
        }

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

            ApplyFilter();
        }

        private bool SupplierSet;
        private int FilterId;
        private bool PlaceSet;

        private void ApplyFilter()
        {
            SupplierSet = !string.IsNullOrEmpty(FilterNameTb.Text);
            int.TryParse(FilterNameTb.Text, out FilterId);
            PlaceSet = !string.IsNullOrEmpty(FilterPlaceTb.Text);

            SuppliersCVS.Filter += SuppliersCVS_Filter;

            SuppliersDG.Focus();
        }

        #endregion

        private void SuppliersDG_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                OpenNewArticleWindow();
            }
        }

        private void Page_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                OwnerWindow.Close();
            }
        }
    }
}
