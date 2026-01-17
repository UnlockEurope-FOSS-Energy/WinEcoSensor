// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2026 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.Drawing;
using System.ServiceProcess;
using System.Windows.Forms;
using WinEcoSensor.Common.Configuration;
using WinEcoSensor.Common.Models;
using WinEcoSensor.Common.Monitoring;
using WinEcoSensor.Common.Utilities;

namespace WinEcoSensor.TrayApp
{
    /// <summary>
    /// Status form showing real-time energy monitoring information.
    /// Displays user activity, display state, energy consumption, and more.
    /// </summary>
    public partial class StatusForm : Form
    {
        private const string ServiceName = "WinEcoSensor";
        private const int UpdateIntervalMs = 1000;

        private readonly Timer _updateTimer;
        private UserActivityMonitor _activityMonitor;
        private DisplayMonitor _displayMonitor;
        private CpuMonitor _cpuMonitor;
        private RemoteSessionMonitor _remoteMonitor;
        private EnergyCalculator _energyCalculator;

        /// <summary>
        /// Create status form
        /// </summary>
        public StatusForm()
        {
            InitializeComponent();
            InitializeMonitors();

            // Create update timer
            _updateTimer = new Timer
            {
                Interval = UpdateIntervalMs
            };
            _updateTimer.Tick += OnUpdateTimerTick;
            _updateTimer.Start();

            // Initial update
            UpdateAllStatus();
        }

        /// <summary>
        /// Initialize monitoring components
        /// </summary>
        private void InitializeMonitors()
        {
            try
            {
                _activityMonitor = new UserActivityMonitor();
                _displayMonitor = new DisplayMonitor();
                _cpuMonitor = new CpuMonitor();
                _remoteMonitor = new RemoteSessionMonitor();
                _energyCalculator = new EnergyCalculator();

                Logger.Debug("Status form monitors initialized");
            }
            catch (Exception ex)
            {
                Logger.Error("Error initializing status monitors", ex);
            }
        }

        /// <summary>
        /// Timer tick - update all status
        /// </summary>
        private void OnUpdateTimerTick(object sender, EventArgs e)
        {
            UpdateAllStatus();
        }

        /// <summary>
        /// Update all status displays
        /// </summary>
        private void UpdateAllStatus()
        {
            try
            {
                UpdateServiceStatus();
                UpdateActivityStatus();
                UpdateDisplayStatus();
                UpdateEnergyStatus();
                UpdateRemoteStatus();
            }
            catch (Exception ex)
            {
                Logger.Debug("Error updating status form", ex);
            }
        }

