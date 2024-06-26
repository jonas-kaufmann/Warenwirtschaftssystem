﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Warenwirtschaftssystem.Model;
using Warenwirtschaftssystem.UI.Pages;
using Microsoft.Win32;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using System;
using System.Collections.Specialized;
using System.IO;

namespace Warenwirtschaftssystem.UI.Windows
{
    public partial class RootWindow : Window
    {
        // Attribute
        private List<Window> ToolWindowList;
        private DataModel Data;
        public List<Window> alreadyClosingWindows = new List<Window>();

        // Konstruktor
        public RootWindow(DataModel data)
        {
            Data = data;
            ToolWindowList = new List<Window>();
            InitializeComponent();
            TopbarFrame.Content = new TopbarPage(data, this); // Topbar laden
            Show();
        }

        // Methoden
        protected override void OnClosing(CancelEventArgs e)
        {
            // Toolfenster schließen, falls Benutzer Schließen eines Toolfensters abbricht, das Schließen des Rootfenster abbrechen
            for (int i = 0; i < ToolWindowList.Count; i++)
            {
                Window windowToClose = ToolWindowList[i];
                if (alreadyClosingWindows.Contains(windowToClose))
                    windowToClose.Focus();
                else
                    windowToClose.Close();
            }

            if (ToolWindowList.Count > 0)
            {
                e.Cancel = true;
                return;
            }

            // Datenbanksicherung abfragen
            MessageBoxResult result = MessageBox.Show("Eine Sicherung kann nur auf dem Computer, welcher den Datenbankserver hostet, ausgeführt werden. Datenbank jetzt sichern?", "Datenbank sichern?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    SaveFileDialog sfd = new SaveFileDialog
                    {
                        Title = "Datenbankbackup speichern",
                        Filter = "BAK-Datei|*.bak",
                        CheckFileExists = false,
                        CheckPathExists = true,
                        OverwritePrompt = true,
                        AddExtension = true,
                        ValidateNames = true
                    };

                    bool? resultSFD = sfd.ShowDialog();

                    if (resultSFD.GetValueOrDefault(false))
                    {
                        #region Sicherung anlegen

                        try
                        {
                            //Backupdatei soll überschrieben und nicht erweitert werden
                            if (File.Exists(sfd.FileName))
                                File.Delete(sfd.FileName);

                            Server server = new Server(new ServerConnection(Data.ConnectionInfo.Address));

                            Database db = server.Databases[Data.ConnectionInfo.DbName];

                            Backup backup = new Backup
                            {
                                Action = BackupActionType.Database,
                                BackupSetDescription = "Full backup of " + Data.ConnectionInfo.DbName + ", " + DateTime.Now.ToShortDateString(),
                                BackupSetName = "Warenwirtschaftssystem Backup",
                                Database = Data.ConnectionInfo.DbName,
                                Incremental = false,
                                LogTruncation = BackupTruncateLogType.Truncate
                            };

                            BackupDeviceItem backupDevice = new BackupDeviceItem(sfd.FileName, DeviceType.File);
                            backup.Devices.Add(backupDevice);

                            backup.SqlBackup(server);
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(exception.ToString(), "Fehler beim Speichern des Backups", MessageBoxButton.OK, MessageBoxImage.Error);
                            e.Cancel = true;
                            return;
                        }

                        #endregion
                    }
                    else
                    {
                        e.Cancel = true;
                        return;
                    }

                    break;
                case MessageBoxResult.Cancel:
                    // Schließen des Fensters abbrechen
                    e.Cancel = true;
                    return;
            }
        }

        public void AddToolWindow(Window toolWindow)
        {
            ToolWindowList.Add(toolWindow);
        }

        public void RemoveToolWindow(Window toolWindow)
        {
            ToolWindowList.Remove(toolWindow);
        }

        private void SettingsMI_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow(Data).ShowDialog();
        }

        private void RestoreDbMI_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult mbr = MessageBox.Show("Alle Fenster müssen geschlossen sein. Die Wiederherstellung kann nur auf dem PC, welcher den Datenbankserver hostet ausgeführt werden. Bei der Wiederherstellung wird die aktuelle Datenbank gelöscht und durch das Backup ersetzt. Fortfahren?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (mbr == MessageBoxResult.Yes)
            {
                OpenFileDialog oFD = new OpenFileDialog
                {
                    Title = "Datenbankbackup laden",
                    Filter = "BAK-Datei|*.bak",
                    CheckFileExists = true,
                    CheckPathExists = true,
                    Multiselect = false
                };

                bool? result = oFD.ShowDialog();

                if (result.GetValueOrDefault(false))
                {
                    try
                    {
                        ServerConnection conn = new ServerConnection(Data.ConnectionInfo.Address);
                        Server server = new Server(conn);

                        Restore restore = new Restore();
                        restore.Devices.AddDevice(oFD.FileName, DeviceType.File);

                        RelocateFile DataFile = new RelocateFile();
                        string MDF = restore.ReadFileList(server).Rows[0][1].ToString();
                        DataFile.LogicalFileName = restore.ReadFileList(server).Rows[0][0].ToString();
                        DataFile.PhysicalFileName = server.Databases[Data.ConnectionInfo.DbName].FileGroups[0].Files[0].FileName;

                        RelocateFile LogFile = new RelocateFile();
                        string LDF = restore.ReadFileList(server).Rows[1][1].ToString();
                        LogFile.LogicalFileName = restore.ReadFileList(server).Rows[1][0].ToString();
                        LogFile.PhysicalFileName = server.Databases[Data.ConnectionInfo.DbName].LogFiles[0].FileName;

                        restore.RelocateFiles.Add(DataFile);
                        restore.RelocateFiles.Add(LogFile);

                        restore.Database = Data.ConnectionInfo.DbName;
                        restore.NoRecovery = false;
                        restore.ReplaceDatabase = true;

                        server.KillAllProcesses(Data.ConnectionInfo.DbName);
                        restore.SqlRestore(server);


                        #region Login mit User verknüpfen
                        Database database = server.Databases[Data.ConnectionInfo.DbName];

                        // delete old users
                        foreach (User u in database.Users)
                        {
                            StringCollection roles = u.EnumRoles();
                            if (roles != null && roles.Count > 0 && roles.Contains("WWSRole"))
                            {
                                u.Drop();
                                break;
                            }
                        }

                        User user = new User(database, Data.ConnectionInfo.Username)
                        {
                            Login = Data.ConnectionInfo.Username
                        };
                        user.Create();
                        database.Roles["WWSRole"].AddMember(Data.ConnectionInfo.Username);
                        #endregion

                        conn.Disconnect();
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.ToString(), "Fehler beim Wiederherstellen des Backups", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    MessageBox.Show("Das Backup wurde erfolgreich geladen", "Erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    return;
                }

            }
            else
                return;
        }
    }
}