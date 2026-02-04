#pragma once

#include <string>
#include <fstream>
#include <mutex>

class Logger {
public:
    static Logger& GetInstance();
    void Log(const std::string& level, const std::string& message);
    void Info(const std::string& message);
    void Error(const std::string& message);
    void Debug(const std::string& message);
    
private:
    Logger();
    ~Logger();
    std::ofstream logFile_;
    std::mutex mutex_;
    static constexpr const char* LOG_PATH = "C:\\ProgramData\\DnsChanger\\dns_core.log";
};
