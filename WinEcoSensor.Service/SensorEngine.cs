// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2024 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using WinEcoSensor.Common.Communication;
using WinEcoSensor.Common.Models;
using WinEcoSensor.Common.Monitoring;
using WinEcoSensor.Common.Utilities;

namespace WinEcoSensor.Service
{
    /// <summary>
    /// Core sensor engine that orchestrates all monitoring components.
    /// Collects data from all monitors and sends events to backend.
    /// </summary>
    public class SensorEngine : IDisposable
    {
        private SensorConfiguration _configuration;
        private bool _disposed;
        private bool _initialized;

        // Monitoring components
        private HardwareMonitor _hardwareMonitor;
        private CpuMonitor _cpuMonitor;
        private DisplayMonitor _displayMonitor;
        private UserActivityMonitor _userActivityMonitor;
        private RemoteSessionMonitor _remoteSessionMonitor;
        private EnergyCalculator _energyCalculator;

        // Communication components
        private CloudEventBuilder _eventBuilder;
        private BackendClient _backendClient;

        // Cached data
        private HardwareInfo _hardwareInfo;
        private UserActivityInfo _userActivity;
        private EnergyState _energyState;

        /// <summary>
        /// Current hardware information
        /// </summary>
        public HardwareInfo HardwareInfo => _hardwareInfo;

        /// <summary>
        /// Current user activity information
        /// </summary>
        public UserActivityInfo UserActivity => _userActivity;

        /// <summary>
        /// Current energy state
        /// </summary>
        public EnergyState EnergyState => _energyState;

        /// <summary>
        /// Display time statistics
        /// </summary>
        public DisplayTimeStats DisplayTimeStats => _displayMonitor?.GetTimeStats();

        /// <summary>
        /// Whether the engine is initialized
        /// </summary>
        public bool IsInitialized => _initialized;

        /// <summary>
        /// Create sensor engine with configuration
        /// </summary>
        /// <param name="configuration">Sensor configuration</param>
        public SensorEngine(SensorConfiguration configuration)
        {
            _configuration = configuration ?? new SensorConfiguration();
        }

        /// <summary>
        /// Initialize all monitoring components
        /// </summary>
        public void Initialize()
        {
            if (_initialized) return;

            try
            {
                Logger.Info("Initializing sensor engine...");

                // Initialize hardware monitor
                _hardwareMonitor = new HardwareMonitor();
                _hardwareInfo = _hardwareMonitor.CollectHardwareInfo();
                Logger.Info($"Hardware detected: {_hardwareInfo.MachineName}, {_hardwareInfo.CpuName}");

                // Initialize CPU monitor
                if (_configuration.MonitorCpu)
                {
                    _cpuMonitor = new CpuMonitor(_hardwareInfo.CpuTdpWatts);
                    Logger.Info("CPU monitoring enabled");
                }

                // Initialize display monitor
                if (_configuration.MonitorDisplays)
                {
                    _displayMonitor = new DisplayMonitor();
                    Logger.Info($"Display monitoring enabled: {_displayMonitor.Monitors.Count} monitor(s)");

                    // Apply EPREL mappings from configuration
                    ApplyEprelMappings();
                }

                // Initialize user activity monitor
                if (_configuration.MonitorUserActivity)
                {
                    _userActivityMonitor = new UserActivityMonitor();
                    Logger.Info("User activity monitoring enabled");
                }

                // Initialize remote session monitor
                if (_configuration.MonitorRemoteSessions)
                {
                    _remoteSessionMonitor = new RemoteSessionMonitor();
                    Logger.Info("Remote session monitoring enabled");
                }

                // Initialize energy calculator
                _energyCalculator = new EnergyCalculator
                {
                    PsuEfficiency = _configuration.PsuEfficiency
                };

                // Initialize communication components
                string hardwareId = _hardwareMonitor.GenerateHardwareId(_hardwareInfo);
                _eventBuilder = new CloudEventBuilder(hardwareId);
                _backendClient = new BackendClient(_configuration.BackendUrl);

                _initialized = true;
                Logger.Info("Sensor engine initialized successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to initialize sensor engine", ex);
                throw;
            }
        }

