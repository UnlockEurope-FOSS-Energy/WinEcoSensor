// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2026 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace WinEcoSensor.Service
{
    /// <summary>
    /// Service installer for WinEcoSensor Windows Service.
    /// Handles installation, uninstallation, and service configuration.
    /// </summary>
    [RunInstaller(true)]
    public class WinEcoSensorServiceInstaller : Installer
    {
        private ServiceProcessInstaller _processInstaller;
        private ServiceInstaller _serviceInstaller;

        /// <summary>
        /// Create service installer
        /// </summary>
        public WinEcoSensorServiceInstaller()
        {
            // Process installer - defines the account the service runs under
            _processInstaller = new ServiceProcessInstaller
            {
                // Run as Local System for maximum compatibility
                // Can be changed to NetworkService or specific user if needed
                Account = ServiceAccount.LocalSystem
            };

            // Service installer - defines service properties
            _serviceInstaller = new ServiceInstaller
            {
                ServiceName = WinEcoSensorService.ServiceName_,
                DisplayName = WinEcoSensorService.DisplayName_,
                Description = WinEcoSensorService.Description_,
                
                // Start automatically with Windows
                StartType = ServiceStartMode.Automatic,
                
                // Allow delayed start for better boot performance
                DelayedAutoStart = true
            };

            // Add both installers
            Installers.Add(_processInstaller);
            Installers.Add(_serviceInstaller);
        }

        /// <summary>
        /// Perform custom actions after installation
        /// </summary>
        /// <param name="stateSaver">State dictionary</param>
        public override void Install(System.Collections.IDictionary stateSaver)
        {
            base.Install(stateSaver);

            // Additional post-install actions can be added here
            // For example: creating required directories, setting permissions, etc.
        }

        /// <summary>
        /// Perform custom actions after uninstallation
        /// </summary>
        /// <param name="savedState">Saved state dictionary</param>
        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            base.Uninstall(savedState);

            // Additional cleanup actions can be added here
            // Note: Log files and configuration are preserved by default
        }

        /// <summary>
        /// Perform custom actions during rollback
        /// </summary>
        /// <param name="savedState">Saved state dictionary</param>
        public override void Rollback(System.Collections.IDictionary savedState)
        {
            base.Rollback(savedState);
        }

        /// <summary>
        /// Perform custom actions after commit
        /// </summary>
        /// <param name="savedState">Saved state dictionary</param>
        public override void Commit(System.Collections.IDictionary savedState)
        {
            base.Commit(savedState);
        }
    }
}
