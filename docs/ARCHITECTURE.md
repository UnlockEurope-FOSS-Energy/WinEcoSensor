# WinEcoSensor Architecture

This document describes the architecture and design of WinEcoSensor.

## Overview

WinEcoSensor follows a three-tier architecture:

```
┌─────────────────────────────────────────────────────────────────┐
│                        User Interface                           │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │              WinEcoSensor.TrayApp                        │   │
│  │   • System Tray Icon                                     │   │
│  │   • Settings Dialog                                      │   │
│  │   • Status Window                                        │   │
│  │   • Service Control                                      │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ Service Control
                              │ Configuration
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                      Background Service                          │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │              WinEcoSensor.Service                        │   │
│  │   • Windows Service (ServiceBase)                        │   │
│  │   • SensorEngine (Core Monitoring Loop)                  │   │
│  │   • Session Change Detection                             │   │
│  │   • Backend Communication                                │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ Shared Components
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                       Shared Library                             │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │              WinEcoSensor.Common                         │   │
│  │   • Models (Data Transfer Objects)                       │   │
│  │   • Monitors (Hardware, CPU, Display, User, Remote)      │   │
│  │   • Utilities (WMI, Logger, Energy Calculator)           │   │
│  │   • Communication (BackendClient, CloudEventBuilder)     │   │
│  │   • Configuration (ConfigurationManager)                 │   │
│  └─────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

## Component Details

### WinEcoSensor.Common

The shared library provides all core functionality used by both the service and tray app.

#### Models

| Class | Purpose |
|-------|---------|
| `HardwareInfo` | System hardware inventory (mainboard, CPU, memory, GPUs, monitors, disks) |
| `UserActivityInfo` | User session state (login, idle, activity times) |
| `EnergyState` | Power consumption and energy totals |
| `MonitorInfo` | Display monitor details including EPREL data |
| `DiskInfo` | Storage device information with power estimates |
| `SensorConfiguration` | Application settings |
| `EprelMapping` | EU Energy Label mapping for devices |
| `CloudEventMessage` | CloudEvents message format |

#### Monitoring Classes

| Class | Purpose | Data Source |
|-------|---------|-------------|
| `HardwareMonitor` | Detects installed hardware | WMI |
| `CpuMonitor` | CPU usage and power estimation | Performance Counters |
| `DisplayMonitor` | Monitor state tracking | Windows API |
| `UserActivityMonitor` | User presence detection | Win32 API |
| `RemoteSessionMonitor` | Remote access detection | WMI + Process API |

#### Utilities

| Class | Purpose |
|-------|---------|
| `WmiHelper` | WMI query wrapper with caching |
| `Logger` | Structured logging with daily rotation |
| `EnergyCalculator` | Power/energy calculations with PSU efficiency |

#### Communication

| Class | Purpose |
|-------|---------|
| `BackendClient` | HTTP client for CloudEvents |
| `CloudEventBuilder` | CloudEvents message construction |

#### Configuration

| Class | Purpose |
|-------|---------|
| `ConfigurationManager` | XML configuration with registry autostart |

### WinEcoSensor.Service

The Windows service runs in the background and performs continuous monitoring.

```
┌─────────────────────────────────────────────────────────────────┐
│                    WinEcoSensorService                           │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  OnStart() ──► Initialize SensorEngine                   │  │
│  │  OnStop()  ──► Cleanup and stop monitoring               │  │
│  │  OnPause() ──► Pause monitoring loop                     │  │
│  │  OnContinue() ──► Resume monitoring                      │  │
│  │  OnSessionChange() ──► Handle logon/logoff/lock/unlock   │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              │                                   │
│                              ▼                                   │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                    SensorEngine                           │  │
│  │  ┌────────────────────────────────────────────────────┐  │  │
│  │  │  Monitor Timer (configurable interval)             │  │  │
│  │  │    └──► Collect hardware, user, display data       │  │  │
│  │  │    └──► Calculate energy consumption               │  │  │
│  │  │    └──► Update internal state                      │  │  │
│  │  └────────────────────────────────────────────────────┘  │  │
│  │  ┌────────────────────────────────────────────────────┐  │  │
│  │  │  Report Timer (configurable interval)              │  │  │
│  │  │    └──► Build CloudEvent message                   │  │  │
│  │  │    └──► Send to backend server                     │  │  │
│  │  └────────────────────────────────────────────────────┘  │  │
│  │  ┌────────────────────────────────────────────────────┐  │  │
│  │  │  Heartbeat Timer (configurable interval)           │  │  │
│  │  │    └──► Send heartbeat to backend                  │  │  │
│  │  └────────────────────────────────────────────────────┘  │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### WinEcoSensor.TrayApp

The system tray application provides user interaction.

