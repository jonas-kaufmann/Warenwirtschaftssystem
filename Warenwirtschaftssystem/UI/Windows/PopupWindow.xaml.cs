using System.Windows;
using Warenwirtschaftssystem.Model;

namespace Warenwirtschaftssystem.UI.Windows
{
    public partial class PopupWindow : Window
    {
        // Attribute
        private DataModel Data;

        // Konstruktor
        public PopupWindow(Window ownerWindow, DataModel data)
        {
            Owner = ownerWindow;
            Data = data;
        }

        public void Initialize()
        {
            InitializeComponent();
        }
    }
}
