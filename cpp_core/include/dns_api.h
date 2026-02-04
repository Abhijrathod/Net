#pragma once

#ifdef _WIN32
    #ifdef DNS_CORE_EXPORTS
        #define DNS_API __declspec(dllexport)
    #else
        #define DNS_API __declspec(dllimport)
    #endif
#else
    #define DNS_API
#endif

#ifdef __cplusplus
extern "C" {
#endif

// Returns JSON string with list of network adapters
// Caller must free the returned string using free_dns_string()
DNS_API const char* list_networks_json();

// Returns JSON string with list of DNS servers
DNS_API const char* list_dns_servers_json();

// Tests DNS speed for all servers and returns JSON results
// This may take several seconds
DNS_API const char* test_dns_speed_json();

// Resolve a domain name using a specific DNS server
// Returns JSON with IP addresses
DNS_API const char* resolve_domain_json(const char* domain, const char* dnsServer);

// Flush DNS cache
DNS_API bool flush_dns_cache();

// Sets DNS servers for a network interface
// interface_name: GUID or adapter name
// dns1: Primary DNS server IP
// dns2: Secondary DNS server IP (can be empty)
// Returns true on success, false on failure
DNS_API bool set_dns(const char* interface_name, const char* dns1, const char* dns2);

// Restores default DNS (DHCP) for a network interface
// Returns true on success, false on failure
DNS_API bool restore_default_dns(const char* interface_name);

// Frees a string returned by the API
DNS_API void free_dns_string(const char* str);

#ifdef __cplusplus
}
#endif
