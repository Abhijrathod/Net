#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <winsock2.h>
#include <ws2tcpip.h>
#include "../include/dns_packet.h"
#include "../include/logger.h"
#include <iostream>
#include <cstring>
#include <chrono>

#pragma comment(lib, "ws2_32.lib")

namespace {
    bool InitializeWinsock() {
        static bool initialized = false;
        if (!initialized) {
            WSADATA wsaData;
            int result = WSAStartup(MAKEWORD(2, 2), &wsaData);
            if (result != 0) {
                Logger::GetInstance().Error("WSAStartup failed: " + std::to_string(result));
                return false;
            }
            initialized = true;
        }
        return true;
    }
}

bool DnsPacket::SendQuery(const std::string& dnsServer, const std::string& queryDomain, int timeoutMs, double& latencyMs) {
    if (!InitializeWinsock()) {
        return false;
    }
    
    // Create UDP socket
    SOCKET sock = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP);
    if (sock == INVALID_SOCKET) {
        Logger::GetInstance().Error("Failed to create socket");
        return false;
    }
    
    // Set timeout
    DWORD timeout = timeoutMs;
    setsockopt(sock, SOL_SOCKET, SO_RCVTIMEO, (const char*)&timeout, sizeof(timeout));
    
    // Build DNS query packet (simplified - just A record query)
    unsigned char query[512];
    int queryLen = BuildDnsQuery(queryDomain, query, sizeof(query));
    
    if (queryLen <= 0) {
        closesocket(sock);
        return false;
    }
    
    // Setup server address
    sockaddr_in serverAddr;
    memset(&serverAddr, 0, sizeof(serverAddr));
    serverAddr.sin_family = AF_INET;
    serverAddr.sin_port = htons(53);
    inet_pton(AF_INET, dnsServer.c_str(), &serverAddr.sin_addr);
    
    // Send query and measure time
    auto start = std::chrono::high_resolution_clock::now();
    
    int sent = sendto(sock, (const char*)query, queryLen, 0, (sockaddr*)&serverAddr, sizeof(serverAddr));
    if (sent != queryLen) {
        closesocket(sock);
        return false;
    }
    
    // Receive response
    unsigned char response[512];
    sockaddr_in fromAddr;
    int fromLen = sizeof(fromAddr);
    int received = recvfrom(sock, (char*)response, sizeof(response), 0, (sockaddr*)&fromAddr, &fromLen);
    
    auto end = std::chrono::high_resolution_clock::now();
    auto duration = std::chrono::duration_cast<std::chrono::microseconds>(end - start);
    latencyMs = duration.count() / 1000.0;
    
    closesocket(sock);
    
    if (received <= 0) {
        return false;
    }
    
    // Basic validation - check if response has answer
    return ParseDnsResponse(response, received);
}

int DnsPacket::BuildDnsQuery(const std::string& domain, unsigned char* buffer, size_t bufferSize) {
    if (bufferSize < 512) return -1;
    
    int pos = 0;
    
    // DNS Header
    unsigned short id = 0x1234;
    memcpy(buffer + pos, &id, 2);
    pos += 2;
    
    unsigned short flags = htons(0x0100); // Standard query, recursion desired
    memcpy(buffer + pos, &flags, 2);
    pos += 2;
    
    unsigned short qdcount = htons(1); // 1 question
    memcpy(buffer + pos, &qdcount, 2);
    pos += 2;
    
    unsigned short ancount = 0;
    memcpy(buffer + pos, &ancount, 2);
    pos += 2;
    
    unsigned short nscount = 0;
    memcpy(buffer + pos, &nscount, 2);
    pos += 2;
    
    unsigned short arcount = 0;
    memcpy(buffer + pos, &arcount, 2);
    pos += 2;
    
    // Question section
    // Domain name
    std::string domainCopy = domain;
    size_t dotPos = 0;
    while ((dotPos = domainCopy.find('.')) != std::string::npos) {
        std::string label = domainCopy.substr(0, dotPos);
        buffer[pos++] = label.length();
        memcpy(buffer + pos, label.c_str(), label.length());
        pos += label.length();
        domainCopy = domainCopy.substr(dotPos + 1);
    }
    if (!domainCopy.empty()) {
        buffer[pos++] = domainCopy.length();
        memcpy(buffer + pos, domainCopy.c_str(), domainCopy.length());
        pos += domainCopy.length();
    }
    buffer[pos++] = 0; // Null terminator
    
    // QTYPE: A record
    unsigned short qtype = htons(1);
    memcpy(buffer + pos, &qtype, 2);
    pos += 2;
    
    // QCLASS: IN
    unsigned short qclass = htons(1);
    memcpy(buffer + pos, &qclass, 2);
    pos += 2;
    
    return pos;
}

bool DnsPacket::ParseDnsResponse(unsigned char* response, int responseLen) {
    if (responseLen < 12) return false;
    
    // Check response code (bits 3-6 of flags byte)
    unsigned short flags = ntohs(*(unsigned short*)(response + 2));
    unsigned char rcode = flags & 0x0F;
    
    if (rcode != 0) {
        return false; // Error in response
    }
    
    // Check if there's at least one answer
    unsigned short ancount = ntohs(*(unsigned short*)(response + 6));
    return ancount > 0;
}
