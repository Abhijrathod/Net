using System;

namespace DNSChanger.Models
{
    public class DnsModel
    {
        public string Provider { get; set; } = string.Empty;
        public string Primary { get; set; } = string.Empty;
        public string Secondary { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // Test results
        public double AvgLatencyMs { get; set; }
        public double PacketLossPercent { get; set; }
        public double StabilityScore { get; set; }
        public int TestCount { get; set; }
        public bool IsReachable { get; set; }
        
        public string LatencyDisplay => AvgLatencyMs < 9999 ? $"{AvgLatencyMs:F1} ms" : "N/A";
        public string PacketLossDisplay => $"{PacketLossPercent:F1}%";
        public string StabilityDisplay => $"{StabilityScore:F0}";
    }
}
