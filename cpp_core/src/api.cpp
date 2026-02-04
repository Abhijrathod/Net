#include "../include/dns_api.h"
#include "../include/core.h"
#include "../include/json_serializer.h"
#include "../include/logger.h"
#include <cstring>
#include <cstdlib>

// Thread-safe string storage for JSON responses
thread_local static std::string g_lastJsonResponse;

const char* list_networks_json() {
    try {
        auto networks = Core::GetNetworks();
        g_lastJsonResponse = JsonSerializer::NetworkListToJson(networks);
        Logger::GetInstance().Info("Returning network list JSON");
        return g_lastJsonResponse.c_str();
    } catch (const std::exception& e) {
        Logger::GetInstance().Error("Error in list_networks_json: " + std::string(e.what()));
        g_lastJsonResponse = "[]";
        return g_lastJsonResponse.c_str();
    }
}

const char* list_dns_servers_json() {
    try {
        auto servers = Core::GetDnsServers();
        g_lastJsonResponse = JsonSerializer::DnsServerListToJson(servers);
        Logger::GetInstance().Info("Returning DNS server list JSON");
        return g_lastJsonResponse.c_str();
    } catch (const std::exception& e) {
        Logger::GetInstance().Error("Error in list_dns_servers_json: " + std::string(e.what()));
        g_lastJsonResponse = "[]";
        return g_lastJsonResponse.c_str();
    }
}

const char* test_dns_speed_json() {
    try {
        Logger::GetInstance().Info("Starting DNS speed test...");
        auto results = Core::TestDnsSpeed(3, 2000); // 3 tests per server, 2 second timeout (faster)
        g_lastJsonResponse = JsonSerializer::DnsTestResultsToJson(results);
        Logger::GetInstance().Info("DNS speed test completed - " + std::to_string(results.size()) + " results");
        return g_lastJsonResponse.c_str();
    } catch (const std::exception& e) {
        Logger::GetInstance().Error("Error in test_dns_speed_json: " + std::string(e.what()));
        g_lastJsonResponse = "[]";
        return g_lastJsonResponse.c_str();
    }
}

const char* resolve_domain_json(const char* domain, const char* dnsServer) {
    try {
        if (!domain || !dnsServer) {
            Logger::GetInstance().Error("Invalid parameters for resolve_domain_json");
            g_lastJsonResponse = "[]";
            return g_lastJsonResponse.c_str();
        }
        
        Logger::GetInstance().Info("Resolving " + std::string(domain) + " using " + std::string(dnsServer));
        auto ips = Core::ResolveDomain(domain, dnsServer);
        g_lastJsonResponse = JsonSerializer::IpListToJson(ips);
        return g_lastJsonResponse.c_str();
    } catch (const std::exception& e) {
        Logger::GetInstance().Error("Error in resolve_domain_json: " + std::string(e.what()));
        g_lastJsonResponse = "[]";
        return g_lastJsonResponse.c_str();
    }
}

bool flush_dns_cache() {
    try {
        return Core::FlushDnsCache();
    } catch (const std::exception& e) {
        Logger::GetInstance().Error("Error in flush_dns_cache: " + std::string(e.what()));
        return false;
    }
}

bool set_dns(const char* interface_name, const char* dns1, const char* dns2) {
    try {
        if (!interface_name || !dns1) {
            Logger::GetInstance().Error("Invalid parameters for set_dns");
            return false;
        }
        
        std::string dns2Str = dns2 ? dns2 : "";
        bool result = Core::SetDns(interface_name, dns1, dns2Str);
        Logger::GetInstance().Info("set_dns result: " + std::string(result ? "success" : "failed"));
        return result;
    } catch (const std::exception& e) {
        Logger::GetInstance().Error("Error in set_dns: " + std::string(e.what()));
        return false;
    }
}

bool restore_default_dns(const char* interface_name) {
    try {
        if (!interface_name) {
            Logger::GetInstance().Error("Invalid parameter for restore_default_dns");
            return false;
        }
        
        bool result = Core::RestoreDefaultDns(interface_name);
        Logger::GetInstance().Info("restore_default_dns result: " + std::string(result ? "success" : "failed"));
        return result;
    } catch (const std::exception& e) {
        Logger::GetInstance().Error("Error in restore_default_dns: " + std::string(e.what()));
        return false;
    }
}

void free_dns_string(const char* str) {
    // Thread-local storage is automatically cleaned up, but we provide this for API completeness
    // In a production system, you might want to use a proper memory pool
}
