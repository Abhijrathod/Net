# DNS Changer - Windows DNS Management Application

A production-grade Windows DNS Changer application with a high-performance C++20 core and a beautiful modern C# WPF desktop interface.

## Architecture

### Layer 1 — Native Engine (C++)
- **DLL**: `dns_core.dll`
- Uses WinAPI with netsh fallback for DNS management
- Exposes clean C API for C# via P/Invoke
- Returns all data as JSON
- Logs to: `C:\ProgramData\DnsChanger\dns_core.log`

### Layer 2 — Windows UI (C# .NET 8)
- **Executable**: `DNSChanger.exe`
- WPF with MVVM pattern
- Dark theme by default
- Smooth animations, modern typography, rounded panels
- System tray icon + background service mode

## Project Structure

```
dns-changer/
├── cpp_core/
│   ├── include/          # Header files
│   ├── src/              # Source files
│   └── CMakeLists.txt   # CMake build configuration
│
├── windows_gui/
│   ├── Models/          # Data models
│   ├── ViewModels/      # MVVM view models
│   ├── Services/        # Business logic services
│   ├── Controls/        # Custom controls
│   ├── Resources/       # Styles and resources
│   ├── Converters/      # Value converters
│   └── DNSChanger.csproj
│
└── README.md
```

## Features

- **Network Discovery**: Automatically detects all network adapters (WiFi, Ethernet, VPN, Virtual)
- **DNS Server Catalog**: Includes 30+ pre-configured DNS servers (Google, Cloudflare, Quad9, OpenDNS, AdGuard, etc.)
- **Custom DNS Support**: Add your own DNS servers
- **Speed Testing**: Real DNS query testing with latency, packet loss, and stability metrics
- **One-Click Apply**: Apply DNS settings to any network adapter
- **Restore Default**: Restore DHCP DNS settings
- **System Tray**: Background operation with system tray integration
- **Modern UI**: Premium dark theme with smooth animations

## Build Requirements

### C++ DLL Build
- **CMake** 3.20 or higher
- **Visual Studio 2022** (or MSVC compiler) with C++20 support
- **Windows SDK** 10.0 or higher