```
┌─────────────────────────────────────────────────────────────────┐
│                   TrayApplicationContext                         │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  NotifyIcon (System Tray)                                 │  │
│  │    ├── Context Menu                                       │  │
│  │    │     ├── Service Status (display only)                │  │
│  │    │     ├── Activity Info (first activity, duration)     │  │
│  │    │     ├── Display State (on/off/idle times)            │  │
│  │    │     ├── Energy Info (current power, daily kWh)       │  │
│  │    │     ├── Service Control (Start/Stop/Pause/Resume)    │  │
│  │    │     ├── Show Status Window                           │  │
│  │    │     ├── Settings                                     │  │
│  │    │     ├── About                                        │  │
│  │    │     └── Exit                                         │  │
│  │    └── Double-click ──► Open Status Window                │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  SettingsForm                                             │  │
│  │    ├── General Tab (autostart, intervals, options)        │  │
│  │    ├── Hardware Tab (device list, EPREL mapping)          │  │
│  │    └── Backend Tab (URL, connection test)                 │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  StatusForm                                               │  │
│  │    ├── Service Group (status with color)                  │  │
│  │    ├── User Activity Group (login, idle, duration)        │  │
│  │    ├── Display Group (state, on/off/idle times)           │  │
│  │    ├── Energy Group (CPU, display, total power, kWh)      │  │
│  │    └── Remote Access Group (RDP, TeamViewer, etc.)        │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

## Data Flow

### Monitoring Flow

```
1. Hardware Detection (on startup)
   HardwareMonitor.DetectHardware()
   └──► WMI Queries (Win32_BaseBoard, Win32_Processor, etc.)
   └──► Returns HardwareInfo

2. Continuous Monitoring (every N seconds)
   CpuMonitor.Update()
   └──► PerformanceCounter ("% Processor Time")
   └──► Estimate power from TDP and usage

   DisplayMonitor.Update()
   └──► Check display power state
   └──► Track on/off/idle durations
   └──► Calculate display power from EPREL or estimates

   UserActivityMonitor.Update()
   └──► GetLastInputInfo() for idle time
   └──► Session state detection
   └──► First activity tracking

   RemoteSessionMonitor.Update()
   └──► WMI: Win32_Process for remote tools
   └──► Check RDP session state
   └──► Detect TeamViewer, AnyDesk, VNC, LogMeIn

3. Energy Calculation
   EnergyCalculator.CalculateCurrentState()
   └──► Sum component powers
   └──► Apply PSU efficiency
   └──► Update daily/session totals
   └──► Calculate efficiency rating
```

### Communication Flow

```
1. Status Report (every N seconds)
   SensorEngine.ReportStatus()
   └──► CloudEventBuilder.CreateStatusEvent()
         └──► Collect HardwareInfo, UserActivityInfo, EnergyState
         └──► Build CloudEvent message
   └──► BackendClient.SendAsync()
         └──► HTTP POST with CloudEvent JSON
         └──► Handle retries and errors

2. Heartbeat (every M seconds)
   SensorEngine.SendHeartbeat()
   └──► CloudEventBuilder.CreateHeartbeatEvent()
   └──► BackendClient.SendAsync()
```

## Configuration

Configuration is stored in XML format:

```
%ProgramData%\WinEcoSensor\
├── WinEcoSensor.config    (XML configuration)
└── Logs\
    └── WinEcoSensor_YYYY-MM-DD.log
```

Registry (for autostart):
```
HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run\WinEcoSensor
```

## CloudEvents Format

WinEcoSensor uses the CloudEvents 1.0 specification:

```json
{
  "specversion": "1.0",
  "type": "eu.unlockeurope.energy.sensor.status",
  "source": "winecosensor://hostname/WinEcoSensor",
  "id": "uuid-v4",
  "time": "2025-01-14T12:00:00Z",
  "datacontenttype": "application/json",
  "data": {
    "hardware": {
      "mainboardManufacturer": "...",
      "cpuName": "...",
      "monitors": [...]
    },
    "userActivity": {
      "isLoggedIn": true,
      "userName": "...",
      "idleTimeSeconds": 0
    },
    "energyState": {
      "totalPowerWatts": 85.5,
      "dailyEnergyWh": 340.2
    }
  }
}
```

## Security Considerations

1. **Service Privileges**: The service runs as LocalSystem to access hardware information
2. **Tray App Privileges**: Runs as the logged-in user for session detection
3. **Configuration Access**: XML config in ProgramData (requires admin to modify)
4. **Registry Access**: User-level registry for autostart (no admin needed)
5. **Network Communication**: HTTPS recommended for backend communication
6. **No Sensitive Data**: No passwords or credentials stored

## Performance Considerations

1. **WMI Caching**: Hardware queries are cached (refreshed on request)
2. **Timer Intervals**: Configurable to balance accuracy vs. overhead
3. **Memory Usage**: Minimal memory footprint (~20-30 MB)
4. **CPU Usage**: Negligible (<1% during monitoring)
5. **Disk I/O**: Log rotation prevents unbounded growth
