#pragma once

#include <string>
#include <vector>

struct DnsServer {
    std::string provider;
    std::string primary;
    std::string secondary;
    std::string category;  // "Public", "Privacy", "Family", "Custom"
    std::string description;
};

struct DnsTestResult {
    std::string provider;
    std::string primary;
    std::string secondary;
    double avgLatencyMs;
    double packetLossPercent;
    double stabilityScore;  // 0-100
    int testCount;
    bool isReachable;
};

struct DnsStatistics {
    double avgLatency;
    double minLatency;
    double maxLatency;
    double packetLoss;
    double stability;
    int successfulTests;
    int failedTests;
};
