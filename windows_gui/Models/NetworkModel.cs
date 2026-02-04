using System;

namespace DNSChanger.Models
{
    public class NetworkModel
    {
        public string Guid { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string DnsPrimary { get; set; } = string.Empty;
        public string DnsSecondary { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public bool IsConnected { get; set; }
        
        public string StatusBadge => IsConnected ? "Connected" : "Disconnected";
    }
}
