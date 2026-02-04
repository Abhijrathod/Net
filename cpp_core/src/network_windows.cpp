#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <winsock2.h>
#include <ws2tcpip.h>
#include <iphlpapi.h>
#include "../include/network_windows.h"
#include "../include/network_types.h"
#include "../include/logger.h"
#include "../include/win_utils.h"
#include <sstream>
#include <vector>

#pragma comment(lib, "iphlpapi.lib")
#pragma comment(lib, "ws2_32.lib")

NetworkList NetworkDiscovery::DiscoverAdapters() {
    NetworkList list;
    Logger::GetInstance().Info("Discovering network adapters...");
    
    ULONG bufferSize = 0;
    GetAdaptersAddresses(AF_UNSPEC, GAA_FLAG_INCLUDE_PREFIX, NULL, NULL, &bufferSize);
    
    if (bufferSize == 0) {
        Logger::GetInstance().Error("Failed to get adapter buffer size");
        return list;
    }
    
    PIP_ADAPTER_ADDRESSES adapterAddresses = (PIP_ADAPTER_ADDRESSES)malloc(bufferSize);
    if (!adapterAddresses) {
        Logger::GetInstance().Error("Failed to allocate memory for adapters");
        return list;
    }
    
    DWORD result = GetAdaptersAddresses(AF_UNSPEC, GAA_FLAG_INCLUDE_PREFIX, NULL, adapterAddresses, &bufferSize);
    
    if (result == NO_ERROR) {
        PIP_ADAPTER_ADDRESSES adapter = adapterAddresses;
        while (adapter) {
            NetworkAdapter netAdapter;
            
            // GUID - AdapterName is ANSI (PCHAR), not wide
            if (adapter->AdapterName) {
                netAdapter.guid = std::string(adapter->AdapterName);
            }
            
            // Name and Description - These are wide strings (PWCHAR)
            if (adapter->FriendlyName) {
                netAdapter.name = WinUtils::WideToUtf8(std::wstring(adapter->FriendlyName));
            }
            if (adapter->Description) {
                netAdapter.description = WinUtils::WideToUtf8(std::wstring(adapter->Description));
            }
            
            // Type
            switch (adapter->IfType) {
                case IF_TYPE_ETHERNET_CSMACD:
                    netAdapter.type = "Ethernet";
                    break;
                case IF_TYPE_IEEE80211:
                    netAdapter.type = "WiFi";
                    break;
                case IF_TYPE_PPP:
                case IF_TYPE_TUNNEL:
                    netAdapter.type = "VPN";
                    break;
                case IF_TYPE_SOFTWARE_LOOPBACK:
                    netAdapter.type = "Loopback";
                    break;
                default:
                    netAdapter.type = "Other";
                    break;
            }
            
            // Status
            netAdapter.isEnabled = (adapter->OperStatus == IfOperStatusUp);
            netAdapter.isConnected = (adapter->OperStatus == IfOperStatusUp);
            
            // IP Address
            PIP_ADAPTER_UNICAST_ADDRESS unicast = adapter->FirstUnicastAddress;
            if (unicast) {
                char ipStr[INET6_ADDRSTRLEN] = {0};
                if (unicast->Address.lpSockaddr->sa_family == AF_INET) {
                    inet_ntop(AF_INET, &((sockaddr_in*)unicast->Address.lpSockaddr)->sin_addr, ipStr, INET6_ADDRSTRLEN);
                } else if (unicast->Address.lpSockaddr->sa_family == AF_INET6) {
                    inet_ntop(AF_INET6, &((sockaddr_in6*)unicast->Address.lpSockaddr)->sin6_addr, ipStr, INET6_ADDRSTRLEN);
                }
                netAdapter.ipAddress = ipStr;
            }
            
            // DNS Servers
            PIP_ADAPTER_DNS_SERVER_ADDRESS dnsServer = adapter->FirstDnsServerAddress;
            if (dnsServer) {
                char dnsStr[INET6_ADDRSTRLEN] = {0};
                if (dnsServer->Address.lpSockaddr->sa_family == AF_INET) {
                    inet_ntop(AF_INET, &((sockaddr_in*)dnsServer->Address.lpSockaddr)->sin_addr, dnsStr, INET6_ADDRSTRLEN);
                } else if (dnsServer->Address.lpSockaddr->sa_family == AF_INET6) {
                    inet_ntop(AF_INET6, &((sockaddr_in6*)dnsServer->Address.lpSockaddr)->sin6_addr, dnsStr, INET6_ADDRSTRLEN);
                }
                netAdapter.dnsPrimary = dnsStr;
                
                dnsServer = dnsServer->Next;
                if (dnsServer) {
                    memset(dnsStr, 0, sizeof(dnsStr));
                    if (dnsServer->Address.lpSockaddr->sa_family == AF_INET) {
                        inet_ntop(AF_INET, &((sockaddr_in*)dnsServer->Address.lpSockaddr)->sin_addr, dnsStr, INET6_ADDRSTRLEN);
                    } else if (dnsServer->Address.lpSockaddr->sa_family == AF_INET6) {
                        inet_ntop(AF_INET6, &((sockaddr_in6*)dnsServer->Address.lpSockaddr)->sin6_addr, dnsStr, INET6_ADDRSTRLEN);
                    }
                    netAdapter.dnsSecondary = dnsStr;
                }
            }
            
            // Only add enabled adapters
            if (netAdapter.isEnabled) {
                list.adapters.push_back(netAdapter);
            }
            
            adapter = adapter->Next;
        }
    } else {
        Logger::GetInstance().Error("GetAdaptersAddresses failed: " + std::to_string(result));
    }
    
    free(adapterAddresses);
    Logger::GetInstance().Info("Found " + std::to_string(list.adapters.size()) + " network adapters");
    return list;
}
