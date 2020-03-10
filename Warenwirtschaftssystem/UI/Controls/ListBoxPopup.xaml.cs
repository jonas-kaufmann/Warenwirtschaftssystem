using System.Collections;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;

namespace Warenwirtschaftssystem.UI.Controls
{
    public partial class ListBoxPopup : Popup
    {
        #region DependencyProperties
        public static DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IList), typeof(ListBoxPopup));
        public static DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(ListBoxPopup));
        public static DependencyProperty SelectedIndexProperty = DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(ListBoxPopup), new PropertyMetadata(-1));
        public static DependencyProperty StringFormatProperty = DependencyProperty.Register(nameof(StringFormat), typeof(string), typeof(ListBoxPopup));
        #endregion

        #region Properties
        public IList ItemsSource
        {
            get => (IList)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        } 
        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }
        public string StringFormat
        {
            get => (string)GetValue(StringFormatProperty);
            set => SetValue(StringFormatProperty, value);
        }
        #endregion

        #region events
        public event SelectionChangedEventHandler SelectionChanged;
        #endregion

        public ListBoxPopup()
        {
            DataContext = this;
            InitializeComponent();

            MainListBox.SelectionChanged += MainListBox_SelectionChanged;
        }

        private void MainListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => SelectionChanged?.Invoke(sender, e);
    }
}