        /// <summary>
        /// Update all monitors and collect data
        /// </summary>
        public void Update()
        {
            if (!_initialized) return;

            try
            {
                // Update CPU monitor
                _cpuMonitor?.Update();

                // Update display monitor
                _displayMonitor?.Update();

                // Update user activity
                if (_userActivityMonitor != null)
                {
                    _userActivity = _userActivityMonitor.GetCurrentActivity();

                    // Check for remote sessions
                    if (_remoteSessionMonitor != null)
                    {
                        _remoteSessionMonitor.Update();
                        _userActivity.IsRdpSession = _remoteSessionMonitor.IsRdpSession;
                        _userActivity.RdpClientName = _remoteSessionMonitor.RdpClientName;
                        _userActivity.RdpClientAddress = _remoteSessionMonitor.RdpClientAddress;
                        _userActivity.IsRemoteSession = _remoteSessionMonitor.IsRemoteAccessActive;
                        _userActivity.RemoteToolName = _remoteSessionMonitor.ActiveRemoteToolName;
                    }

                    // Update display state based on idle time
                    if (_displayMonitor != null && _userActivity.IdleTimeSeconds > _configuration.IdleThresholdSeconds)
                    {
                        if (_displayMonitor.CurrentPowerState == MonitorPowerState.On)
                        {
                            _displayMonitor.SetPowerState(MonitorPowerState.Standby);
                        }
                    }
                    else if (_displayMonitor != null && _userActivity.IdleTimeSeconds < 10)
                    {
                        if (_displayMonitor.CurrentPowerState != MonitorPowerState.On)
                        {
                            _displayMonitor.SetPowerState(MonitorPowerState.On);
                        }
                    }
                }

                // Calculate energy state
                _energyState = _energyCalculator?.CalculateCurrentState(
                    _cpuMonitor,
                    _displayMonitor,
                    _hardwareInfo);
            }
            catch (Exception ex)
            {
                Logger.Error("Error updating sensor engine", ex);
            }
        }

