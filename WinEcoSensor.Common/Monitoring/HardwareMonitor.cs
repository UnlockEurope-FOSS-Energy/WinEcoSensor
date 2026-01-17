// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2026 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using WinEcoSensor.Common.Models;
using WinEcoSensor.Common.Utilities;

namespace WinEcoSensor.Common.Monitoring
{
    /// <summary>
    /// Monitors and collects hardware information from the system.
    /// Uses WMI to gather details about CPU, memory, displays, storage, and graphics.
    /// </summary>
    public class HardwareMonitor
    {
        private HardwareInfo _cachedHardwareInfo;
        private DateTime _lastCollectionTime;
        private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(60);

        /// <summary>
        /// Collect comprehensive hardware information
        /// </summary>
        /// <param name="forceRefresh">Force refresh even if cache is valid</param>
        public HardwareInfo CollectHardwareInfo(bool forceRefresh = false)
        {
            // Return cached info if still valid
            if (!forceRefresh && _cachedHardwareInfo != null && 
                DateTime.UtcNow - _lastCollectionTime < _cacheExpiry)
            {
                return _cachedHardwareInfo;
            }

            Logger.Info("Collecting hardware information...");

            var hardwareInfo = new HardwareInfo
            {
                Hostname = Environment.MachineName,
                CollectedAt = DateTime.UtcNow
            };

            try
            {
                // Collect computer system info
                CollectComputerSystemInfo(hardwareInfo);

                // Collect mainboard info
                CollectMainboardInfo(hardwareInfo);

                // Collect processor info
                CollectProcessorInfo(hardwareInfo);

                // Collect memory info
                CollectMemoryInfo(hardwareInfo);

                // Collect monitor info
                CollectMonitorInfo(hardwareInfo);

                // Collect disk info
                CollectDiskInfo(hardwareInfo);

                // Collect graphics card info
                CollectGraphicsInfo(hardwareInfo);

                // Collect OS info
                CollectOsInfo(hardwareInfo);

                // Generate hardware ID
                hardwareInfo.HardwareId = GenerateHardwareId(hardwareInfo);

                // Calculate estimated TDP
                hardwareInfo.EstimatedTdpWatts = CalculateTotalTdp(hardwareInfo);

                _cachedHardwareInfo = hardwareInfo;
                _lastCollectionTime = DateTime.UtcNow;

                Logger.Info($"Hardware collection complete. Estimated TDP: {hardwareInfo.EstimatedTdpWatts}W");
            }
            catch (Exception ex)
            {
                Logger.Error("Error collecting hardware information", ex);
            }

            return hardwareInfo;
        }

        /// <summary>
        /// Collect computer system information
        /// </summary>
        private void CollectComputerSystemInfo(HardwareInfo info)
        {
            var data = WmiHelper.GetComputerSystemInfo();
            
            info.Manufacturer = data.GetValueOrDefault("Manufacturer")?.ToString();
            info.Model = data.GetValueOrDefault("Model")?.ToString();

            if (data.ContainsKey("TotalPhysicalMemory") && data["TotalPhysicalMemory"] != null)
            {
                info.TotalMemoryBytes = Convert.ToInt64(data["TotalPhysicalMemory"]);
            }
        }

        /// <summary>
        /// Collect mainboard/motherboard information
        /// </summary>
        private void CollectMainboardInfo(HardwareInfo info)
        {
            var data = WmiHelper.GetMainboardInfo();
            
            info.Mainboard = new MainboardInfo
            {
                Manufacturer = data.GetValueOrDefault("Manufacturer")?.ToString(),
                Product = data.GetValueOrDefault("Product")?.ToString(),
                SerialNumber = data.GetValueOrDefault("SerialNumber")?.ToString(),
                Version = data.GetValueOrDefault("Version")?.ToString()
            };
        }

