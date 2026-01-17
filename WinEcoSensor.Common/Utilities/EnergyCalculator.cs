// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2026 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.Collections.Generic;
using WinEcoSensor.Common.Models;
using WinEcoSensor.Common.Monitoring;

namespace WinEcoSensor.Common.Utilities
{
    /// <summary>
    /// Calculates aggregate energy consumption based on hardware components and usage patterns.
    /// Provides methods for instantaneous power, daily energy totals, and efficiency ratings.
    /// </summary>
    public class EnergyCalculator
    {
        // Base system power (motherboard, fans, peripherals) in watts
        private const double BaseSystemPowerWatts = 30.0;

        // Default PSU efficiency (85%)
        private const double DefaultPsuEfficiency = 0.85;

        // Session tracking
        private DateTime _sessionStartUtc;
        private double _sessionEnergyWh;
        private double _dailyEnergyWh;
        private DateTime _dailyResetDate;
        private double _lastPowerWatts;
        private DateTime _lastCalculationTime;

        /// <summary>
        /// PSU efficiency factor (0.0 to 1.0)
        /// </summary>
        public double PsuEfficiency { get; set; } = DefaultPsuEfficiency;

        /// <summary>
        /// Current session start time
        /// </summary>
        public DateTime SessionStartUtc => _sessionStartUtc;

        /// <summary>
        /// Total energy consumed this session in Wh
        /// </summary>
        public double SessionEnergyWh => _sessionEnergyWh;

        /// <summary>
        /// Total energy consumed today in Wh
        /// </summary>
        public double DailyEnergyWh => _dailyEnergyWh;

        /// <summary>
        /// Last calculated power in Watts
        /// </summary>
        public double LastPowerWatts => _lastPowerWatts;

