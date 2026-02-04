# Creating a Release on GitHub

This project is set up to build a **Windows installer** and attach it to a GitHub Release when you push a version tag.

## Quick steps to release

1. **Commit and push all changes** to `https://github.com/Abhijrathod/Net`:
   ```powershell
   git add .
   git commit -m "Prepare release 1.0.0"
   git push origin main
   ```

2. **Create and push a version tag** (e.g. `v1.0.0`):
   ```powershell
   git tag v1.0.0
   git push origin v1.0.0
   ```

3. **Wait for the workflow**  
   Go to [Actions](https://github.com/Abhijrathod/Net/actions). The "Release" workflow will run, build the app, create `DNSChanger-Setup-1.0.0.exe`, and create a new [Release](https://github.com/Abhijrathod/Net/releases) with the installer attached.

4. **Users can then**  
   Download `DNSChanger-Setup-1.0.0.exe` from the Releases page and run it to install DNS Changer on Windows (64-bit). They should run the app **as administrator** when changing DNS.

## Building the installer locally (optional)

If you want to test the installer on your machine without using GitHub Actions:

1. **Install Inno Setup 6**: https://jrsoftware.org/isinfo.php  

2. **Run the release build script**:
   ```powershell
   .\build-release.ps1 -Version "1.0.0"
   ```

3. **Build the installer**:
   ```powershell
   & "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" /DMyAppVersion=1.0.0 installer.iss
   ```

4. The installer will be created at `release\DNSChanger-Setup-1.0.0.exe`.
