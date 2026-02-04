# DNS Changer - Release Build Script
# Builds C++ DLL, publishes C# app for Windows (self-contained), ready for installer

param(
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"
$ProjectRoot = $PSScriptRoot

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "DNS Changer - Release Build v$Version" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Build C++ DLL
Write-Host "[1/4] Building C++ DLL..." -ForegroundColor Green
Push-Location "$ProjectRoot\cpp_core"
if (-not (Test-Path "build")) { New-Item -ItemType Directory -Path "build" | Out-Null }
Push-Location "build"

cmake .. -G "Visual Studio 17 2022" -A x64
if ($LASTEXITCODE -ne 0) { Pop-Location; Pop-Location; exit 1 }
cmake --build . --config $Configuration
if ($LASTEXITCODE -ne 0) { Pop-Location; Pop-Location; exit 1 }

$dllSrc = $null
foreach ($p in @("bin\$Configuration\dns_core.dll", "$Configuration\dns_core.dll", "x64\$Configuration\dns_core.dll")) {
    if (Test-Path $p) { $dllSrc = $p; break }
}
if (-not $dllSrc) { Write-Host "Error: dns_core.dll not found." -ForegroundColor Red; exit 1 }

Pop-Location
Pop-Location

# Step 2: Copy DLL to WPF project
Write-Host "[2/4] Copying DLL to WPF project..." -ForegroundColor Green
Copy-Item "$ProjectRoot\cpp_core\build\$dllSrc" -Destination "$ProjectRoot\windows_gui\dns_core.dll" -Force

# Step 3: Publish WPF Application (self-contained, single file optional)
Write-Host "[3/4] Publishing WPF Application..." -ForegroundColor Green
Push-Location "$ProjectRoot\windows_gui"

$PublishPath = "$ProjectRoot\publish\DNSChanger"
if (Test-Path $PublishPath) { Remove-Item $PublishPath -Recurse -Force }
New-Item -ItemType Directory -Path $PublishPath -Force | Out-Null

dotnet publish -c $Configuration -r win-x64 --self-contained true `
    -p:PublishSingleFile=false `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $PublishPath

if ($LASTEXITCODE -ne 0) { Write-Host "Error: Publish failed." -ForegroundColor Red; Pop-Location; exit 1 }

# Ensure DLL is in publish output
Copy-Item "$ProjectRoot\windows_gui\dns_core.dll" -Destination "$PublishPath\dns_core.dll" -Force
Pop-Location

# Step 4: Create version file for installer
Write-Host "[4/4] Preparing release artifacts..." -ForegroundColor Green
Set-Content -Path "$ProjectRoot\publish\DNSChanger\version.txt" -Value $Version

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Release build complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Output: $ProjectRoot\publish\DNSChanger\" -ForegroundColor Yellow
Write-Host "Run Inno Setup (installer.iss) to create the Windows installer." -ForegroundColor Yellow
Write-Host ""
