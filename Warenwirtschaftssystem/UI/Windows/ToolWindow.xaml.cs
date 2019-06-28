using System.ComponentModel;
using System.Windows;
using Warenwirtschaftssystem.Model;

namespace Warenwirtschaftssystem.UI.Windows
{
    public partial class ToolWindow : Window
    {
        // Attribute
        private DataModel Data;
        public RootWindow RootWindow;
        public bool DisableClosingPrompt = false;
        public bool Closable = true;

        // Konstruktor
        public ToolWindow(RootWindow rootWindow, DataModel data)
        {
            RootWindow = rootWindow;
            Owner = RootWindow;
            Data = data;
            InitializeComponent();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!Closable)
            {
                e.Cancel = true;
                return;
            }

            if (DisableClosingPrompt)
                return;

            RootWindow.alreadyClosingWindows.Add(this);

            // Speichern der Daten abfragen
            MessageBoxResult result = MessageBox.Show(this, "Nicht gespeicherte Änderungen gehen verloren. Wirklich schließen? ", "Warenwirtschaftssystem", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

            switch (result)
            {
                case MessageBoxResult.OK:
                    RootWindow.RemoveToolWindow(this);
                    break;
                case MessageBoxResult.Cancel:
                    e.Cancel = true;
                    break;
            }

            RootWindow.alreadyClosingWindows.Remove(this);


        }
    }
}
