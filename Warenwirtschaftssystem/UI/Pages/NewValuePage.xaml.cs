using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Warenwirtschaftssystem.UI.Pages
{
    public partial class NewValuePage : Page
    {
        private Window OwnerWindow;
        public string NewValue;

        public NewValuePage(Window ownerWindow)
        {
            OwnerWindow = ownerWindow;
            OwnerWindow.Title = "Neuer Wert";

            InitializeComponent();
        }

        public NewValuePage(Window ownerWindow, string oldValue)
        {
            OwnerWindow = ownerWindow;
            OwnerWindow.Title = "Neuer Wert";

            InitializeComponent();
            NewValueTb.Text = oldValue;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            NewValue = NewValueTb.Text;
            OwnerWindow.Close();
        }

        private void AbortBtn_Click(object sender, RoutedEventArgs e)
        {
            OwnerWindow.Close();
        }

        private void NewValueTb_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SaveBtn_Click(null, null);
                e.Handled = true;
            }

        }
    }
}
