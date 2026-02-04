#include "../include/dns_validator.h"
#include <regex>
#include <sstream>

bool DnsValidator::IsValidIpAddress(const std::string& ip) {
    // IPv4 validation
    std::regex ipv4Regex(R"(^(\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})$)");
    std::smatch match;
    
    if (std::regex_match(ip, match, ipv4Regex)) {
        for (int i = 1; i <= 4; ++i) {
            int octet = std::stoi(match[i].str());
            if (octet < 0 || octet > 255) {
                return false;
            }
        }
        return true;
    }
    
    // IPv6 validation (simplified)
    std::regex ipv6Regex(R"(^([0-9a-fA-F]{0,4}:){7}[0-9a-fA-F]{0,4}$)");
    if (std::regex_match(ip, ipv6Regex)) {
        return true;
    }
    
    return false;
}

bool DnsValidator::IsValidDnsServer(const std::string& dns) {
    return IsValidIpAddress(dns);
}
