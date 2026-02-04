#pragma once

#include "../include/dns_types.h"
#include <vector>
#include <string>

class DnsCatalog {
public:
    static std::vector<DnsServer> GetAllServers();
    static std::vector<DnsServer> GetServersByCategory(const std::string& category);
    static void AddCustomServer(const DnsServer& server);
    static std::vector<DnsServer> GetAllIncludingCustom();
    
private:
    static std::vector<DnsServer> customServers_;
};
