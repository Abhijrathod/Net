#include "../include/dns_latency.h"
#include "../include/dns_packet.h"
#include "../include/logger.h"
#include <thread>
#include <algorithm>
#include <numeric>
#include <cmath>
#include <mutex>

DnsTestResult DnsLatencyTester::TestServer(const DnsServer& server, int testCount, int timeoutMs) {
    DnsTestResult result;
    result.provider = server.provider;
    result.primary = server.primary;
    result.secondary = server.secondary;
    result.testCount = 0;
    result.isReachable = false;
    
    std::vector<double> latencies;
    int successCount = 0;
    
    Logger::GetInstance().Info("Testing DNS: " + server.provider + " (" + server.primary + ")");
    
    // Test primary DNS server
    for (int i = 0; i < testCount; ++i) {
        double latency = 0.0;
        bool success = DnsPacket::SendQuery(server.primary, "google.com", timeoutMs, latency);
        
        if (success && latency > 0) {
            latencies.push_back(latency);
            successCount++;
        }
        
        // Small delay between tests
        std::this_thread::sleep_for(std::chrono::milliseconds(100));
    }
    
    result.testCount = testCount;
    
    if (latencies.empty()) {
        result.avgLatencyMs = 9999.0;
        result.packetLossPercent = 100.0;
        result.stabilityScore = 0.0;
        result.isReachable = false;
        return result;
    }
    
    // Calculate statistics
    result.isReachable = true;
    result.avgLatencyMs = std::accumulate(latencies.begin(), latencies.end(), 0.0) / latencies.size();
    
    auto minmax = std::minmax_element(latencies.begin(), latencies.end());
    double minLatency = *minmax.first;
    double maxLatency = *minmax.second;
    
    result.packetLossPercent = ((testCount - successCount) * 100.0) / testCount;
    
    // Stability score: lower variance = higher score
    double variance = 0.0;
    for (double lat : latencies) {
        double diff = lat - result.avgLatencyMs;
        variance += diff * diff;
    }
    variance /= latencies.size();
    double stdDev = sqrt(variance);
    
    // Normalize stability score (0-100)
    // Lower stdDev and packet loss = higher score
    double stability = 100.0 - std::min(100.0, (stdDev * 10.0 + result.packetLossPercent));
    result.stabilityScore = std::max(0.0, stability);
    
    return result;
}

std::vector<DnsTestResult> DnsLatencyTester::TestServers(const std::vector<DnsServer>& servers, int testCount, int timeoutMs) {
    std::vector<DnsTestResult> results;
    results.reserve(servers.size());
    
    // Parallel testing using threads
    std::vector<std::thread> threads;
    std::mutex resultsMutex;
    
    for (const auto& server : servers) {
        threads.emplace_back([&server, testCount, timeoutMs, &results, &resultsMutex]() {
            DnsTestResult result = TestServer(server, testCount, timeoutMs);
            std::lock_guard<std::mutex> lock(resultsMutex);
            results.push_back(result);
        });
    }
    
    // Wait for all threads to complete
    for (auto& thread : threads) {
        thread.join();
    }
    
    // Sort by average latency
    std::sort(results.begin(), results.end(), [](const DnsTestResult& a, const DnsTestResult& b) {
        return a.avgLatencyMs < b.avgLatencyMs;
    });
    
    return results;
}
