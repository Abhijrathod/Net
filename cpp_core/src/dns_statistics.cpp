#include "../include/dns_statistics.h"
#include "../include/dns_types.h"
#include <algorithm>
#include <numeric>
#include <cmath>

DnsStatistics StatisticsCalculator::Calculate(const std::vector<DnsTestResult>& results) {
    DnsStatistics stats = {0};
    
    if (results.empty()) {
        return stats;
    }
    
    std::vector<double> latencies;
    int successful = 0;
    int failed = 0;
    
    for (const auto& result : results) {
        if (result.isReachable && result.avgLatencyMs < 9999.0) {
            latencies.push_back(result.avgLatencyMs);
            successful += result.testCount;
        } else {
            failed += result.testCount;
        }
    }
    
    if (!latencies.empty()) {
        stats.avgLatency = std::accumulate(latencies.begin(), latencies.end(), 0.0) / latencies.size();
        stats.minLatency = *std::min_element(latencies.begin(), latencies.end());
        stats.maxLatency = *std::max_element(latencies.begin(), latencies.end());
    }
    
    int totalTests = successful + failed;
    if (totalTests > 0) {
        stats.packetLoss = (failed * 100.0) / totalTests;
    }
    
    // Calculate overall stability (average of individual stability scores)
    double totalStability = 0.0;
    int stabilityCount = 0;
    for (const auto& result : results) {
        if (result.isReachable) {
            totalStability += result.stabilityScore;
            stabilityCount++;
        }
    }
    if (stabilityCount > 0) {
        stats.stability = totalStability / stabilityCount;
    }
    
    stats.successfulTests = successful;
    stats.failedTests = failed;
    
    return stats;
}
