using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Warenwirtschaftssystem.Model;
using Warenwirtschaftssystem.Model.Db;
using Warenwirtschaftssystem.Model.LocalAppData;
using Warenwirtschaftssystem.UI.Windows;

namespace Warenwirtschaftssystem.UI.Pages
{
    public partial class ConfigureDbPage : Page
    {
        // Attribute
        private Window OwnerWindow;
        private DataModel Data;

        // Konstruktor
        public ConfigureDbPage(Window ownerWindow, DataModel data)
        {
            Data = data;
            OwnerWindow = ownerWindow;
            OwnerWindow.Title = "Datenbank einrichten";
            InitializeComponent();

            AddressTb.Focus();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveAndConnect();
        }

        private void OnKeyDown_SaveAndConnect(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SaveAndConnect();
                e.Handled = true;
            }
        }

        private void SaveAndConnect()
        {
            string address = AddressTb.Text;
            string dbName = DbNameTb.Text;
            string username = UserTb.Text;
            string pw = PasswordTb.Password;

            Server server = null;

            if (!string.IsNullOrWhiteSpace(address) && !string.IsNullOrWhiteSpace(dbName) && !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(pw))
            {
                //Neue Datenbank erstellen
                if (NewDbRB.IsChecked.GetValueOrDefault(false))
                {
                    #region User & Db anlegen

                    try
                    {
                        server = new Server(new ServerConnection(address));

                        #region SQL User Authentication falls nötig aktivieren

                        if (server.LoginMode != ServerLoginMode.Mixed)
                        {
                            server.LoginMode = ServerLoginMode.Mixed;
                            server.Alter();

                            Process p = new Process();
                            p.StartInfo.FileName = "cmd.exe";
                            p.StartInfo.Verb = "runas";
                            p.StartInfo.Arguments = "/C net stop MSSQL$SQLEXPRESS2014 && net start MSSQL$SQLEXPRESS2014";
                            p.Start();
                            p.WaitForExit();
                        }

                        #endregion

                    }
                    catch
                    {
                        MessageBox.Show("Datenbankserver über die angegebene Adresse nicht erreichbar", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    Database existingDb = server.Databases[dbName];
                    Login existingLogin = server.Logins[username];
                    bool dbExists = existingDb != null;
                    bool loginExists = existingLogin != null;

                    if (dbExists || loginExists)
                    {
                        MessageBoxResult result = MessageBox.Show("Datenbank und/oder Login existieren bereits. Beide löschen und neu erstellen?", "Warnung", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (result != MessageBoxResult.Yes)
                            return;
                        else
                        {
                            if (dbExists)
                            {
                                server.KillAllProcesses(existingDb.Name);
                                existingDb.Drop();
                            }
                            if (loginExists)
                                existingLogin.Drop();
                        }
                    }

                    Database db;
                    try
                    {
                        db = new Database(server, dbName)
                        {
                            RecoveryModel = RecoveryModel.Full,
                            Collation = "Latin1_General_CS_AS"
                        };
                        db.Create();

                    }
                    catch
                    {
                        MessageBox.Show("Datenbank konnte nicht angelegt werden", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    try
                    {
                        Login login = new Login(server, username)
                        {
                            LoginType = LoginType.SqlLogin,
                            DefaultDatabase = dbName
                        };
                        login.Create(pw);
                        login.Enable();

                        User user = new User(db, username)
                        {
                            Login = login.Name
                        };
                        user.Create();

                        DatabasePermissionSet permissionSet = new DatabasePermissionSet(new DatabasePermission[]
                        {
                            DatabasePermission.Alter,
                            DatabasePermission.BackupDatabase,
                            DatabasePermission.Connect,
                            DatabasePermission.ConnectReplication,
                            DatabasePermission.CreateSchema,
                            DatabasePermission.CreateTable,
                            DatabasePermission.CreateQueue,
                            DatabasePermission.CreateView,
                            DatabasePermission.Delete,
                            DatabasePermission.Execute,
                            DatabasePermission.Insert,
                            DatabasePermission.Select,
                            DatabasePermission.Update,
                            DatabasePermission.ViewDatabaseState,
                            DatabasePermission.CreateType,
                            DatabasePermission.References
                        });

                        DatabaseRole role = new DatabaseRole(db, "WWSRole");
                        role.Create();

                        db.Grant(permissionSet, "WWSRole");
                        role.AddMember(username);
                    }
                    catch
                    {
                        MessageBox.Show("User und/oder Login konnten nicht angelegt werden", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    #endregion
                }

                #region ConnectionInfo in JSON speichern

                Data.ConnectionInfo = new ConnectionInfo
                {
                    Address = address,
                    DbName = dbName,
                    Username = username
                };

                Data.SaveToJSON();

                #endregion

                #region ConnectionString erzeugen und Verbindung testen

                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder()
                {
                    DataSource = address,
                    InitialCatalog = dbName,
                    UserID = username,
                    Password = pw
                };
                Data.MainConnectionString = builder.ConnectionString;


                Model.Db.DbModel mainDb = null;

                try
                {
                    mainDb = new Model.Db.DbModel(Data.MainConnectionString);

                    //Datenbank auf den neusten Stand bringen
                    var configuration = new Migrations.Configuration
                    {
                        TargetDatabase = new DbConnectionInfo(Data.MainConnectionString, "System.Data.SqlClient")
                    };
                    var migrator = new DbMigrator(configuration);
                    migrator.Update();

                    //Verbindung testen

                    mainDb.Settings.Add(new Setting
                    {
                        Key = "ConnectionTest",
                        Value = ""
                    });

                    mainDb.SaveChanges();
                    mainDb.Settings.Remove(mainDb.Settings.Where(s => s.Key == "ConnectionTest").Single());
                    mainDb.SaveChanges();
                }
                catch (Exception e)
                {
                    if (mainDb != null)
                        mainDb.Dispose();
                    MessageBox.Show(e.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);

                    return;
                }

                #endregion

                #region EasyToRunDb importieren

                if (ImportFromEasyToRunCbx.IsVisible && ImportFromEasyToRunCbx.IsChecked.GetValueOrDefault(false))
                {
                    EasyToRunDb easyToRunDb = new EasyToRunDb(Data);

                    if (ReworkAttributesCbx.IsVisible && ReworkAttributesCbx.IsChecked.GetValueOrDefault(false))
                    {
                        string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Warenwirtschaftssystem\\Überarbeitung Artikelattribute";

                        if (Directory.Exists(dirPath) && Directory.GetFiles(dirPath).Length > 0)
                        {
                            MessageBox.Show("Der Ordner für die Überarbeitung der Artikelattribute enthält bereits Dateien. Wenn Sie diese wiederverwenden wollen, müssen diese bevor Sie diesen Dialog bestätigen woanders gespeichert werden.", "Dateien in Überarbeitungsverzeichnis vorhanden", MessageBoxButton.OK, MessageBoxImage.Warning);
                            foreach (FileInfo file in new DirectoryInfo(dirPath).GetFiles())
                            {
                                file.Delete();
                            }
                        }

                        easyToRunDb.CreateAttributeCSVs();

                        Process.Start(dirPath);

                        MessageBoxResult result = MessageBox.Show("Bearbeiten Sie nun die CSV-Dateien in dem geöffneten Ordner. Sobald die Daten vom Programm verarbeitet werden können bestätigen Sie diesen Dialog. Fortfahren?", "Fortfahren?", MessageBoxButton.OKCancel, MessageBoxImage.Information, MessageBoxResult.OK);

                        if (result != MessageBoxResult.OK)
                        {
                            Application.Current.Shutdown();

                            return;
                        }

                        easyToRunDb.ImportData(true);
                    }
                    else easyToRunDb.ImportData(false);

                    easyToRunDb.Dispose();

                    GC.Collect();
                    GC.WaitForFullGCComplete();
                }

                #endregion

                //RootWindow starten;
                new RootWindow(Data).Show();
                OwnerWindow.Close();

            }
        }
    }
}
