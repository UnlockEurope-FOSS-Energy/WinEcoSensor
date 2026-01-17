// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2026 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.ServiceProcess;
using System.Threading;
using WinEcoSensor.Common.Configuration;
using WinEcoSensor.Common.Models;
using WinEcoSensor.Common.Utilities;

namespace WinEcoSensor.Service
{
    /// <summary>
    /// Windows Service implementation for WinEcoSensor.
    /// Monitors user presence, display activity, and energy-relevant states.
    /// </summary>
    public partial class WinEcoSensorService : ServiceBase
    {
        // Service name constants
        public const string ServiceName_ = "WinEcoSensor";
        public const string DisplayName_ = "WinEcoSensor – Windows Eco Energy Sensor";
        public const string Description_ = "Monitors user presence, display activity, and energy-relevant states on Windows systems to support energy-efficient operations";

        private SensorEngine _engine;
        private ConfigurationManager _configManager;
        private Thread _serviceThread;
        private ManualResetEvent _stopEvent;
        private bool _paused;

        /// <summary>
        /// Create new service instance
        /// </summary>
        public WinEcoSensorService()
        {
            this.ServiceName = ServiceName_;
            this.CanStop = true;
            this.CanPauseAndContinue = true;
            this.CanShutdown = true;
            this.AutoLog = true;

            _stopEvent = new ManualResetEvent(false);
        }

        /// <summary>
        /// Service start handler
        /// </summary>
        /// <param name="args">Start arguments</param>
        protected override void OnStart(string[] args)
        {
            StartService(args);
        }

        /// <summary>
        /// Start the service (can be called externally for console mode)
        /// </summary>
        /// <param name="args">Start arguments</param>
        public void StartService(string[] args)
        {
            var startupStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var serviceStartTime = DateTime.Now;

            try
            {
                // Load configuration
                _configManager = new ConfigurationManager();
                var config = _configManager.Load();

                // Initialize logging
                string logPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "WinEcoSensor", "Logs", "Service.log");

                Logger.Initialize(
                    logFilePath: logPath,
                    logLevel: config.LogLevelValue,
                    logToConsole: Environment.UserInteractive
                );

                Logger.Info("==============================================");
                Logger.Info("WinEcoSensor Service Starting");
                Logger.Info($"Version: {GetServiceVersion()}");
                Logger.Info($"Service Start Time: {serviceStartTime:yyyy-MM-dd HH:mm:ss.fff}");
                Logger.Info("==============================================");

                // Create and initialize sensor engine
                _engine = new SensorEngine(config);
                _engine.Initialize();

                // Reset stop event
                _stopEvent.Reset();
                _paused = false;

                // Start service thread
                _serviceThread = new Thread(ServiceLoop)
                {
                    Name = "WinEcoSensorServiceThread",
                    IsBackground = true
                };
                _serviceThread.Start();

                startupStopwatch.Stop();
                Logger.Info($"WinEcoSensor Service started successfully (startup time: {startupStopwatch.ElapsedMilliseconds} ms)");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to start service", ex);
                throw;
            }
        }

        /// <summary>
        /// Service stop handler
        /// </summary>
        protected override void OnStop()
        {
            StopService();
        }

        /// <summary>
        /// Stop the service (can be called externally for console mode)
        /// </summary>
        public void StopService()
        {
            try
            {
                Logger.Info("WinEcoSensor Service stopping...");

                // Signal stop
                _stopEvent.Set();

                // Wait for service thread to finish
                if (_serviceThread != null && _serviceThread.IsAlive)
                {
                    if (!_serviceThread.Join(TimeSpan.FromSeconds(30)))
                    {
                        Logger.Warning("Service thread did not stop gracefully, forcing abort");
                        _serviceThread.Abort();
                    }
                }

                // Send session end event
                _engine?.SendSessionEndEvent();

                // Dispose engine
                _engine?.Dispose();
                _engine = null;

                Logger.Info("WinEcoSensor Service stopped");
            }
            catch (Exception ex)
            {
                Logger.Error("Error stopping service", ex);
            }
        }

        /// <summary>
        /// Service pause handler
        /// </summary>
        protected override void OnPause()
        {
            Logger.Info("WinEcoSensor Service pausing...");
            _paused = true;
            Logger.Info("WinEcoSensor Service paused");
        }

        /// <summary>
        /// Service continue handler
        /// </summary>
        protected override void OnContinue()
        {
            Logger.Info("WinEcoSensor Service resuming...");
            _paused = false;
            Logger.Info("WinEcoSensor Service resumed");
        }

        /// <summary>
        /// System shutdown handler
        /// </summary>
        protected override void OnShutdown()
        {
            Logger.Info("System shutdown detected");
            StopService();
        }

