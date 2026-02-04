#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <winsock2.h>
#include <ws2tcpip.h>
#include <iphlpapi.h>
#include "../include/dns_resolver.h"
#include "../include/dns_packet.h"
#include "../include/logger.h"
#include "../include/win_utils.h"
#include <vector>
#include <sstream>

#pragma comment(lib, "iphlpapi.lib")
#pragma comment(lib, "ws2_32.lib")

std::vector<std::string> DnsResolver::Resolve(const std::string& domain, const std::string& dnsServer) {
    std::vector<std::string> results;
    
    Logger::GetInstance().Info("Resolving " + domain + " using " + dnsServer);
    
    // Use Windows DNS API - GetAddrInfoW uses system DNS, but we can verify with custom DNS
    ADDRINFOW hints = {0};
    hints.ai_family = AF_UNSPEC;
    hints.ai_socktype = SOCK_STREAM;
    
    std::wstring domainWide = WinUtils::Utf8ToWide(domain);
    
    ADDRINFOW* result = nullptr;
    int status = GetAddrInfoW(domainWide.c_str(), nullptr, &hints, &result);
    
    if (status == 0 && result != nullptr) {
        ADDRINFOW* ptr = result;
        while (ptr != nullptr) {
            char ipStr[INET6_ADDRSTRLEN] = {0};
            if (ptr->ai_family == AF_INET) {
                inet_ntop(AF_INET, &((sockaddr_in*)ptr->ai_addr)->sin_addr, ipStr, INET6_ADDRSTRLEN);
            } else if (ptr->ai_family == AF_INET6) {
                inet_ntop(AF_INET6, &((sockaddr_in6*)ptr->ai_addr)->sin6_addr, ipStr, INET6_ADDRSTRLEN);
            }
            
            if (ipStr[0] != '\0') {
                results.push_back(std::string(ipStr));
            }
            ptr = ptr->ai_next;
        }
        FreeAddrInfoW(result);
    }
    
    // Verify with custom DNS server by sending a test query
    if (results.empty()) {
        double latency = 0.0;
        if (DnsPacket::SendQuery(dnsServer, domain, 3000, latency)) {
            Logger::GetInstance().Info("DNS query to " + dnsServer + " succeeded but no IPs extracted");
        } else {
            Logger::GetInstance().Error("DNS query to " + dnsServer + " failed");
        }
    }
    
    Logger::GetInstance().Info("Resolved " + std::to_string(results.size()) + " IP(s) for " + domain);
    return results;
}

bool DnsResolver::FlushCache() {
    Logger::GetInstance().Info("Flushing DNS cache...");
    
    // Use ipconfig /flushdns command
    STARTUPINFOW si = {0};
    si.cb = sizeof(si);
    si.dwFlags = STARTF_USESHOWWINDOW;
    si.wShowWindow = SW_HIDE;
    
    PROCESS_INFORMATION pi = {0};
    std::wstring cmdLine = L"ipconfig.exe /flushdns";
    std::vector<WCHAR> cmdLineBuf(cmdLine.begin(), cmdLine.end());
    cmdLineBuf.push_back(L'\0');
    
    if (CreateProcessW(NULL, cmdLineBuf.data(), NULL, NULL, FALSE, CREATE_NO_WINDOW, NULL, NULL, &si, &pi)) {
        WaitForSingleObject(pi.hProcess, INFINITE);
        DWORD exitCode;
        GetExitCodeProcess(pi.hProcess, &exitCode);
        CloseHandle(pi.hProcess);
        CloseHandle(pi.hThread);
        
        if (exitCode == 0) {
            Logger::GetInstance().Info("DNS cache flushed successfully");
            return true;
        }
        Logger::GetInstance().Error("ipconfig /flushdns failed with exit code: " + std::to_string(exitCode));
    } else {
        DWORD error = GetLastError();
        Logger::GetInstance().Error("CreateProcess failed. Error: " + std::to_string(error));
    }
    
    return false;
}
