#pragma once

#include <string>

class DnsManager {
public:
    static bool SetDns(const std::string& interfaceGuid, const std::string& dns1, const std::string& dns2);
    static bool RestoreDefaultDns(const std::string& interfaceGuid);
};
