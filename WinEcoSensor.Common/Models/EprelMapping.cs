// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2024 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WinEcoSensor.Common.Models
{
    [DataContract]
    public class EprelMapping
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "hardwareType")]
        public EprelHardwareType HardwareType { get; set; }

        [DataMember(Name = "detectedManufacturer")]
        public string DetectedManufacturer { get; set; }

        [DataMember(Name = "detectedModel")]
        public string DetectedModel { get; set; }

        [DataMember(Name = "eprelNumber")]
        public string EprelNumber { get; set; }

        [DataMember(Name = "eprelUrl")]
        public string EprelUrl { get; set; }

        [DataMember(Name = "energyClass")]
        public string EnergyClass { get; set; }

        [DataMember(Name = "powerOnWatts")]
        public double PowerOnWatts { get; set; }

        [DataMember(Name = "powerStandbyWatts")]
        public double PowerStandbyWatts { get; set; }

        [DataMember(Name = "powerOffWatts")]
        public double PowerOffWatts { get; set; }

        [DataMember(Name = "annualEnergyKwh")]
        public double AnnualEnergyKwh { get; set; }

        [DataMember(Name = "isManual")]
        public bool IsManual { get; set; }

        [DataMember(Name = "isVerified")]
        public bool IsVerified { get; set; }

        [DataMember(Name = "createdAtUtc")]
        public DateTime CreatedAtUtc { get; set; }

        [DataMember(Name = "updatedAtUtc")]
        public DateTime UpdatedAtUtc { get; set; }

        [DataMember(Name = "additionalData")]
        public Dictionary<string, string> AdditionalData { get; set; }

        public EprelMapping()
        {
            Id = Guid.NewGuid().ToString();
            CreatedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = DateTime.UtcNow;
            AdditionalData = new Dictionary<string, string>();
        }

        // Backward-compatible alias properties
        public string ModelName
        {
            get { return DetectedModel; }
            set { DetectedModel = value; }
        }

        public string Manufacturer
        {
            get { return DetectedManufacturer; }
            set { DetectedManufacturer = value; }
        }

        public void GenerateEprelUrl()
        {
            if (!string.IsNullOrEmpty(EprelNumber))
            {
                EprelUrl = "https://eprel.ec.europa.eu/screen/product/electronicdisplays/" + EprelNumber;
            }
        }
    }

    [DataContract]
    public class EprelMappingCollection
    {
        [DataMember(Name = "machineId")]
        public string MachineId { get; set; }

        [DataMember(Name = "mappings")]
        public List<EprelMapping> Mappings { get; set; }

        [DataMember(Name = "lastUpdatedUtc")]
        public DateTime LastUpdatedUtc { get; set; }

        public EprelMappingCollection()
        {
            Mappings = new List<EprelMapping>();
            LastUpdatedUtc = DateTime.UtcNow;
        }

        public EprelMapping FindMapping(string manufacturer, string model)
        {
            return Mappings.Find(m =>
                (string.IsNullOrEmpty(m.DetectedManufacturer) ||
                 m.DetectedManufacturer.Equals(manufacturer, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(m.DetectedModel) ||
                 m.DetectedModel.Equals(model, StringComparison.OrdinalIgnoreCase)));
        }

        public void AddOrUpdateMapping(EprelMapping mapping)
        {
            var existing = FindMapping(mapping.DetectedManufacturer, mapping.DetectedModel);
            if (existing != null) Mappings.Remove(existing);
            mapping.UpdatedAtUtc = DateTime.UtcNow;
            Mappings.Add(mapping);
            LastUpdatedUtc = DateTime.UtcNow;
        }
    }

    public enum EprelHardwareType
    {
        Unknown = 0, Monitor = 1, Television = 2, Computer = 3, Laptop = 4,
        Server = 5, Printer = 6, NetworkDevice = 7, StorageDevice = 8, PowerSupply = 9, Other = 99
    }

    public enum HardwareCategory
    {
        Unknown = 0, Monitor = 1, Television = 2, Computer = 3, Laptop = 4,
        Server = 5, Printer = 6, NetworkDevice = 7, StorageDevice = 8, PowerSupply = 9, Other = 99
    }

    public static class DefaultPowerValues
    {
        public static readonly Dictionary<string, double> MonitorDefaults = new Dictionary<string, double>
        {
            { "small", 25 }, { "medium", 35 }, { "large", 50 }, { "xlarge", 70 }
        };

        public static readonly Dictionary<string, double> CpuDefaults = new Dictionary<string, double>
        {
            { "mobile_ultralow", 15 }, { "mobile_standard", 35 }, { "desktop_entry", 65 },
            { "desktop_standard", 95 }, { "desktop_high", 125 }, { "workstation", 150 }
        };

        public static readonly Dictionary<string, double> GpuDefaults = new Dictionary<string, double>
        {
            { "integrated", 15 }, { "entry", 75 }, { "midrange", 150 }, { "highend", 250 }, { "enthusiast", 350 }
        };

        public static readonly double SystemBasePower = 30;
        public static readonly double MemoryPowerPerModule = 3;

        public static double GetMonitorPower(double screenSizeInches)
        {
            if (screenSizeInches < 22) return MonitorDefaults["small"];
            if (screenSizeInches < 27) return MonitorDefaults["medium"];
            if (screenSizeInches < 32) return MonitorDefaults["large"];
            return MonitorDefaults["xlarge"];
        }

        public static double EstimateCpuTdp(string cpuName)
        {
            cpuName = cpuName != null ? cpuName.ToLowerInvariant() : "";
            if (cpuName.Contains("celeron") || cpuName.Contains("pentium")) return CpuDefaults["desktop_entry"];
            if (cpuName.Contains("i3")) return CpuDefaults["desktop_entry"];
            if (cpuName.Contains("i5")) return CpuDefaults["desktop_standard"];
            if (cpuName.Contains("i7")) return CpuDefaults["desktop_standard"];
            if (cpuName.Contains("i9")) return CpuDefaults["desktop_high"];
            if (cpuName.Contains("xeon")) return CpuDefaults["workstation"];
            if (cpuName.Contains("ryzen 3")) return CpuDefaults["desktop_entry"];
            if (cpuName.Contains("ryzen 5")) return CpuDefaults["desktop_standard"];
            if (cpuName.Contains("ryzen 7")) return CpuDefaults["desktop_standard"];
            if (cpuName.Contains("ryzen 9")) return CpuDefaults["desktop_high"];
            if (cpuName.Contains("threadripper")) return CpuDefaults["workstation"];
            if (cpuName.Contains("u") || cpuName.Contains("mobile") || cpuName.Contains("laptop"))
                return CpuDefaults["mobile_standard"];
            return CpuDefaults["desktop_standard"];
        }

        public static double EstimateGpuTdp(string gpuName)
        {
            gpuName = gpuName != null ? gpuName.ToLowerInvariant() : "";
            if (gpuName.Contains("intel") && (gpuName.Contains("graphics") || gpuName.Contains("uhd") || gpuName.Contains("iris")))
                return GpuDefaults["integrated"];
            if (gpuName.Contains("amd") && gpuName.Contains("vega") && gpuName.Contains("graphics"))
                return GpuDefaults["integrated"];
            if (gpuName.Contains("rtx 40")) return GpuDefaults["enthusiast"];
            if (gpuName.Contains("rtx 30")) return GpuDefaults["highend"];
            if (gpuName.Contains("rtx 20")) return GpuDefaults["midrange"];
            if (gpuName.Contains("gtx 16")) return GpuDefaults["entry"];
            if (gpuName.Contains("rx 7")) return GpuDefaults["highend"];
            if (gpuName.Contains("rx 6")) return GpuDefaults["midrange"];
            if (gpuName.Contains("rx 5")) return GpuDefaults["entry"];
            return GpuDefaults["midrange"];
        }
    }
}
