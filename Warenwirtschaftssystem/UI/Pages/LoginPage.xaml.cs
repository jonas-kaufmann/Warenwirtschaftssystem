using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Warenwirtschaftssystem.Migrations;
using Warenwirtschaftssystem.Model;
using Warenwirtschaftssystem.Model.Db;
using Warenwirtschaftssystem.UI.Windows;

namespace Warenwirtschaftssystem.UI.Pages
{
    public partial class LoginPage : Page
    {
        private Window OwnerWindow;
        private DataModel Data;

        public LoginPage(Window ownerWindow, DataModel data)
        {
            Data = data;
            OwnerWindow = ownerWindow;
            OwnerWindow.Title = "Login";
            InitializeComponent();

            UserTb.Focus();
        }

        private void ConnectBtn_Click(object sender, RoutedEventArgs e)
        {
            Connect();
        }

        private void ChangeDbButton_Click(object sender, RoutedEventArgs e)
        {
            OwnerWindow.Content = new ConfigureDbPage(OwnerWindow, Data);
        }

        private void OnKeyDown_Connect(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Connect();
                e.Handled = true;
            }
        }

        private void Connect()
        {
            string user = UserTb.Text;
            string pw = PasswordTb.Password;

            if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pw))
            {
                #region ConnectionString erzeugen und Verbindung testen

                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder()
                {
                    DataSource = Data.ConnectionInfo.Address,
                    InitialCatalog = Data.ConnectionInfo.DbName,
                    UserID = user,
                    Password = pw
                };
                Data.MainConnectionString = builder.ConnectionString;

                DbModel MainDb = null;

                try
                {
                    //Datenbank auf den neusten Stand bringen
                    Configuration configuration = new Configuration(Data.MainConnectionString, "System.Data.SqlClient");
                    var migrator = new DbMigrator(configuration);
                    migrator.Update();

                    //Verbindung testen
                    MainDb = new DbModel(Data.MainConnectionString);
                    MainDb.Settings.Add(new Setting
                    {
                        Key = "ConnectionTest",
                        Value = ""
                    });
                    MainDb.SaveChanges();
                    MainDb.Settings.Remove(MainDb.Settings.Where(s => s.Key == "ConnectionTest").Single());
                    MainDb.SaveChanges();
                }
                catch
                {
                    if (MainDb != null)
                        MainDb.Dispose();

                    MessageBox.Show("Fehler beim Herstellen einer Verbindung zur Datenbank.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                #endregion

                //RootWindow starten
                new RootWindow(Data).Show();
                OwnerWindow.Close();
            }
        }
    }
}
