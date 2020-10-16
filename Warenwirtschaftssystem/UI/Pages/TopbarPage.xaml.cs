using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Warenwirtschaftssystem.Model;
using Warenwirtschaftssystem.UI.Windows;
using Xceed.Wpf.Toolkit;

namespace Warenwirtschaftssystem.UI.Pages
{
    public partial class TopbarPage : Page
    {
        // Attribute
        private DataModel Data;
        private RootWindow RootWindow;

        // Konstruktor
        public TopbarPage(DataModel data, RootWindow rootWindow)
        {
            Data = data;
            RootWindow = rootWindow;
            InitializeComponent();
        }

        // Methoden
        private void MenuBtn_Click(object sender, RoutedEventArgs e)
        {
            string senderName = ((IconButton)sender).Name;
            if (senderName == "CalculatorBtn")
            {
                // Rechner starten
                Process.Start("calc.exe");
            }
            else
            {
                Window toolWindow = null;
                switch (senderName)
                {
                    case "ArticlesBtn":
                        toolWindow = new ToolWindow(RootWindow, Data);

                        toolWindow.MinWidth = 1200;
                        toolWindow.Width = 1200;
                        toolWindow.MinHeight = 800;
                        toolWindow.Height = 800;

                        (toolWindow as ToolWindow).Content = new ArticlePage(Data, toolWindow as ToolWindow);
                        toolWindow.Title = "Artikel";
                        break;
                    case "SuppliersBtn":
                        toolWindow = new ToolWindow(RootWindow, Data);

                        toolWindow.MinWidth = 1040;
                        toolWindow.Width = 1040;
                        toolWindow.MinHeight = 600;
                        toolWindow.Height = 600;

                        (toolWindow as ToolWindow).Content = new SuppliersPage(Data, toolWindow as ToolWindow);
                        toolWindow.Title = "Lieferanten";
                        break;
                    case "SellingBtn":
                        toolWindow = new ToolWindowWithoutWarning(RootWindow, Data);

                        toolWindow.MinWidth = 800;
                        toolWindow.Width = 800;
                        toolWindow.MinHeight = 600;
                        toolWindow.Height = 600;

                        (toolWindow as ToolWindowWithoutWarning).Content = new CommercePage(toolWindow, Data);
                        toolWindow.Title = "Verkauf, Reservierung, Retoure";
                        break;
                    case "StatisticsBtn":
                        toolWindow = new ToolWindowWithoutWarning(RootWindow, Data);

                        toolWindow.MinWidth = 800;
                        toolWindow.Width = 800;
                        toolWindow.MinHeight = 600;
                        toolWindow.Height = 600;

                        (toolWindow as ToolWindowWithoutWarning).Content = new StatisticsPage(toolWindow, Data);
                        toolWindow.Title = "Statistiken";
                        break;
                    case "DocumentsBtn":
                        toolWindow = new ToolWindowWithoutWarning(RootWindow, Data);

                        toolWindow.MinWidth = 800;
                        toolWindow.Width = 800;
                        toolWindow.MinHeight = 600;
                        toolWindow.Height = 600;

                        (toolWindow as ToolWindowWithoutWarning).Content = new ReprintDocuments(toolWindow, Data);
                        toolWindow.Title = "Belege";
                        break;
                }
                RootWindow.AddToolWindow(toolWindow);
                toolWindow.Show();
            }
        }
    }
}
