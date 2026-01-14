// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2024 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.Drawing;
using System.ServiceProcess;
using System.Timers;
using System.Windows.Forms;
using WinEcoSensor.Common.Configuration;
using WinEcoSensor.Common.Models;
using WinEcoSensor.Common.Monitoring;
using WinEcoSensor.Common.Utilities;

namespace WinEcoSensor.TrayApp
{
    /// <summary>
    /// Application context for the system tray application.
    /// Manages the tray icon, context menu, and service interaction.
    /// </summary>
    public class TrayApplicationContext : ApplicationContext
    {
        private const string ServiceName = "WinEcoSensor";
        private const int UpdateIntervalMs = 5000;

        private readonly NotifyIcon _notifyIcon;
        private readonly System.Timers.Timer _updateTimer;
        private readonly ContextMenuStrip _contextMenu;

        // Menu items that need state updates
        private ToolStripMenuItem _startServiceItem;
        private ToolStripMenuItem _stopServiceItem;
        private ToolStripMenuItem _pauseServiceItem;
        private ToolStripMenuItem _resumeServiceItem;
        private ToolStripSeparator _serviceSeparator;
        private ToolStripMenuItem _serviceStatusItem;
        private ToolStripMenuItem _firstActivityItem;
        private ToolStripMenuItem _durationItem;
        private ToolStripMenuItem _displayStateItem;
        private ToolStripMenuItem _energyConsumptionItem;

        // Monitoring components
        private UserActivityMonitor _activityMonitor;
        private DisplayMonitor _displayMonitor;
        private ConfigurationManager _configManager;
        private SensorConfiguration _configuration;

        // Forms
        private SettingsForm _settingsForm;
        private StatusForm _statusForm;

        /// <summary>
        /// Current service status
        /// </summary>
        public ServiceControllerStatus? ServiceStatus { get; private set; }

        /// <summary>
        /// Create the tray application context
        /// </summary>
        public TrayApplicationContext()
        {
            try
            {
                // Load configuration
                _configManager = new ConfigurationManager();
                _configuration = _configManager.Load();

                // Initialize monitoring
                _activityMonitor = new UserActivityMonitor();
                _displayMonitor = new DisplayMonitor();

                // Create context menu
                _contextMenu = CreateContextMenu();

                // Create notify icon
                _notifyIcon = new NotifyIcon
                {
                    Icon = CreateDefaultIcon(),
                    Text = "WinEcoSensor - Energy Monitor",
                    ContextMenuStrip = _contextMenu,
                    Visible = true
                };

                // Handle double-click to show status
                _notifyIcon.DoubleClick += OnNotifyIconDoubleClick;

                // Create update timer
                _updateTimer = new System.Timers.Timer(UpdateIntervalMs);
                _updateTimer.Elapsed += OnUpdateTimerElapsed;
                _updateTimer.AutoReset = true;
                _updateTimer.Start();

                // Initial update
                UpdateStatus();

                Logger.Info("Tray application context initialized");
            }
            catch (Exception ex)
            {
                Logger.Error("Error initializing tray application context", ex);
                throw;
            }
        }

        /// <summary>
        /// Create the default tray icon
        /// </summary>
        private Icon CreateDefaultIcon()
        {
            // Try to load from resources, fall back to system icon
            try
            {
                // Create a simple green leaf icon programmatically
                var bitmap = new Bitmap(16, 16);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Color.Transparent);
                    
                    // Draw a stylized leaf shape
                    using (var brush = new SolidBrush(Color.FromArgb(76, 175, 80))) // Material Green
                    {
                        // Leaf body
                        Point[] leafPoints = new Point[]
                        {
                            new Point(8, 2),   // Top
                            new Point(14, 6),  // Right top
                            new Point(14, 10), // Right
                            new Point(8, 14),  // Bottom
                            new Point(2, 10),  // Left
                            new Point(2, 6)    // Left top
                        };
                        g.FillPolygon(brush, leafPoints);
                    }

                    // Draw leaf vein
                    using (var pen = new Pen(Color.FromArgb(46, 125, 50), 1))
                    {
                        g.DrawLine(pen, 8, 3, 8, 13);
                    }
                }

