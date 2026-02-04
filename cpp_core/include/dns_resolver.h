#pragma once

#include <string>
#include <vector>

class DnsResolver {
public:
    static std::vector<std::string> Resolve(const std::string& domain, const std::string& dnsServer);
    static bool FlushCache();
};
