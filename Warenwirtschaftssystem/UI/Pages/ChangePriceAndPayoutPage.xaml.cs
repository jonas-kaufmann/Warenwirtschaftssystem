using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Warenwirtschaftssystem.Model;
using Warenwirtschaftssystem.Model.Db;

namespace Warenwirtschaftssystem.UI.Pages
{
    public partial class ChangePriceAndPayoutPage : Page, INotifyPropertyChanged
    {
        private DataModel Data;
        private Window OwnerWindow;
        private DbModel MainDb;

        private Article Article;
        private decimal newPrice;
        private decimal newPayout;

        public event PropertyChangedEventHandler PropertyChanged;

        public ChangePriceAndPayoutPage(DataModel data, Window ownerWindow, DbModel mainDb, Article article)
        {
            Data = data;
            OwnerWindow = ownerWindow;
            MainDb = mainDb;

            Article = article;
            DataContext = Article;
            newPrice = Article.Price;
            newPayout = Article.SupplierProportion;

            InitializeComponent();

            // register events
            OwnerWindow.Closed += OwnerWindow_Closed;
            PriceTB.TextChanged += CurrencyTBs_TextChanged;
            SupplierProportionTB.TextChanged += CurrencyTBs_TextChanged;
        }

        private void OwnerWindow_Closed(object sender, System.EventArgs e)
        {
            // handle close as CancelBtn click
            CancelBtn_Click(null, null);
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            OwnerWindow.Closed -= OwnerWindow_Closed;
            OwnerWindow.Close();

            Article.ChangePriceAndPayout(MainDb, newPrice, newPayout);
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            OwnerWindow.Closed -= OwnerWindow_Closed;
            OwnerWindow.Close();
        }

        private void CurrencyTBs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(PriceTB.Text, NumberStyles.Currency, DataModel.CultureInfo, out decimal price)
                && decimal.TryParse(SupplierProportionTB.Text, NumberStyles.Currency, DataModel.CultureInfo, out decimal payout)
                && price >= 0
                && payout >= 0
                && price >= payout)
            {
                Article dummyArticle = new();
                dummyArticle.SetPriceAndPayoutSkipChecks(price, payout);
                
                // calculate payout when price changed
                if (price != newPrice)
                {
                    newPayout = dummyArticle.SuggestedPayoutFromPrice(MainDb);
                    SupplierProportionTB.Text = newPayout.ToString("P", DataModel.CultureInfo);
                }

                PercentageTB.Text = dummyArticle.Percentage.Value.ToString("P", DataModel.CultureInfo);
                newPrice = price;
                newPayout = payout;

                SaveBtn.IsEnabled = true;
            }
            else
            {
                SaveBtn.IsEnabled = false;
            }
        }
    }
}
