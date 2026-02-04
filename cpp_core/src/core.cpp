#include "../include/core.h"
#include "../include/network_windows.h"
#include "../include/dns_catalog.h"
#include "../include/dns_latency.h"
#include "../include/dns_apply_windows.h"
#include "../include/dns_resolver.h"
#include "../include/json_serializer.h"
#include "../include/logger.h"

NetworkList Core::GetNetworks() {
    return NetworkDiscovery::DiscoverAdapters();
}

std::vector<DnsServer> Core::GetDnsServers() {
    return DnsCatalog::GetAllIncludingCustom();
}

std::vector<DnsTestResult> Core::TestDnsSpeed(int testCount, int timeoutMs) {
    auto servers = GetDnsServers();
    return DnsLatencyTester::TestServers(servers, testCount, timeoutMs);
}

bool Core::SetDns(const std::string& interfaceGuid, const std::string& dns1, const std::string& dns2) {
    return DnsManager::SetDns(interfaceGuid, dns1, dns2);
}

bool Core::RestoreDefaultDns(const std::string& interfaceGuid) {
    return DnsManager::RestoreDefaultDns(interfaceGuid);
}

void Core::AddCustomDnsServer(const DnsServer& server) {
    DnsCatalog::AddCustomServer(server);
}

std::vector<std::string> Core::ResolveDomain(const std::string& domain, const std::string& dnsServer) {
    return DnsResolver::Resolve(domain, dnsServer);
}

bool Core::FlushDnsCache() {
    return DnsResolver::FlushCache();
}
