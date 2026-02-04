using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using DNSChanger.Models;
using DNSChanger.Services;

namespace DNSChanger.Views
{
    public partial class AddCustomDnsWindow : Window
    {
        public DnsModel? Result { get; private set; }

        public AddCustomDnsWindow()
        {
            InitializeComponent();
        }

        private bool ValidateIpAddress(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return false;

            // IPv4 validation
            if (Regex.IsMatch(ip, @"^(\d{1,3}\.){3}\d{1,3}$"))
            {
                return IPAddress.TryParse(ip, out _);
            }

            // IPv6 validation (simplified)
            if (ip.Contains(":"))
            {
                return IPAddress.TryParse(ip, out _);
            }

            return false;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string provider = ProviderTextBox.Text.Trim();
            string primary = PrimaryDnsTextBox.Text.Trim();
            string secondary = SecondaryDnsTextBox.Text.Trim();
            string category = (CategoryComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "Custom";
            string description = DescriptionTextBox.Text.Trim();

            // Validation
            if (string.IsNullOrWhiteSpace(provider))
            {
                ShowError("Provider name is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(primary))
            {
                ShowError("Primary DNS is required.");
                return;
            }

            if (!ValidateIpAddress(primary))
            {
                ShowError("Primary DNS must be a valid IP address.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(secondary) && !ValidateIpAddress(secondary))
            {
                ShowError("Secondary DNS must be a valid IP address.");
                return;
            }

            Result = new DnsModel
            {
                Provider = provider,
                Primary = primary,
                Secondary = secondary,
                Category = category,
                Description = description
            };

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShowError(string message)
        {
            ErrorMessageTextBlock.Text = message;
            ErrorMessageTextBlock.Visibility = Visibility.Visible;
        }
    }
}
