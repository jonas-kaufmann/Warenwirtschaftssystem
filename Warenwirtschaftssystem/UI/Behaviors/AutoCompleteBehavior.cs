using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using Warenwirtschaftssystem.UI.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Warenwirtschaftssystem.UI.Behaviors
{
    public class AutoCompleteBehavior : Behavior<TextBox>
    {
        #region DependencyProperties
        public static DependencyProperty MaxHeightProperty = DependencyProperty.Register(nameof(MaxHeight), typeof(double), typeof(AutoCompleteBehavior), new PropertyMetadata(double.PositiveInfinity));
        public static DependencyProperty StringFormatProperty = DependencyProperty.Register(nameof(StringFormat), typeof(string), typeof(AutoCompleteBehavior));
        public static DependencyProperty SuggestionsProviderProperty = DependencyProperty.Register(nameof(SuggestionsProvider), typeof(IAutoCompleteProvider), typeof(AutoCompleteBehavior));
        #endregion

        #region Properties
        public double MaxHeight
        {
            get => (double)GetValue(MaxHeightProperty);
            set => SetValue(MaxHeightProperty, value);
        }
        public string StringFormat
        {
            get => (string)GetValue(StringFormatProperty);
            set => SetValue(StringFormatProperty, value);
        }
        public IAutoCompleteProvider SuggestionsProvider
        {
            get => (IAutoCompleteProvider)GetValue(SuggestionsProviderProperty);
            set => SetValue(SuggestionsProviderProperty, value);
        }
        #endregion

        private ListBoxPopup ListBoxPopup = new ListBoxPopup();
        private ObservableCollection<object> Suggestions = new ObservableCollection<object>();

        protected override void OnAttached()
        {
            ListBoxPopup.PlacementTarget = AssociatedObject;
            ListBoxPopup.Placement = PlacementMode.Bottom;
            ListBoxPopup.ItemsSource = Suggestions;
            HidePopup();

            // bindings
            var widthBinding = new Binding("ActualWidth") { Source = AssociatedObject };
            ListBoxPopup.SetBinding(FrameworkElement.WidthProperty, widthBinding);
            var maxHeightBinding = new Binding("MaxHeight") { Source = this };
            ListBoxPopup.SetBinding(FrameworkElement.MaxHeightProperty, maxHeightBinding);
            var stringFormatBinding = new Binding("StringFormat") { Source = this };
            ListBoxPopup.SetBinding(ListBoxPopup.StringFormatProperty, stringFormatBinding);

            // AssociatedObject event handlers
            AssociatedObject.GotFocus += AssociatedObject_GotFocus;
            AssociatedObject.LostFocus += AssociatedObject_LostFocus;
            AssociatedObject.TextChanged += AssociatedObject_TextChanged;
            AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;

            //ListBoxPopup event handlers
            ListBoxPopup.PreviewMouseLeftButtonDown += ListBoxPopup_PreviewMouseLeftButtonDown;
        }

        private void ShowPopup()
        {
            ListBoxPopup.Visibility = Visibility.Visible;
            ListBoxPopup.IsOpen = true;
        }

        public void HidePopup()
        {
            ListBoxPopup.Visibility = Visibility.Collapsed;
            ListBoxPopup.IsOpen = false;
        }

        private void AssociatedObject_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (Suggestions != null && Suggestions.Count != 0)
                ShowPopup();
        }

        private void AssociatedObject_GotFocus(object sender, RoutedEventArgs e)
        {
            RefreshSuggestions();
            if (Suggestions != null && Suggestions.Count != 0)
                ShowPopup();
        }

        private void AssociatedObject_LostFocus(object sender, RoutedEventArgs e)
        {
            HidePopup();
        }

        private void RefreshSuggestions()
        {
            Suggestions.Clear();
            if (SuggestionsProvider != null)
            {
                var filteredSuggestions = SuggestionsProvider.GetSuggestions(AssociatedObject.Text);
                foreach (var filteredSuggestion in filteredSuggestions)
                    Suggestions.Add(filteredSuggestion);

            }
        }

        private void AssociatedObject_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshSuggestions();
            if (Suggestions != null && Suggestions.Count != 0)
                ShowPopup();
        }

        private void AssociatedObject_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (ListBoxPopup.IsOpen && ListBoxPopup.SelectedIndex > -1)
                {
                    ListBoxPopup.SelectedIndex -= 1;
                    e.Handled = true;
                }

                if (ListBoxPopup.IsOpen && ListBoxPopup.SelectedIndex == -1)
                    HidePopup();
            }
            else if (e.Key == Key.Down)
            {
                if (ListBoxPopup.SelectedIndex < Suggestions.Count - 1)
                {
                    ListBoxPopup.SelectedIndex += 1;
                    ShowPopup();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                if (ListBoxPopup.IsOpen && ListBoxPopup.SelectedItem != null)
                {
                    AssociatedObject.Text = ListBoxPopup.SelectedItem.ToString();
                    AssociatedObject.CaretIndex = AssociatedObject.Text.Length;
                    HidePopup();

                    if (e.Key == Key.Tab)
                        e.Handled = true;
                }
            }
        }

        private void ListBoxPopup_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Border border && border.Child is ContentPresenter contentPresenter && contentPresenter.Content != null)
            {
                AssociatedObject.Text = contentPresenter.Content.ToString();
                AssociatedObject.CaretIndex = AssociatedObject.Text.Length;
                HidePopup();
            }
            else if (e.OriginalSource is TextBlock textBlock && textBlock.Text != null)
            {
                AssociatedObject.Text = textBlock.Text;
                AssociatedObject.CaretIndex = AssociatedObject.Text.Length;
                HidePopup();
            }
        }
    }
}