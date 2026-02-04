#pragma once

#include <string>

class DnsValidator {
public:
    static bool IsValidIpAddress(const std::string& ip);
    static bool IsValidDnsServer(const std::string& dns);
};