        /// <summary>
        /// Create new energy calculator
        /// </summary>
        public EnergyCalculator()
        {
            _sessionStartUtc = DateTime.UtcNow;
            _dailyResetDate = DateTime.Today;
            _lastCalculationTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Calculate current total system power consumption
        /// </summary>
        /// <param name="cpuMonitor">CPU monitor instance</param>
        /// <param name="displayMonitor">Display monitor instance</param>
        /// <param name="hardwareInfo">Hardware information</param>
        /// <returns>Current energy state with power breakdown</returns>
        public EnergyState CalculateCurrentState(
            CpuMonitor cpuMonitor,
            DisplayMonitor displayMonitor,
            HardwareInfo hardwareInfo)
        {
            var now = DateTime.UtcNow;
            var state = new EnergyState
            {
                TimestampUtc = now
            };

            // Reset daily counters if new day
            if (DateTime.Today != _dailyResetDate)
            {
                _dailyEnergyWh = 0;
                _dailyResetDate = DateTime.Today;
            }

            try
            {
                // Calculate CPU power
                double cpuPower = cpuMonitor?.CurrentPowerWatts ?? 0;
                state.CpuPowerWatts = cpuPower;

                // Calculate display power
                double displayPower = displayMonitor?.TotalPowerWatts ?? 0;
                state.DisplayPowerWatts = displayPower;

                // Calculate disk power
                double diskPower = CalculateDiskPower(hardwareInfo?.Disks);
                state.DiskPowerWatts = diskPower;

                // Calculate GPU power (estimated from hardware info)
                double gpuPower = CalculateGpuPower(hardwareInfo?.Gpus);
                state.GpuPowerWatts = gpuPower;

                // Calculate memory power (approximately 3W per 8GB)
                double memoryPower = CalculateMemoryPower(hardwareInfo?.TotalMemoryMB ?? 0);
                state.MemoryPowerWatts = memoryPower;

                // Calculate base system power
                state.BasePowerWatts = BaseSystemPowerWatts;

                // Total DC power (before PSU losses)
                double dcPower = cpuPower + displayPower + diskPower + gpuPower + memoryPower + BaseSystemPowerWatts;

                // Apply PSU efficiency to get AC (wall) power
                double acPower = dcPower / PsuEfficiency;
                state.TotalPowerWatts = acPower;
                state.PsuEfficiency = PsuEfficiency;

                // Update energy totals
                var timeSinceLastCalculation = now - _lastCalculationTime;
                double hours = timeSinceLastCalculation.TotalHours;
                
                if (hours > 0 && hours < 1) // Sanity check: only count reasonable intervals
                {
                    double energyWh = _lastPowerWatts * hours;
                    _sessionEnergyWh += energyWh;
                    _dailyEnergyWh += energyWh;
                }

                _lastPowerWatts = acPower;
                _lastCalculationTime = now;

                // Set totals in state
                state.DailyEnergyWh = _dailyEnergyWh;
                state.SessionEnergyWh = _sessionEnergyWh;
                state.SessionDuration = now - _sessionStartUtc;

                // Calculate efficiency rating
                state.EfficiencyRating = CalculateEfficiencyRating(acPower, displayMonitor?.ActiveMonitorCount ?? 0);
            }
            catch (Exception ex)
            {
                Logger.Error("Error calculating energy state", ex);
            }

            return state;
        }

        /// <summary>
        /// Calculate disk power consumption
        /// </summary>
        private double CalculateDiskPower(List<DiskInfo> disks)
        {
            if (disks == null || disks.Count == 0)
                return 0;

            double total = 0;
            foreach (var disk in disks)
            {
                // Use active power as default estimate
                total += disk.PowerActiveWatts * 0.3 + disk.PowerIdleWatts * 0.7;
            }
            return total;
        }

        /// <summary>
        /// Calculate GPU power consumption
        /// </summary>
        private double CalculateGpuPower(List<string> gpus)
        {
            if (gpus == null || gpus.Count == 0)
                return 0;

            double total = 0;
            foreach (var gpu in gpus)
            {
                // Estimate GPU power at idle (about 20% of TDP)
                double tdp = EstimateGpuTdp(gpu);
                total += tdp * 0.2;
            }
            return total;
        }

        /// <summary>
        /// Estimate GPU TDP based on name
        /// </summary>
        private double EstimateGpuTdp(string gpuName)
        {
            if (string.IsNullOrEmpty(gpuName))
                return 15; // Integrated

            string name = gpuName.ToUpperInvariant();

            // Integrated graphics
            if (name.Contains("INTEL") && (name.Contains("HD") || name.Contains("UHD") || name.Contains("IRIS")))
                return 15;
            if (name.Contains("AMD") && name.Contains("VEGA") && name.Contains("GRAPHICS"))
                return 15;

            // Entry level
            if (name.Contains("GT 1030") || name.Contains("RX 550") || name.Contains("GTX 1050"))
                return 75;

            // Mid-range
            if (name.Contains("GTX 1060") || name.Contains("GTX 1650") || name.Contains("RTX 3050") ||
                name.Contains("RX 580") || name.Contains("RX 5600"))
                return 120;

            // High-end
            if (name.Contains("RTX 3060") || name.Contains("RTX 3070") || name.Contains("RTX 4060") ||
                name.Contains("RX 6700") || name.Contains("RX 7600"))
                return 200;

            // Enthusiast
            if (name.Contains("RTX 3080") || name.Contains("RTX 3090") || name.Contains("RTX 4070") ||
                name.Contains("RTX 4080") || name.Contains("RTX 4090") ||
                name.Contains("RX 6800") || name.Contains("RX 6900") || name.Contains("RX 7800") || name.Contains("RX 7900"))
                return 350;

            // Default to entry level for unknown dedicated GPUs
            return 75;
        }

        /// <summary>
        /// Calculate memory power consumption
        /// </summary>
        private double CalculateMemoryPower(long totalMemoryMB)
        {
            // DDR4/DDR5: approximately 3-4W per 8GB module
            // Estimate based on total memory
            double modules = Math.Ceiling(totalMemoryMB / 8192.0);
            return modules * 3.0;
        }

        /// <summary>
        /// Calculate efficiency rating (A-G scale)
        /// </summary>
        private EnergyEfficiencyRating CalculateEfficiencyRating(double totalPowerWatts, int activeMonitors)
        {
            // Base threshold depends on usage pattern
            double threshold = 50 + (activeMonitors * 30);

            double ratio = totalPowerWatts / threshold;

            if (ratio <= 0.5) return EnergyEfficiencyRating.Excellent;
            if (ratio <= 0.75) return EnergyEfficiencyRating.Good;
            if (ratio <= 1.25) return EnergyEfficiencyRating.Average;
            return EnergyEfficiencyRating.NeedsImprovement;
        }

        /// <summary>
        /// Start a new monitoring session
        /// </summary>
        public void StartNewSession()
        {
            _sessionStartUtc = DateTime.UtcNow;
            _sessionEnergyWh = 0;
            _lastCalculationTime = DateTime.UtcNow;
            _lastPowerWatts = 0;
            Logger.Info("New energy monitoring session started");
        }

        /// <summary>
        /// Get estimated daily energy consumption in kWh
        /// </summary>
        public double GetEstimatedDailyKwh()
        {
            return _dailyEnergyWh / 1000.0;
        }

        /// <summary>
        /// Get estimated monthly energy consumption in kWh
        /// </summary>
        public double GetEstimatedMonthlyKwh()
        {
            // Assume 22 working days per month
            return GetEstimatedDailyKwh() * 22;
        }

        /// <summary>
        /// Get estimated annual energy consumption in kWh
        /// </summary>
        public double GetEstimatedAnnualKwh()
        {
            // Assume 260 working days per year
            return GetEstimatedDailyKwh() * 260;
        }

        /// <summary>
        /// Calculate CO2 equivalent emissions
        /// </summary>
        /// <param name="energyKwh">Energy consumption in kWh</param>
        /// <param name="co2PerKwh">CO2 per kWh (default: EU average 0.276 kg/kWh)</param>
        /// <returns>CO2 emissions in kg</returns>
        public static double CalculateCo2Emissions(double energyKwh, double co2PerKwh = 0.276)
        {
            return energyKwh * co2PerKwh;
        }

        /// <summary>
        /// Calculate estimated energy cost
        /// </summary>
        /// <param name="energyKwh">Energy consumption in kWh</param>
        /// <param name="pricePerKwh">Price per kWh (default: 0.25 EUR)</param>
        /// <returns>Estimated cost</returns>
        public static double CalculateEnergyCost(double energyKwh, double pricePerKwh = 0.25)
        {
            return energyKwh * pricePerKwh;
        }

        /// <summary>
        /// Reset all counters
        /// </summary>
        public void Reset()
        {
            _sessionStartUtc = DateTime.UtcNow;
            _sessionEnergyWh = 0;
            _dailyEnergyWh = 0;
            _dailyResetDate = DateTime.Today;
            _lastPowerWatts = 0;
            _lastCalculationTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Calculate current power from individual components (convenience method for tray app)
        /// </summary>
        /// <param name="cpuPowerWatts">CPU power consumption</param>
        /// <param name="displayPowerWatts">Display power consumption</param>
        /// <param name="diskPowerWatts">Disk power consumption (optional)</param>
        /// <param name="gpuPowerWatts">GPU power consumption (optional)</param>
        /// <param name="memoryPowerWatts">Memory power consumption (optional)</param>
        /// <returns>Total estimated power in watts</returns>
        public double CalculateCurrentPower(
            double cpuPowerWatts, 
            double displayPowerWatts,
            double? diskPowerWatts,
            double? gpuPowerWatts,
            double? memoryPowerWatts)
        {
            double dcPower = cpuPowerWatts + displayPowerWatts;
            dcPower += diskPowerWatts ?? 5.0; // Default disk power
            dcPower += gpuPowerWatts ?? 15.0; // Default integrated GPU power
            dcPower += memoryPowerWatts ?? 6.0; // Default memory power
            dcPower += BaseSystemPowerWatts;

            // Apply PSU efficiency
            return dcPower / PsuEfficiency;
        }

        /// <summary>
        /// Estimate daily energy based on display on-time and average CPU usage
        /// </summary>
        /// <param name="displayOnTime">Time displays have been on today</param>
        /// <param name="averageCpuUsagePercent">Average CPU usage percentage</param>
        /// <returns>Estimated daily energy in kWh</returns>
        public double EstimateDailyEnergy(TimeSpan displayOnTime, double averageCpuUsagePercent)
        {
            // Estimate average power during operation
            double estimatedCpuPower = 65 * (0.15 + 0.85 * averageCpuUsagePercent / 100.0); // Based on 65W TDP
            double estimatedDisplayPower = 35.0; // Average monitor
            double estimatedOtherPower = BaseSystemPowerWatts + 15.0 + 6.0; // Base + GPU idle + memory

            double totalPower = (estimatedCpuPower + estimatedDisplayPower + estimatedOtherPower) / PsuEfficiency;

            // Energy = Power * Time
            double hours = displayOnTime.TotalHours;
            double energyWh = totalPower * hours;

            return energyWh / 1000.0; // Convert to kWh
        }
    }
}
