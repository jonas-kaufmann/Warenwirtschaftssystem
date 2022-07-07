using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Warenwirtschaftssystem.Model.Db;
using System.Linq;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;

namespace Warenwirtschaftssystem.UI.Pages
{
    public partial class ReservedArticlesPage : Page
    {
        private DbModel MainDb;
        private CollectionViewSource ArticlesCVS;
        private Window OwnerWindow;

        public ReservedArticlesPage(Window ownerWindow, DbModel dbModel)
        {
            InitializeComponent();

            OwnerWindow = ownerWindow;
            OwnerWindow.Title = "Übersicht Reservierungen";
            MainDb = dbModel;
            ArticlesCVS = FindResource("ArticlesCVS") as CollectionViewSource;

            MainDb.Articles.Where(a => a.Status == Status.Reserved).Load();
            ArticlesCVS.Source = new ObservableCollection<Article>(MainDb.Articles.Where(a => a.Status == Status.Reserved).ToList());
        }

        private void ArticlesCVS_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is Article article)
            {
                if (int.TryParse(SupplierIdTb.Text, out int id) && article.ReservingSupplier.Id != id)
                {
                    e.Accepted = false;
                    return;
                }
                if (ShowReservationExpired.IsChecked.GetValueOrDefault(false) && article.ReservedUntil.HasValue && article.ReservedUntil > DateTime.Now)
                {
                    e.Accepted = false;
                    return;
                }
            }
            else e.Accepted = false;
        }

        private void SupplierIdTb_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (int.TryParse(SupplierIdTb.Text, out int id))
                    ArticlesCVS.Filter += ArticlesCVS_Filter;
                else ArticlesCVS.Filter -= ArticlesCVS_Filter;
                e.Handled = true;
            }
        }

        private void ResetFilterBtn_Click(object sender, RoutedEventArgs e)
        {
            ArticlesCVS.Filter -= ArticlesCVS_Filter;
            ShowReservationExpired.IsChecked = false;
            SupplierIdTb.Text = "";
        }

        private void UnreserveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ArticlesDG.SelectedItems.Count > 0)
            {
                List<Article> selectedItems = new List<Article>();
                foreach (Article article in ArticlesDG.SelectedItems)
                    selectedItems.Add(article);

                foreach (Article article in selectedItems)
                {
                    article.Status = Status.Sortiment;
                    article.ReservedFrom = null;
                    article.ReservedUntil = null;
                    article.ReservingSupplier = null;

                    (ArticlesCVS.Source as ObservableCollection<Article>).Remove(article);
                }
            }
        }

        public void OwnerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainDb.SaveChangesRetryOnUserInput();
        }

        private void ShowReservationExpired_Click(object sender, RoutedEventArgs e)
        {
            ArticlesCVS.Filter += ArticlesCVS_Filter;
        }

    }
}