        /// <summary>
        /// Handle power events (suspend/resume)
        /// </summary>
        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            switch (powerStatus)
            {
                case PowerBroadcastStatus.Suspend:
                    Logger.Info("System suspending");
                    _engine?.OnSystemSuspend();
                    break;

                case PowerBroadcastStatus.ResumeSuspend:
                case PowerBroadcastStatus.ResumeAutomatic:
                    Logger.Info("System resuming");
                    _engine?.OnSystemResume();
                    break;

                case PowerBroadcastStatus.QuerySuspend:
                    Logger.Debug("System query suspend");
                    break;

                case PowerBroadcastStatus.QuerySuspendFailed:
                    Logger.Debug("System query suspend failed");
                    break;
            }

            return true;
        }

        /// <summary>
        /// Handle session change events (logon/logoff)
        /// </summary>
        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            switch (changeDescription.Reason)
            {
                case SessionChangeReason.SessionLogon:
                    Logger.Info($"User logon detected (Session {changeDescription.SessionId})");
                    _engine?.OnUserLogon(changeDescription.SessionId);
                    break;

                case SessionChangeReason.SessionLogoff:
                    Logger.Info($"User logoff detected (Session {changeDescription.SessionId})");
                    _engine?.OnUserLogoff(changeDescription.SessionId);
                    break;

                case SessionChangeReason.SessionLock:
                    Logger.Debug($"Session locked (Session {changeDescription.SessionId})");
                    _engine?.OnSessionLock(changeDescription.SessionId);
                    break;

                case SessionChangeReason.SessionUnlock:
                    Logger.Debug($"Session unlocked (Session {changeDescription.SessionId})");
                    _engine?.OnSessionUnlock(changeDescription.SessionId);
                    break;

                case SessionChangeReason.RemoteConnect:
                    Logger.Info($"Remote session connected (Session {changeDescription.SessionId})");
                    _engine?.OnRemoteConnect(changeDescription.SessionId);
                    break;

                case SessionChangeReason.RemoteDisconnect:
                    Logger.Info($"Remote session disconnected (Session {changeDescription.SessionId})");
                    _engine?.OnRemoteDisconnect(changeDescription.SessionId);
                    break;
            }

            base.OnSessionChange(changeDescription);
        }

        /// <summary>
        /// Main service loop
        /// </summary>
        private void ServiceLoop()
        {
            Logger.Debug("Service loop started");

            // Send registration event on startup
            _engine?.SendRegistrationEvent();

            // Send session start event
            _engine?.SendSessionStartEvent();

            DateTime lastHeartbeat = DateTime.MinValue;
            DateTime lastStatus = DateTime.MinValue;
            DateTime lastDailySummary = DateTime.Today;

            while (!_stopEvent.WaitOne(1000)) // Check every second
            {
                if (_paused)
                {
                    continue;
                }

                try
                {
                    var now = DateTime.UtcNow;
                    var config = _configManager?.Configuration ?? new SensorConfiguration();

                    // Update all monitors
                    _engine?.Update();

                    // Check if daily summary needs to be sent
                    if (DateTime.Today > lastDailySummary)
                    {
                        _engine?.SendDailySummaryEvent();
                        lastDailySummary = DateTime.Today;
                        Logger.Info("Daily summary event sent");
                    }

                    // Check if status update is due
                    if ((now - lastStatus).TotalSeconds >= config.StatusIntervalSeconds)
                    {
                        _engine?.SendStatusEvent();
                        lastStatus = now;
                        Logger.Debug("Status event sent");
                    }

                    // Check if heartbeat is due
                    if ((now - lastHeartbeat).TotalSeconds >= config.HeartbeatIntervalSeconds)
                    {
                        _engine?.SendHeartbeatEvent();
                        lastHeartbeat = now;
                        Logger.Debug("Heartbeat event sent");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Error in service loop", ex);
                }
            }

            Logger.Debug("Service loop ended");
        }

        /// <summary>
        /// Get service version from assembly
        /// </summary>
        private string GetServiceVersion()
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                return version?.ToString() ?? "1.0.0.0";
            }
            catch
            {
                return "1.0.0.0";
            }
        }

        /// <summary>
        /// Reload configuration
        /// </summary>
        public void ReloadConfiguration()
        {
            try
            {
                Logger.Info("Reloading configuration...");
                var config = _configManager?.Load();
                if (config != null && _engine != null)
                {
                    _engine.UpdateConfiguration(config);
                    Logger.Info("Configuration reloaded");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error reloading configuration", ex);
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stopEvent?.Dispose();
                _engine?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
