using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Warenwirtschaftssystem.Model;
using Warenwirtschaftssystem.Model.Db;

namespace Warenwirtschaftssystem.UI.Pages
{
    public partial class ChangePriceAndPayoutPage : Page
    {
        private DataModel Data;
        private Window OwnerWindow;
        private DbModel MainDb;

        private Article Article;
        private decimal oldPayout;
        private decimal oldPrice;

        public ChangePriceAndPayoutPage(DataModel data, Window ownerWindow, DbModel mainDb, Article article)
        {
            Data = data;
            OwnerWindow = ownerWindow;
            MainDb = mainDb;

            Article = article;
            DataContext = Article;
            oldPayout = Article.SupplierProportion;
            oldPrice = Article.Price;

            InitializeComponent();

            // register events
            OwnerWindow.Closed += OwnerWindow_Closed;
        }

        private void OwnerWindow_Closed(object sender, System.EventArgs e)
        {
            // handle close as CancelBtn click
            CancelBtn_Click(null, null);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Article.PropertyChanged += Article_PropertyChanged;
            PriceTB.TextChanged += CurrencyTBs_TextChanged;
            SupplierProportionTB.TextChanged += CurrencyTBs_TextChanged;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Article != null)
                Article.PropertyChanged -= Article_PropertyChanged;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            OwnerWindow.Close();

            if (Article.Price != oldPrice || Article.SupplierProportion != oldPayout)
                new Documents(Data, MainDb).NotifyArticlePropertiesChanged(Article.Id);
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Article.Price = oldPrice;
            Article.SupplierProportion = oldPayout;

            try
            {
                OwnerWindow.Close();
            }
            catch { }
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

        private void CurrencyTBs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(PriceTB.Text, NumberStyles.Currency, CultureInfo.CreateSpecificCulture("de-DE"), out decimal price)
                && decimal.TryParse(SupplierProportionTB.Text, NumberStyles.Currency, CultureInfo.CreateSpecificCulture("de-DE"), out decimal supplierProportion)
                && price >= 0
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
    }
}
