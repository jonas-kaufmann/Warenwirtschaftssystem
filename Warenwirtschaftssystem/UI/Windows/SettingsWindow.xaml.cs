using System.Data.Entity;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Warenwirtschaftssystem.Model;
using Warenwirtschaftssystem.Model.Db;

namespace Warenwirtschaftssystem.UI.Windows
{
    public partial class SettingsWindow : Window
    {
        // Attribute
        private DbModel MainDb;
        private DataModel Data;

        #region Initialisierung

        // Konstruktor
        public SettingsWindow(DataModel data)
        {
            Data = data;
            MainDb = new DbModel(data.MainConnectionString);

            InitializeComponent();

            Setting defaultPickUp = MainDb.Settings.Where(s => s.Key == "DefaultPickUp").SingleOrDefault();
            if (defaultPickUp != null)
            {
                DefaultPickUpTb.Value = short.Parse(defaultPickUp.Value);
            }

            CollectionViewSource graduationSupplierProportionViewSource = ((CollectionViewSource)(FindResource("graduationSupplierProportionViewSource")));
            MainDb.GraduationSupplierProportion.Load();
            graduationSupplierProportionViewSource.Source = MainDb.GraduationSupplierProportion.Local;


            #region Drucker 

            BonPrinterCB.Items.Add("");
            TagPrinterCB.Items.Add("");
            TagLandscapePrinterCB.Items.Add("");
            DocumentPrinterCB.Items.Add("");

            foreach (PrintQueue pQ in new LocalPrintServer().GetPrintQueues(new[] { EnumeratedPrintQueueTypes.Local, EnumeratedPrintQueueTypes.Connections }))
            {
                BonPrinterCB.Items.Add(pQ.FullName);
                if (pQ.FullName == Data.StandardPrinters.BonPrinter)
                    BonPrinterCB.SelectedItem = pQ.FullName;

                TagPrinterCB.Items.Add(pQ.FullName);
                if (pQ.FullName == Data.StandardPrinters.TagPrinter)
                    TagPrinterCB.SelectedItem = pQ.FullName;

                TagLandscapePrinterCB.Items.Add(pQ.FullName);
                if (pQ.FullName == Data.StandardPrinters.TagLandscapePrinter)
                    TagLandscapePrinterCB.SelectedItem = pQ.FullName;

                DocumentPrinterCB.Items.Add(pQ.FullName);
                if (pQ.FullName == Data.StandardPrinters.DocumentPrinter)
                    DocumentPrinterCB.SelectedItem = pQ.FullName;
            }

            #endregion

            #region Dokumentenkopf Abschnitt - vorhandene Werte aus Db laden

            Setting shopdescription = MainDb.Settings.Where(s => s.Key == "Shopdescription").FirstOrDefault();
            Setting shopinformation = MainDb.Settings.Where(s => s.Key == "Shopinformation").FirstOrDefault();
            Setting shopplace = MainDb.Settings.Where(s => s.Key == "Shopplace").FirstOrDefault();

            if (shopdescription != null)
                ShopdescriptionTb.Text = shopdescription.Value;
            if (shopinformation != null)
                ShopinfoTb.Text = shopinformation.Value;
            if (shopplace != null)
                PlaceTb.Text = shopplace.Value;

            #endregion

            #region Bon-Footer - Load values from db

            Setting bonFooter = MainDb.Settings.Where(s => s.Key == "Bon-Footer").FirstOrDefault();

            if (bonFooter != null)
                bonFooterTb.Text = bonFooter.Value;

            #endregion
        }

        #endregion

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DefaultPickUpTb.Value != null)
            {
                Setting defaultPickUp = MainDb.Settings.Where(s => s.Key == "DefaultPickUp").SingleOrDefault();

                if (defaultPickUp == null)
                {
                    MainDb.Settings.Add(new Setting
                    {
                        Key = "DefaultPickUp",
                        Value = DefaultPickUpTb.Value.ToString()
                    });
                }
                else
                {
                    defaultPickUp.Value = DefaultPickUpTb.Value.ToString();
                }
            }

            #region Drucker

            Data.StandardPrinters.BonPrinter = BonPrinterCB.SelectedValue as string;
            Data.StandardPrinters.DocumentPrinter = DocumentPrinterCB.SelectedValue as string;
            Data.StandardPrinters.TagPrinter = TagPrinterCB.SelectedValue as string;
            Data.StandardPrinters.TagLandscapePrinter = TagLandscapePrinterCB.SelectedValue as string;

            Data.SaveToJSON();

            #endregion

            #region Dokumentenkopf

            Setting shopdescription = MainDb.Settings.Where(s => s.Key == "Shopdescription").FirstOrDefault();
            Setting shopinformation = MainDb.Settings.Where(s => s.Key == "Shopinformation").FirstOrDefault();
            Setting shopplace = MainDb.Settings.Where(s => s.Key == "Shopplace").FirstOrDefault();

            if (shopdescription == null)
            {
                MainDb.Settings.Add(new Setting
                {
                    Key = "Shopdescription",
                    Value = ShopdescriptionTb.Text
                });
            } else
            {
                shopdescription.Value = ShopdescriptionTb.Text;
            }

            if (shopinformation == null)
            {
                MainDb.Settings.Add(new Setting
                {
                    Key = "Shopinformation",
                    Value = ShopinfoTb.Text
                });
            }
            else
            {
                shopinformation.Value = ShopinfoTb.Text;
            }

            if (shopplace == null)
            {
                MainDb.Settings.Add(new Setting
                {
                    Key = "Shopplace",
                    Value = PlaceTb.Text
                });
            }
            else
            {
                shopplace.Value = PlaceTb.Text;
            }

            #endregion

            #region Save Bon-Footer

            Setting bonFooter = MainDb.Settings.Where(s => s.Key == "Bon-Footer").FirstOrDefault();

            if (bonFooter == null)
                MainDb.Settings.Add(new Setting
                {
                    Key = "Bon-Footer",
                    Value = bonFooterTb.Text
                });
            else
                bonFooter.Value = bonFooterTb.Text;

            #endregion

            Close();
            MainDb.SaveChanges();
            MainDb.Dispose();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
            MainDb.Dispose();
        }

        private void DefaultPickUpTb_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (DefaultPickUpTb.Value == null)
            {
                DefaultPickUpTb.Foreground = Brushes.Red;
            }
            else
            {
                DefaultPickUpTb.Foreground = Brushes.Black;
            }
        }
    }
}
