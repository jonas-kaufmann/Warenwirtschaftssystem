using System.Windows;
using System.Windows.Controls;
using Warenwirtschaftssystem.Model;
using Warenwirtschaftssystem.UI.Pages;

namespace Warenwirtschaftssystem.UI.Windows
{
    public partial class ToolWindowWithoutWarning : Window
    {
        //Attribute
        private RootWindow RootWindow;

        // Konstruktor
        public ToolWindowWithoutWarning(RootWindow rootWindow, DataModel data)
        {
            RootWindow = rootWindow;
            Owner = RootWindow;
            InitializeComponent();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            RootWindow.RemoveToolWindow(this);
        }
    }
}
