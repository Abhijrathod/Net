using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
            IProgress<DnsModel> progress)
        {
            return await Task.Run(() =>
            {
                try
                {
                    IntPtr jsonPtr = DnsInterop.test_dns_speed_json();
                    string json = DnsInterop.GetJsonString(jsonPtr);
                    
                    var results = JsonConvert.DeserializeObject<List<DnsModel>>(json) ?? new List<DnsModel>();
                    
                    // Update existing servers with test results
                    foreach (var result in results)
                    {
                        var existing = servers.FirstOrDefault(s => s.Primary == result.Primary);
                        if (existing != null)
                        {
                            existing.AvgLatencyMs = result.AvgLatencyMs;
                            existing.PacketLossPercent = result.PacketLossPercent;
                            existing.StabilityScore = result.StabilityScore;
                            existing.TestCount = result.TestCount;
                            existing.IsReachable = result.IsReachable;
                            progress?.Report(existing);
                        }
                    }
                    
                    // Sort by latency
                    var sorted = new ObservableCollection<DnsModel>(
                        servers.OrderBy(s => s.AvgLatencyMs));
                    
                    return sorted;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error testing DNS speed: {ex.Message}");
                    return servers;
                }
            });
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
    }
}
