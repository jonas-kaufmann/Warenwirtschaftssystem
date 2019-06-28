using System.Windows;
using Warenwirtschaftssystem.UI.Pages;
using Warenwirtschaftssystem.Model;

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
    }
}
