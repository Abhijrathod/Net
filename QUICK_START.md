# Quick Start Guide

## Prerequisites

1. **Visual Studio 2022** (or VS 2019) with:
   - C++ Desktop Development workload
   - .NET Desktop Development workload

2. **CMake** 3.20+ (download from https://cmake.org/download/)

3. **.NET 8 SDK** (download from https://dotnet.microsoft.com/download/dotnet/8.0)

## Quick Build

### Option 1: Automated Build Script

```powershell
.\build.ps1
```

This will:
- Build the C++ DLL
- Copy it to the WPF project
- Build the WPF application

### Option 2: Manual Build

#### Step 1: Build C++ DLL

```powershell
cd cpp_core
mkdir build
cd build
cmake .. -G "Visual Studio 17 2022" -A x64
cmake --build . --config Release
```

#### Step 2: Copy DLL

```powershell
copy build\bin\Release\dns_core.dll ..\..\windows_gui\
```

#### Step 3: Build WPF App

```powershell
cd ..\..\windows_gui
dotnet restore
dotnet build --configuration Release
```

## Running the Application

**IMPORTANT**: The application requires Administrator privileges to change DNS settings.

```powershell
cd windows_gui\bin\Release\net8.0-windows
.\DNSChanger.exe
```

Or right-click `DNSChanger.exe` → "Run as administrator"

## Project Files Summary

### C++ Core (cpp_core/)
- **Headers**: 14 header files defining interfaces and types
- **Sources**: 12 implementation files
- **Output**: `dns_core.dll` (C API for P/Invoke)

### C# WPF Application (windows_gui/)
- **Models**: NetworkModel, DnsModel
- **ViewModels**: MainViewModel (MVVM pattern)
- **Services**: DnsInterop (P/Invoke), DnsService, UiService
- **UI**: MainWindow.xaml with dark theme
- **Output**: `DNSChanger.exe`

## Key Features Implemented

✅ Network adapter discovery (WiFi, Ethernet, VPN, Virtual)  
✅ 30+ pre-configured DNS servers  
✅ Real DNS speed testing with latency/packet loss/stability  
✅ One-click DNS application  
✅ Restore default DNS (DHCP)  
✅ System tray integration  
✅ Premium dark theme UI  
✅ MVVM architecture  
✅ JSON-based C++ ↔ C# communication  

## Troubleshooting

**"dns_core.dll not found"**
- Ensure DLL is in the same directory as DNSChanger.exe
- Check build output location

**"Access Denied"**
- Run as Administrator
- Check app.manifest is included in build

**Build errors**
- Verify CMake version: `cmake --version`
- Verify .NET SDK: `dotnet --version`
- Check Visual Studio installation

## Next Steps

1. Build the project using the instructions above
2. Test DNS changes on a non-critical network adapter first
3. Customize DNS server list in `cpp_core/src/dns_catalog.cpp`
4. Modify UI styles in `windows_gui/Resources/Styles.xaml`
