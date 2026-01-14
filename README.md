# WinEcoSensor

**Windows Eco Energy Sensor** - A Windows service that monitors user presence, display activity, and energy-relevant states to support energy-efficient operations.

[![License: EUPL-1.2](https://img.shields.io/badge/License-EUPL--1.2-blue.svg)](https://joinup.ec.europa.eu/collection/eupl/eupl-text-eupl-12)
[![Platform: Windows](https://img.shields.io/badge/Platform-Windows%2010%2B-blue.svg)](https://www.microsoft.com/windows)
[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8-purple.svg)](https://dotnet.microsoft.com/download/dotnet-framework)

## Overview

WinEcoSensor is a Free and Open Source Software (FOSS) project developed as part of the **Unlock Europe - Energy** initiative. It provides real-time monitoring of PC usage and energy consumption estimation for Windows systems.

### Key Features

- **Hardware Detection**: Automatically detects mainboard, CPU, memory, GPUs, monitors, and storage devices
- **User Activity Monitoring**: Tracks login state, session activity, idle time, and first daily activity
- **Display Monitoring**: Monitors display on/off/idle states with duration tracking
- **Remote Session Detection**: Detects RDP, TeamViewer, AnyDesk, VNC, and other remote access tools
- **Energy Calculation**: Estimates power consumption and daily energy usage (kWh)
- **EPREL Integration**: Supports EU Energy Label (EPREL) numbers for monitors
- **CloudEvents Communication**: Sends status updates to backend servers using CloudEvents format
- **System Tray Application**: Easy-to-use interface for configuration and real-time status viewing

## Architecture

The solution consists of three components:

```
WinEcoSensor/
├── WinEcoSensor.Common/      # Shared library (models, monitors, utilities)
├── WinEcoSensor.Service/     # Windows background service
└── WinEcoSensor.TrayApp/     # System tray application
```

### Component Overview

| Component | Description |
|-----------|-------------|
| **Common** | Shared models, monitoring classes, energy calculator, backend client |
| **Service** | Windows service that runs in background, collects data, reports to backend |
| **TrayApp** | System tray application for service control, settings, and status display |

## Requirements

- Windows 10 or later (64-bit)
- .NET Framework 4.8
- Administrator privileges (for service installation)

## Installation

### Using the Installer

1. Download the latest release from [Releases](../../releases)
2. Run `WinEcoSensor-Setup.exe` as Administrator
3. Follow the installation wizard
4. The service will be installed and started automatically
5. The tray application will be added to Windows startup

### Manual Installation

1. Build the solution in Visual Studio or using MSBuild
2. Copy the output files to `C:\Program Files\WinEcoSensor\`
3. Install the service using the command line:

```batch
# Install service
sc create WinEcoSensor binPath= "C:\Program Files\WinEcoSensor\WinEcoSensor.Service.exe" DisplayName= "WinEcoSensor – Windows Eco Energy Sensor" start= auto

# Set service description
sc description WinEcoSensor "Monitors user presence, display activity, and energy-relevant states on Windows systems to support energy-efficient operations"

# Start service
sc start WinEcoSensor
```

4. Add the tray application to startup (optional):
   - Copy `WinEcoSensor.TrayApp.exe` shortcut to `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup`

### Using InstallUtil

```batch
# Install
%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe WinEcoSensor.Service.exe

# Uninstall
%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe /u WinEcoSensor.Service.exe
```

## Configuration

Configuration is stored in `%ProgramData%\WinEcoSensor\WinEcoSensor.config` (XML format).

### Settings

| Setting | Description | Default |
|---------|-------------|---------|
| `MonitorIntervalSeconds` | Data collection interval | 10 seconds |
| `ReportIntervalSeconds` | Backend reporting interval | 60 seconds |
| `BackendUrl` | Backend server URL | (empty) |
| `AutoStart` | Start tray app with Windows | true |
| `MonitorCpu` | Enable CPU monitoring | true |
| `MonitorDisplay` | Enable display monitoring | true |
| `MonitorRemoteAccess` | Enable remote access detection | true |
| `LogLevel` | Logging verbosity | Info |

### EPREL Mapping

EPREL (European Product Registry for Energy Labelling) numbers can be assigned to monitors via the Settings dialog. This enables accurate power consumption data based on EU Energy Labels.

## System Tray Features

### Context Menu

- **Service Status**: Shows current service state (Running/Stopped/Paused)
- **Activity Info**: First activity time, duration, display state
- **Energy Display**: Current power consumption and daily energy
- **Service Control**: Start/Stop/Pause/Resume service
- **Status Window**: Detailed real-time monitoring display
- **Settings**: Configuration dialog

### Status Window

Real-time display of:
- Service status with color coding
- User activity details (login, session, idle time)
- Display state and duration tracking
- Energy consumption with progress bars
- Remote access detection status

### Settings Dialog

Three tabs for configuration:
1. **General**: Autostart, intervals, monitoring options
2. **Hardware & EPREL**: Hardware list, EPREL number assignment
3. **Backend Server**: URL configuration with connection test

## CloudEvents Integration

WinEcoSensor sends status updates as CloudEvents to the configured backend:

```json
{
  "specversion": "1.0",
  "type": "eu.unlockeurope.energy.sensor.status",
  "source": "winecosensor://HOSTNAME/WinEcoSensor",
  "id": "uuid-v4",
  "time": "2025-01-13T12:00:00Z",
  "datacontenttype": "application/json",
  "data": {
    "hardware": { ... },
    "userActivity": { ... },
    "energyState": { ... }
  }
}
```

## Monitored Data

### Hardware Information
- Mainboard manufacturer and model
- CPU name, cores, base clock
- Memory size
- GPU names and memory
- Monitor names, resolutions, EPREL numbers
- Disk types and sizes

### User Activity
- Logged in status
- Username and domain
- Session state (Active/Locked/Disconnected)
- First activity time of the day
- Activity duration
- Idle time
- Screen saver state
- Workstation lock state

### Display State
- Number of monitors
- Current state (On/Off/Idle)
- State change timestamps
- Daily on/off/idle durations
- Power consumption per monitor

### Remote Access
- RDP connection status and client IP
- TeamViewer presence
- AnyDesk presence
- VNC sessions
- Other remote tools (LogMeIn, etc.)

### Energy Estimation
- CPU usage percentage
- CPU power (estimated)
- Display power (from EPREL or estimated)
- Total system power
- Daily energy consumption (kWh)
- Cost estimate (configurable rate)

## Building from Source

### Prerequisites

- Visual Studio 2019 or later
- .NET Framework 4.8 SDK
- Windows 10 SDK (for WMI support)

### Build Steps

```batch
# Clone repository
git clone https://github.com/yourusername/WinEcoSensor.git
cd WinEcoSensor

# Build solution
msbuild WinEcoSensor.sln /p:Configuration=Release /p:Platform="Any CPU"

# Output in bin\Release folders
```

### Project Dependencies

- `System.Management` - WMI queries
- `System.ServiceProcess` - Windows service infrastructure
- `System.Runtime.Serialization` - JSON serialization
- `System.Configuration.Install` - Service installation

## Logging

Logs are written to `%ProgramData%\WinEcoSensor\Logs\` with daily rotation:
- `WinEcoSensor_YYYY-MM-DD.log`

Log levels: Error, Warning, Info, Debug

## Troubleshooting

### Service won't start
1. Check Event Viewer for error details
2. Verify .NET Framework 4.8 is installed
3. Run as Administrator
4. Check log files in `%ProgramData%\WinEcoSensor\Logs\`

### Tray app not showing
1. Check system tray overflow area
2. Verify single instance (check Task Manager)
3. Run as normal user (not Administrator)

### Backend connection fails
1. Verify URL format (include http:// or https://)
2. Check network connectivity
3. Test connection in Settings dialog
4. Review firewall settings

### High CPU usage
1. Increase `MonitorIntervalSeconds` in settings
2. Disable unused monitoring features
3. Check for WMI service issues

## Contributing

Contributions are welcome! Please read our contributing guidelines and submit pull requests to the main repository.

### Development Guidelines

- Use English for all code, comments, and documentation
- Follow C# coding conventions
- Include XML documentation for public APIs
- Add unit tests for new features
- Update README for significant changes

## License

This project is licensed under the **European Union Public License 1.2 (EUPL-1.2)**.

See [LICENSE](LICENSE) for the full license text.

### EUPL-1.2 Summary

- ✅ Commercial use
- ✅ Modification
- ✅ Distribution
- ✅ Patent use
- ✅ Private use
- ⚠️ Copyleft (derivative works must use compatible license)
- ⚠️ License and copyright notice required
- ⚠️ State changes required

## Acknowledgments

- **Unlock Europe - Energy**: Initiative for energy-efficient software
- **CloudEvents**: Specification for event data
- **EPREL**: EU Product Registry for Energy Labelling

## Contact

For questions, issues, or contributions, please use the GitHub issue tracker.

---

**WinEcoSensor** - Supporting energy-efficient computing in Europe 🌱