        /// <summary>
        /// Update service status display
        /// </summary>
        private void UpdateServiceStatus()
        {
            try
            {
                using (var controller = new ServiceController(ServiceName))
                {
                    string statusText;
                    Color statusColor;

                    switch (controller.Status)
                    {
                        case ServiceControllerStatus.Running:
                            statusText = "Running";
                            statusColor = Color.Green;
                            break;
                        case ServiceControllerStatus.Stopped:
                            statusText = "Stopped";
                            statusColor = Color.Red;
                            break;
                        case ServiceControllerStatus.Paused:
                            statusText = "Paused";
                            statusColor = Color.Orange;
                            break;
                        default:
                            statusText = controller.Status.ToString();
                            statusColor = Color.Gray;
                            break;
                    }

                    lblServiceStatus.Text = statusText;
                    lblServiceStatus.ForeColor = statusColor;
                }
            }
            catch (InvalidOperationException)
            {
                lblServiceStatus.Text = "Not Installed";
                lblServiceStatus.ForeColor = Color.Red;
            }
            catch
            {
                lblServiceStatus.Text = "Error";
                lblServiceStatus.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// Update user activity status
        /// </summary>
        private void UpdateActivityStatus()
        {
            try
            {
                var info = _activityMonitor.Update();

                // Login status
                lblLoginStatus.Text = info.IsLoggedIn ? "Logged In" : "Logged Out";
                lblLoginStatus.ForeColor = info.IsLoggedIn ? Color.Green : Color.Gray;

                // User name
                lblUserName.Text = info.UserName ?? "Unknown";

                // Session state
                lblSessionState.Text = info.SessionState.ToString();

                // First activity today
                var firstActivity = _activityMonitor.GetFirstActivityToday();
                if (firstActivity.HasValue)
                {
                    lblFirstActivity.Text = firstActivity.Value.ToLocalTime().ToString("HH:mm:ss");
                }
                else
                {
                    lblFirstActivity.Text = "No activity yet";
                }

                // Duration since first activity
                var duration = _activityMonitor.GetTimeSinceFirstActivity();
                if (duration.TotalSeconds > 0)
                {
                    lblDuration.Text = FormatDuration(duration);
                }
                else
                {
                    lblDuration.Text = "--:--:--";
                }

                // Idle time
                lblIdleTime.Text = FormatDuration(info.IdleTime);

                // Screen saver
                lblScreenSaver.Text = info.IsScreenSaverRunning ? "Active" : "Inactive";
                lblScreenSaver.ForeColor = info.IsScreenSaverRunning ? Color.Orange : Color.Green;

                // Workstation locked
                lblWorkstationLocked.Text = info.IsWorkstationLocked ? "Locked" : "Unlocked";
                lblWorkstationLocked.ForeColor = info.IsWorkstationLocked ? Color.Orange : Color.Green;
            }
            catch (Exception ex)
            {
                Logger.Debug("Error updating activity status", ex);
            }
        }

        /// <summary>
        /// Update display status
        /// </summary>
        private void UpdateDisplayStatus()
        {
            try
            {
                _displayMonitor.Update();
                var stats = _displayMonitor.GetTimeStats();

                // Monitor count
                lblMonitorCount.Text = _displayMonitor.Monitors.Count.ToString();

                // Current state
                string stateText;
                Color stateColor;

                switch (stats.CurrentState)
                {
                    case MonitorPowerState.On:
                        stateText = "On";
                        stateColor = Color.Green;
                        break;
                    case MonitorPowerState.Off:
                        stateText = "Off";
                        stateColor = Color.Gray;
                        break;
                    case MonitorPowerState.Standby:
                        stateText = "Standby";
                        stateColor = Color.Orange;
                        break;
                    case MonitorPowerState.Suspend:
                        stateText = "Suspended";
                        stateColor = Color.DarkOrange;
                        break;
                    default:
                        stateText = "Unknown";
                        stateColor = Color.Gray;
                        break;
                }

                lblDisplayState.Text = stateText;
                lblDisplayState.ForeColor = stateColor;

                // Time in current state
                lblStateDuration.Text = FormatDuration(stats.CurrentStateDuration);

                // Today's times
                lblOnTime.Text = FormatDuration(stats.OnTimeToday);
                lblOffTime.Text = FormatDuration(stats.OffTimeToday);
                lblIdleDisplayTime.Text = FormatDuration(stats.IdleTimeToday);

                // Display power
                lblDisplayPower.Text = $"{_displayMonitor.TotalPowerWatts:F1} W";
            }
            catch (Exception ex)
            {
                Logger.Debug("Error updating display status", ex);
            }
        }

        /// <summary>
        /// Update energy status
        /// </summary>
        private void UpdateEnergyStatus()
        {
            try
            {
                _cpuMonitor.Update();

                // CPU usage
                var cpuUsage = _cpuMonitor.CurrentUsagePercent;
                lblCpuUsage.Text = $"{cpuUsage:F1}%";
                progressCpu.Value = Math.Min(100, Math.Max(0, (int)cpuUsage));

                // CPU power
                lblCpuPower.Text = $"{_cpuMonitor.EstimatedPowerWatts:F1} W";

                // Calculate total power
                var totalPower = _energyCalculator.CalculateCurrentPower(
                    _cpuMonitor.EstimatedPowerWatts,
                    _displayMonitor.TotalPowerWatts,
                    null, null, null);

                lblTotalPower.Text = $"{totalPower:F1} W";

                // Energy today (estimated)
                var displayStats = _displayMonitor.GetTimeStats();
                var energyToday = _energyCalculator.EstimateDailyEnergy(
                    displayStats.OnTimeToday,
                    cpuUsage);

                lblEnergyToday.Text = $"{energyToday:F3} kWh";

                // Cost estimate (assuming €0.30/kWh)
                var costEstimate = energyToday * 0.30;
                lblCostEstimate.Text = $"€{costEstimate:F4}";
            }
            catch (Exception ex)
            {
                Logger.Debug("Error updating energy status", ex);
            }
        }

        /// <summary>
        /// Update remote access status
        /// </summary>
        private void UpdateRemoteStatus()
        {
            try
            {
                _remoteMonitor.Update();

                // RDP
                lblRdpStatus.Text = _remoteMonitor.IsRdpActive ? "Connected" : "Not Connected";
                lblRdpStatus.ForeColor = _remoteMonitor.IsRdpActive ? Color.Blue : Color.Gray;

                if (_remoteMonitor.IsRdpActive && !string.IsNullOrEmpty(_remoteMonitor.RdpClientName))
                {
                    lblRdpClient.Text = _remoteMonitor.RdpClientName;
                }
                else
                {
                    lblRdpClient.Text = "-";
                }

                // TeamViewer
                bool isTeamViewerRunning = _remoteMonitor.IsRemoteToolRunning(RemoteSessionType.TeamViewer);
                lblTeamViewerStatus.Text = isTeamViewerRunning ? "Running" : "Not Running";
                lblTeamViewerStatus.ForeColor = isTeamViewerRunning ? Color.Blue : Color.Gray;

                // AnyDesk
                bool isAnyDeskRunning = _remoteMonitor.IsRemoteToolRunning(RemoteSessionType.AnyDesk);
                lblAnyDeskStatus.Text = isAnyDeskRunning ? "Running" : "Not Running";
                lblAnyDeskStatus.ForeColor = isAnyDeskRunning ? Color.Blue : Color.Gray;

                // Other remote tools (VNC, LogMeIn, etc.)
                bool isOtherRunning = _remoteMonitor.IsRemoteToolRunning(RemoteSessionType.VNC) ||
                                       _remoteMonitor.IsRemoteToolRunning(RemoteSessionType.LogMeIn) ||
                                       _remoteMonitor.IsRemoteToolRunning(RemoteSessionType.Other);
                lblOtherRemote.Text = isOtherRunning ? "Detected" : "None";
                lblOtherRemote.ForeColor = isOtherRunning ? Color.Blue : Color.Gray;
            }
            catch (Exception ex)
            {
                Logger.Debug("Error updating remote status", ex);
            }
        }

        /// <summary>
        /// Format timespan as HH:mm:ss
        /// </summary>
        private string FormatDuration(TimeSpan duration)
        {
            return $"{(int)duration.TotalHours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}";
        }

        /// <summary>
        /// Minimize to tray on close
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                base.OnFormClosing(e);
            }
        }

        /// <summary>
        /// Clean up resources
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _updateTimer?.Stop();
                _updateTimer?.Dispose();

                _activityMonitor?.Dispose();
                _displayMonitor?.Dispose();
                _cpuMonitor?.Dispose();
                _remoteMonitor?.Dispose();

                components?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
