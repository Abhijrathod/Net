using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DNSChanger.Models;
using DNSChanger.Services;
using System.Security.Principal;

namespace DNSChanger.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DnsService _dnsService = new DnsService();
        
        private ObservableCollection<NetworkModel> _networks = new();
        private ObservableCollection<DnsModel> _dnsServers = new();
        private NetworkModel? _selectedNetwork;
        private DnsModel? _selectedDns;
        private bool _isTesting;
        private double _testProgress;
        private string _statusMessage = "Ready";

        public ObservableCollection<NetworkModel> Networks
        {
            get => _networks;
            set { _networks = value; OnPropertyChanged(); }
        }

        public ObservableCollection<DnsModel> DnsServers
        {
            get => _dnsServers;
            set { _dnsServers = value; OnPropertyChanged(); }
        }

        public NetworkModel? SelectedNetwork
        {
            get => _selectedNetwork;
            set { _selectedNetwork = value; OnPropertyChanged(); }
        }

        public DnsModel? SelectedDns
        {
            get => _selectedDns;
            set { _selectedDns = value; OnPropertyChanged(); }
        }

        public bool IsTesting
        {
            get => _isTesting;
            set { _isTesting = value; OnPropertyChanged(); }
        }

        public double TestProgress
        {
            get => _testProgress;
            set { _testProgress = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ICommand RefreshNetworksCommand { get; }
        public ICommand RefreshDnsServersCommand { get; }
        public ICommand RunSpeedTestCommand { get; }
        public ICommand ApplyDnsCommand { get; }
        public ICommand RestoreDefaultCommand { get; }
        public ICommand CopyDnsCommand { get; }

        public MainViewModel()
        {
            RefreshNetworksCommand = new RelayCommand(async _ => await RefreshNetworksAsync());
            RefreshDnsServersCommand = new RelayCommand(async _ => await RefreshDnsServersAsync());
            RunSpeedTestCommand = new RelayCommand(async _ => await RunSpeedTestAsync(), _ => !IsTesting);
            ApplyDnsCommand = new RelayCommand(_ => ApplyDns(), _ => SelectedNetwork != null && SelectedDns != null);
            RestoreDefaultCommand = new RelayCommand(_ => RestoreDefaultDns(), _ => SelectedNetwork != null);
            CopyDnsCommand = new RelayCommand(_ => CopyDns(), _ => SelectedDns != null);

            _ = RefreshNetworksAsync();
            _ = RefreshDnsServersAsync();
        }

        private async Task RefreshNetworksAsync()
        {
            StatusMessage = "Refreshing networks...";
            Networks = await _dnsService.GetNetworksAsync();
            if (Networks.Count > 0 && SelectedNetwork == null)
            {
                SelectedNetwork = Networks[0];
            }
            StatusMessage = $"Found {Networks.Count} network(s)";
        }

        private async Task RefreshDnsServersAsync()
        {
            StatusMessage = "Loading DNS servers...";
            DnsServers = await _dnsService.GetDnsServersAsync();
            StatusMessage = $"Loaded {DnsServers.Count} DNS server(s)";
        }

        private async Task RunSpeedTestAsync()
        {
            IsTesting = true;
            StatusMessage = "Testing DNS speed...";
            TestProgress = 0;

            try
            {
                var progress = new Progress<DnsModel>(dns =>
                {
                    TestProgress += 100.0 / DnsServers.Count;
                    StatusMessage = $"Testing {dns.Provider}...";
                });

                DnsServers = await _dnsService.TestDnsSpeedAsync(DnsServers, progress);
                StatusMessage = $"Speed test completed - {DnsServers.Count} server(s) tested";
            }
            catch (Exception ex)
            {
                UiService.ShowError($"Speed test failed: {ex.Message}");
                StatusMessage = "Speed test failed";
            }
            finally
            {
                IsTesting = false;
                TestProgress = 100;
            }
        }

        private void ApplyDns()
        {
            if (SelectedNetwork == null || SelectedDns == null)
                return;

            // Check admin rights before attempting
            if (!AdminService.IsRunningAsAdministrator())
            {
                UiService.ShowError(
                    "Administrator privileges required!\n\n" +
                    "Please restart the application as Administrator:\n" +
                    "1. Close this application\n" +
                    "2. Right-click DNSChanger.exe\n" +
                    "3. Select 'Run as administrator'");
                StatusMessage = "Administrator rights required";
                return;
            }

            if (!UiService.ShowConfirmation(
                $"Apply DNS servers to {SelectedNetwork.Name}?\n\nPrimary: {SelectedDns.Primary}\nSecondary: {SelectedDns.Secondary}",
                "Apply DNS"))
                return;

            StatusMessage = "Applying DNS...";
            bool success = _dnsService.SetDns(SelectedNetwork.Guid, SelectedDns.Primary, SelectedDns.Secondary);
            
            if (success)
            {
                UiService.ShowMessage("DNS servers applied successfully!");
                StatusMessage = $"DNS applied to {SelectedNetwork.Name}";
                _ = RefreshNetworksAsync(); // Refresh to show new DNS
            }
            else
            {
                UiService.ShowError(
                    "Failed to apply DNS servers.\n\n" +
                    "Possible causes:\n" +
                    "• Not running as Administrator\n" +
                    "• Network adapter is disabled\n" +
                    "• Invalid DNS server address\n\n" +
                    "Check the log file: C:\\ProgramData\\DnsChanger\\dns_core.log");
                StatusMessage = "Failed to apply DNS";
            }
        }

        private void RestoreDefaultDns()
        {
            if (SelectedNetwork == null)
                return;

            // Check admin rights before attempting
            if (!AdminService.IsRunningAsAdministrator())
            {
                UiService.ShowError(
                    "Administrator privileges required!\n\n" +
                    "Please restart the application as Administrator:\n" +
                    "1. Close this application\n" +
                    "2. Right-click DNSChanger.exe\n" +
                    "3. Select 'Run as administrator'");
                StatusMessage = "Administrator rights required";
                return;
            }

            if (!UiService.ShowConfirmation(
                $"Restore default DNS (DHCP) for {SelectedNetwork.Name}?",
                "Restore Default DNS"))
                return;

            StatusMessage = "Restoring default DNS...";
            bool success = _dnsService.RestoreDefaultDns(SelectedNetwork.Guid);
            
            if (success)
            {
                UiService.ShowMessage("Default DNS restored successfully!");
                StatusMessage = $"Default DNS restored for {SelectedNetwork.Name}";
                _ = RefreshNetworksAsync();
            }
            else
            {
                UiService.ShowError(
                    "Failed to restore default DNS.\n\n" +
                    "Possible causes:\n" +
                    "• Not running as Administrator\n" +
                    "• Network adapter is disabled\n\n" +
                    "Check the log file: C:\\ProgramData\\DnsChanger\\dns_core.log");
                StatusMessage = "Failed to restore DNS";
            }
        }

        private void CopyDns()
        {
            if (SelectedDns == null)
                return;

            string dnsText = $"{SelectedDns.Primary}\n{SelectedDns.Secondary}";
            Clipboard.SetText(dnsText);
            StatusMessage = "DNS copied to clipboard";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => _execute(parameter);
    }
}
