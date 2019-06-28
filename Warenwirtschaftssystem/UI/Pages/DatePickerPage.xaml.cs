using System;
using System.Windows;
using System.Windows.Controls;

namespace Warenwirtschaftssystem.UI.Pages
{
    public partial class DatePickerPage : Page
    {
        public DateTime? SelectedDate;

        private Window OwnerWindow;

        public DatePickerPage(Window ownerWindow, DateTime? dateSet)
        {
            OwnerWindow = ownerWindow;

            InitializeComponent();

            if (dateSet.HasValue)
                MainDatePicker.SelectedDate = dateSet;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MainDatePicker.SelectedDate.HasValue)
            {
                SelectedDate = MainDatePicker.SelectedDate.Value;
                OwnerWindow.Close();
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            OwnerWindow.Close();
        }
    }
}
