using System;
using System.Windows;

namespace DNSChanger.Services
{
    public class UiService
    {
        public static void ShowMessage(string message, string title = "DNS Changer")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ShowError(string message, string title = "DNS Changer - Error")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static bool ShowConfirmation(string message, string title = "DNS Changer")
        {
            return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
}
