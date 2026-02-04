#pragma once

#include <windows.h>
#include <iphlpapi.h>
#include <string>

namespace WinUtils {
    std::string GetLastErrorString();
    bool IsElevated();
    std::string WideToUtf8(const std::wstring& wstr);
    std::wstring Utf8ToWide(const std::string& str);
}