        /// <summary>
        /// Apply EPREL mappings from configuration to monitors
        /// </summary>
        private void ApplyEprelMappings()
        {
            if (_displayMonitor == null || _configuration.EprelMappings == null)
                return;

            foreach (var mapping in _configuration.EprelMappings)
            {
                if (mapping.HardwareType == EprelHardwareType.Monitor)
                {
                    // Try to match by model name or apply to primary monitor
                    for (int i = 0; i < _displayMonitor.Monitors.Count; i++)
                    {
                        var monitor = _displayMonitor.Monitors[i];
                        if (!string.IsNullOrEmpty(monitor.Model) &&
                            monitor.Model.Contains(mapping.ModelName))
                        {
                            _displayMonitor.UpdateMonitorEprel(i, mapping);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update configuration
        /// </summary>
        /// <param name="configuration">New configuration</param>
        public void UpdateConfiguration(SensorConfiguration configuration)
        {
            _configuration = configuration ?? new SensorConfiguration();

            // Update backend client
            if (_backendClient != null)
            {
                _backendClient.BackendUrl = _configuration.BackendUrl;
                _backendClient.ResetFailureCounter();
            }

            // Update energy calculator
            if (_energyCalculator != null)
            {
                _energyCalculator.PsuEfficiency = _configuration.PsuEfficiency;
            }

            // Reapply EPREL mappings
            ApplyEprelMappings();

            Logger.Info("Configuration updated");
        }

        /// <summary>
        /// Send registration event
        /// </summary>
        public void SendRegistrationEvent()
        {
            if (_eventBuilder == null || _backendClient == null) return;

            try
            {
                var message = _eventBuilder.BuildRegistrationEvent(_hardwareInfo);
                _backendClient.SendEventAsync(message);
                Logger.Info("Registration event sent");
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending registration event", ex);
            }
        }

        /// <summary>
        /// Send status event
        /// </summary>
        public void SendStatusEvent()
        {
            if (_eventBuilder == null || _backendClient == null) return;

            try
            {
                var displayStats = _displayMonitor?.GetTimeStats();
                var message = _eventBuilder.BuildStatusEvent(
                    _hardwareInfo,
                    _userActivity,
                    _energyState,
                    displayStats);
                _backendClient.SendEventAsync(message);
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending status event", ex);
            }
        }

        /// <summary>
        /// Send heartbeat event
        /// </summary>
        public void SendHeartbeatEvent()
        {
            if (_eventBuilder == null || _backendClient == null) return;

            try
            {
                var message = _eventBuilder.BuildHeartbeatEvent(_energyState);
                _backendClient.SendEventAsync(message);
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending heartbeat event", ex);
            }
        }

        /// <summary>
        /// Send session start event
        /// </summary>
        public void SendSessionStartEvent()
        {
            if (_eventBuilder == null || _backendClient == null) return;

            try
            {
                _energyCalculator?.StartNewSession();
                var message = _eventBuilder.BuildSessionStartEvent(_userActivity);
                _backendClient.SendEventAsync(message);
                Logger.Info("Session start event sent");
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending session start event", ex);
            }
        }

        /// <summary>
        /// Send session end event
        /// </summary>
        public void SendSessionEndEvent()
        {
            if (_eventBuilder == null || _backendClient == null) return;

            try
            {
                var displayStats = _displayMonitor?.GetTimeStats();
                var message = _eventBuilder.BuildSessionEndEvent(_energyState, displayStats);
                _backendClient.SendEvent(message); // Synchronous for shutdown
                Logger.Info("Session end event sent");
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending session end event", ex);
            }
        }

        /// <summary>
        /// Send daily summary event
        /// </summary>
        public void SendDailySummaryEvent()
        {
            if (_eventBuilder == null || _backendClient == null) return;

            try
            {
                var displayStats = _displayMonitor?.GetTimeStats();
                var message = _eventBuilder.BuildDailySummaryEvent(_energyState, displayStats);
                _backendClient.SendEventAsync(message);
                Logger.Info("Daily summary event sent");
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending daily summary event", ex);
            }
        }

        #region System Event Handlers

        /// <summary>
        /// Handle system suspend
        /// </summary>
        public void OnSystemSuspend()
        {
            try
            {
                if (_displayMonitor != null)
                {
                    _displayMonitor.SetPowerState(MonitorPowerState.Suspend);
                }
                SendStatusEvent();
            }
            catch (Exception ex)
            {
                Logger.Error("Error handling system suspend", ex);
            }
        }

        /// <summary>
        /// Handle system resume
        /// </summary>
        public void OnSystemResume()
        {
            try
            {
                if (_displayMonitor != null)
                {
                    _displayMonitor.SetPowerState(MonitorPowerState.On);
                }
                SendStatusEvent();
            }
            catch (Exception ex)
            {
                Logger.Error("Error handling system resume", ex);
            }
        }

        /// <summary>
        /// Handle user logon
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        public void OnUserLogon(int sessionId)
        {
            try
            {
                _userActivityMonitor?.RecordFirstActivity();
                SendSessionStartEvent();
            }
            catch (Exception ex)
            {
                Logger.Error("Error handling user logon", ex);
            }
        }

        /// <summary>
        /// Handle user logoff
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        public void OnUserLogoff(int sessionId)
        {
            try
            {
                SendSessionEndEvent();
            }
            catch (Exception ex)
            {
                Logger.Error("Error handling user logoff", ex);
            }
        }

        /// <summary>
        /// Handle session lock
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        public void OnSessionLock(int sessionId)
        {
            try
            {
                if (_displayMonitor != null)
                {
                    _displayMonitor.SetPowerState(MonitorPowerState.Off);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error handling session lock", ex);
            }
        }

        /// <summary>
        /// Handle session unlock
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        public void OnSessionUnlock(int sessionId)
        {
            try
            {
                if (_displayMonitor != null)
                {
                    _displayMonitor.SetPowerState(MonitorPowerState.On);
                }
                _userActivityMonitor?.RecordFirstActivity();
            }
            catch (Exception ex)
            {
                Logger.Error("Error handling session unlock", ex);
            }
        }

        /// <summary>
        /// Handle remote connect
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        public void OnRemoteConnect(int sessionId)
        {
            try
            {
                _remoteSessionMonitor?.Update();
                SendStatusEvent();
            }
            catch (Exception ex)
            {
                Logger.Error("Error handling remote connect", ex);
            }
        }

        /// <summary>
        /// Handle remote disconnect
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        public void OnRemoteDisconnect(int sessionId)
        {
            try
            {
                _remoteSessionMonitor?.Update();
                SendStatusEvent();
            }
            catch (Exception ex)
            {
                Logger.Error("Error handling remote disconnect", ex);
            }
        }

        #endregion

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                _cpuMonitor?.Dispose();
                _displayMonitor?.Dispose();
                _userActivityMonitor?.Dispose();
                _remoteSessionMonitor?.Dispose();
                _backendClient?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Error("Error disposing sensor engine", ex);
            }
        }
    }
}
