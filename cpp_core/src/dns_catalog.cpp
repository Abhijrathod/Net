#include "../include/dns_catalog.h"
#include "../include/dns_types.h"
#include <algorithm>
#include <vector>
#include <iterator>

std::vector<DnsServer> DnsCatalog::customServers_;

std::vector<DnsServer> DnsCatalog::GetAllServers() {
    std::vector<DnsServer> servers = {
        // Public DNS
        {"Google", "8.8.8.8", "8.8.4.4", "Public", "Google Public DNS"},
        {"Cloudflare", "1.1.1.1", "1.0.0.1", "Privacy", "Cloudflare DNS"},
        {"Quad9", "9.9.9.9", "149.112.112.112", "Privacy", "Quad9 Security DNS"},
        {"OpenDNS", "208.67.222.222", "208.67.220.220", "Public", "Cisco OpenDNS"},
        {"AdGuard", "94.140.14.14", "94.140.15.15", "Privacy", "AdGuard DNS"},
        {"CleanBrowsing", "185.228.168.9", "185.228.169.9", "Family", "CleanBrowsing Family Filter"},
        {"Comodo", "8.26.56.26", "8.20.247.20", "Public", "Comodo Secure DNS"},
        {"Yandex", "77.88.8.8", "77.88.8.1", "Public", "Yandex DNS"},
        {"ControlD", "76.76.2.0", "76.76.10.0", "Privacy", "ControlD DNS"},
        {"Alternate DNS", "76.76.19.19", "76.223.122.150", "Public", "Alternate DNS"},
        {"NextDNS", "45.90.28.0", "45.90.30.0", "Privacy", "NextDNS"},
        {"Mullvad", "194.242.2.2", "194.242.2.3", "Privacy", "Mullvad DNS"},
        {"Cisco Umbrella", "208.67.222.222", "208.67.220.220", "Public", "Cisco Umbrella"},
        {"Hurricane Electric", "74.82.42.42", "74.82.42.42", "Public", "Hurricane Electric DNS"},
        {"Neustar", "156.154.70.1", "156.154.71.1", "Public", "Neustar DNS"},
        {"Verisign", "64.6.64.6", "64.6.65.6", "Public", "Verisign Public DNS"},
        {"Level3", "4.2.2.1", "4.2.2.2", "Public", "Level3 DNS"},
        {"OpenNIC", "185.121.177.177", "169.239.202.202", "Public", "OpenNIC DNS"},
        {"Shecan", "178.22.122.100", "185.51.200.2", "Public", "Shecan DNS"},
        {"Tiarap", "172.104.93.80", "172.104.93.80", "Public", "Tiarap DNS"},
        {"Begzar", "185.55.226.26", "185.55.225.25", "Public", "Begzar DNS"},
        {"Radar", "10.202.10.10", "10.202.10.11", "Public", "Radar Game DNS"},
        
        // Additional Privacy DNS
        {"Cloudflare (IPv6)", "2606:4700:4700::1111", "2606:4700:4700::1001", "Privacy", "Cloudflare DNS IPv6"},
        {"Google (IPv6)", "2001:4860:4860::8888", "2001:4860:4860::8844", "Public", "Google Public DNS IPv6"},
        {"Quad9 (IPv6)", "2620:fe::fe", "2620:fe::9", "Privacy", "Quad9 Security DNS IPv6"},
        
        // Family/Filtering DNS
        {"CleanBrowsing Adult", "185.228.168.10", "185.228.169.11", "Family", "CleanBrowsing Adult Filter"},
        {"CleanBrowsing Security", "185.228.168.9", "185.228.169.9", "Family", "CleanBrowsing Security Filter"},
        {"OpenDNS Family Shield", "208.67.222.123", "208.67.220.123", "Family", "OpenDNS Family Shield"},
        
        // Regional DNS
        {"Norton ConnectSafe", "199.85.126.10", "199.85.127.10", "Family", "Norton ConnectSafe"},
        {"SafeDNS", "195.46.39.39", "195.46.39.40", "Family", "SafeDNS"},
        {"DNS.WATCH", "84.200.69.80", "84.200.70.40", "Privacy", "DNS.WATCH"},
        {"FreeDNS", "37.235.1.174", "37.235.1.177", "Public", "FreeDNS"},
        {"UncensoredDNS", "91.239.100.100", "89.233.43.71", "Privacy", "UncensoredDNS"},
        {"Dyn", "216.146.35.35", "216.146.36.36", "Public", "Dyn DNS"},
        {"Comodo Secure IPv6", "2001:4860:4860::8888", "2001:4860:4860::8844", "Public", "Comodo Secure DNS IPv6"},
    };
    
    return servers;
}

std::vector<DnsServer> DnsCatalog::GetServersByCategory(const std::string& category) {
    auto all = GetAllServers();
    std::vector<DnsServer> filtered;
    std::copy_if(all.begin(), all.end(), std::back_inserter(filtered),
        [&category](const DnsServer& s) { return s.category == category; });
    return filtered;
}

void DnsCatalog::AddCustomServer(const DnsServer& server) {
    customServers_.push_back(server);
}

std::vector<DnsServer> DnsCatalog::GetAllIncludingCustom() {
    auto servers = GetAllServers();
    servers.insert(servers.end(), customServers_.begin(), customServers_.end());
    return servers;
}
