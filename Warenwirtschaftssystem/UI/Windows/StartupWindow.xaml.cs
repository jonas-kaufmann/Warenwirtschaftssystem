using System.Windows;
using Warenwirtschaftssystem.UI.Pages;
using Warenwirtschaftssystem.Model;
using AutoUpdaterDotNET;
using System;

namespace Warenwirtschaftssystem.UI.Windows
{
    public partial class StartupWindow : Window
    {
        private DataModel Data;

        // Konstruktor
        public StartupWindow()
        {
            Data = new DataModel();

            if (Data.ConnectionInfo != null)
            {
                Content = new LoginPage(this, Data);
            }
            else
            {
                Content = new ConfigureDbPage(this, Data);
            }

            InitializeComponent();
            Show();
        }

        // Check for App Updates
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // AutoUpdater
            AutoUpdater.RunUpdateAsAdmin = true;
            AutoUpdater.Start("https://wws.mokka-mode.de/AutoUpdater.xml");
        }
    }
}
