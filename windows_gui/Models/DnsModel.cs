using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DNSChanger.Models
{
    public class DnsModel : INotifyPropertyChanged
    {
        private double _avgLatencyMs;
        private double _packetLossPercent;
        private double _stabilityScore;
        private int _testCount;
        private bool _isReachable;

        public string Provider { get; set; } = string.Empty;
        public string Primary { get; set; } = string.Empty;
        public string Secondary { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        // Test results with property change notifications
        public double AvgLatencyMs 
        { 
            get => _avgLatencyMs;
            set { _avgLatencyMs = value; OnPropertyChanged(); OnPropertyChanged(nameof(LatencyDisplay)); }
        }
        
        public double PacketLossPercent 
        { 
            get => _packetLossPercent;
            set { _packetLossPercent = value; OnPropertyChanged(); OnPropertyChanged(nameof(PacketLossDisplay)); }
        }
        
        public double StabilityScore 
        { 
            get => _stabilityScore;
            set { _stabilityScore = value; OnPropertyChanged(); OnPropertyChanged(nameof(StabilityDisplay)); }
        }
        
        public int TestCount 
        { 
            get => _testCount;
            set { _testCount = value; OnPropertyChanged(); }
        }
        
        public bool IsReachable 
        { 
            get => _isReachable;
            set { _isReachable = value; OnPropertyChanged(); }
        }
        
        public bool IsFavorite { get; set; }
        public bool IsCustom { get; set; }
        
        public string LatencyDisplay => AvgLatencyMs < 9999 ? $"{AvgLatencyMs:F1} ms" : "N/A";
        public string PacketLossDisplay => $"{PacketLossPercent:F1}%";
        public string StabilityDisplay => $"{StabilityScore:F0}";

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
