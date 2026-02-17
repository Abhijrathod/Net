using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DNSChanger.Models;
using DNSChanger.Services;
using DNSChanger.Views;
using System.Security.Principal;
using System.Collections.Generic;

namespace DNSChanger.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private const int MaxSpeedSamples = 60;
        private const double GraphWidth = 500;
        private const double GraphHeight = 170;

        private readonly DnsService _dnsService = new DnsService();
        
        private ObservableCollection<NetworkModel> _networks = new();
        private ObservableCollection<DnsModel> _dnsServers = new();
        private ObservableCollection<NetworkDriverModel> _networkDrivers = new();
        private NetworkModel? _selectedNetwork;
        private DnsModel? _selectedDns;
        private bool _isTesting;
        private double _testProgress;
        private string _statusMessage = "Ready";
        private string _searchFilter = string.Empty;
        private CancellationTokenSource? _testCancellation;
        private string _resolveDomain = string.Empty;
        private List<string> _resolvedIps = new List<string>();
        private AppSettings _settings;
        private List<DnsProfile> _profiles = new List<DnsProfile>();
        private string _selectedCategory = "All";
        private DispatcherTimer? _networkMonitorTimer;
        private string _activeDriverName = "Detecting...";
        private string _activeDriverType = "-";
        private string _activeDriverLinkSpeed = "-";
        private double _currentDownloadMbps;
        private double _currentUploadMbps;
        private double _graphScaleMaxMbps = 10;
        private PointCollection _downloadGraphPoints = new PointCollection();
        private PointCollection _uploadGraphPoints = new PointCollection();
        private readonly Queue<double> _downloadHistory = new Queue<double>();
        private readonly Queue<double> _uploadHistory = new Queue<double>();
        private string _lastMonitoredInterfaceId = string.Empty;
        private long _lastBytesReceived;
        private long _lastBytesSent;
        private DateTime _lastMonitorTimestampUtc = DateTime.UtcNow;
        private bool _hasBaselineSample;

        public ObservableCollection<DnsProfile> Profiles { get; set; } = new ObservableCollection<DnsProfile>();

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                FilterDnsServers();
            }
        }

        public ObservableCollection<NetworkModel> Networks
        {
            get => _networks;
            set { _networks = value; OnPropertyChanged(); }
        }

        public ObservableCollection<NetworkDriverModel> NetworkDrivers
        {
            get => _networkDrivers;
            set { _networkDrivers = value; OnPropertyChanged(); }
        }

        public ObservableCollection<DnsModel> DnsServers
        {
            get => _dnsServers;
            set { _dnsServers = value; OnPropertyChanged(); }
        }

        public NetworkModel? SelectedNetwork
        {
            get => _selectedNetwork;
            set
            {
                _selectedNetwork = value;
                OnPropertyChanged();
                ResetNetworkSpeedBaseline();
            }
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

        public string ActiveDriverName
        {
            get => _activeDriverName;
            set { _activeDriverName = value; OnPropertyChanged(); }
        }

        public string ActiveDriverType
        {
            get => _activeDriverType;
            set { _activeDriverType = value; OnPropertyChanged(); }
        }

        public string ActiveDriverLinkSpeed
        {
            get => _activeDriverLinkSpeed;
            set { _activeDriverLinkSpeed = value; OnPropertyChanged(); }
        }

        public double CurrentDownloadMbps
        {
            get => _currentDownloadMbps;
            set
            {
                _currentDownloadMbps = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentDownloadDisplay));
            }
        }

        public double CurrentUploadMbps
        {
            get => _currentUploadMbps;
            set
            {
                _currentUploadMbps = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentUploadDisplay));
            }
        }

        public string CurrentDownloadDisplay => $"{CurrentDownloadMbps:F2} Mbps";

        public string CurrentUploadDisplay => $"{CurrentUploadMbps:F2} Mbps";

        public double GraphScaleMaxMbps
        {
            get => _graphScaleMaxMbps;
            set
            {
                _graphScaleMaxMbps = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(GraphScaleLabel));
            }
        }

        public string GraphScaleLabel => $"{GraphScaleMaxMbps:F0} Mbps scale";

        public PointCollection DownloadGraphPoints
        {
            get => _downloadGraphPoints;
            set { _downloadGraphPoints = value; OnPropertyChanged(); }
        }

        public PointCollection UploadGraphPoints
        {
            get => _uploadGraphPoints;
            set { _uploadGraphPoints = value; OnPropertyChanged(); }
        }

        public ICommand RefreshNetworksCommand { get; }
        public ICommand RefreshDnsServersCommand { get; }
        public ICommand RunSpeedTestCommand { get; }
        public ICommand CancelSpeedTestCommand { get; }
        public ICommand ApplyDnsCommand { get; }
        public ICommand RestoreDefaultCommand { get; }
        public ICommand CopyDnsCommand { get; }
        public ICommand FlushDnsCacheCommand { get; }
        public ICommand ResolveDomainCommand { get; }
        public ICommand AddCustomDnsCommand { get; }
        public ICommand EditCustomDnsCommand { get; }
        public ICommand DeleteCustomDnsCommand { get; }
        public ICommand ToggleFavoriteCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand CreateProfileCommand { get; }
        public ICommand ApplyProfileCommand { get; }
        public ICommand ExportDnsListCommand { get; }
        public ICommand ImportDnsListCommand { get; }
        public ICommand BackupDnsCommand { get; }
        public ICommand RestoreDnsCommand { get; }
        public ICommand FilterCategoryCommand { get; }

        public string SearchFilter
        {
            get => _searchFilter;
            set
            {
                _searchFilter = value;
                OnPropertyChanged();
                FilterDnsServers();
            }
        }

        public string ResolveDomain
        {
            get => _resolveDomain;
            set { _resolveDomain = value; OnPropertyChanged(); }
        }

        public List<string> ResolvedIps
        {
            get => _resolvedIps;
            set { _resolvedIps = value; OnPropertyChanged(); }
        }

        public MainViewModel()
        {
            RefreshNetworksCommand = new RelayCommand(async _ => await RefreshNetworksAsync());
            RefreshDnsServersCommand = new RelayCommand(async _ => await RefreshDnsServersAsync());
            RunSpeedTestCommand = new RelayCommand(async _ => await RunSpeedTestAsync(), _ => !IsTesting);
            CancelSpeedTestCommand = new RelayCommand(_ => CancelSpeedTest(), _ => IsTesting);
            ApplyDnsCommand = new RelayCommand(_ => ApplyDns(), _ => SelectedNetwork != null && SelectedDns != null);
            RestoreDefaultCommand = new RelayCommand(_ => RestoreDefaultDns(), _ => SelectedNetwork != null);
            CopyDnsCommand = new RelayCommand(_ => CopyDns(), _ => SelectedDns != null);
            FlushDnsCacheCommand = new RelayCommand(_ => FlushDnsCache());
            ResolveDomainCommand = new RelayCommand(async _ => await ResolveDomainAsync());
            AddCustomDnsCommand = new RelayCommand(_ => AddCustomDns());
            EditCustomDnsCommand = new RelayCommand(_ => EditCustomDns(), _ => SelectedDns != null && SelectedDns.IsCustom);
            DeleteCustomDnsCommand = new RelayCommand(_ => DeleteCustomDns(), _ => SelectedDns != null && SelectedDns.IsCustom);
            ToggleFavoriteCommand = new RelayCommand(_ => ToggleFavorite(), _ => SelectedDns != null);
            OpenSettingsCommand = new RelayCommand(_ => OpenSettings());
            CreateProfileCommand = new RelayCommand(_ => CreateProfile());
            ApplyProfileCommand = new RelayCommand(_ => ApplyProfile(), _ => SelectedNetwork != null);
            ExportDnsListCommand = new RelayCommand(_ => ExportDnsList());
            ImportDnsListCommand = new RelayCommand(_ => ImportDnsList());
            BackupDnsCommand = new RelayCommand(_ => BackupDns());
            RestoreDnsCommand = new RelayCommand(_ => RestoreDns());
            FilterCategoryCommand = new RelayCommand(category => 
            {
                if (category is string cat)
                {
                    SelectedCategory = cat;
                }
            });

            // Load settings and profiles
            _settings = SettingsService.LoadSettings();
            _profiles = SettingsService.LoadProfiles();
            Profiles = new ObservableCollection<DnsProfile>(_profiles);

            RefreshNetworkDrivers();
            InitializeNetworkMonitor();
            _ = RefreshNetworksAsync();
            _ = RefreshDnsServersAsync();
        }

        private ObservableCollection<DnsModel> _allDnsServers = new ObservableCollection<DnsModel>();

        private void FilterDnsServers()
        {
            var filtered = _allDnsServers.AsEnumerable();

            // Category filter
            if (SelectedCategory != "All")
            {
                if (SelectedCategory == "Favorites")
                {
                    filtered = filtered.Where(s => s.IsFavorite);
                }
                else
                {
                    filtered = filtered.Where(s => s.Category == SelectedCategory);
                }
            }

            // Search filter
            if (!string.IsNullOrWhiteSpace(SearchFilter))
            {
                filtered = filtered.Where(s =>
                    s.Provider.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase) ||
                    s.Primary.Contains(SearchFilter) ||
                    s.Secondary.Contains(SearchFilter) ||
                    s.Category.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase)
                );
            }

            DnsServers = new ObservableCollection<DnsModel>(filtered.OrderBy(s => s.IsFavorite ? 0 : 1).ThenBy(s => s.AvgLatencyMs));
        }

        private async Task RefreshNetworksAsync()
        {
            StatusMessage = "Refreshing networks...";
            Networks = await _dnsService.GetNetworksAsync();
            RefreshNetworkDrivers();
            if (Networks.Count > 0 && SelectedNetwork == null)
            {
                SelectedNetwork = Networks[0];
            }
            StatusMessage = $"Found {Networks.Count} network(s) and {NetworkDrivers.Count} driver(s)";
        }

        private async Task RefreshDnsServersAsync()
        {
            StatusMessage = "Loading DNS servers...";
            _allDnsServers = await _dnsService.GetDnsServersAsync();
            DnsServers = _allDnsServers;
            StatusMessage = $"Loaded {DnsServers.Count} DNS server(s)";
        }

        private async Task RunSpeedTestAsync()
        {
            IsTesting = true;
            StatusMessage = "Testing DNS speed...";
            TestProgress = 0;
            _testCancellation = new CancellationTokenSource();

            try
            {
                int totalServers = _allDnsServers.Count;
                int completed = 0;

                var progress = new Progress<DnsModel>(dns =>
                {
                    completed++;
                    TestProgress = (completed * 100.0) / totalServers;
                    StatusMessage = $"Testing {dns.Provider}... ({completed}/{totalServers})";
                    
                    // Update on UI thread
                    System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                    {
                        // Update the filtered view
                        FilterDnsServers();
                        // Force collection refresh
                        OnPropertyChanged(nameof(DnsServers));
                    });
                });

                // Test all servers (not just filtered ones)
                var result = await _dnsService.TestDnsSpeedAsync(_allDnsServers, progress, _testCancellation.Token);
                
                // Re-filter to update the displayed list on UI thread
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    FilterDnsServers();
                    OnPropertyChanged(nameof(DnsServers));
                });
                
                StatusMessage = $"Speed test completed - {completed} server(s) tested";
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Speed test cancelled";
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
                _testCancellation?.Dispose();
                _testCancellation = null;
            }
        }

        private void CancelSpeedTest()
        {
            _testCancellation?.Cancel();
            StatusMessage = "Cancelling speed test...";
        }

        private void FlushDnsCache()
        {
            if (!AdminService.IsRunningAsAdministrator())
            {
                UiService.ShowError("Administrator privileges required to flush DNS cache!");
                return;
            }

            if (!UiService.ShowConfirmation("Flush DNS cache?", "Flush DNS Cache"))
                return;

            bool success = _dnsService.FlushDnsCache();
            if (success)
            {
                UiService.ShowMessage("DNS cache flushed successfully!");
                StatusMessage = "DNS cache flushed";
            }
            else
            {
                UiService.ShowError("Failed to flush DNS cache.");
                StatusMessage = "Failed to flush DNS cache";
            }
        }

        private async Task ResolveDomainAsync()
        {
            if (string.IsNullOrWhiteSpace(ResolveDomain))
            {
                UiService.ShowError("Please enter a domain name to resolve.");
                return;
            }

            if (SelectedDns == null)
            {
                UiService.ShowError("Please select a DNS server first.");
                return;
            }

            StatusMessage = $"Resolving {ResolveDomain}...";
            var ips = await _dnsService.ResolveDomainAsync(ResolveDomain, SelectedDns.Primary);
            ResolvedIps = ips;

            if (ips.Count > 0)
            {
                string ipList = string.Join("\n", ips);
                UiService.ShowMessage($"Resolved {ResolveDomain}:\n\n{ipList}");
                StatusMessage = $"Resolved {ResolveDomain} - {ips.Count} IP(s) found";
            }
            else
            {
                UiService.ShowError($"Could not resolve {ResolveDomain}");
                StatusMessage = $"Failed to resolve {ResolveDomain}";
            }
        }

        public void ApplyDns()
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

        public void RestoreDefaultDns()
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

        private void AddCustomDns()
        {
            var dialog = new AddCustomDnsWindow();
            if (dialog.ShowDialog() == true && dialog.Result != null)
            {
                dialog.Result.IsCustom = true;
                _allDnsServers.Add(dialog.Result);
                FilterDnsServers();
                StatusMessage = $"Added custom DNS: {dialog.Result.Provider}";
            }
        }

        private void EditCustomDns()
        {
            if (SelectedDns == null || !SelectedDns.IsCustom)
                return;

            // Similar to AddCustomDns but pre-populate fields
            var dialog = new AddCustomDnsWindow();
            // Pre-populate would go here
            if (dialog.ShowDialog() == true && dialog.Result != null)
            {
                var index = _allDnsServers.IndexOf(SelectedDns);
                if (index >= 0)
                {
                    dialog.Result.IsCustom = true;
                    _allDnsServers[index] = dialog.Result;
                    FilterDnsServers();
                    StatusMessage = $"Updated custom DNS: {dialog.Result.Provider}";
                }
            }
        }

        private void DeleteCustomDns()
        {
            if (SelectedDns == null || !SelectedDns.IsCustom)
                return;

            if (!UiService.ShowConfirmation($"Delete custom DNS server '{SelectedDns.Provider}'?", "Delete Custom DNS"))
                return;

            _allDnsServers.Remove(SelectedDns);
            FilterDnsServers();
            StatusMessage = "Custom DNS server deleted";
        }

        private void ToggleFavorite()
        {
            if (SelectedDns == null)
                return;

            SelectedDns.IsFavorite = !SelectedDns.IsFavorite;
            
            if (SelectedDns.IsFavorite)
            {
                if (!_settings.FavoriteDnsServers.Contains(SelectedDns.Primary))
                    _settings.FavoriteDnsServers.Add(SelectedDns.Primary);
            }
            else
            {
                _settings.FavoriteDnsServers.Remove(SelectedDns.Primary);
            }

            SettingsService.SaveSettings(_settings);
            FilterDnsServers();
            StatusMessage = SelectedDns.IsFavorite ? "Added to favorites" : "Removed from favorites";
        }

        private void OpenSettings()
        {
            var settingsWindow = new SettingsWindow(_settings);
            if (settingsWindow.ShowDialog() == true)
            {
                _settings = settingsWindow.Settings;
                StatusMessage = "Settings saved";
            }
        }

        private void CreateProfile()
        {
            if (SelectedNetwork == null || SelectedDns == null)
            {
                UiService.ShowError("Please select a network adapter and DNS server first.");
                return;
            }

            // Simple profile creation - could be enhanced with dialog
            var profile = new DnsProfile
            {
                Name = $"{SelectedDns.Provider} - {SelectedNetwork.Name}",
                Description = $"DNS profile for {SelectedNetwork.Name}",
                PrimaryDns = SelectedDns.Primary,
                SecondaryDns = SelectedDns.Secondary,
                Category = SelectedDns.Category,
                NetworkAdapters = new List<string> { SelectedNetwork.Guid }
            };

            _profiles.Add(profile);
            Profiles.Add(profile);
            SettingsService.SaveProfiles(_profiles);
            StatusMessage = $"Profile '{profile.Name}' created";
        }

        private void ApplyProfile()
        {
            // Would show profile selection dialog
            UiService.ShowMessage("Profile selection dialog would open here");
        }

        private void ExportDnsList()
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|CSV files (*.csv)|*.csv",
                    FileName = "dns_servers.json"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    if (saveDialog.FileName.EndsWith(".json"))
                    {
                        var json = Newtonsoft.Json.JsonConvert.SerializeObject(_allDnsServers, Newtonsoft.Json.Formatting.Indented);
                        System.IO.File.WriteAllText(saveDialog.FileName, json);
                    }
                    else
                    {
                        var csv = "Provider,Primary DNS,Secondary DNS,Category,Description\n";
                        csv += string.Join("\n", _allDnsServers.Select(s => 
                            $"\"{s.Provider}\",\"{s.Primary}\",\"{s.Secondary}\",\"{s.Category}\",\"{s.Description}\""));
                        System.IO.File.WriteAllText(saveDialog.FileName, csv);
                    }
                    StatusMessage = "DNS list exported successfully";
                }
            }
            catch (Exception ex)
            {
                UiService.ShowError($"Export failed: {ex.Message}");
            }
        }

        private void ImportDnsList()
        {
            try
            {
                var openDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|CSV files (*.csv)|*.csv"
                };

                if (openDialog.ShowDialog() == true)
                {
                    // Import logic would go here
                    StatusMessage = "DNS list imported successfully";
                }
            }
            catch (Exception ex)
            {
                UiService.ShowError($"Import failed: {ex.Message}");
            }
        }

        private void BackupDns()
        {
            try
            {
                var backup = new Dictionary<string, Dictionary<string, string>>();
                foreach (var network in Networks)
                {
                    backup[network.Guid] = new Dictionary<string, string>
                    {
                        { "Primary", network.DnsPrimary },
                        { "Secondary", network.DnsSecondary }
                    };
                }

                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json",
                    FileName = $"dns_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(backup, Newtonsoft.Json.Formatting.Indented);
                    System.IO.File.WriteAllText(saveDialog.FileName, json);
                    StatusMessage = "DNS backup created successfully";
                }
            }
            catch (Exception ex)
            {
                UiService.ShowError($"Backup failed: {ex.Message}");
            }
        }

        private void RestoreDns()
        {
            try
            {
                var openDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json"
                };

                if (openDialog.ShowDialog() == true)
                {
                    var json = System.IO.File.ReadAllText(openDialog.FileName);
                    var backup = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);
                    
                    if (backup != null && UiService.ShowConfirmation("Restore DNS settings from backup?", "Restore Backup"))
                    {
                        foreach (var kvp in backup)
                        {
                            var guid = kvp.Key;
                            var dns = kvp.Value;
                            if (dns.ContainsKey("Primary"))
                            {
                                _dnsService.SetDns(guid, dns["Primary"], dns.GetValueOrDefault("Secondary", ""));
                            }
                        }
                        StatusMessage = "DNS restored from backup";
                        _ = RefreshNetworksAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                UiService.ShowError($"Restore failed: {ex.Message}");
            }
        }

        private void InitializeNetworkMonitor()
        {
            _networkMonitorTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _networkMonitorTimer.Tick += OnNetworkMonitorTick;
            _networkMonitorTimer.Start();
            MonitorNetworkSpeed();
        }

        private void OnNetworkMonitorTick(object? sender, EventArgs e)
        {
            MonitorNetworkSpeed();
        }

        private void RefreshNetworkDrivers()
        {
            try
            {
                var drivers = NetworkInterface.GetAllNetworkInterfaces()
                    .OrderByDescending(nic => nic.OperationalStatus == OperationalStatus.Up)
                    .ThenBy(nic => nic.Name)
                    .Select(nic => new NetworkDriverModel
                    {
                        InterfaceId = nic.Id,
                        Name = nic.Name,
                        Description = nic.Description,
                        Type = nic.NetworkInterfaceType.ToString(),
                        Status = nic.OperationalStatus.ToString(),
                        LinkSpeedDisplay = FormatLinkSpeed(nic.Speed),
                        IsUp = nic.OperationalStatus == OperationalStatus.Up
                    });

                NetworkDrivers = new ObservableCollection<NetworkDriverModel>(drivers);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to refresh network drivers: {ex.Message}");
            }
        }

        private void MonitorNetworkSpeed()
        {
            try
            {
                var interfaceToMonitor = ResolveInterfaceToMonitor();
                if (interfaceToMonitor == null)
                {
                    ActiveDriverName = "No active interface";
                    ActiveDriverType = "-";
                    ActiveDriverLinkSpeed = "-";
                    CurrentDownloadMbps = 0;
                    CurrentUploadMbps = 0;
                    UpdateSpeedGraph(0, 0);
                    ResetNetworkSpeedBaseline();
                    return;
                }

                ActiveDriverName = interfaceToMonitor.Name;
                ActiveDriverType = interfaceToMonitor.NetworkInterfaceType.ToString();
                ActiveDriverLinkSpeed = FormatLinkSpeed(interfaceToMonitor.Speed);

                var stats = interfaceToMonitor.GetIPv4Statistics();
                var nowUtc = DateTime.UtcNow;
                var isInterfaceChanged = !string.Equals(_lastMonitoredInterfaceId, interfaceToMonitor.Id, StringComparison.OrdinalIgnoreCase);

                if (!_hasBaselineSample || isInterfaceChanged)
                {
                    _lastMonitoredInterfaceId = interfaceToMonitor.Id;
                    _lastBytesReceived = stats.BytesReceived;
                    _lastBytesSent = stats.BytesSent;
                    _lastMonitorTimestampUtc = nowUtc;
                    _hasBaselineSample = true;
                    CurrentDownloadMbps = 0;
                    CurrentUploadMbps = 0;
                    UpdateSpeedGraph(0, 0);
                    return;
                }

                var elapsedSeconds = (nowUtc - _lastMonitorTimestampUtc).TotalSeconds;
                if (elapsedSeconds <= 0.01)
                {
                    return;
                }

                var bytesReceivedDelta = stats.BytesReceived - _lastBytesReceived;
                var bytesSentDelta = stats.BytesSent - _lastBytesSent;
                if (bytesReceivedDelta < 0) bytesReceivedDelta = 0;
                if (bytesSentDelta < 0) bytesSentDelta = 0;

                var downloadMbps = (bytesReceivedDelta * 8d) / elapsedSeconds / 1_000_000d;
                var uploadMbps = (bytesSentDelta * 8d) / elapsedSeconds / 1_000_000d;

                CurrentDownloadMbps = downloadMbps;
                CurrentUploadMbps = uploadMbps;
                UpdateSpeedGraph(downloadMbps, uploadMbps);

                _lastBytesReceived = stats.BytesReceived;
                _lastBytesSent = stats.BytesSent;
                _lastMonitorTimestampUtc = nowUtc;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Network speed monitor failed: {ex.Message}");
            }
        }

        private NetworkInterface? ResolveInterfaceToMonitor()
        {
            var activeInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic =>
                    nic.OperationalStatus == OperationalStatus.Up &&
                    nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .ToList();

            if (activeInterfaces.Count == 0)
            {
                return null;
            }

            if (SelectedNetwork != null)
            {
                var selectedId = NormalizeInterfaceId(SelectedNetwork.Guid);
                if (!string.IsNullOrWhiteSpace(selectedId))
                {
                    var byId = activeInterfaces.FirstOrDefault(nic => NormalizeInterfaceId(nic.Id) == selectedId);
                    if (byId != null)
                    {
                        return byId;
                    }
                }

                var byName = activeInterfaces.FirstOrDefault(nic =>
                    nic.Name.Equals(SelectedNetwork.Name, StringComparison.OrdinalIgnoreCase));
                if (byName != null)
                {
                    return byName;
                }
            }

            return activeInterfaces.OrderByDescending(nic => nic.Speed).FirstOrDefault();
        }

        private void ResetNetworkSpeedBaseline()
        {
            _hasBaselineSample = false;
            _lastMonitoredInterfaceId = string.Empty;
        }

        private void UpdateSpeedGraph(double downloadMbps, double uploadMbps)
        {
            _downloadHistory.Enqueue(downloadMbps);
            _uploadHistory.Enqueue(uploadMbps);

            while (_downloadHistory.Count > MaxSpeedSamples)
            {
                _downloadHistory.Dequeue();
            }

            while (_uploadHistory.Count > MaxSpeedSamples)
            {
                _uploadHistory.Dequeue();
            }

            var maxObserved = Math.Max(
                10,
                Math.Max(_downloadHistory.DefaultIfEmpty(0).Max(), _uploadHistory.DefaultIfEmpty(0).Max()) * 1.25);

            GraphScaleMaxMbps = Math.Ceiling(maxObserved);
            DownloadGraphPoints = BuildSpeedGraphPoints(_downloadHistory, GraphScaleMaxMbps);
            UploadGraphPoints = BuildSpeedGraphPoints(_uploadHistory, GraphScaleMaxMbps);
        }

        private static PointCollection BuildSpeedGraphPoints(IEnumerable<double> values, double maxScaleMbps)
        {
            var snapshot = values.ToList();
            var points = new PointCollection(snapshot.Count);
            if (snapshot.Count == 0)
            {
                return points;
            }

            var safeScale = maxScaleMbps <= 0 ? 1 : maxScaleMbps;
            var stepX = snapshot.Count == 1 ? GraphWidth : GraphWidth / (snapshot.Count - 1);

            for (int index = 0; index < snapshot.Count; index++)
            {
                var normalizedValue = Math.Clamp(snapshot[index] / safeScale, 0, 1);
                var x = index * stepX;
                var y = GraphHeight - (normalizedValue * GraphHeight);
                points.Add(new Point(x, y));
            }

            return points;
        }

        private static string FormatLinkSpeed(long bitsPerSecond)
        {
            if (bitsPerSecond <= 0)
            {
                return "-";
            }

            var mbps = bitsPerSecond / 1_000_000d;
            if (mbps >= 1000)
            {
                return $"{mbps / 1000d:F1} Gbps";
            }

            return $"{mbps:F0} Mbps";
        }

        private static string NormalizeInterfaceId(string? interfaceId)
        {
            if (string.IsNullOrWhiteSpace(interfaceId))
            {
                return string.Empty;
            }

            return interfaceId.Trim().Trim('{', '}').ToUpperInvariant();
        }

        public void Dispose()
        {
            if (_networkMonitorTimer != null)
            {
                _networkMonitorTimer.Stop();
                _networkMonitorTimer.Tick -= OnNetworkMonitorTick;
                _networkMonitorTimer = null;
            }

            _testCancellation?.Cancel();
            _testCancellation?.Dispose();
            _testCancellation = null;
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
