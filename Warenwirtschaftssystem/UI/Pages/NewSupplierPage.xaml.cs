using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Warenwirtschaftssystem.Model;
using Warenwirtschaftssystem.Model.Db;

namespace Warenwirtschaftssystem.UI.Pages
{
    public partial class NewSupplierPage : Page
    {
        // Attribute
        private Window OwnerWindow;
        private DataModel Data;
        public Supplier Supplier = null;
        private DbModel MainDb;

        #region Initialisierung

        // Konstruktors
        public NewSupplierPage(Window ownerWindow, DataModel data, DbModel mainDb)
        {
            OwnerWindow = ownerWindow;
            Data = data;
            MainDb = mainDb;

            InitializeComponent();

            if (MainDb.Settings.Where(s => s.Key == "DefaultPickUp").SingleOrDefault() is Setting setting)
                PickUpTb.Text = setting.Value;

            TitleCB.SelectedValue = Model.Db.Title.Mrs;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Events registrieren
            PickUpTb.TextChanged += PickUpTb_TextChanged;
        }

        #endregion

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            OwnerWindow.Close();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if(TitleCB.SelectedIndex != -1 && !string.IsNullOrWhiteSpace(PickUpTb.Text))
            {
                Supplier = new Supplier()
                {
                    Company = CompanyTB.Text,
                    EMail = EMailTB.Text,
                    Name = NameTB.Text,
                    Phone = PhoneTB.Text,
                    Place = PlaceTB.Text,
                    Street = StreetTB.Text,
                    Title = (Title)TitleCB.SelectedItem,
                    Postcode = PostcodeTb.Text,
                    Notes = NotesTb.Text,
                    PickUp = (PickUpTb.Value as short?).Value,
                    SupplierProportion = SupplierProportionTb.Value as decimal?,
                    CreationDate = DateTime.Now
                };
                OwnerWindow.Close();
            }
        }

        private void PickUpTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PickUpTb.Value == null) PickUpBorder.BorderThickness = new Thickness(1);
            else PickUpBorder.BorderThickness = new Thickness(0);
        }

        private void SupplierProportionTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SupplierProportionTb.Text) && SupplierProportionTb.Value == null) SupplierProportionBorder.BorderThickness = new Thickness(1);
            else SupplierProportionBorder.BorderThickness = new Thickness(0);
        }
    }
}
