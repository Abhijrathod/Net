using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using DNSChanger.Models;
using DNSChanger.Services;
using Newtonsoft.Json;

namespace DNSChanger.Services
{
    public class DnsService
    {
        public async Task<ObservableCollection<NetworkModel>> GetNetworksAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    IntPtr jsonPtr = DnsInterop.list_networks_json();
                    if (jsonPtr == IntPtr.Zero)
                    {
                        System.Diagnostics.Debug.WriteLine("list_networks_json returned null pointer");
                        return new ObservableCollection<NetworkModel>();
                    }
                    
                    string json = DnsInterop.GetJsonString(jsonPtr);
                    System.Diagnostics.Debug.WriteLine($"Networks JSON: {json}");
                    
                    if (string.IsNullOrEmpty(json) || json == "[]")
                    {
                        System.Diagnostics.Debug.WriteLine("Empty or invalid JSON returned");
                        return new ObservableCollection<NetworkModel>();
                    }
                    
                    var adapters = JsonConvert.DeserializeObject<List<NetworkModel>>(json) ?? new List<NetworkModel>();
                    System.Diagnostics.Debug.WriteLine($"Deserialized {adapters.Count} network adapters");
                    return new ObservableCollection<NetworkModel>(adapters);
                }
                catch (DllNotFoundException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"DLL not found: {ex.Message}");
                    System.Windows.MessageBox.Show($"DLL not found: {ex.Message}\n\nMake sure dns_core.dll is in the same directory as DNSChanger.exe", 
                        "DLL Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return new ObservableCollection<NetworkModel>();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting networks: {ex.Message}\n{ex.StackTrace}");
                    System.Windows.MessageBox.Show($"Error loading networks: {ex.Message}", 
                        "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return new ObservableCollection<NetworkModel>();
                }
            });
        }

        public async Task<ObservableCollection<DnsModel>> GetDnsServersAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    IntPtr jsonPtr = DnsInterop.list_dns_servers_json();
                    if (jsonPtr == IntPtr.Zero)
                    {
                        System.Diagnostics.Debug.WriteLine("list_dns_servers_json returned null pointer");
                        return new ObservableCollection<DnsModel>();
                    }
                    
                    string json = DnsInterop.GetJsonString(jsonPtr);
                    System.Diagnostics.Debug.WriteLine($"DNS Servers JSON length: {json?.Length ?? 0}");
                    
                    if (string.IsNullOrEmpty(json) || json == "[]")
                    {
                        System.Diagnostics.Debug.WriteLine("Empty or invalid JSON returned for DNS servers");
                        return new ObservableCollection<DnsModel>();
                    }
                    
                    var servers = JsonConvert.DeserializeObject<List<DnsModel>>(json) ?? new List<DnsModel>();
                    System.Diagnostics.Debug.WriteLine($"Deserialized {servers.Count} DNS servers");
                    return new ObservableCollection<DnsModel>(servers);
                }
                catch (DllNotFoundException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"DLL not found: {ex.Message}");
                    return new ObservableCollection<DnsModel>();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error getting DNS servers: {ex.Message}\n{ex.StackTrace}");
                    return new ObservableCollection<DnsModel>();
                }
            });
        }

        public async Task<ObservableCollection<DnsModel>> TestDnsSpeedAsync(
            ObservableCollection<DnsModel> servers,
            IProgress<DnsModel> progress,
            CancellationToken cancellationToken = default)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("Starting DNS speed test...");
                    
                    // Call C++ function for comprehensive test (this does the actual DNS queries)
                    IntPtr jsonPtr = DnsInterop.test_dns_speed_json();
                    if (jsonPtr == IntPtr.Zero)
                    {
                        System.Diagnostics.Debug.WriteLine("test_dns_speed_json returned null pointer");
                        return servers;
                    }
                    
                    string json = DnsInterop.GetJsonString(jsonPtr);
                    System.Diagnostics.Debug.WriteLine($"Speed test JSON length: {json?.Length ?? 0}");
                    System.Diagnostics.Debug.WriteLine($"Speed test JSON (first 500 chars): {json?.Substring(0, Math.Min(500, json?.Length ?? 0))}");
                    
                    if (string.IsNullOrEmpty(json) || json == "[]")
                    {
                        System.Diagnostics.Debug.WriteLine("Empty or invalid JSON returned from speed test");
                        return servers;
                    }
                    
                    var results = JsonConvert.DeserializeObject<List<DnsModel>>(json) ?? new List<DnsModel>();
                    System.Diagnostics.Debug.WriteLine($"Deserialized {results.Count} test results");
                    
                    // Update existing servers with test results
                    int updatedCount = 0;
                    foreach (var result in results)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;
                        
                        // Try multiple matching strategies
                        DnsModel? existing = null;
                        
                        // First try exact Primary IP match
                        existing = servers.FirstOrDefault(s => 
                            !string.IsNullOrEmpty(s.Primary) && 
                            !string.IsNullOrEmpty(result.Primary) &&
                            s.Primary.Trim() == result.Primary.Trim());
                        
                        // If no match, try by Provider name
                        if (existing == null)
                        {
                            existing = servers.FirstOrDefault(s => 
                                !string.IsNullOrEmpty(s.Provider) &&
                                !string.IsNullOrEmpty(result.Provider) &&
                                s.Provider.Equals(result.Provider, StringComparison.OrdinalIgnoreCase));
                        }
                        
                        // If still no match, try by Secondary IP
                        if (existing == null && !string.IsNullOrEmpty(result.Secondary))
                        {
                            existing = servers.FirstOrDefault(s => 
                                !string.IsNullOrEmpty(s.Secondary) &&
                                s.Secondary.Trim() == result.Secondary.Trim());
                        }
                            
                        if (existing != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Updating {existing.Provider} ({existing.Primary}): Latency={result.AvgLatencyMs}ms, Loss={result.PacketLossPercent}%, Stability={result.StabilityScore}");
                            
                            // Update properties
                            existing.AvgLatencyMs = result.AvgLatencyMs;
                            existing.PacketLossPercent = result.PacketLossPercent;
                            existing.StabilityScore = result.StabilityScore;
                            existing.TestCount = result.TestCount;
                            existing.IsReachable = result.IsReachable;
                            
                            // Force property change notifications on UI thread
                            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                            {
                                existing.OnPropertyChanged(nameof(existing.AvgLatencyMs));
                                existing.OnPropertyChanged(nameof(existing.PacketLossPercent));
                                existing.OnPropertyChanged(nameof(existing.StabilityScore));
                                existing.OnPropertyChanged(nameof(existing.TestCount));
                                existing.OnPropertyChanged(nameof(existing.IsReachable));
                                existing.OnPropertyChanged(nameof(existing.LatencyDisplay));
                                existing.OnPropertyChanged(nameof(existing.PacketLossDisplay));
                                existing.OnPropertyChanged(nameof(existing.StabilityDisplay));
                            });
                            
                            updatedCount++;
                            progress?.Report(existing);
                            
                            // Small delay for UI updates
                            await Task.Delay(10, cancellationToken);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"No matching server found for {result.Provider} ({result.Primary})");
                        }
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"Updated {updatedCount} servers with test results");
                    
                    // Return the updated collection (don't create a new one to preserve references)
                    return servers;
                }
                catch (OperationCanceledException)
                {
                    System.Diagnostics.Debug.WriteLine("Speed test was cancelled");
                    return servers;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error testing DNS speed: {ex.Message}\n{ex.StackTrace}");
                    return servers;
                }
            }, cancellationToken);
        }

        public bool SetDns(string interfaceGuid, string dns1, string dns2)
        {
            try
            {
                return DnsInterop.set_dns(interfaceGuid, dns1, dns2 ?? string.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting DNS: {ex.Message}");
                return false;
            }
        }

        public bool RestoreDefaultDns(string interfaceGuid)
        {
            try
            {
                return DnsInterop.restore_default_dns(interfaceGuid);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring DNS: {ex.Message}");
                return false;
            }
        }

        public async Task<List<string>> ResolveDomainAsync(string domain, string dnsServer)
        {
            return await Task.Run(() =>
            {
                try
                {
                    IntPtr jsonPtr = DnsInterop.resolve_domain_json(domain, dnsServer);
                    string json = DnsInterop.GetJsonString(jsonPtr);
                    return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error resolving domain: {ex.Message}");
                    return new List<string>();
                }
            });
        }

        public bool FlushDnsCache()
        {
            try
            {
                return DnsInterop.flush_dns_cache();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error flushing DNS cache: {ex.Message}");
                return false;
            }
        }
    }
}
