#pragma once

#include <string>
#include <vector>

struct NetworkAdapter {
    std::string guid;
    std::string name;
    std::string description;
    std::string type;  // "Ethernet", "WiFi", "VPN", "Virtual", etc.
    std::string ipAddress;
    std::string dnsPrimary;
    std::string dnsSecondary;
    bool isEnabled;
    bool isConnected;
};

struct NetworkList {
    std::vector<NetworkAdapter> adapters;
};
