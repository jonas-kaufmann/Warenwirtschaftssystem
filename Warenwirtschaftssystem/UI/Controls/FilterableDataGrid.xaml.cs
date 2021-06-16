using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Warenwirtschaftssystem.UI.Controls
{
    public partial class FilterableDataGrid : Grid
    {
        public DataGrid DataGrid { get; set; }
        public bool AskForConfirmationAfterEdit { get; set; } = false;
        public bool DisableEnteringEditThroughTyping { get; set; } = false;

        private CollectionViewSource CollectionViewSource;
        private bool Disabled = false;

        public FilterableDataGrid()
        {
            InitializeComponent();
            FilterTextBox.Visibility = Visibility.Collapsed;
        }


        #region Grid
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            DataGrid.BeginningEdit += DataGrid_BeginningEdit;
            DataGrid.CellEditEnding += DataGrid_CellEditEnding;
            DataGrid.RowEditEnding += DataGrid_RowEditEnding;

            ContentPresenter.Content = DataGrid;
            CollectionViewSource = (CollectionViewSource)DataGrid.DataContext;

            RememberSelectedItems();
            CollectionViewSource.Filter += CollectionViewSource_Filter;
            RestoreSelectedItems();
        }
        #endregion

        private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Disabled)
                return;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.F)
            {
                if (!FilterTextBox.IsFocused)
                {
                    FilterTextBox.Visibility = Visibility.Visible;
                    FilterTextBox.Focus();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Escape)
            {
                if (FilterTextBox.Visibility == Visibility.Visible)
                {
                    ClearFilter();
                    e.Handled = true;
                }
            } else if (!FilterTextBox.IsFocused && DisableEnteringEditThroughTyping)
            {
                e.Handled = true;
            }
        }


        #region CollectionViewSource
        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            e.Accepted = true;

            if (DataGrid.SelectedItems.Contains(e.Item))
                return;

            if (!string.IsNullOrWhiteSpace(FilterTextBox.Text))
            {
                e.Accepted = e.Item.ToString().StartsWith(FilterTextBox.Text.Trim(), StringComparison.CurrentCultureIgnoreCase);
            }
        }
        #endregion


        #region FilterTextBox
        private void FilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void FilterTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FilterTextBox.Text))
            {
                ClearFilter();
            }
        }
        #endregion


        #region DataGrid
        private void DataGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Cancel)
                return;

            Disabled = true;
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Cancel)
                return;

            Disabled = false;
        }

        private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.Cancel)
                return;

            Disabled = false;

            // ask for confirmation
            if (AskForConfirmationAfterEdit && e.EditAction != DataGridEditAction.Cancel)
            {
                MessageBoxResult result = MessageBox.Show("Sollen die Änderungen an den Merkmalen gespeichert werden?", "Merkmale wurden geändert", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);

                if (result == MessageBoxResult.No)
                {
                    (sender as DataGrid).CancelEdit();
                }
            }
        }
        #endregion


        private void ApplyFilter()
        {
            if (!Disabled)
            {
                RememberSelectedItems();
                CollectionViewSource.View.Refresh();
                RestoreSelectedItems();
            }
        }

        private void ClearFilter()
        {
            FilterTextBox.Clear();
            FilterTextBox.Visibility = Visibility.Collapsed;
            DataGrid.Focus();

            if (DataGrid.SelectedItem != null)
                DataGrid.ScrollIntoView(DataGrid.SelectedItem);
        }

        private object[] SelectedItems = null;

        private void RememberSelectedItems()
        {
            SelectedItems = new object[Math.Max(1, DataGrid.SelectedItems.Count)];
            if (DataGrid.SelectedItems.Count >= 1)
            {
                
                DataGrid.SelectedItems.CopyTo(SelectedItems, 0);
            }
            else
            {
                SelectedItems[0] = null;
            }
        }

        private void RestoreSelectedItems()
        {
            if (SelectedItems.Length == 1)
            {
                if (SelectedItems[0] == null)
                    DataGrid.SelectedItem = null;
                else
                    DataGrid.SelectedItem = SelectedItems[0];
            } else
            {
                DataGrid.SelectedItem = SelectedItems[0];

                foreach (var item in SelectedItems[1..])
                {
                    if (!DataGrid.SelectedItems.Contains(item))
                        DataGrid.SelectedItems.Add(item);
                }
            }

            SelectedItems = null;
        }
    }
}
