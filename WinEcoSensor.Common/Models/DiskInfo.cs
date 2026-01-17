// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2026 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.Runtime.Serialization;

namespace WinEcoSensor.Common.Models
{
    [DataContract]
    public class DiskInfo
    {
        [DataMember(Name = "deviceId")]
        public string DeviceId { get; set; }

        [DataMember(Name = "model")]
        public string Model { get; set; }

        [DataMember(Name = "manufacturer")]
        public string Manufacturer { get; set; }

        [DataMember(Name = "serialNumber")]
        public string SerialNumber { get; set; }

        [DataMember(Name = "firmwareVersion")]
        public string FirmwareVersion { get; set; }

        [DataMember(Name = "capacityBytes")]
        public long CapacityBytes { get; set; }

        [DataMember(Name = "diskType")]
        public DiskType DiskType { get; set; }

        [DataMember(Name = "interfaceType")]
        public string InterfaceType { get; set; }

        [DataMember(Name = "mediaType")]
        public string MediaType { get; set; }

        [DataMember(Name = "driveLetters")]
        public string DriveLetters { get; set; }

        [DataMember(Name = "powerState")]
        public DiskPowerState PowerState { get; set; }

        [DataMember(Name = "spindleSpeedRpm")]
        public int SpindleSpeedRpm { get; set; }

        [DataMember(Name = "powerConsumptionActiveWatts")]
        public double PowerConsumptionActiveWatts { get; set; }

        [DataMember(Name = "powerConsumptionIdleWatts")]
        public double PowerConsumptionIdleWatts { get; set; }

        [DataMember(Name = "powerConsumptionStandbyWatts")]
        public double PowerConsumptionStandbyWatts { get; set; }

        [DataMember(Name = "isSystemDisk")]
        public bool IsSystemDisk { get; set; }

        [DataMember(Name = "diskIndex")]
        public int DiskIndex { get; set; }

        [DataMember(Name = "partitionCount")]
        public int PartitionCount { get; set; }

        [DataMember(Name = "healthStatus")]
        public string HealthStatus { get; set; }

        [DataMember(Name = "temperatureCelsius")]
        public int? TemperatureCelsius { get; set; }

        public DiskInfo()
        {
            PowerState = DiskPowerState.Unknown;
            PowerConsumptionActiveWatts = 3.0;
            PowerConsumptionIdleWatts = 1.0;
            PowerConsumptionStandbyWatts = 0.5;
        }

        // Backward-compatible alias properties
        public string Type
        {
            get { return DiskType.ToString(); }
        }

        public double SizeGB
        {
            get { return CapacityBytes / (1024.0 * 1024.0 * 1024.0); }
        }

        public double PowerActiveWatts
        {
            get { return PowerConsumptionActiveWatts; }
            set { PowerConsumptionActiveWatts = value; }
        }

        public double PowerIdleWatts
        {
            get { return PowerConsumptionIdleWatts; }
            set { PowerConsumptionIdleWatts = value; }
        }

        public double GetCurrentPowerConsumption()
        {
            switch (PowerState)
            {
                case DiskPowerState.Active:
                    return PowerConsumptionActiveWatts;
                case DiskPowerState.Idle:
                    return PowerConsumptionIdleWatts;
                case DiskPowerState.Standby:
                case DiskPowerState.Sleep:
                    return PowerConsumptionStandbyWatts;
                case DiskPowerState.Off:
                    return 0;
                default:
                    return PowerConsumptionIdleWatts;
            }
        }

        public string GetCapacityString()
        {
            const long GB = 1024L * 1024L * 1024L;
            const long TB = GB * 1024L;

            if (CapacityBytes >= TB)
                return string.Format("{0:F2} TB", CapacityBytes / (double)TB);
            else
                return string.Format("{0:F2} GB", CapacityBytes / (double)GB);
        }
    }

    public enum DiskType
    {
        Unknown = 0,
        Hdd = 1,
        Ssd = 2,
        Nvme = 3,
        Removable = 4
    }

    public enum DiskPowerState
    {
        Unknown = 0,
        Active = 1,
        Idle = 2,
        Standby = 3,
        Sleep = 4,
        Off = 5
    }
}
