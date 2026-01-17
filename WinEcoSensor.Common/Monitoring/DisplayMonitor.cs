// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2026 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using WinEcoSensor.Common.Models;
using WinEcoSensor.Common.Utilities;

namespace WinEcoSensor.Common.Monitoring
{
    public class DisplayMonitor : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip,
            MonitorEnumProc lpfnEnum, IntPtr dwData);

        // Delegate must match exactly - using ref for RECT
        private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor,
            ref RECT lprcMonitor, IntPtr dwData);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        private const int MONITOR_ON = -1;
        private const int MONITOR_OFF = 2;
        private const int MONITOR_STANDBY = 1;

        private List<MonitorInfo> _monitors;
        private MonitorPowerState _currentPowerState;
        private DateTime _lastStateChangeTime;
        private bool _disposed;
        private int _monitorDetectionIndex;

        private TimeSpan _displayOnTimeToday;
        private TimeSpan _displayOffTimeToday;
        private TimeSpan _displayIdleTimeToday;
        private DateTime _todayDate;
        private DateTime _lastUpdateTime;

        public MonitorPowerState CurrentPowerState { get { return _currentPowerState; } }
        public List<MonitorInfo> Monitors { get { return _monitors; } }
        public int ActiveMonitorCount { get; private set; }
        public double TotalPowerWatts { get; private set; }
        public TimeSpan DisplayOnTimeToday { get { return _displayOnTimeToday; } }
        public TimeSpan DisplayOffTimeToday { get { return _displayOffTimeToday; } }
        public TimeSpan DisplayIdleTimeToday { get { return _displayIdleTimeToday; } }

        public DisplayMonitor()
        {
            _monitors = new List<MonitorInfo>();
            _currentPowerState = MonitorPowerState.On; // Assume display is on at start
            _lastStateChangeTime = DateTime.UtcNow;
            _todayDate = DateTime.Today;
            _lastUpdateTime = DateTime.UtcNow;
            SystemEvents.PowerModeChanged += OnPowerModeChanged;
            SystemEvents.SessionSwitch += OnSessionSwitch;
            DetectMonitors();
        }

        public void DetectMonitors()
        {
            _monitors.Clear();
            _monitorDetectionIndex = 0;

            try
            {
                // Create delegate instance explicitly - C# 7.3 compatible
                MonitorEnumProc callback = new MonitorEnumProc(MonitorEnumCallback);
                EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);

                if (_monitors.Count > 0)
                {
                    _monitors[0].IsPrimary = true;
                }
                Logger.Debug(string.Format("Detected {0} monitor(s)", _monitors.Count));
            }
            catch (Exception ex)
            {
                Logger.Error("Error detecting monitors", ex);
            }
        }

        private bool MonitorEnumCallback(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
        {
            var monitorInfo = new MonitorInfo();
            monitorInfo.MonitorIndex = _monitorDetectionIndex++;
            monitorInfo.HorizontalResolution = lprcMonitor.right - lprcMonitor.left;
            monitorInfo.VerticalResolution = lprcMonitor.bottom - lprcMonitor.top;
            monitorInfo.PowerState = _currentPowerState != MonitorPowerState.Unknown ? _currentPowerState : MonitorPowerState.On;
            monitorInfo.ScreenSizeInches = EstimateScreenSize(monitorInfo.HorizontalResolution, monitorInfo.VerticalResolution);
            monitorInfo.PowerConsumptionOnWatts = DefaultPowerValues.GetMonitorPower(monitorInfo.ScreenSizeInches);
            _monitors.Add(monitorInfo);
            return true;
        }

        public void Update()
        {
            try
            {
                if (DateTime.Today != _todayDate)
                {
                    _displayOnTimeToday = TimeSpan.Zero;
                    _displayOffTimeToday = TimeSpan.Zero;
                    _displayIdleTimeToday = TimeSpan.Zero;
                    _todayDate = DateTime.Today;
                }

                var timeSinceLastUpdate = DateTime.UtcNow - _lastUpdateTime;
                _lastUpdateTime = DateTime.UtcNow;

                switch (_currentPowerState)
                {
                    case MonitorPowerState.On:
                        _displayOnTimeToday += timeSinceLastUpdate;
                        break;
                    case MonitorPowerState.Off:
                        _displayOffTimeToday += timeSinceLastUpdate;
                        break;
                    case MonitorPowerState.Standby:
                    case MonitorPowerState.Suspend:
                        _displayIdleTimeToday += timeSinceLastUpdate;
                        break;
                }

                foreach (var monitor in _monitors)
                {
                    monitor.PowerState = _currentPowerState;
                    monitor.LastStateChangeUtc = _lastStateChangeTime;
                }

                ActiveMonitorCount = _currentPowerState == MonitorPowerState.On ? _monitors.Count : 0;
                TotalPowerWatts = CalculateTotalPower();
            }
            catch (Exception ex)
            {
                Logger.Error("Error updating display monitor", ex);
            }
        }

        private double CalculateTotalPower()
        {
            double total = 0;
            foreach (var monitor in _monitors) { total += monitor.GetCurrentPowerConsumption(); }
            return total;
        }

        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            var previousState = _currentPowerState;
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    _currentPowerState = MonitorPowerState.On;
                    Logger.Info("Display power state: Resume (On)");
                    break;
                case PowerModes.Suspend:
                    _currentPowerState = MonitorPowerState.Suspend;
                    Logger.Info("Display power state: Suspend");
                    break;
                case PowerModes.StatusChange:
                    Logger.Debug("Display power status change");
                    break;
            }
            if (previousState != _currentPowerState) { _lastStateChangeTime = DateTime.UtcNow; }
        }

        private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            var previousState = _currentPowerState;
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    _currentPowerState = MonitorPowerState.Off;
                    Logger.Info("Display power state: Session locked (Off)");
                    break;
                case SessionSwitchReason.SessionUnlock:
                    _currentPowerState = MonitorPowerState.On;
                    Logger.Info("Display power state: Session unlocked (On)");
                    break;
                case SessionSwitchReason.ConsoleDisconnect:
                case SessionSwitchReason.RemoteDisconnect:
                    _currentPowerState = MonitorPowerState.Off;
                    Logger.Info("Display power state: Disconnected (Off)");
                    break;
                case SessionSwitchReason.ConsoleConnect:
                case SessionSwitchReason.RemoteConnect:
                    _currentPowerState = MonitorPowerState.On;
                    Logger.Info("Display power state: Connected (On)");
                    break;
                case SessionSwitchReason.SessionLogoff:
                    _currentPowerState = MonitorPowerState.Off;
                    Logger.Info("Display power state: Logoff (Off)");
                    break;
                case SessionSwitchReason.SessionLogon:
                    _currentPowerState = MonitorPowerState.On;
                    Logger.Info("Display power state: Logon (On)");
                    break;
            }
            if (previousState != _currentPowerState) { _lastStateChangeTime = DateTime.UtcNow; }
        }

        public void SetPowerState(MonitorPowerState state)
        {
            if (_currentPowerState != state)
            {
                _currentPowerState = state;
                _lastStateChangeTime = DateTime.UtcNow;
                foreach (var monitor in _monitors)
                {
                    monitor.PowerState = state;
                    monitor.LastStateChangeUtc = _lastStateChangeTime;
                }
                Logger.Debug(string.Format("Display power state set to: {0}", state));
            }
        }

        private double EstimateScreenSize(int width, int height)
        {
            long pixels = (long)width * height;
            if (pixels >= 3840 * 2160) return 32.0;
            if (pixels >= 2560 * 1440) return 27.0;
            if (pixels >= 1920 * 1200) return 24.0;
            if (pixels >= 1920 * 1080) return 24.0;
            if (pixels >= 1680 * 1050) return 22.0;
            if (pixels >= 1280 * 1024) return 19.0;
            return 22.0;
        }

        public void UpdateMonitorEprel(int monitorIndex, EprelMapping mapping)
        {
            if (monitorIndex >= 0 && monitorIndex < _monitors.Count)
            {
                var monitor = _monitors[monitorIndex];
                monitor.EprelNumber = mapping.EprelNumber;
                monitor.EnergyClass = mapping.EnergyClass;
                monitor.PowerConsumptionOnWatts = mapping.PowerOnWatts;
                monitor.PowerConsumptionStandbyWatts = mapping.PowerStandbyWatts;
                monitor.PowerConsumptionOffWatts = mapping.PowerOffWatts;
                monitor.AnnualEnergyConsumptionKwh = mapping.AnnualEnergyKwh;
                Logger.Info(string.Format("Updated EPREL mapping for monitor {0}: {1}", monitorIndex, mapping.EprelNumber));
            }
        }

        public DisplayTimeStats GetTimeStats()
        {
            return new DisplayTimeStats
            {
                OnTimeToday = _displayOnTimeToday,
                OffTimeToday = _displayOffTimeToday,
                IdleTimeToday = _displayIdleTimeToday,
                CurrentState = _currentPowerState,
                CurrentStateDuration = DateTime.UtcNow - _lastStateChangeTime
            };
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try
            {
                SystemEvents.PowerModeChanged -= OnPowerModeChanged;
                SystemEvents.SessionSwitch -= OnSessionSwitch;
            }
            catch (Exception ex) { Logger.Debug("Error disposing display monitor", ex); }
        }
    }

    public class DisplayTimeStats
    {
        public TimeSpan OnTimeToday { get; set; }
        public TimeSpan OffTimeToday { get; set; }
        public TimeSpan IdleTimeToday { get; set; }
        public MonitorPowerState CurrentState { get; set; }
        public TimeSpan CurrentStateDuration { get; set; }
    }
}