        /// <summary>
        /// Collect processor/CPU information
        /// </summary>
        private void CollectProcessorInfo(HardwareInfo info)
        {
            var processors = WmiHelper.GetProcessorInfo();
            
            if (processors.Count > 0)
            {
                var cpu = processors[0];
                
                info.Processor = new ProcessorInfo
                {
                    Name = cpu.GetValueOrDefault("Name")?.ToString()?.Trim(),
                    Manufacturer = cpu.GetValueOrDefault("Manufacturer")?.ToString(),
                    ProcessorId = cpu.GetValueOrDefault("ProcessorId")?.ToString(),
                    NumberOfCores = Convert.ToInt32(cpu.GetValueOrDefault("NumberOfCores") ?? 0),
                    NumberOfLogicalProcessors = Convert.ToInt32(cpu.GetValueOrDefault("NumberOfLogicalProcessors") ?? 0),
                    MaxClockSpeedMhz = Convert.ToInt32(cpu.GetValueOrDefault("MaxClockSpeed") ?? 0)
                };

                // Estimate TDP based on CPU name
                info.Processor.TdpWatts = DefaultPowerValues.EstimateCpuTdp(info.Processor.Name);
            }
        }

        /// <summary>
        /// Collect memory information
        /// </summary>
        private void CollectMemoryInfo(HardwareInfo info)
        {
            var memory = WmiHelper.GetMemoryInfo();
            // Memory info is already collected as TotalMemoryBytes
            // This could be expanded to track individual DIMM modules
        }

        /// <summary>
        /// Collect display/monitor information
        /// </summary>
        private void CollectMonitorInfo(HardwareInfo info)
        {
            var monitors = WmiHelper.GetMonitorInfo();
            int index = 0;

            foreach (var mon in monitors)
            {
                var monitorInfo = new MonitorInfo
                {
                    DeviceId = mon.GetValueOrDefault("DeviceID")?.ToString(),
                    DisplayName = mon.GetValueOrDefault("Name")?.ToString(),
                    Manufacturer = mon.GetValueOrDefault("MonitorManufacturer")?.ToString(),
                    Model = mon.GetValueOrDefault("MonitorType")?.ToString(),
                    VerticalResolution = Convert.ToInt32(mon.GetValueOrDefault("ScreenHeight") ?? 0),
                    HorizontalResolution = Convert.ToInt32(mon.GetValueOrDefault("ScreenWidth") ?? 0),
                    MonitorIndex = index++,
                    IsPrimary = index == 1,
                    PowerState = MonitorPowerState.On
                };

                // Estimate screen size based on resolution (rough approximation)
                monitorInfo.ScreenSizeInches = EstimateScreenSize(
                    monitorInfo.HorizontalResolution, 
                    monitorInfo.VerticalResolution);

                // Set default power consumption based on size
                monitorInfo.PowerConsumptionOnWatts = DefaultPowerValues.GetMonitorPower(monitorInfo.ScreenSizeInches);
                monitorInfo.PowerConsumptionStandbyWatts = 0.5;
                monitorInfo.PowerConsumptionOffWatts = 0.3;

                info.Monitors.Add(monitorInfo);
            }

            // Also try to get info from video controllers for resolution
            var videoControllers = WmiHelper.GetVideoControllerInfo();
            for (int i = 0; i < Math.Min(videoControllers.Count, info.Monitors.Count); i++)
            {
                var vc = videoControllers[i];
                var monitor = info.Monitors[i];

                if (monitor.HorizontalResolution == 0)
                {
                    monitor.HorizontalResolution = Convert.ToInt32(vc.GetValueOrDefault("CurrentHorizontalResolution") ?? 0);
                }
                if (monitor.VerticalResolution == 0)
                {
                    monitor.VerticalResolution = Convert.ToInt32(vc.GetValueOrDefault("CurrentVerticalResolution") ?? 0);
                }
                monitor.RefreshRateHz = Convert.ToInt32(vc.GetValueOrDefault("CurrentRefreshRate") ?? 60);
            }

            Logger.Debug($"Found {info.Monitors.Count} monitor(s)");
        }

