using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Drawing;
using DNSChanger.ViewModels;
using DNSChanger.Services;

namespace DNSChanger
{
    public partial class Shell : System.Windows.Controls.UserControl
    {
        private NotifyIcon? _notifyIcon;
        private MainViewModel? _viewModel;

        public Shell()
        {
            InitializeComponent();
        }

        public void InitializeTrayIcon(MainViewModel viewModel)
        {
            _viewModel = viewModel;
            
            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Visible = true,
                Text = "DNS Changer - Click to open"
            };

            _notifyIcon.DoubleClick += (s, e) =>
            {
                if (System.Windows.Application.Current.MainWindow != null)
                {
                    System.Windows.Application.Current.MainWindow.Show();
                    System.Windows.Application.Current.MainWindow.WindowState = WindowState.Normal;
                    System.Windows.Application.Current.MainWindow.Activate();
                }
            };

            UpdateTrayMenu();
        }

        public void UpdateTrayMenu()
        {
            if (_notifyIcon == null || _viewModel == null)
                return;

            var contextMenu = new ContextMenuStrip();
            
            // Current DNS info
            if (_viewModel.SelectedNetwork != null && _viewModel.SelectedDns != null)
            {
                var infoItem = new ToolStripMenuItem($"Current: {_viewModel.SelectedDns.Provider}");
                infoItem.Enabled = false;
                contextMenu.Items.Add(infoItem);
                contextMenu.Items.Add("-");
            }

            // Quick DNS switches
            var quickDns = new ToolStripMenuItem("Quick Switch DNS");
            quickDns.DropDownItems.Add("Google (8.8.8.8)", null, (s, e) => QuickSwitchDns("8.8.8.8", "8.8.4.4"));
            quickDns.DropDownItems.Add("Cloudflare (1.1.1.1)", null, (s, e) => QuickSwitchDns("1.1.1.1", "1.0.0.1"));
            quickDns.DropDownItems.Add("Quad9 (9.9.9.9)", null, (s, e) => QuickSwitchDns("9.9.9.9", "149.112.112.112"));
            quickDns.DropDownItems.Add("-");
            quickDns.DropDownItems.Add("Restore Default", null, (s, e) => RestoreDefault());
            contextMenu.Items.Add(quickDns);
            
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Open DNS Changer", null, (s, e) => 
            {
                if (System.Windows.Application.Current.MainWindow != null)
                {
                    System.Windows.Application.Current.MainWindow.Show();
                    System.Windows.Application.Current.MainWindow.WindowState = WindowState.Normal;
                }
            });
            contextMenu.Items.Add("Exit", null, (s, e) => System.Windows.Application.Current.Shutdown());

            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void QuickSwitchDns(string primary, string secondary)
        {
            if (_viewModel?.SelectedNetwork != null)
            {
                _viewModel.ApplyDns(); // This would need to be public or use a command
            }
        }

        private void RestoreDefault()
        {
            if (_viewModel?.SelectedNetwork != null)
            {
                _viewModel.RestoreDefaultDns(); // This would need to be public or use a command
            }
        }

        private void Shell_Unloaded(object sender, RoutedEventArgs e)
        {
            _notifyIcon?.Dispose();
        }
    }
}
