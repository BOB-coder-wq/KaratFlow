using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using KaratFlowAvalonia.ViewModels;
using KaratFlowAvalonia.Services;
using System;

namespace KaratFlowAvalonia.Views
{
    public partial class SimpleDemoWindow : Window
    {
        private readonly LocalAuthService _authService;

        public SimpleDemoWindow()
        {
            InitializeComponent();
            _authService = new LocalAuthService();
        }

        private void LoginButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var username = this.FindControl<TextBox>("UsernameTextBox")?.Text ?? "";
            var password = this.FindControl<TextBox>("PasswordTextBox")?.Text ?? "";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                var messageBox = new Window
                {
                    Title = "Error",
                    Content = new TextBlock { Text = "Please enter username and password", Margin = new Avalonia.Thickness(20) },
                    Width = 300,
                    Height = 100,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                messageBox.ShowDialog(this);
                return;
            }

            var result = _authService.LoginAsync(username, password).Result;
            if (result.Success)
            {
                var currentUser = _authService.GetCurrentUser();
                ShowMainWindow(currentUser);
            }
            else
            {
                var messageBox = new Window
                {
                    Title = "Login Failed",
                    Content = new TextBlock { Text = result.Message, Margin = new Avalonia.Thickness(20) },
                    Width = 300,
                    Height = 100,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                messageBox.ShowDialog(this);
            }
        }

        private void CreateAccountButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var username = this.FindControl<TextBox>("UsernameTextBox")?.Text ?? "";
            var password = this.FindControl<TextBox>("PasswordTextBox")?.Text ?? "";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                var messageBox = new Window
                {
                    Title = "Error",
                    Content = new TextBlock { Text = "Please enter username and password", Margin = new Avalonia.Thickness(20) },
                    Width = 300,
                    Height = 100,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                messageBox.ShowDialog(this);
                return;
            }

            var result = _authService.CreateAccountAsync(username, username + "@example.com", password, password).Result;
            if (result.Success)
            {
                var currentUser = _authService.GetCurrentUser();
                ShowMainWindow(currentUser);
            }
            else
            {
                var messageBox = new Window
                {
                    Title = "Account Creation Failed",
                    Content = new TextBlock { Text = result.Message, Margin = new Avalonia.Thickness(20) },
                    Width = 300,
                    Height = 100,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                messageBox.ShowDialog(this);
            }
        }

        private void GoogleSignInButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Google Sign-In implementation - simplified for now
            var messageBox = new Window
            {
                Title = "Google Sign-In",
                Content = new TextBlock { Text = "Google Sign-In requires OAuth setup. Please use username/password or demo user.", Margin = new Avalonia.Thickness(20) },
                Width = 400,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            messageBox.ShowDialog(this);
        }

        private void FullscreenButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal || WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.FullScreen;
                CanResize = true;
            }
            else
            {
                WindowState = WindowState.Normal;
                CanResize = false;
            }
        }

        private void DemoUserButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Create demo user
            var result = _authService.CreateAccountAsync("demo", "demo@karatflow.com", "demo123", "demo123").Result;
            if (result.Success)
            {
                var currentUser = _authService.GetCurrentUser();
                ShowMainWindow(currentUser);
            }
            else
            {
                // If demo user already exists, just login
                var loginResult = _authService.LoginAsync("demo", "demo123").Result;
                if (loginResult.Success)
                {
                    var currentUser = _authService.GetCurrentUser();
                    ShowMainWindow(currentUser);
                }
            }
        }

        private void ShowMainWindow(KaratFlowAvalonia.Models.User? user)
        {
            var mainWindow = new MainWindow();
            mainWindow.DataContext = new MainWindowViewModel(user);
            mainWindow.Show();
            this.Close();
        }
    }
}
