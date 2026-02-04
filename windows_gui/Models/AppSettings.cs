using System;
using System.Collections.Generic;

namespace DNSChanger.Models
{
    public class AppSettings
    {
        // General Settings
        public bool AutoStartWithWindows { get; set; } = false;
        public bool MinimizeToTray { get; set; } = true;
        public bool CheckForUpdates { get; set; } = true;
        public string Language { get; set; } = "en-US";
        
        // Speed Test Settings
        public int TestCountPerServer { get; set; } = 3;
        public int TestTimeoutMs { get; set; } = 2000;
        public List<string> TestDomains { get; set; } = new List<string> { "google.com", "cloudflare.com" };
        public bool AutoTestOnStartup { get; set; } = false;
        
        // UI Settings
        public string Theme { get; set; } = "Dark";
        public int FontSize { get; set; } = 12;
        public double WindowWidth { get; set; } = 1200;
        public double WindowHeight { get; set; } = 700;
        public double WindowLeft { get; set; } = 100;
        public double WindowTop { get; set; } = 100;
        public bool RememberWindowPosition { get; set; } = true;
        
        // Advanced Settings
        public string LogLevel { get; set; } = "Info";
        public string LogFileLocation { get; set; } = @"C:\ProgramData\DnsChanger\dns_core.log";
        
        // Features
        public bool EnableDnsLeakTesting { get; set; } = true;
        public bool EnableQueryLogging { get; set; } = false;
        public bool EnableNotifications { get; set; } = true;
        
        // Favorites
        public List<string> FavoriteDnsServers { get; set; } = new List<string>(); // Primary IPs
        
        // Last Used
        public Dictionary<string, string> LastUsedDnsPerAdapter { get; set; } = new Dictionary<string, string>(); // Adapter GUID -> Primary DNS
    }
}