        /// <summary>
        /// Collect storage device information
        /// </summary>
        private void CollectDiskInfo(HardwareInfo info)
        {
            var disks = WmiHelper.GetDiskInfo();

            foreach (var disk in disks)
            {
                var diskInfo = new DiskInfo
                {
                    DeviceId = disk.GetValueOrDefault("DeviceID")?.ToString(),
                    Model = disk.GetValueOrDefault("Model")?.ToString()?.Trim(),
                    Manufacturer = disk.GetValueOrDefault("Manufacturer")?.ToString(),
                    SerialNumber = disk.GetValueOrDefault("SerialNumber")?.ToString()?.Trim(),
                    FirmwareVersion = disk.GetValueOrDefault("FirmwareRevision")?.ToString(),
                    CapacityBytes = Convert.ToInt64(disk.GetValueOrDefault("Size") ?? 0),
                    InterfaceType = disk.GetValueOrDefault("InterfaceType")?.ToString(),
                    MediaType = disk.GetValueOrDefault("MediaType")?.ToString(),
                    DiskIndex = Convert.ToInt32(disk.GetValueOrDefault("Index") ?? 0),
                    PartitionCount = Convert.ToInt32(disk.GetValueOrDefault("Partitions") ?? 0),
                    HealthStatus = disk.GetValueOrDefault("Status")?.ToString()
                };

                // Determine disk type
                diskInfo.DiskType = DetermineDiskType(diskInfo);
                
                // Set power consumption based on type
                SetDiskPowerConsumption(diskInfo);

                info.Disks.Add(diskInfo);
            }

            Logger.Debug($"Found {info.Disks.Count} disk(s)");
        }

        /// <summary>
        /// Collect graphics card information
        /// </summary>
        private void CollectGraphicsInfo(HardwareInfo info)
        {
            var gpus = WmiHelper.GetVideoControllerInfo();

            foreach (var gpu in gpus)
            {
                var gpuInfo = new GraphicsCardInfo
                {
                    Name = gpu.GetValueOrDefault("Name")?.ToString(),
                    Manufacturer = gpu.GetValueOrDefault("AdapterCompatibility")?.ToString(),
                    AdapterRamBytes = Convert.ToInt64(gpu.GetValueOrDefault("AdapterRAM") ?? 0),
                    DriverVersion = gpu.GetValueOrDefault("DriverVersion")?.ToString()
                };

                // Estimate TDP
                gpuInfo.EstimatedTdpWatts = DefaultPowerValues.EstimateGpuTdp(gpuInfo.Name);

                info.GraphicsCards.Add(gpuInfo);
            }

            Logger.Debug($"Found {info.GraphicsCards.Count} graphics card(s)");
        }

        /// <summary>
        /// Collect operating system information
        /// </summary>
        private void CollectOsInfo(HardwareInfo info)
        {
            var osData = WmiHelper.GetOperatingSystemInfo();
            
            info.OperatingSystem = osData.GetValueOrDefault("Caption")?.ToString();
            info.OsVersion = osData.GetValueOrDefault("Version")?.ToString();
        }

        /// <summary>
        /// Estimate screen size based on resolution
        /// </summary>
        private double EstimateScreenSize(int width, int height)
        {
            // Common resolutions and typical screen sizes
            long pixels = (long)width * height;

            if (pixels >= 3840 * 2160) return 27.0; // 4K typically 27"+
            if (pixels >= 2560 * 1440) return 27.0; // QHD typically 27"
            if (pixels >= 1920 * 1200) return 24.0; // WUXGA typically 24"
            if (pixels >= 1920 * 1080) return 24.0; // FHD typically 24"
            if (pixels >= 1680 * 1050) return 22.0; // WSXGA+ typically 22"
            if (pixels >= 1280 * 1024) return 19.0; // SXGA typically 19"
            
            return 22.0; // Default estimate
        }

