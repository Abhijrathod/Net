using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Drawing;

namespace DNSChanger
{
    public partial class Shell : System.Windows.Controls.UserControl
    {
        private NotifyIcon? _notifyIcon;

        public Shell()
        {
            InitializeComponent();
            InitializeTrayIcon();
        }

        private void InitializeTrayIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Visible = true,
                Text = "DNS Changer"
            };

            _notifyIcon.DoubleClick += (s, e) =>
            {
                if (System.Windows.Application.Current.MainWindow != null)
                {
                    System.Windows.Application.Current.MainWindow.Show();
                    System.Windows.Application.Current.MainWindow.WindowState = WindowState.Normal;
                }
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Change DNS", null, (s, e) => { });
            contextMenu.Items.Add("Restore Default", null, (s, e) => { });
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Exit", null, (s, e) => System.Windows.Application.Current.Shutdown());

            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void Shell_Unloaded(object sender, RoutedEventArgs e)
        {
            _notifyIcon?.Dispose();
        }
    }
}
