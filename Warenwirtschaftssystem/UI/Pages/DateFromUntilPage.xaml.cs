using System;
using System.Windows;
using System.Windows.Controls;

namespace Warenwirtschaftssystem.UI.Pages
{
    public partial class DateFromUntilPage : Page
    {
        public bool Result { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateUntil { get; set; }

        private Window OwnerWindow;

        public DateFromUntilPage(Window ownerWindow)
        {
            OwnerWindow = ownerWindow;

            InitializeComponent();
            DP1.SelectedDate = DateTime.Today.Date;
            DP2.SelectedDate = DateTime.Today.Date.AddDays(3);
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DP1.SelectedDate != null && DP1.SelectedDate != null && DP1.SelectedDate.Value <= DP2.SelectedDate.Value)
            {
                Result = true;
                DateFrom = DP1.SelectedDate.Value;
                DateUntil = DP2.SelectedDate.Value;

                OwnerWindow.Close();
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            OwnerWindow.Close();
        }
    }
}
