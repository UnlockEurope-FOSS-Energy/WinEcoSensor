// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2024 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.Diagnostics;
using WinEcoSensor.Common.Utilities;

namespace WinEcoSensor.Common.Monitoring
{
    public class CpuMonitor : IDisposable
    {
        private PerformanceCounter _cpuCounter;
        private PerformanceCounter _cpuPrivilegedCounter;
        private double _cpuTdpWatts;
        private bool _initialized;
        private bool _disposed;

        public double CurrentUsagePercent { get; private set; }
        public double EstimatedPowerWatts { get; private set; }
        
        // Alias for EstimatedPowerWatts
        public double CurrentPowerWatts
        {
            get { return EstimatedPowerWatts; }
        }

        public double CpuTdpWatts
        {
            get { return _cpuTdpWatts; }
            set { _cpuTdpWatts = value; }
        }

        public CpuMonitor(double cpuTdpWatts = 65)
        {
            _cpuTdpWatts = cpuTdpWatts;
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
                _cpuPrivilegedCounter = new PerformanceCounter("Processor", "% Privileged Time", "_Total", true);
                _cpuCounter.NextValue();
                _cpuPrivilegedCounter.NextValue();
                _initialized = true;
                Logger.Debug("CPU monitor initialized successfully");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to initialize CPU performance counters", ex);
                _initialized = false;
            }
        }

        public void Update()
        {
            if (!_initialized || _disposed)
            {
                UpdateViaWmi();
                return;
            }

            try
            {
                CurrentUsagePercent = _cpuCounter.NextValue();
                if (CurrentUsagePercent < 0) CurrentUsagePercent = 0;
                if (CurrentUsagePercent > 100) CurrentUsagePercent = 100;

                double idlePower = _cpuTdpWatts * 0.15;
                EstimatedPowerWatts = idlePower + (CurrentUsagePercent / 100.0) * (_cpuTdpWatts - idlePower);

                if (CurrentUsagePercent < 5)
                {
                    EstimatedPowerWatts = _cpuTdpWatts * 0.05;
                }
                else if (CurrentUsagePercent < 20)
                {
                    EstimatedPowerWatts = _cpuTdpWatts * (0.05 + (CurrentUsagePercent / 20.0) * 0.15);
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("Error reading CPU counter, falling back to WMI", ex);
                UpdateViaWmi();
            }
        }

        private void UpdateViaWmi()
        {
            try
            {
                var loadPercent = WmiHelper.GetValue<int>(
                    "SELECT LoadPercentage FROM Win32_Processor",
                    "LoadPercentage", 0);
                CurrentUsagePercent = loadPercent;
                double idlePower = _cpuTdpWatts * 0.15;
                EstimatedPowerWatts = idlePower + (CurrentUsagePercent / 100.0) * (_cpuTdpWatts - idlePower);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to get CPU usage via WMI", ex);
                CurrentUsagePercent = 0;
                EstimatedPowerWatts = _cpuTdpWatts * 0.15;
            }
        }

        public double GetMemoryUsagePercent()
        {
            try
            {
                using (var memCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use", true))
                {
                    return memCounter.NextValue();
                }
            }
            catch { return 0; }
        }

        public long GetAvailableMemoryBytes()
        {
            try
            {
                using (var memCounter = new PerformanceCounter("Memory", "Available Bytes", true))
                {
                    return (long)memCounter.NextValue();
                }
            }
            catch { return 0; }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try
            {
                _cpuCounter?.Dispose();
                _cpuPrivilegedCounter?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Debug("Error disposing CPU monitor", ex);
            }
        }
    }
}
