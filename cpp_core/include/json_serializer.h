#pragma once

#include <string>
#include "../include/network_types.h"
#include "../include/dns_types.h"
#include <vector>

class JsonSerializer {
public:
    static std::string EscapeJson(const std::string& str);
    static std::string NetworkListToJson(const NetworkList& list);
    static std::string DnsServerListToJson(const std::vector<DnsServer>& servers);
    static std::string DnsTestResultsToJson(const std::vector<DnsTestResult>& results);
};
