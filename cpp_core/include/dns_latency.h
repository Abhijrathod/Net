#pragma once

#include "../include/dns_types.h"
#include <vector>
#include <cmath>

class DnsLatencyTester {
public:
    static DnsTestResult TestServer(const DnsServer& server, int testCount = 5, int timeoutMs = 3000);
    static std::vector<DnsTestResult> TestServers(const std::vector<DnsServer>& servers, int testCount = 5, int timeoutMs = 3000);
};
