using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using KaratFlowAvalonia.ViewModels;
using KaratFlowAvalonia.Views;
using Microsoft.Extensions.Configuration;

namespace KaratFlowAvalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Show login window instead of main window
            var loginWindow = new LoginWindow();
            
            // Subscribe to login success event
            if (loginWindow.DataContext is ViewModels.LoginWindowViewModel loginViewModel)
            {
                loginViewModel.LoginSuccess += (sender, e) =>
                {
                    // Get the logged-in user
                    var user = loginViewModel.CurrentUser;
                    if (user != null)
                    {
                        // Create and show main window with user data
                        var mainWindow = new MainWindow();
                        var mainViewModel = new MainWindowViewModel();
                        mainViewModel.SetCurrentUser(user);
                        mainWindow.DataContext = mainViewModel;
                        mainWindow.Show();
                        
                        // Close login window
                        loginWindow.Close();
                    }
                };
            }
            
            desktop.MainWindow = loginWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}