using System.Windows;
using System.Windows.Input;

namespace Warenwirtschaftssystem.UI.Windows
{
    public enum PressedButton
    {
        None, First, Second, Third
    };

    public partial class CustomMessageBox : Window
    {

        public PressedButton Result = PressedButton.None;


        public CustomMessageBox(Window ownerWindow, string title, string content, string textButtonOne, string textButtonTwo, string textButtonThree)
        {
            Owner = ownerWindow;

            InitializeComponent();

            Title = title;
            ContentTxt.Text = content;
            BtnOne.Content = textButtonOne + " [1]";
            BtnTwo.Content = textButtonTwo + " [2]";
            BtnThree.Content = textButtonThree + " [3]";
        }

        private void BtnOne_Click(object sender, RoutedEventArgs e)
        {
            Result = PressedButton.First;
            Close();
        }

        private void BtnTwo_Click(object sender, RoutedEventArgs e)
        {
            Result = PressedButton.Second;
            Close();
        }

        private void BtnThree_Click(object sender, RoutedEventArgs e)
        {
            Result = PressedButton.Third;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(BtnOne);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                Close();
            }
            else if (e.Key == Key.D1 || e.Key == Key.NumPad1)
            {
                BtnOne_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.D2 || e.Key == Key.NumPad2)
            {
                BtnTwo_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.D3 || e.Key == Key.NumPad3)
            {
                BtnThree_Click(null, null);
                e.Handled = true;
            }
        }
    }
}
