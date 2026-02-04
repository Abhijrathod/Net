#pragma once

#include "../include/network_types.h"
#include "../include/dns_types.h"
#include <vector>
#include <string>

class Core {
public:
    static NetworkList GetNetworks();
    static std::vector<DnsServer> GetDnsServers();
    static std::vector<DnsTestResult> TestDnsSpeed(int testCount = 5, int timeoutMs = 3000);
    static bool SetDns(const std::string& interfaceGuid, const std::string& dns1, const std::string& dns2);
    static bool RestoreDefaultDns(const std::string& interfaceGuid);
    static void AddCustomDnsServer(const DnsServer& server);
};
