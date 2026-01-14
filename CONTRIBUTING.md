# Contributing to WinEcoSensor

Thank you for your interest in contributing to WinEcoSensor! This document provides guidelines and information for contributors.

## Code of Conduct

By participating in this project, you agree to maintain a respectful and inclusive environment for everyone.

## How to Contribute

### Reporting Issues

1. Check if the issue already exists in the [Issues](../../issues) section
2. If not, create a new issue with:
   - Clear, descriptive title
   - Detailed description of the problem
   - Steps to reproduce (if applicable)
   - Expected vs. actual behavior
   - System information (Windows version, .NET version)
   - Log files if relevant (from `%ProgramData%\WinEcoSensor\Logs\`)

### Suggesting Features

1. Open an issue with the "Feature Request" label
2. Describe the feature and its use case
3. Explain how it aligns with the project's goals

### Pull Requests

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-feature`)
3. Make your changes following the coding standards below
4. Test your changes thoroughly
5. Commit with clear, descriptive messages
6. Push to your fork
7. Create a Pull Request

## Development Setup

### Prerequisites

- Windows 10 or later
- Visual Studio 2019 or later with:
  - .NET Framework 4.8 SDK
  - Windows 10 SDK
- Inno Setup 6.0+ (for installer creation)

### Building

```batch
# Clone your fork
git clone https://github.com/YOUR_USERNAME/WinEcoSensor.git
cd WinEcoSensor

# Open solution in Visual Studio
start WinEcoSensor.sln

# Or build from command line
build.bat release
```

### Testing

Before submitting a PR, test:

1. **Service Installation**: Install/uninstall service via `sc` or `InstallUtil`
2. **Service Operation**: Verify service starts and logs correctly
3. **Tray Application**: Test all menu items and dialogs
4. **Settings**: Verify configuration saves and loads correctly
5. **Backend Communication**: Test with a mock endpoint if available

## Coding Standards

### Language

- All code, comments, and documentation must be in **English**
- Variable names should be descriptive and follow C# conventions

### Code Style

```csharp
// Use PascalCase for public members
public string BackendUrl { get; set; }

// Use camelCase for private members
private readonly Timer _monitorTimer;

// Use XML documentation for public APIs
/// <summary>
/// Calculates the current power consumption in watts.
/// </summary>
/// <param name="cpuUsage">CPU usage percentage (0-100)</param>
/// <returns>Power consumption in watts</returns>
public double CalculatePower(double cpuUsage)
{
    // Implementation
}

// Use meaningful exception messages
if (string.IsNullOrEmpty(url))
{
    throw new ArgumentNullException(nameof(url), "Backend URL cannot be null or empty");
}
```

### File Headers

Every source file must include the EUPL-1.2 license header:

```csharp
// WinEcoSensor - Windows Eco Energy Sensor
// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) 2025 WinEcoSensor Contributors
```

### Commit Messages

Follow the conventional commits format:

```
type(scope): description

[optional body]

[optional footer]
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

Examples:
```
feat(tray): add display power monitoring to status window
fix(service): handle null reference in hardware detection
docs(readme): update installation instructions
```

## Project Structure

```
WinEcoSensor/
├── WinEcoSensor.Common/       # Shared library
│   ├── Models/                # Data models
│   ├── Monitoring/            # Monitor classes
│   ├── Utilities/             # Helper classes
│   ├── Communication/         # Backend client
│   └── Configuration/         # Config management
├── WinEcoSensor.Service/      # Windows service
├── WinEcoSensor.TrayApp/      # System tray app
├── Installer/                 # Inno Setup scripts
└── Resources/                 # Icons and assets
```

## Key Classes

| Class | Purpose |
|-------|---------|
| `SensorEngine` | Core monitoring loop in service |
| `TrayApplicationContext` | System tray icon and menu |
| `HardwareMonitor` | Hardware detection via WMI |
| `UserActivityMonitor` | User session monitoring |
| `DisplayMonitor` | Display state tracking |
| `EnergyCalculator` | Power consumption estimation |
| `BackendClient` | CloudEvents HTTP communication |
| `ConfigurationManager` | XML configuration handling |

## Areas for Contribution

### High Priority

- [ ] Unit tests for monitoring classes
- [ ] Integration tests for service
- [ ] Additional remote access tool detection
- [ ] More accurate energy estimation models

### Medium Priority

- [ ] Localization support (German, French, etc.)
- [ ] Dark mode support for dialogs
- [ ] Historical data storage and export
- [ ] Improved EPREL data integration

### Future Features

- [ ] macOS/Linux sensor implementations
- [ ] Web dashboard for multi-PC monitoring
- [ ] API for third-party integrations
- [ ] Machine learning for energy prediction

## Questions?

If you have questions about contributing, please open an issue with the "Question" label.

## License

By contributing to WinEcoSensor, you agree that your contributions will be licensed under the EUPL-1.2 license.

---

Thank you for helping make WinEcoSensor better! 🌱
