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
    public class HardwareInfo
    {
        [DataMember(Name = "hardwareId")]
        public string HardwareId { get; set; }

        [DataMember(Name = "manufacturer")]
        public string Manufacturer { get; set; }

        [DataMember(Name = "model")]
        public string Model { get; set; }

        [DataMember(Name = "mainboard")]
        public MainboardInfo Mainboard { get; set; }

        [DataMember(Name = "processor")]
        public ProcessorInfo Processor { get; set; }

        [DataMember(Name = "totalMemoryBytes")]
        public long TotalMemoryBytes { get; set; }

        [DataMember(Name = "monitors")]
        public List<MonitorInfo> Monitors { get; set; }

        [DataMember(Name = "disks")]
        public List<DiskInfo> Disks { get; set; }

        [DataMember(Name = "graphicsCards")]
        public List<GraphicsCardInfo> GraphicsCards { get; set; }

        [DataMember(Name = "operatingSystem")]
        public string OperatingSystem { get; set; }

        [DataMember(Name = "osVersion")]
        public string OsVersion { get; set; }

        [DataMember(Name = "hostname")]
        public string Hostname { get; set; }

        [DataMember(Name = "collectedAt")]
        public DateTime CollectedAt { get; set; }

        [DataMember(Name = "estimatedTdpWatts")]
        public double EstimatedTdpWatts { get; set; }

        public HardwareInfo()
        {
            Monitors = new List<MonitorInfo>();
            Disks = new List<DiskInfo>();
            GraphicsCards = new List<GraphicsCardInfo>();
            CollectedAt = DateTime.UtcNow;
            Mainboard = new MainboardInfo();
            Processor = new ProcessorInfo();
        }

        // Backward-compatible alias properties
        public string MachineName
        {
            get { return Hostname ?? Environment.MachineName; }
            set { Hostname = value; }
        }

        public string MainboardManufacturer
        {
            get { return Mainboard != null ? Mainboard.Manufacturer : null; }
            set { if (Mainboard != null) Mainboard.Manufacturer = value; }
        }

        public string MainboardProduct
        {
            get { return Mainboard != null ? Mainboard.Product : null; }
            set { if (Mainboard != null) Mainboard.Product = value; }
        }

        public string CpuName
        {
            get { return Processor != null ? Processor.Name : null; }
            set { if (Processor != null) Processor.Name = value; }
        }

        public int CpuCores
        {
            get { return Processor != null ? Processor.NumberOfCores : 0; }
            set { if (Processor != null) Processor.NumberOfCores = value; }
        }

        public int CpuLogicalProcessors
        {
            get { return Processor != null ? Processor.NumberOfLogicalProcessors : 0; }
            set { if (Processor != null) Processor.NumberOfLogicalProcessors = value; }
        }

        public int CpuMaxClockSpeedMhz
        {
            get { return Processor != null ? Processor.MaxClockSpeedMhz : 0; }
            set { if (Processor != null) Processor.MaxClockSpeedMhz = value; }
        }

        public double CpuTdpWatts
        {
            get { return Processor != null ? Processor.TdpWatts : 0; }
            set { if (Processor != null) Processor.TdpWatts = value; }
        }

        public long TotalMemoryMB
        {
            get { return TotalMemoryBytes / (1024 * 1024); }
            set { TotalMemoryBytes = value * 1024 * 1024; }
        }

        public int MonitorCount
        {
            get { return Monitors != null ? Monitors.Count : 0; }
        }

        public int DiskCount
        {
            get { return Disks != null ? Disks.Count : 0; }
        }

        public int GpuCount
        {
            get { return GraphicsCards != null ? GraphicsCards.Count : 0; }
        }

        public List<string> Gpus
        {
            get
            {
                var list = new List<string>();
                if (GraphicsCards != null)
                {
                    foreach (var gpu in GraphicsCards)
                    {
                        if (gpu.Name != null)
                            list.Add(gpu.Name);
                    }
                }
                return list;
            }
        }
    }

    [DataContract]
    public class MainboardInfo
    {
        [DataMember(Name = "manufacturer")]
        public string Manufacturer { get; set; }

        [DataMember(Name = "product")]
        public string Product { get; set; }

        [DataMember(Name = "serialNumber")]
        public string SerialNumber { get; set; }

        [DataMember(Name = "version")]
        public string Version { get; set; }
    }

    [DataContract]
    public class ProcessorInfo
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "manufacturer")]
        public string Manufacturer { get; set; }

        [DataMember(Name = "numberOfCores")]
        public int NumberOfCores { get; set; }

        [DataMember(Name = "numberOfLogicalProcessors")]
        public int NumberOfLogicalProcessors { get; set; }

        [DataMember(Name = "maxClockSpeedMhz")]
        public int MaxClockSpeedMhz { get; set; }

        [DataMember(Name = "tdpWatts")]
        public double TdpWatts { get; set; }

        [DataMember(Name = "processorId")]
        public string ProcessorId { get; set; }
    }

    [DataContract]
    public class GraphicsCardInfo
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "manufacturer")]
        public string Manufacturer { get; set; }

        [DataMember(Name = "adapterRamBytes")]
        public long AdapterRamBytes { get; set; }

        [DataMember(Name = "driverVersion")]
        public string DriverVersion { get; set; }

        [DataMember(Name = "estimatedTdpWatts")]
        public double EstimatedTdpWatts { get; set; }
    }
}
