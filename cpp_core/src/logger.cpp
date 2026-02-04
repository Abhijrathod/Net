#include "../include/logger.h"
#include <iostream>
#include <sstream>
#include <iomanip>
#include <chrono>
#include <filesystem>

Logger& Logger::GetInstance() {
    static Logger instance;
    return instance;
}

Logger::Logger() {
    // Ensure directory exists
    std::filesystem::path logPath(LOG_PATH);
    std::filesystem::create_directories(logPath.parent_path());
    
    logFile_.open(LOG_PATH, std::ios::app);
    if (!logFile_.is_open()) {
        std::cerr << "Failed to open log file: " << LOG_PATH << std::endl;
    }
}

Logger::~Logger() {
    if (logFile_.is_open()) {
        logFile_.close();
    }
}

void Logger::Log(const std::string& level, const std::string& message) {
    std::lock_guard<std::mutex> lock(mutex_);
    
    auto now = std::chrono::system_clock::now();
    auto time = std::chrono::system_clock::to_time_t(now);
    auto ms = std::chrono::duration_cast<std::chrono::milliseconds>(
        now.time_since_epoch()) % 1000;
    
    std::stringstream ss;
    ss << std::put_time(std::localtime(&time), "%Y-%m-%d %H:%M:%S");
    ss << "." << std::setfill('0') << std::setw(3) << ms.count();
    ss << " [" << level << "] " << message << std::endl;
    
    std::string logLine = ss.str();
    
    if (logFile_.is_open()) {
        logFile_ << logLine;
        logFile_.flush();
    }
    
    std::cout << logLine;
}

void Logger::Info(const std::string& message) {
    Log("INFO", message);
}

void Logger::Error(const std::string& message) {
    Log("ERROR", message);
}

void Logger::Debug(const std::string& message) {
    Log("DEBUG", message);
}
