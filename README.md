# DNS Changer

A Windows desktop application for managing DNS servers across your network adapters. Built with a high-performance C++20 core and a modern C# WPF interface.

---

## Architecture

### Layer 1 — Native engine (C++)

| Component | Description |
|-----------|-------------|
| **DLL** | `dns_core.dll` |
| **API** | WinAPI with netsh fallback for DNS management |
| **Interop** | C API for C# via P/Invoke; all data as JSON |
| **Logs** | `C:\ProgramData\DnsChanger\dns_core.log` |

### Layer 2 — Desktop UI (C# .NET 8)

| Component | Description |
|-----------|-------------|
| **Executable** | `DNSChanger.exe` |
| **Stack** | WPF, MVVM |
| **Theme** | Dark by default; modern typography and layout |
| **Extras** | System tray icon, background mode |

---

## Features

- **Network discovery** — Detects adapters (Wi‑Fi, Ethernet, VPN, virtual)
- **DNS catalog** — 30+ providers (Google, Cloudflare, Quad9, OpenDNS, AdGuard, etc.)
- **Custom DNS** — Add and manage your own servers
- **Speed test** — Real DNS queries with latency, packet loss, and stability
- **One-click apply** — Set DNS per adapter
- **Restore default** — Revert to DHCP DNS
- **System tray** — Quick actions and background operation

---

## Requirements

| Layer | Requirements |
|-------|--------------|
| **C++ DLL** | CMake 3.20+, Visual Studio 2022 (or MSVC), Windows SDK 10.0+ |
| **WPF app** | .NET 8 SDK, Visual Studio 2022 or VS Code with C# |

---

## Build

### 1. Build the C++ DLL

```powershell
cd cpp_core
mkdir build -ErrorAction SilentlyContinue
cd build
cmake .. -G "Visual Studio 17 2022" -A x64
cmake --build . --config Release
```

Output: `build\bin\Release\dns_core.dll` (or `build\Release\dns_core.dll`).

### 2. Copy DLL into the WPF project

From `cpp_core\build`:

```powershell
copy bin\Release\dns_core.dll ..\..\windows_gui\
# or, if different layout:
copy Release\dns_core.dll ..\..\windows_gui\
```

### 3. Build and run the WPF app

```powershell
cd ..\..\windows_gui
dotnet restore
dotnet build --configuration Release
dotnet run --configuration Release
```

### One-command build

From the repository root:

```powershell
.\build.ps1
```

This builds the C++ DLL, copies it to `windows_gui`, and builds the WPF application.

---

## Release installer

### Download (recommended)

1. Open [Releases](https://github.com/Abhijrathod/Net/releases).
2. Download the latest **DNSChanger-Setup-x.x.x.exe**.
3. Run the installer (UAC may prompt).
4. Launch **DNS Changer** from the Start menu or desktop. Use **Run as administrator** when changing DNS.