                return Icon.FromHandle(bitmap.GetHicon());
            }
            catch
            {
                // Fallback to system information icon
                return SystemIcons.Information;
            }
        }

        /// <summary>
        /// Create the context menu
        /// </summary>
        private ContextMenuStrip CreateContextMenu()
        {
            var menu = new ContextMenuStrip();

            // Status header
            _serviceStatusItem = new ToolStripMenuItem("Service: Checking...")
            {
                Enabled = false,
                Font = new Font(menu.Font, FontStyle.Bold)
            };
            menu.Items.Add(_serviceStatusItem);
            menu.Items.Add(new ToolStripSeparator());

            // Activity info section
            _firstActivityItem = new ToolStripMenuItem("First activity: --:--")
            {
                Enabled = false,
                Image = CreateSmallIcon(Color.Blue)
            };
            menu.Items.Add(_firstActivityItem);

            _durationItem = new ToolStripMenuItem("Duration: --:--:--")
            {
                Enabled = false,
                Image = CreateSmallIcon(Color.Orange)
            };
            menu.Items.Add(_durationItem);

            _displayStateItem = new ToolStripMenuItem("Display: Unknown")
            {
                Enabled = false,
                Image = CreateSmallIcon(Color.Purple)
            };
            menu.Items.Add(_displayStateItem);

            _energyConsumptionItem = new ToolStripMenuItem("Energy: --- W")
            {
                Enabled = false,
                Image = CreateSmallIcon(Color.Green)
            };
            menu.Items.Add(_energyConsumptionItem);

            menu.Items.Add(new ToolStripSeparator());

            // Service control section
            _startServiceItem = new ToolStripMenuItem("Start Service", null, OnStartServiceClick);
            menu.Items.Add(_startServiceItem);

            _stopServiceItem = new ToolStripMenuItem("Stop Service", null, OnStopServiceClick);
            menu.Items.Add(_stopServiceItem);

            _pauseServiceItem = new ToolStripMenuItem("Pause Service", null, OnPauseServiceClick);
            menu.Items.Add(_pauseServiceItem);

            _resumeServiceItem = new ToolStripMenuItem("Resume Service", null, OnResumeServiceClick);
            menu.Items.Add(_resumeServiceItem);

            _serviceSeparator = new ToolStripSeparator();
            menu.Items.Add(_serviceSeparator);

            // Show status window
            var statusItem = new ToolStripMenuItem("Show Status Window", null, OnShowStatusClick);
            menu.Items.Add(statusItem);

            // Settings
            var settingsItem = new ToolStripMenuItem("Settings...", null, OnSettingsClick);
            menu.Items.Add(settingsItem);

            menu.Items.Add(new ToolStripSeparator());

            // About
            var aboutItem = new ToolStripMenuItem("About WinEcoSensor", null, OnAboutClick);
            menu.Items.Add(aboutItem);

            // Exit
            var exitItem = new ToolStripMenuItem("Exit", null, OnExitClick);
            menu.Items.Add(exitItem);

            return menu;
        }

        /// <summary>
        /// Create a small colored icon for menu items
        /// </summary>
        private Image CreateSmallIcon(Color color)
        {
            var bitmap = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                using (var brush = new SolidBrush(color))
                {
                    g.FillEllipse(brush, 4, 4, 8, 8);
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Timer elapsed handler - update status
        /// </summary>
        private void OnUpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (_notifyIcon != null && _contextMenu != null)
                {
                    // Update on UI thread
                    if (_contextMenu.InvokeRequired)
                    {
                        _contextMenu.BeginInvoke(new Action(UpdateStatus));
                    }
                    else
                    {
                        UpdateStatus();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("Error in update timer", ex);
            }
        }

        /// <summary>
        /// Update all status information
        /// </summary>
        private void UpdateStatus()
        {
            try
            {
                UpdateServiceStatus();
                UpdateActivityInfo();
                UpdateMenuState();
            }
            catch (Exception ex)
            {
                Logger.Debug("Error updating status", ex);
            }
        }

        /// <summary>
        /// Update service status
        /// </summary>
        private void UpdateServiceStatus()
        {
            try
            {
                using (var controller = new ServiceController(ServiceName))
                {
                    ServiceStatus = controller.Status;
                    
                    string statusText;
                    Color statusColor;

                    switch (controller.Status)
                    {
                        case ServiceControllerStatus.Running:
                            statusText = "Service: Running";
                            statusColor = Color.Green;
                            break;
                        case ServiceControllerStatus.Stopped:
                            statusText = "Service: Stopped";
                            statusColor = Color.Red;
                            break;
                        case ServiceControllerStatus.Paused:
                            statusText = "Service: Paused";
                            statusColor = Color.Orange;
                            break;
                        case ServiceControllerStatus.StartPending:
                            statusText = "Service: Starting...";
                            statusColor = Color.Yellow;
                            break;
                        case ServiceControllerStatus.StopPending:
                            statusText = "Service: Stopping...";
                            statusColor = Color.Yellow;
                            break;
                        case ServiceControllerStatus.PausePending:
                            statusText = "Service: Pausing...";
                            statusColor = Color.Yellow;
                            break;
                        case ServiceControllerStatus.ContinuePending:
                            statusText = "Service: Resuming...";
                            statusColor = Color.Yellow;
                            break;
                        default:
                            statusText = "Service: Unknown";
                            statusColor = Color.Gray;
                            break;
                    }

                    _serviceStatusItem.Text = statusText;
                    _notifyIcon.Text = $"WinEcoSensor - {statusText}";
                }
            }
            catch (InvalidOperationException)
            {
                // Service not installed
                ServiceStatus = null;
                _serviceStatusItem.Text = "Service: Not Installed";
                _notifyIcon.Text = "WinEcoSensor - Service Not Installed";
            }
            catch (Exception ex)
            {
                Logger.Debug("Error getting service status", ex);
                ServiceStatus = null;
                _serviceStatusItem.Text = "Service: Error";
            }
        }

        /// <summary>
        /// Update activity information display
        /// </summary>
        private void UpdateActivityInfo()
        {
            try
            {
                // Update activity monitor
                _activityMonitor.Update();
                _displayMonitor.Update();

                // First activity of day
                var firstActivity = _activityMonitor.GetFirstActivityToday();
                if (firstActivity.HasValue)
                {
                    _firstActivityItem.Text = $"First activity: {firstActivity.Value.ToLocalTime():HH:mm:ss}";
                }
                else
                {
                    _firstActivityItem.Text = "First activity: No activity yet";
                }

                // Duration since first activity
                var duration = _activityMonitor.GetTimeSinceFirstActivity();
                if (duration.TotalSeconds > 0)
                {
                    _durationItem.Text = $"Duration: {FormatDuration(duration)}";
                }
                else
                {
                    _durationItem.Text = "Duration: --:--:--";
                }

                // Display state
                var displayStats = _displayMonitor.GetTimeStats();
                var stateText = displayStats.CurrentState switch
                {
                    MonitorPowerState.On => "On",
                    MonitorPowerState.Off => "Off",
                    MonitorPowerState.Standby => "Standby",
                    MonitorPowerState.Suspend => "Suspended",
                    _ => "Unknown"
                };
                
                _displayStateItem.Text = $"Display: {stateText} (On: {FormatDuration(displayStats.OnTimeToday)}, " +
                                        $"Off: {FormatDuration(displayStats.OffTimeToday)}, " +
                                        $"Idle: {FormatDuration(displayStats.IdleTimeToday)})";

                // Estimated energy consumption
                var powerWatts = _displayMonitor.TotalPowerWatts;
                _energyConsumptionItem.Text = $"Display Power: {powerWatts:F1} W ({_displayMonitor.Monitors.Count} monitor(s))";
            }
            catch (Exception ex)
            {
                Logger.Debug("Error updating activity info", ex);
            }
        }

        /// <summary>
        /// Format duration as HH:mm:ss
        /// </summary>
        private string FormatDuration(TimeSpan duration)
        {
            return $"{(int)duration.TotalHours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
        }

        /// <summary>
        /// Update menu item enabled states based on service status
        /// </summary>
        private void UpdateMenuState()
        {
            if (!ServiceStatus.HasValue)
            {
                // Service not installed
                _startServiceItem.Enabled = false;
                _stopServiceItem.Enabled = false;
                _pauseServiceItem.Enabled = false;
                _resumeServiceItem.Enabled = false;
                return;
            }

            switch (ServiceStatus.Value)
            {
                case ServiceControllerStatus.Running:
                    _startServiceItem.Enabled = false;
                    _stopServiceItem.Enabled = true;
                    _pauseServiceItem.Enabled = true;
                    _resumeServiceItem.Enabled = false;
                    break;

                case ServiceControllerStatus.Stopped:
                    _startServiceItem.Enabled = true;
                    _stopServiceItem.Enabled = false;
                    _pauseServiceItem.Enabled = false;
                    _resumeServiceItem.Enabled = false;
                    break;

                case ServiceControllerStatus.Paused:
                    _startServiceItem.Enabled = false;
                    _stopServiceItem.Enabled = true;
                    _pauseServiceItem.Enabled = false;
                    _resumeServiceItem.Enabled = true;
                    break;

                default:
                    // Transitional states - disable all
                    _startServiceItem.Enabled = false;
                    _stopServiceItem.Enabled = false;
                    _pauseServiceItem.Enabled = false;
                    _resumeServiceItem.Enabled = false;
                    break;
            }
        }

        #region Menu Event Handlers

        private void OnStartServiceClick(object sender, EventArgs e)
        {
            try
            {
                using (var controller = new ServiceController(ServiceName))
                {
                    if (controller.Status == ServiceControllerStatus.Stopped)
                    {
                        controller.Start();
                        controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                        Logger.Info("Service started via tray app");
                        UpdateStatus();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error starting service", ex);
                MessageBox.Show(
                    $"Failed to start service:\n\n{ex.Message}",
                    "WinEcoSensor",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OnStopServiceClick(object sender, EventArgs e)
        {
            try
            {
                using (var controller = new ServiceController(ServiceName))
                {
                    if (controller.Status == ServiceControllerStatus.Running ||
                        controller.Status == ServiceControllerStatus.Paused)
                    {
                        controller.Stop();
                        controller.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                        Logger.Info("Service stopped via tray app");
                        UpdateStatus();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error stopping service", ex);
                MessageBox.Show(
                    $"Failed to stop service:\n\n{ex.Message}",
                    "WinEcoSensor",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OnPauseServiceClick(object sender, EventArgs e)
        {
            try
            {
                using (var controller = new ServiceController(ServiceName))
                {
                    if (controller.Status == ServiceControllerStatus.Running && controller.CanPauseAndContinue)
                    {
                        controller.Pause();
                        controller.WaitForStatus(ServiceControllerStatus.Paused, TimeSpan.FromSeconds(30));
                        Logger.Info("Service paused via tray app");
                        UpdateStatus();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error pausing service", ex);
                MessageBox.Show(
                    $"Failed to pause service:\n\n{ex.Message}",
                    "WinEcoSensor",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OnResumeServiceClick(object sender, EventArgs e)
        {
            try
            {
                using (var controller = new ServiceController(ServiceName))
                {
                    if (controller.Status == ServiceControllerStatus.Paused)
                    {
                        controller.Continue();
                        controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                        Logger.Info("Service resumed via tray app");
                        UpdateStatus();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error resuming service", ex);
                MessageBox.Show(
                    $"Failed to resume service:\n\n{ex.Message}",
                    "WinEcoSensor",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OnShowStatusClick(object sender, EventArgs e)
        {
            ShowStatusForm();
        }

        private void OnNotifyIconDoubleClick(object sender, EventArgs e)
        {
            ShowStatusForm();
        }

        private void ShowStatusForm()
        {
            if (_statusForm == null || _statusForm.IsDisposed)
            {
                _statusForm = new StatusForm();
            }

            if (!_statusForm.Visible)
            {
                _statusForm.Show();
            }

            _statusForm.Activate();
            _statusForm.BringToFront();
        }

        private void OnSettingsClick(object sender, EventArgs e)
        {
            if (_settingsForm == null || _settingsForm.IsDisposed)
            {
                _settingsForm = new SettingsForm();
            }

            if (_settingsForm.ShowDialog() == DialogResult.OK)
            {
                // Reload configuration
                _configuration = _configManager.Load();
                Logger.Info("Settings updated via tray app");
            }
        }

        private void OnAboutClick(object sender, EventArgs e)
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            MessageBox.Show(
                $"WinEcoSensor - Windows Eco Energy Sensor\n\n" +
                $"Version {version}\n\n" +
                $"Monitors user presence, display activity, and energy-relevant states\n" +
                $"on Windows systems to support energy-efficient operations.\n\n" +
                $"Copyright © 2024 Unlock Europe\n" +
                $"FOSS Energy Initiative\n\n" +
                $"Licensed under the European Union Public License (EUPL-1.2)",
                "About WinEcoSensor",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void OnExitClick(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to exit the WinEcoSensor tray application?\n\n" +
                "Note: The WinEcoSensor service will continue running in the background.",
                "Exit WinEcoSensor",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Logger.Info("User requested tray app exit");
                ExitThread();
            }
        }

        #endregion

        /// <summary>
        /// Clean up resources
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    _updateTimer?.Stop();
                    _updateTimer?.Dispose();

                    _notifyIcon.Visible = false;
                    _notifyIcon?.Dispose();

                    _contextMenu?.Dispose();

                    _activityMonitor?.Dispose();
                    _displayMonitor?.Dispose();

                    _settingsForm?.Dispose();
                    _statusForm?.Dispose();

                    Logger.Info("Tray application context disposed");
                }
                catch (Exception ex)
                {
                    Logger.Debug("Error disposing tray context", ex);
                }
            }

            base.Dispose(disposing);
        }
    }
}
