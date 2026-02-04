#pragma once

#include <string>
#include <chrono>

class DnsPacket {
public:
    static bool SendQuery(const std::string& dnsServer, const std::string& queryDomain, int timeoutMs, double& latencyMs);
    
private:
    static int BuildDnsQuery(const std::string& domain, unsigned char* buffer, size_t bufferSize);
    static bool ParseDnsResponse(unsigned char* response, int responseLen);
};
