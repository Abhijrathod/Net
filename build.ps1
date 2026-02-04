# DNS Changer Build Script
# Builds both C++ DLL and C# WPF application

param(
    [string]$Configuration = "Release",
    [string]$Platform = "x64"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "DNS Changer Build Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Build C++ DLL
Write-Host "[1/3] Building C++ DLL..." -ForegroundColor Green
Push-Location "cpp_core"

if (-not (Test-Path "build")) {
    New-Item -ItemType Directory -Path "build" | Out-Null
}

Push-Location "build"

# Detect Visual Studio version
$vsVersion = "Visual Studio 17 2022"
if (-not (Get-Command cmake -ErrorAction SilentlyContinue)) {
    Write-Host "Error: CMake not found. Please install CMake." -ForegroundColor Red
    exit 1
}

Write-Host "  Generating CMake files..." -ForegroundColor Yellow
cmake .. -G $vsVersion -A $Platform

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: CMake configuration failed." -ForegroundColor Red
    Pop-Location
    Pop-Location
    exit 1
}

Write-Host "  Building DLL..." -ForegroundColor Yellow
cmake --build . --config $Configuration

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: DLL build failed." -ForegroundColor Red
    Pop-Location
    Pop-Location
    exit 1
}

Pop-Location
Pop-Location

# Step 2: Copy DLL to WPF project
Write-Host "[2/3] Copying DLL to WPF project..." -ForegroundColor Green

$dllPaths = @(
    "cpp_core\build\bin\$Configuration\dns_core.dll",
    "cpp_core\build\$Configuration\dns_core.dll",
    "cpp_core\build\bin\$Platform\$Configuration\dns_core.dll"
)

$dllFound = $false
foreach ($dllPath in $dllPaths) {
    if (Test-Path $dllPath) {
        Copy-Item $dllPath -Destination "windows_gui\dns_core.dll" -Force
        Write-Host "  Copied: $dllPath -> windows_gui\dns_core.dll" -ForegroundColor Yellow
        $dllFound = $true
        break
    }
}

if (-not $dllFound) {
    Write-Host "Warning: DLL not found in expected locations. Please copy manually." -ForegroundColor Yellow
    Write-Host "  Expected locations:" -ForegroundColor Yellow
    foreach ($dllPath in $dllPaths) {
        Write-Host "    - $dllPath" -ForegroundColor Yellow
    }
}

# Step 3: Build WPF Application
Write-Host "[3/3] Building WPF Application..." -ForegroundColor Green
Push-Location "windows_gui"

Write-Host "  Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: NuGet restore failed." -ForegroundColor Red
    Pop-Location
    exit 1
}

Write-Host "  Building application..." -ForegroundColor Yellow
dotnet build --configuration $Configuration

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: WPF application build failed." -ForegroundColor Red
    Pop-Location
    exit 1
}

Pop-Location

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Build Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Output location: windows_gui\bin\$Configuration\net8.0-windows\" -ForegroundColor Yellow
Write-Host ""
Write-Host "To run the application:" -ForegroundColor Cyan
Write-Host "  cd windows_gui" -ForegroundColor White
Write-Host "  dotnet run --configuration $Configuration" -ForegroundColor White
Write-Host ""
