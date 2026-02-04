using System;
using System.Windows;
using DNSChanger.Models;
using DNSChanger.Services;

namespace DNSChanger.Views
{
    public partial class SettingsWindow : Window
    {
        private AppSettings _settings;

        public AppSettings Settings => _settings;

        public SettingsWindow(AppSettings currentSettings)
        {
            InitializeComponent();
            _settings = currentSettings;
            LoadSettings();
        }

        private void LoadSettings()
        {
            AutoStartCheckBox.IsChecked = _settings.AutoStartWithWindows;
            MinimizeToTrayCheckBox.IsChecked = _settings.MinimizeToTray;
            CheckForUpdatesCheckBox.IsChecked = _settings.CheckForUpdates;
            EnableNotificationsCheckBox.IsChecked = _settings.EnableNotifications;
            RememberWindowPositionCheckBox.IsChecked = _settings.RememberWindowPosition;
            
            TestCountTextBox.Text = _settings.TestCountPerServer.ToString();
            TimeoutTextBox.Text = _settings.TestTimeoutMs.ToString();
            AutoTestOnStartupCheckBox.IsChecked = _settings.AutoTestOnStartup;
            
            ThemeComboBox.SelectedIndex = _settings.Theme == "Dark" ? 0 : 1;
            FontSizeTextBox.Text = _settings.FontSize.ToString();
            
            LogLevelComboBox.SelectedIndex = _settings.LogLevel switch
            {
                "Debug" => 0,
                "Info" => 1,
                "Warning" => 2,
                "Error" => 3,
                _ => 1
            };
            LogFileTextBox.Text = _settings.LogFileLocation;
            EnableQueryLoggingCheckBox.IsChecked = _settings.EnableQueryLogging;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _settings.AutoStartWithWindows = AutoStartCheckBox.IsChecked ?? false;
            _settings.MinimizeToTray = MinimizeToTrayCheckBox.IsChecked ?? true;
            _settings.CheckForUpdates = CheckForUpdatesCheckBox.IsChecked ?? true;
            _settings.EnableNotifications = EnableNotificationsCheckBox.IsChecked ?? true;
            _settings.RememberWindowPosition = RememberWindowPositionCheckBox.IsChecked ?? true;
            
            if (int.TryParse(TestCountTextBox.Text, out int testCount))
                _settings.TestCountPerServer = testCount;
            if (int.TryParse(TimeoutTextBox.Text, out int timeout))
                _settings.TestTimeoutMs = timeout;
            _settings.AutoTestOnStartup = AutoTestOnStartupCheckBox.IsChecked ?? false;
            
            _settings.Theme = ThemeComboBox.SelectedIndex == 0 ? "Dark" : "Light";
            if (int.TryParse(FontSizeTextBox.Text, out int fontSize))
                _settings.FontSize = fontSize;
            
            _settings.LogLevel = LogLevelComboBox.SelectedIndex switch
            {
                0 => "Debug",
                1 => "Info",
                2 => "Warning",
                3 => "Error",
                _ => "Info"
            };
            _settings.EnableQueryLogging = EnableQueryLoggingCheckBox.IsChecked ?? false;
            
            SettingsService.SaveSettings(_settings);
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
