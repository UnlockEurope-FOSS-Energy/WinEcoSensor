; WinEcoSensor Installer Script
; Inno Setup Script for WinEcoSensor - Windows Eco Energy Sensor
; SPDX-License-Identifier: EUPL-1.2
; Copyright (c) 2025 WinEcoSensor Contributors
;
; This installer script creates a Windows installer for WinEcoSensor.
; It installs the Windows Service, Tray Application, and Common library.
; 
; Requirements:
; - Inno Setup 6.0 or later (https://jrsoftware.org/isinfo.php)
; - Build the solution first (Release configuration)
;
; Usage:
; 1. Build the WinEcoSensor solution in Release mode
; 2. Open this script in Inno Setup Compiler
; 3. Click Build > Compile to create the installer

#define MyAppName "WinEcoSensor"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Unlock Europe - Energy"
#define MyAppURL "https://github.com/unlockeurope/WinEcoSensor"
#define MyAppExeName "WinEcoSensor.TrayApp.exe"
#define MyServiceName "WinEcoSensor"
#define MyServiceExeName "WinEcoSensor.Service.exe"

[Setup]
; Application identity
AppId={{8A7E3F5C-9B2D-4E1A-B8C7-6D5F4A3E2B1C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

; Installation directories
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes

; Output settings
OutputDir=Output
OutputBaseFilename=WinEcoSensor-Setup-{#MyAppVersion}
SetupIconFile=..\Resources\WinEcoSensor.ico

; Compression
Compression=lzma2/ultra64
SolidCompression=yes

; Windows version requirements
MinVersion=10.0

; Privileges (require admin for service installation)
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog

; Installer appearance
WizardStyle=modern
WizardSizePercent=100

; License
LicenseFile=..\LICENSE

; Uninstall settings
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}

; Architecture
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "autostart"; Description: "Start {#MyAppName} Tray App with Windows"; GroupDescription: "Startup Options:"; Flags: checked
Name: "installservice"; Description: "Install and start {#MyAppName} Windows Service"; GroupDescription: "Service Options:"; Flags: checked

[Files]
; Main application files (from Release build)
Source: "..\WinEcoSensor.TrayApp\bin\Release\WinEcoSensor.TrayApp.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\WinEcoSensor.TrayApp\bin\Release\WinEcoSensor.TrayApp.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\WinEcoSensor.Service\bin\Release\WinEcoSensor.Service.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\WinEcoSensor.Service\bin\Release\WinEcoSensor.Service.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\WinEcoSensor.Common\bin\Release\WinEcoSensor.Common.dll"; DestDir: "{app}"; Flags: ignoreversion

; Documentation
Source: "..\README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion

; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
; Start Menu icons
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "WinEcoSensor Tray Application"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"

; Desktop icon (optional)
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; Comment: "WinEcoSensor Tray Application"

[Registry]
; Autostart registry entry
Root: HKCU; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#MyAppName}"; ValueData: """{app}\{#MyAppExeName}"""; Flags: uninsdeletevalue; Tasks: autostart

[Run]
; Install and start the Windows service
Filename: "{sys}\sc.exe"; Parameters: "create {#MyServiceName} binPath= ""{app}\{#MyServiceExeName}"" DisplayName= ""{#MyAppName} – Windows Eco Energy Sensor"" start= auto"; Flags: runhidden waituntilterminated; Tasks: installservice; StatusMsg: "Installing Windows Service..."
Filename: "{sys}\sc.exe"; Parameters: "description {#MyServiceName} ""Monitors user presence, display activity, and energy-relevant states on Windows systems to support energy-efficient operations"""; Flags: runhidden waituntilterminated; Tasks: installservice; StatusMsg: "Configuring Service..."
Filename: "{sys}\sc.exe"; Parameters: "start {#MyServiceName}"; Flags: runhidden waituntilterminated; Tasks: installservice; StatusMsg: "Starting Windows Service..."

; Launch tray app after installation
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; Stop and remove the Windows service on uninstall
Filename: "{sys}\sc.exe"; Parameters: "stop {#MyServiceName}"; Flags: runhidden waituntilterminated
Filename: "{sys}\sc.exe"; Parameters: "delete {#MyServiceName}"; Flags: runhidden waituntilterminated

[UninstallDelete]
; Clean up log files and configuration
Type: filesandordirs; Name: "{commonappdata}\{#MyAppName}"

[Code]
// Pascal Script code for custom actions

var
  ServiceWasRunning: Boolean;

// Check if .NET Framework 4.8 is installed
function IsDotNetInstalled(): Boolean;
var
  Release: Cardinal;
begin
  Result := False;
  if RegQueryDWordValue(HKEY_LOCAL_MACHINE, 
    'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 
    'Release', Release) then
  begin
    // 4.8 = 528040 or higher
    Result := (Release >= 528040);
  end;
end;

// Check system requirements before installation
function InitializeSetup(): Boolean;
begin
  Result := True;
  
  // Check .NET Framework version
  if not IsDotNetInstalled() then
  begin
    MsgBox('WinEcoSensor requires .NET Framework 4.8 or later.' + #13#10 + #13#10 +
           'Please install .NET Framework 4.8 from Microsoft''s website and run this installer again.',
           mbCriticalError, MB_OK);
    Result := False;
    Exit;
  end;
end;

// Stop the service before installation (for upgrades)
procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
begin
  if CurStep = ssInstall then
  begin
    // Check if service exists and is running
    ServiceWasRunning := False;
    if Exec(ExpandConstant('{sys}\sc.exe'), 'query {#MyServiceName}', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
    begin
      if ResultCode = 0 then
      begin
        // Service exists, try to stop it
        Exec(ExpandConstant('{sys}\sc.exe'), 'stop {#MyServiceName}', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
        ServiceWasRunning := (ResultCode = 0);
        
        // Wait a moment for service to stop
        Sleep(2000);
        
        // Delete the old service
        Exec(ExpandConstant('{sys}\sc.exe'), 'delete {#MyServiceName}', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
        
        // Wait for service deletion to complete
        Sleep(1000);
      end;
    end;
    
    // Kill any running tray app instances
    Exec(ExpandConstant('{sys}\taskkill.exe'), '/F /IM {#MyAppExeName}', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  end;
end;

// Uninstall: Stop service and tray app first
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  ResultCode: Integer;
begin
  if CurUninstallStep = usUninstall then
  begin
    // Kill tray app first
    Exec(ExpandConstant('{sys}\taskkill.exe'), '/F /IM {#MyAppExeName}', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Sleep(500);
    
    // Service stop/delete is handled in [UninstallRun]
  end;
end;
