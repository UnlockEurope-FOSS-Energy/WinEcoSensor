# Changelog

All notable changes to WinEcoSensor will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned
- Unit test coverage
- Localization support (German, French)
- Historical data export
- Web dashboard integration

## [1.0.0] - 2025-01-14

### Added
- Initial release of WinEcoSensor
- **WinEcoSensor.Common** shared library
  - Hardware detection via WMI (mainboard, CPU, memory, GPU, monitors, disks)
  - User activity monitoring (login state, idle time, session tracking)
  - Display monitoring (on/off/idle states, power consumption)
  - Remote session detection (RDP, TeamViewer, AnyDesk, VNC, LogMeIn)
  - Energy calculation with PSU efficiency modeling
  - CloudEvents message builder for backend communication
  - XML-based configuration management with registry autostart
  - Structured logging with daily rotation
- **WinEcoSensor.Service** Windows service
  - Background monitoring with configurable intervals
  - Automatic service recovery on failure
  - Heartbeat and status reporting to backend
  - Session change detection (logon, logoff, lock, unlock)
- **WinEcoSensor.TrayApp** system tray application
  - System tray icon with context menu
  - Real-time status display with 1-second updates
  - Service control (start, stop, pause, resume)
  - Settings dialog with three tabs:
    - General: Autostart, intervals, monitoring options
    - Hardware & EPREL: Hardware list, EU Energy Label mapping
    - Backend Server: URL configuration with connection test
  - Status window with grouped information sections
- **Installer** with Inno Setup
  - .NET Framework 4.8 prerequisite check
  - Service installation and configuration
  - Autostart registry setup
  - Clean uninstall with service removal
- **Documentation**
  - README.md with installation and usage instructions
  - CONTRIBUTING.md with development guidelines
  - EUPL-1.2 license
- **CI/CD**
  - GitHub Actions workflow for build and release
  - Automatic installer creation on tags

### Technical Details
- Target Framework: .NET Framework 4.8
- Minimum Windows Version: Windows 10 (1903+)
- Architecture: x64
- License: EUPL-1.2

## [0.9.0] - 2025-01-13 (Pre-release)

### Added
- Core monitoring functionality
- Basic service implementation
- Prototype tray application

### Changed
- Refactored from single project to three-component architecture

---

## Version History Format

### Types of Changes
- **Added** for new features
- **Changed** for changes in existing functionality
- **Deprecated** for soon-to-be removed features
- **Removed** for now removed features
- **Fixed** for any bug fixes
- **Security** for vulnerability fixes

[Unreleased]: https://github.com/unlockeurope/WinEcoSensor/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/unlockeurope/WinEcoSensor/releases/tag/v1.0.0
[0.9.0]: https://github.com/unlockeurope/WinEcoSensor/releases/tag/v0.9.0
