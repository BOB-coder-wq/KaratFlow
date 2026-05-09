using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using KaratFlowAvalonia.ViewModels;

namespace KaratFlowAvalonia.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            
            // Set up ViewModel
            var viewModel = new LoginWindowViewModel(
                new Services.LocalAuthService(),
                new Services.GoogleOAuthService(
                    Avalonia.Application.Current?.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>() ?? 
                    new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .Build()
                )
            );
            
            DataContext = viewModel;
            
            // Subscribe to login success event
            viewModel.LoginSuccess += OnLoginSuccess;
        }

        private void OnLoginSuccess(object? sender, EventArgs e)
        {
            // Close login window and show main window
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