### C# WPF Application Build
- **.NET 8 SDK**
- **Visual Studio 2022** (or Visual Studio Code with C# extension)

## Build Instructions

### Step 1: Build the C++ DLL

1. Open PowerShell or Command Prompt in the `cpp_core` directory:

```powershell
cd cpp_core
```

2. Create a build directory:

```powershell
mkdir build
cd build
```

3. Generate Visual Studio solution with CMake:

```powershell
cmake .. -G "Visual Studio 17 2022" -A x64
```

Or for a different Visual Studio version:
```powershell
cmake .. -G "Visual Studio 16 2019" -A x64
```

4. Build the solution:

```powershell
cmake --build . --config Release
```

Or open the generated `.sln` file in Visual Studio and build from there.

5. The DLL will be created at: `build/bin/Release/dns_core.dll` (or `build/Release/dns_core.dll`)

### Step 2: Copy DLL to WPF Project

Copy `dns_core.dll` to the `windows_gui` directory:

```powershell
copy build\bin\Release\dns_core.dll ..\..\windows_gui\
```

Or if the DLL is in a different location:
```powershell
copy build\Release\dns_core.dll ..\..\windows_gui\
```

### Step 3: Build the WPF Application

1. Navigate to the `windows_gui` directory:

```powershell
cd ..\..\windows_gui
```

2. Restore NuGet packages:

```powershell
dotnet restore
```

3. Build the application:

```powershell
dotnet build --configuration Release
```

4. Run the application:

```powershell
dotnet run --configuration Release
```

Or build and run directly:
```powershell
dotnet run
```

### Alternative: Build Everything with a Script

Create a `build.ps1` script in the root directory:

```powershell
# Build C++ DLL
Write-Host "Building C++ DLL..." -ForegroundColor Green
cd cpp_core
if (-not (Test-Path build)) { mkdir build }
cd build
cmake .. -G "Visual Studio 17 2022" -A x64
cmake --build . --config Release
cd ..\..

# Copy DLL
Write-Host "Copying DLL..." -ForegroundColor Green
$dllPath = "cpp_core\build\bin\Release\dns_core.dll"
if (-not (Test-Path $dllPath)) {
    $dllPath = "cpp_core\build\Release\dns_core.dll"
}
Copy-Item $dllPath -Destination "windows_gui\dns_core.dll" -Force

# Build WPF App
Write-Host "Building WPF Application..." -ForegroundColor Green
cd windows_gui
dotnet restore
dotnet build --configuration Release
cd ..

Write-Host "Build complete!" -ForegroundColor Green
```

Run with:
```powershell
.\build.ps1
```

## Releases and Windows Installer

### Download the installer (recommended)

1. Go to [Releases](https://github.com/Abhijrathod/Net/releases).
2. Download the latest **DNSChanger-Setup-x.x.x.exe**.
3. Run the installer (you may need to approve UAC).
4. Launch **DNS Changer** from the Start menu or desktop. **Run as administrator** when changing DNS.

### Creating a new release on GitHub

1. **Create a release build and installer locally** (optional, for testing):
   ```powershell
   .\build-release.ps1 -Version "1.0.0"
   ```
   Then install [Inno Setup 6](https://jrsoftware.org/isinfo.php) and build the installer:
   ```powershell
   "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer.iss
   ```
   The installer will be in the `release\` folder.

2. **Publish a release via GitHub** (creates the installer automatically):
   - Create and push a version tag, e.g. `v1.0.0`:
     ```powershell
     git tag v1.0.0
     git push origin v1.0.0
     ```
   - The [Release workflow](.github/workflows/release.yml) will build the app and create **DNSChanger-Setup-1.0.0.exe** and attach it to the new release.

### Installer contents

- Installs to `C:\Program Files\DNS Changer\` (64-bit).
- Adds Start menu and optional desktop shortcut.
- Requires **Windows 10/11 64-bit** and **run as administrator** for DNS changes.

## Usage

### Running the Application

1. **Run as Administrator** (required for DNS changes):
   - Right-click `DNSChanger.exe` → "Run as administrator"
   - Or set the application to always run as admin in Properties → Compatibility

2. **Select Network Adapter**:
   - Choose a network adapter from the left panel
   - View current IP and DNS settings

3. **Test DNS Speed** (optional):
   - Click "Run Speed Test"
   - Wait for results (may take 1-2 minutes)
   - Results show latency, packet loss, and stability

4. **Apply DNS**:
   - Select a DNS server from the table
   - Click "Apply DNS"
   - Confirm the action

5. **Restore Default**:
   - Select a network adapter
   - Click "Restore Default"
   - DNS will be set back to DHCP

### System Tray

- Right-click the system tray icon for quick actions
- Double-click to restore the main window

## API Reference (C++ DLL)

The DLL exposes the following C API functions:

```c
// Get list of network adapters as JSON
const char* list_networks_json();

// Get list of DNS servers as JSON
const char* list_dns_servers_json();

// Test DNS speed for all servers (returns JSON)
const char* test_dns_speed_json();

// Set DNS servers for an interface
bool set_dns(const char* interface_name, const char* dns1, const char* dns2);

// Restore default DNS (DHCP) for an interface
bool restore_default_dns(const char* interface_name);

// Free a string returned by the API
void free_dns_string(const char* str);
```

## Troubleshooting

### DLL Not Found
- Ensure `dns_core.dll` is in the same directory as `DNSChanger.exe`
- Check that the DLL was built for the correct architecture (x64)

### Access Denied Errors
- Run the application as Administrator
- Check Windows UAC settings

### DNS Changes Not Applied
- Verify administrator privileges
- Check Windows Firewall settings
- Review logs at `C:\ProgramData\DnsChanger\dns_core.log`

### Build Errors

**CMake not found:**
- Install CMake from https://cmake.org/download/
- Add CMake to PATH

**Visual Studio not found:**
- Install Visual Studio 2022 with C++ Desktop Development workload
- Or use Visual Studio Build Tools

**NET 8 SDK not found:**
- Download from https://dotnet.microsoft.com/download/dotnet/8.0
- Install .NET 8 SDK

## License

This project is provided as-is for educational and personal use.

## Contributing

Contributions are welcome! Please ensure:
- Code follows existing style conventions
- C++ code uses C++20 standard
- C# code follows MVVM pattern
- All builds complete successfully

## Notes

- The application requires administrator privileges to change DNS settings
- DNS speed testing may take several minutes for all servers
- Some DNS servers may be unreachable depending on your network configuration
- IPv6 DNS servers are supported but may not work on all networks
