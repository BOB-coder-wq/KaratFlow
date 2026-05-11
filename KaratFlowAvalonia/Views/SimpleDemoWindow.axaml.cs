using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using KaratFlowAvalonia.ViewModels;
using System;

namespace KaratFlowAvalonia.Views
{
    public partial class SimpleDemoWindow : Window
    {
        public SimpleDemoWindow()
        {
            InitializeComponent();
        }

        private void ContinueButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Close login window and show main window
            var mainWindow = new MainWindow();
            mainWindow.DataContext = new MainWindowViewModel();
            mainWindow.Show();
            this.Close();
        }
    }
}
