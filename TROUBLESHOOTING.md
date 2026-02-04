# Troubleshooting Guide

## Issue: Application runs but shows no data

### Quick Checks

1. **Verify DLL exists:**
   ```powershell
   Test-Path "windows_gui\bin\Release\net8.0-windows\dns_core.dll"
   ```
   Should return `True`

2. **Check log file:**
   ```
   C:\ProgramData\DnsChanger\dns_core.log
   ```
   This file contains C++ DLL logs and will show any errors.

3. **Run with Visual Studio Output Window:**
   - Open the project in Visual Studio
   - Run in Debug mode
   - Check the Output window for debug messages

### Common Issues

#### DLL Not Found
**Symptoms:** Empty lists, no error messages

**Solution:**
- Ensure `dns_core.dll` is in the same directory as `DNSChanger.exe`
- Check the output directory: `windows_gui\bin\Release\net8.0-windows\`
- The DLL should be automatically copied during build

#### DLL Functions Not Exported
**Symptoms:** DllNotFoundException or EntryPointNotFoundException

**Solution:**
- Rebuild the C++ DLL:
  ```powershell
  cd cpp_core\build
  cmake --build . --config Release
  ```
- Verify exports using:
  ```powershell
  dumpbin /exports windows_gui\bin\Release\net8.0-windows\dns_core.dll
  ```

#### Empty JSON Returned
**Symptoms:** Lists are empty but no errors

**Solution:**
- Check `C:\ProgramData\DnsChanger\dns_core.log` for C++ errors
- Verify network adapters are enabled in Windows
- Check Windows Firewall isn't blocking the application

### Debug Steps

1. **Enable Debug Output:**
   - The application now writes debug messages to Visual Studio Output window
   - Look for messages like:
     - "DLL found at: ..."
     - "Networks JSON: ..."
     - "Deserialized X network adapters"

2. **Check DLL Loading:**
   - The application will show a message box if DLL is not found
   - Check the exact path shown in the error message

3. **Verify Network Discovery:**
   - Run `ipconfig /all` in Command Prompt
   - Compare with what the application discovers
   - Ensure at least one adapter is enabled

4. **Test DLL Functions Manually:**
   - Use a tool like Dependency Walker or Process Explorer
   - Verify all required DLLs are loaded
   - Check for missing dependencies (iphlpapi.dll, ws2_32.dll)

### Rebuild Everything

If nothing works, rebuild from scratch:

```powershell
# 1. Clean C++ build
cd cpp_core\build
cmake --build . --config Release --clean-first

# 2. Copy DLL
cd ..\..
Copy-Item cpp_core\build\bin\Release\dns_core.dll windows_gui\dns_core.dll -Force

# 3. Clean and rebuild C# app
cd windows_gui
dotnet clean
dotnet build --configuration Release

# 4. Verify DLL is in output
Test-Path bin\Release\net8.0-windows\dns_core.dll
```

### Expected Behavior

When working correctly:
- Network adapters list should show all enabled adapters
- DNS servers list should show 30+ pre-configured servers
- Status message should show "Found X network(s)" and "Loaded X DNS server(s)"
- Log file should contain successful operation messages

### Getting Help

If issues persist:
1. Check `C:\ProgramData\DnsChanger\dns_core.log` for C++ errors
2. Check Visual Studio Output window for C# debug messages
3. Verify you're running as Administrator (required for DNS changes)
4. Ensure Windows SDK 10.0+ is installed
5. Verify .NET 8 SDK is installed
