#include "../include/json_serializer.h"
#include "../include/network_types.h"
#include "../include/dns_types.h"
#include <sstream>
#include <iomanip>

std::string JsonSerializer::EscapeJson(const std::string& str) {
    std::ostringstream o;
    for (size_t i = 0; i < str.length(); ++i) {
        switch (str[i]) {
            case '"': o << "\\\""; break;
            case '\\': o << "\\\\"; break;
            case '\b': o << "\\b"; break;
            case '\f': o << "\\f"; break;
            case '\n': o << "\\n"; break;
            case '\r': o << "\\r"; break;
            case '\t': o << "\\t"; break;
            default:
                if ('\x00' <= str[i] && str[i] <= '\x1f') {
                    o << "\\u" << std::hex << std::setw(4) << std::setfill('0') << (int)str[i];
                } else {
                    o << str[i];
                }
        }
    }
    return o.str();
}

std::string JsonSerializer::NetworkListToJson(const NetworkList& list) {
    std::ostringstream json;
    json << "[";
    for (size_t i = 0; i < list.adapters.size(); ++i) {
        const auto& adapter = list.adapters[i];
        if (i > 0) json << ",";
        json << "{"
             << "\"guid\":\"" << EscapeJson(adapter.guid) << "\","
             << "\"name\":\"" << EscapeJson(adapter.name) << "\","
             << "\"description\":\"" << EscapeJson(adapter.description) << "\","
             << "\"type\":\"" << EscapeJson(adapter.type) << "\","
             << "\"ipAddress\":\"" << EscapeJson(adapter.ipAddress) << "\","
             << "\"dnsPrimary\":\"" << EscapeJson(adapter.dnsPrimary) << "\","
             << "\"dnsSecondary\":\"" << EscapeJson(adapter.dnsSecondary) << "\","
             << "\"isEnabled\":" << (adapter.isEnabled ? "true" : "false") << ","
             << "\"isConnected\":" << (adapter.isConnected ? "true" : "false")
             << "}";
    }
    json << "]";
    return json.str();
}

std::string JsonSerializer::DnsServerListToJson(const std::vector<DnsServer>& servers) {
    std::ostringstream json;
    json << "[";
    for (size_t i = 0; i < servers.size(); ++i) {
        const auto& server = servers[i];
        if (i > 0) json << ",";
        json << "{"
             << "\"provider\":\"" << EscapeJson(server.provider) << "\","
             << "\"primary\":\"" << EscapeJson(server.primary) << "\","
             << "\"secondary\":\"" << EscapeJson(server.secondary) << "\","
             << "\"category\":\"" << EscapeJson(server.category) << "\","
             << "\"description\":\"" << EscapeJson(server.description) << "\""
             << "}";
    }
    json << "]";
    return json.str();
}

std::string JsonSerializer::DnsTestResultsToJson(const std::vector<DnsTestResult>& results) {
    std::ostringstream json;
    json << "[";
    for (size_t i = 0; i < results.size(); ++i) {
        const auto& result = results[i];
        if (i > 0) json << ",";
        json << std::fixed << std::setprecision(2)
             << "{"
             << "\"provider\":\"" << EscapeJson(result.provider) << "\","
             << "\"primary\":\"" << EscapeJson(result.primary) << "\","
             << "\"secondary\":\"" << EscapeJson(result.secondary) << "\","
             << "\"avgLatencyMs\":" << result.avgLatencyMs << ","
             << "\"packetLossPercent\":" << result.packetLossPercent << ","
             << "\"stabilityScore\":" << result.stabilityScore << ","
             << "\"testCount\":" << result.testCount << ","
             << "\"isReachable\":" << (result.isReachable ? "true" : "false")
             << "}";
    }
    json << "]";
    return json.str();
}
