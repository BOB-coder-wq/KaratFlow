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

        private async void GoogleSignInButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            try
            {
                var oauthService = new GoogleOAuthService(null);
                var authUrl = oauthService.GetAuthorizationUrl();
                
                // Open browser for OAuth flow
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = authUrl,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);
                
                // Show instructions to user
                var messageBox = new Window
                {
                    Title = "Google Sign-In",
                    Content = new StackPanel
                    {
                        Margin = new Avalonia.Thickness(20),
                        Children =
                        {
                            new TextBlock { Text = "Google Sign-In opened in your browser.", Margin = new Avalonia.Thickness(0,0,0,10) },
                            new TextBlock { Text = "After signing in, you'll need to manually enter the authorization code.", Margin = new Avalonia.Thickness(0,0,0,10) },
                            new TextBlock { Text = "For now, please use username/password or demo user.", Margin = new Avalonia.Thickness(0,0,0,10) }
                        }
                    },
                    Width = 400,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                messageBox.ShowDialog(this);
            }
            catch (Exception ex)
            {
                var messageBox = new Window
                {
                    Title = "Google Sign-In Error",
                    Content = new TextBlock { Text = $"Error: {ex.Message}", Margin = new Avalonia.Thickness(20) },
                    Width = 400,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                messageBox.ShowDialog(this);
            }
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
