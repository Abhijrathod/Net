; DNS Changer - Windows Installer (Inno Setup)
; Build after running: .\build-release.ps1
; Or pass version: iscc /DMyAppVersion=1.0.0 installer.iss

#if !exist("publish\DNSChanger\DNSChanger.exe")
  #error "Run build-release.ps1 first to create publish\DNSChanger. From project root: .\build-release.ps1"
#endif

#ifndef MyAppVersion
#define MyAppVersion "1.0.0"
#endif
#define MyAppName "DNS Changer"
#define MyAppPublisher "Abhijrathod"
#define MyAppURL "https://github.com/Abhijrathod/Net"
#define MyAppExeName "DNSChanger.exe"

[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}/releases
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=release
OutputBaseFilename=DNSChanger-Setup-{#MyAppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "publish\DNSChanger\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "Manage DNS servers and network adapters"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; Comment: "DNS Changer"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall runascurrentuser

[Code]
function InitializeSetup(): Boolean;
begin
  Result := True;
  if not IsWin64 then
  begin
    MsgBox('This application is 64-bit only. Please use a 64-bit Windows system.', mbError, MB_OK);
    Result := False;
  end;
end;
