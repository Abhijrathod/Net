using System;
using System.Windows;
using DNSChanger.Services;
using DNSChanger.Models;

namespace DNSChanger
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Check if running as administrator
            AdminService.CheckAdminAndWarn();
            
            // Load and apply saved window position
            var settings = SettingsService.LoadSettings();
            if (settings.RememberWindowPosition && MainWindow != null)
            {
                MainWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                MainWindow.Left = settings.WindowLeft;
                MainWindow.Top = settings.WindowTop;
                MainWindow.Width = settings.WindowWidth;
                MainWindow.Height = settings.WindowHeight;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Save window position
            if (MainWindow != null)
            {
                var settings = SettingsService.LoadSettings();
                settings.WindowLeft = MainWindow.Left;
                settings.WindowTop = MainWindow.Top;
                settings.WindowWidth = MainWindow.Width;
                settings.WindowHeight = MainWindow.Height;
                SettingsService.SaveSettings(settings);
            }
            
            base.OnExit(e);
        }
    }
}
