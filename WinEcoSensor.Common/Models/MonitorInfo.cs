// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2026 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.Runtime.Serialization;

namespace WinEcoSensor.Common.Models
{
    /// <summary>
    /// Represents display/monitor information including energy-relevant data.
    /// Supports EPREL (European Product Registry for Energy Labelling) integration.
    /// </summary>
    [DataContract]
    public class MonitorInfo
    {
        /// <summary>
        /// Unique device identifier
        /// </summary>
        [DataMember(Name = "deviceId")]
        public string DeviceId { get; set; }

        /// <summary>
        /// Monitor manufacturer name
        /// </summary>
        [DataMember(Name = "manufacturer")]
        public string Manufacturer { get; set; }

        /// <summary>
        /// Monitor model/product name
        /// </summary>
        [DataMember(Name = "model")]
        public string Model { get; set; }

        /// <summary>
        /// Monitor serial number (if available)
        /// </summary>
        [DataMember(Name = "serialNumber")]
        public string SerialNumber { get; set; }

        /// <summary>
        /// Display name as reported by Windows
        /// </summary>
        [DataMember(Name = "displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Horizontal resolution in pixels
        /// </summary>
        [DataMember(Name = "horizontalResolution")]
        public int HorizontalResolution { get; set; }

        /// <summary>
        /// Vertical resolution in pixels
        /// </summary>
        [DataMember(Name = "verticalResolution")]
        public int VerticalResolution { get; set; }

        /// <summary>
        /// Screen diagonal size in inches
        /// </summary>
        [DataMember(Name = "screenSizeInches")]
        public double ScreenSizeInches { get; set; }

        /// <summary>
        /// Bits per pixel (color depth)
        /// </summary>
        [DataMember(Name = "bitsPerPixel")]
        public int BitsPerPixel { get; set; }

        /// <summary>
        /// Current refresh rate in Hz
        /// </summary>
        [DataMember(Name = "refreshRateHz")]
        public int RefreshRateHz { get; set; }

        /// <summary>
        /// Whether this is the primary display
        /// </summary>
        [DataMember(Name = "isPrimary")]
        public bool IsPrimary { get; set; }

        /// <summary>
        /// Current power state of the monitor
        /// </summary>
        [DataMember(Name = "powerState")]
        public MonitorPowerState PowerState { get; set; }

        /// <summary>
        /// Connection type (HDMI, DisplayPort, VGA, DVI, etc.)
        /// </summary>
        [DataMember(Name = "connectionType")]
        public string ConnectionType { get; set; }

        /// <summary>
        /// EPREL product registration number (if mapped)
        /// </summary>
        [DataMember(Name = "eprelNumber")]
        public string EprelNumber { get; set; }

        /// <summary>
        /// EU Energy efficiency class (A-G)
        /// </summary>
        [DataMember(Name = "energyClass")]
        public string EnergyClass { get; set; }

        /// <summary>
        /// Power consumption when active (watts)
        /// </summary>
        [DataMember(Name = "powerConsumptionOnWatts")]
        public double PowerConsumptionOnWatts { get; set; }

        /// <summary>
        /// Power consumption in standby/sleep (watts)
        /// </summary>
        [DataMember(Name = "powerConsumptionStandbyWatts")]
        public double PowerConsumptionStandbyWatts { get; set; }

        /// <summary>
        /// Power consumption when off (watts)
        /// </summary>
        [DataMember(Name = "powerConsumptionOffWatts")]
        public double PowerConsumptionOffWatts { get; set; }

        /// <summary>
        /// Annual energy consumption in kWh (based on EPREL data)
        /// </summary>
        [DataMember(Name = "annualEnergyConsumptionKwh")]
        public double AnnualEnergyConsumptionKwh { get; set; }

        /// <summary>
        /// Year of manufacture
        /// </summary>
        [DataMember(Name = "yearOfManufacture")]
        public int? YearOfManufacture { get; set; }

        /// <summary>
        /// Monitor index (for multi-monitor setups)
        /// </summary>
        [DataMember(Name = "monitorIndex")]
        public int MonitorIndex { get; set; }

        /// <summary>
        /// Timestamp of last state change
        /// </summary>
        [DataMember(Name = "lastStateChangeUtc")]
        public DateTime LastStateChangeUtc { get; set; }

        public MonitorInfo()
        {
            PowerState = MonitorPowerState.Unknown;
            LastStateChangeUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Get current power consumption based on power state
        /// </summary>
        public double GetCurrentPowerConsumption()
        {
            switch (PowerState)
            {
                case MonitorPowerState.On:
                    return PowerConsumptionOnWatts > 0 ? PowerConsumptionOnWatts : EstimateDefaultPowerConsumption();
                case MonitorPowerState.Standby:
                case MonitorPowerState.Suspend:
                    return PowerConsumptionStandbyWatts > 0 ? PowerConsumptionStandbyWatts : 0.5;
                case MonitorPowerState.Off:
                    return PowerConsumptionOffWatts > 0 ? PowerConsumptionOffWatts : 0.3;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Estimate default power consumption based on screen size and resolution
        /// </summary>
        private double EstimateDefaultPowerConsumption()
        {
            // Rough estimation based on screen size and resolution
            // Typical values: 22" ~30W, 24" ~35W, 27" ~45W, 32" ~55W
            double basePower = 20 + (ScreenSizeInches * 1.2);
            
            // Adjust for high resolution
            long pixels = (long)HorizontalResolution * VerticalResolution;
            if (pixels > 3840 * 2160) // 4K+
                basePower *= 1.3;
            else if (pixels > 2560 * 1440) // QHD+
                basePower *= 1.15;

            return Math.Round(basePower, 1);
        }
    }

    /// <summary>
    /// Monitor power states
    /// </summary>
    public enum MonitorPowerState
    {
        Unknown = 0,
        On = 1,
        Standby = 2,
        Suspend = 3,
        Off = 4
    }
}
