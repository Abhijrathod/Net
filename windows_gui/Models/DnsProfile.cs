using System;
using System.Collections.Generic;

namespace DNSChanger.Models
{
    public class DnsProfile
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PrimaryDns { get; set; } = string.Empty;
        public string SecondaryDns { get; set; } = string.Empty;
        public string Category { get; set; } = "Custom";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastUsedDate { get; set; } = DateTime.Now;
        public int UseCount { get; set; }
        public List<string> NetworkAdapters { get; set; } = new List<string>(); // GUIDs of adapters to apply to
    }
}
