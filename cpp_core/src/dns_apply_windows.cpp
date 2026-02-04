#include "../include/dns_apply_windows.h"
#include "../include/logger.h"
#include "../include/win_utils.h"
#include <windows.h>
#include <netioapi.h>
#include <iphlpapi.h>
#include <sstream>
#include <vector>

#pragma comment(lib, "iphlpapi.lib")
#pragma comment(lib, "netapi32.lib")

bool DnsManager::SetDns(const std::string& interfaceGuid, const std::string& dns1, const std::string& dns2) {
    Logger::GetInstance().Info("Setting DNS for interface: " + interfaceGuid + ", DNS1: " + dns1 + ", DNS2: " + dns2);
    
    if (!WinUtils::IsElevated()) {
        Logger::GetInstance().Error("Administrator privileges required");
        return false;
    }
    
    // Try Windows SDK API first (Windows 10 1809+)
    GUID guid;
    std::wstring guidWide = WinUtils::Utf8ToWide(interfaceGuid);
    HRESULT hr = CLSIDFromString(guidWide.c_str(), &guid);
    if (SUCCEEDED(hr)) {
        // Prepare DNS server addresses
        std::vector<WCHAR> dnsServers;
        std::wstring dns1Wide = WinUtils::Utf8ToWide(dns1);
        
        // Format: "dns1" or "dns1 dns2" (space-separated, null-terminated, double null-terminated)
        dnsServers.resize(dns1Wide.length() + 1);
        wcscpy_s(dnsServers.data(), dnsServers.size(), dns1Wide.c_str());
        
        if (!dns2.empty()) {
            std::wstring dns2Wide = WinUtils::Utf8ToWide(dns2);
            size_t currentSize = dnsServers.size();
            dnsServers.resize(currentSize + dns2Wide.length() + 2);
            dnsServers[currentSize - 1] = L' ';
            wcscpy_s(dnsServers.data() + currentSize, dns2Wide.length() + 1, dns2Wide.c_str());
        }
        dnsServers.push_back(L'\0');
        
        DNS_INTERFACE_SETTINGS settings = {0};
        settings.Version = DNS_INTERFACE_SETTINGS_VERSION1;
        settings.Flags = 0x00000001; // DNS_SETTINGS_NAMESERVER
        settings.NameServer = dnsServers.data();
        
        DWORD status = SetInterfaceDnsSettings(guid, &settings);
        if (status == NO_ERROR) {
            Logger::GetInstance().Info("DNS set successfully using SetInterfaceDnsSettings");
            return true;
        }
        Logger::GetInstance().Info("SetInterfaceDnsSettings failed, falling back to netsh. Error: " + std::to_string(status));
    }
    
    // Fallback to netsh if SDK API fails or not available
    std::wstringstream cmd;
    cmd << L"interface ip set dns \"" << guidWide << L"\" static " << WinUtils::Utf8ToWide(dns1);
    
    STARTUPINFOW si = {0};
    si.cb = sizeof(si);
    si.dwFlags = STARTF_USESHOWWINDOW;
    si.wShowWindow = SW_HIDE;
    
    PROCESS_INFORMATION pi = {0};
    std::wstring cmdLine = L"netsh.exe " + cmd.str();
    std::vector<WCHAR> cmdLineBuf(cmdLine.begin(), cmdLine.end());
    cmdLineBuf.push_back(L'\0');
    
    if (CreateProcessW(NULL, cmdLineBuf.data(), NULL, NULL, FALSE, CREATE_NO_WINDOW, NULL, NULL, &si, &pi)) {
        WaitForSingleObject(pi.hProcess, INFINITE);
        DWORD exitCode;
        GetExitCodeProcess(pi.hProcess, &exitCode);
        CloseHandle(pi.hProcess);
        CloseHandle(pi.hThread);
        
        if (exitCode == 0) {
            // Set secondary DNS if provided
            if (!dns2.empty()) {
                std::wstringstream cmd2;
                cmd2 << L"interface ip add dns \"" << guidWide << L"\" " << WinUtils::Utf8ToWide(dns2) << L" index=2";
                std::wstring cmdLine2 = L"netsh.exe " + cmd2.str();
                std::vector<WCHAR> cmdLineBuf2(cmdLine2.begin(), cmdLine2.end());
                cmdLineBuf2.push_back(L'\0');
                
                PROCESS_INFORMATION pi2 = {0};
                if (CreateProcessW(NULL, cmdLineBuf2.data(), NULL, NULL, FALSE, CREATE_NO_WINDOW, NULL, NULL, &si, &pi2)) {
                    WaitForSingleObject(pi2.hProcess, INFINITE);
                    GetExitCodeProcess(pi2.hProcess, &exitCode);
                    CloseHandle(pi2.hProcess);
                    CloseHandle(pi2.hThread);
                }
            }
            Logger::GetInstance().Info("DNS set successfully using netsh");
            return true;
        }
        Logger::GetInstance().Error("netsh command failed with exit code: " + std::to_string(exitCode));
    } else {
        DWORD error = GetLastError();
        Logger::GetInstance().Error("CreateProcess failed. Error: " + std::to_string(error));
    }
    
    return false;
}

