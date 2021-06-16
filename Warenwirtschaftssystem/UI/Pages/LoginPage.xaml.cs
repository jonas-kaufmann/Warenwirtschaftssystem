using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private void OnKeyUp_Connect(object sender, KeyEventArgs e)
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
                    MainDb = Data.CreateDbConnection();

                    //Datenbank auf den neusten Stand bringen
                    MainDb.Database.Migrate();

                    //Verbindung testen
                    MainDb.Settings.Add(new Setting
                    {
                        Key = "ConnectionTest",
                        Value = ""
                    });
                    MainDb.SaveChanges();
                    MainDb.Settings.Remove(MainDb.Settings.Where(s => s.Key == "ConnectionTest").Single());
                    MainDb.SaveChanges();
                }
                catch (Exception e)
                {
                    if (MainDb != null)
                        MainDb.Dispose();

                    MessageBox.Show(e.ToString(), "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
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
