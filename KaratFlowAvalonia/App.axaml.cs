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
            // Show simple demo window instead of main window
            var demoWindow = new SimpleDemoWindow();
            desktop.MainWindow = demoWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}