bool DnsManager::RestoreDefaultDns(const std::string& interfaceGuid) {
    Logger::GetInstance().Info("Restoring default DNS for interface: " + interfaceGuid);
    
    if (!WinUtils::IsElevated()) {
        Logger::GetInstance().Error("Administrator privileges required");
        return false;
    }
    
    // Try Windows SDK API first
    GUID guid;
    std::wstring guidWide = WinUtils::Utf8ToWide(interfaceGuid);
    HRESULT hr = CLSIDFromString(guidWide.c_str(), &guid);
    if (SUCCEEDED(hr)) {
        DNS_INTERFACE_SETTINGS settings = {0};
        settings.Version = DNS_INTERFACE_SETTINGS_VERSION1;
        settings.Flags = 0; // Clear flags to use DHCP
        settings.NameServer = NULL; // NULL means use DHCP
        
        DWORD status = SetInterfaceDnsSettings(guid, &settings);
        if (status == NO_ERROR) {
            Logger::GetInstance().Info("Default DNS restored using SetInterfaceDnsSettings");
            return true;
        }
        Logger::GetInstance().Info("SetInterfaceDnsSettings failed, falling back to netsh. Error: " + std::to_string(status));
    }
    
    // Fallback to netsh
    std::wstringstream cmd;
    cmd << L"interface ip set dns \"" << guidWide << L"\" dhcp";
    
    STARTUPINFOW si = {0};
    si.cb = sizeof(si);
    si.dwFlags = STARTF_USESHOWWINDOW;
    si.wShowWindow = SW_HIDE;
    
    PROCESS_INFORMATION pi = {0};
    std::wstring cmdLine = L"netsh.exe " + cmd.str();
    std::vector<WCHAR> cmdLineBuf(cmdLine.begin(), cmdLine.end());
    cmdLineBuf.push_back(L'\0');
    
    if (CreateProcessW(NULL, cmdLineBuf.data(), NULL, NULL, FALSE, CREATE_NO_WINDOW, NULL, NULL, &si, &pi)) {
        WaitForSingleObject(pi.hProcess, INFINITE);
        DWORD exitCode;
        GetExitCodeProcess(pi.hProcess, &exitCode);
        CloseHandle(pi.hProcess);
        CloseHandle(pi.hThread);
        
        if (exitCode == 0) {
            Logger::GetInstance().Info("Default DNS restored using netsh");
            return true;
        }
        Logger::GetInstance().Error("netsh command failed with exit code: " + std::to_string(exitCode));
    } else {
        DWORD error = GetLastError();
        Logger::GetInstance().Error("CreateProcess failed. Error: " + std::to_string(error));
    }
    
    return false;
}
