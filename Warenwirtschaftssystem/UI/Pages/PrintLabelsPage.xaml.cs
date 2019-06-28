using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Warenwirtschaftssystem.Model;
using Warenwirtschaftssystem.Model.Db;
using System.Linq;
using Label = Warenwirtschaftssystem.Model.Documents.Label;
using Warenwirtschaftssystem.Model.Documents;

namespace Warenwirtschaftssystem.UI.Pages
{
    public partial class PrintLabelsPage : Page
    {
        private DataModel Data;
		private Window OwnerWindow;
		private List<LabelArticle> ShownArticles;
        private List<Article> Articles;


        public PrintLabelsPage(DataModel data, Window ownerWindow, List<Article> articles)
        {
            Data = data;
            OwnerWindow = ownerWindow;
            Articles = articles;

            InitializeComponent();

            ShownArticles = new List<LabelArticle>();
            foreach (Article article in Articles)
            {
                LabelArticle labelArticle = new LabelArticle
                {
                    Id = article.Id,
                    ConvertedId = article.ConvertedId.ToString(),
                    Description = article.Description,
                    SupplierId = article.Supplier.Id,
                    SupplierName = article.Supplier.Name,
                    Format = Format.Normal,
                    Price = article.Price
                };
                labelArticle.PropertyChanged += LabelArticle_PropertyChanged;

                ShownArticles.Add(labelArticle);
            }

			LabelsDG.DataContext = ShownArticles;
        }

        private void LabelArticle_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateLabelsDGColumWidth();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
			OwnerWindow.Close();
        }

        private void MakeNormalBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (LabelArticle article in LabelsDG.SelectedItems)
            {
                article.Format = Format.Normal;
            }
            UpdateLabelsDGColumWidth();
        }

        private void MakeLandscapeBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (LabelArticle article in LabelsDG.SelectedItems)
            {
                article.Format = Format.Landscape;
            }
            UpdateLabelsDGColumWidth();
        }

        private void UpdateLabelsDGColumWidth ()
        {
            LabelsDG.Columns[1].Width = 0;
            LabelsDG.UpdateLayout();
            LabelsDG.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
        }

        private void LabelsDG_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LabelsDG.SelectedItem is LabelArticle article)
            {
                if (article.Format == Format.Normal)
                    article.Format = Format.Landscape;
                else article.Format = Format.Normal;
            }
        }

        private void PrintLabelsBtn_Click(object sender, RoutedEventArgs e)
        {
            List<Article> labels2Print = new List<Article>();
            foreach (LabelArticle article in ShownArticles)
            {
                if (article.Format == Format.Normal)
                {
                    Article article2Print = Articles.Where(a => a.Id == article.Id).Single();
                    for (int i = 0; i < article.Count; i++)
                        labels2Print.Add(article2Print);
                }
            }

            if (labels2Print.Count != 0)
                new Label(Data, labels2Print).CreateAndPrint();
        }

        private void PrintLandscapeLabelsBtn_Click(object sender, RoutedEventArgs e)
        {
            List<Article> labels2Print = new List<Article>();
            foreach (LabelArticle article in ShownArticles)
            {
                if (article.Format == Format.Landscape)
                {
                    Article article2Print = Articles.Where(a => a.Id == article.Id).Single();
                    for (int i = 0; i < article.Count; i++)
                        labels2Print.Add(article2Print);
                }
            }

            if (labels2Print.Count != 0)
                new LabelLandscape(Data, labels2Print).CreateAndPrint();
        }
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum Format
    {
        [Description("Hängeetikett")]
        Normal,
        [Description("Klebeetikett")]
        Landscape
    }

    public class LabelArticle : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string ConvertedId { get; set; }
        public string Description { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public int Count { get; set; } = 1;
        public decimal Price { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private Format _format;

        public Format Format {
            get
            {
                return _format;
            }
            set
            {
                _format = value;
                OnPropertyChanged("Format");
            }
        }
    }
}