        /// <summary>
        /// Determine disk type from model name and interface
        /// </summary>
        private DiskType DetermineDiskType(DiskInfo disk)
        {
            string model = disk.Model?.ToLowerInvariant() ?? "";
            string interfaceType = disk.InterfaceType?.ToLowerInvariant() ?? "";
            string mediaType = disk.MediaType?.ToLowerInvariant() ?? "";

            // Check interface type
            if (interfaceType.Contains("nvme") || model.Contains("nvme"))
                return DiskType.Nvme;

            // Check for SSD indicators
            if (model.Contains("ssd") || model.Contains("solid state") || 
                mediaType.Contains("ssd") || mediaType.Contains("solid"))
                return DiskType.Ssd;

            // Check for known SSD brands/models
            if (model.Contains("samsung 9") || model.Contains("crucial") || 
                model.Contains("sandisk") || model.Contains("wd blue sn") ||
                model.Contains("intel ssd"))
                return DiskType.Ssd;

            // USB drives
            if (interfaceType.Contains("usb"))
                return DiskType.Removable;

            // Default to HDD if fixed disk
            if (mediaType.Contains("fixed") || mediaType.Contains("hard"))
                return DiskType.Hdd;

            return DiskType.Unknown;
        }

        /// <summary>
        /// Set power consumption values based on disk type
        /// </summary>
        private void SetDiskPowerConsumption(DiskInfo disk)
        {
            switch (disk.DiskType)
            {
                case DiskType.Hdd:
                    disk.PowerConsumptionActiveWatts = 6.0;
                    disk.PowerConsumptionIdleWatts = 4.5;
                    disk.PowerConsumptionStandbyWatts = 0.8;
                    break;
                case DiskType.Ssd:
                    disk.PowerConsumptionActiveWatts = 2.5;
                    disk.PowerConsumptionIdleWatts = 0.05;
                    disk.PowerConsumptionStandbyWatts = 0.02;
                    break;
                case DiskType.Nvme:
                    disk.PowerConsumptionActiveWatts = 5.0;
                    disk.PowerConsumptionIdleWatts = 0.03;
                    disk.PowerConsumptionStandbyWatts = 0.01;
                    break;
                default:
                    disk.PowerConsumptionActiveWatts = 3.0;
                    disk.PowerConsumptionIdleWatts = 1.0;
                    disk.PowerConsumptionStandbyWatts = 0.5;
                    break;
            }
        }

        /// <summary>
        /// Generate unique hardware ID based on system components
        /// </summary>
        public string GenerateHardwareId(HardwareInfo info)
        {
            string combined = $"{info.Mainboard?.Manufacturer}|{info.Mainboard?.Product}|" +
                            $"{info.Processor?.ProcessorId}|{Environment.MachineName}";

            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(combined));
                return BitConverter.ToString(hash).Replace("-", "").Substring(0, 16).ToLowerInvariant();
            }
        }

        /// <summary>
        /// Calculate total estimated TDP
        /// </summary>
        private double CalculateTotalTdp(HardwareInfo info)
        {
            double total = 0;

            // CPU TDP
            if (info.Processor != null)
                total += info.Processor.TdpWatts;

            // GPU TDP
            if (info.GraphicsCards != null)
                total += info.GraphicsCards.Sum(g => g.EstimatedTdpWatts);

            // Memory (estimated 3W per 8GB module)
            if (info.TotalMemoryBytes > 0)
            {
                int memoryModules = (int)Math.Ceiling(info.TotalMemoryBytes / (8.0 * 1024 * 1024 * 1024));
                total += memoryModules * DefaultPowerValues.MemoryPowerPerModule;
            }

            // Disks
            if (info.Disks != null)
                total += info.Disks.Sum(d => d.PowerConsumptionActiveWatts);

            // Monitors
            if (info.Monitors != null)
                total += info.Monitors.Sum(m => m.PowerConsumptionOnWatts);

            // System base power
            total += DefaultPowerValues.SystemBasePower;

            return Math.Round(total, 1);
        }
    }

    /// <summary>
    /// Extension method for dictionary
    /// </summary>
    internal static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default)
        {
            return dict.TryGetValue(key, out TValue value) ? value : defaultValue;
        }
    }
}
