// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2024 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.Collections.Generic;
using System.Management;

namespace WinEcoSensor.Common.Utilities
{
    /// <summary>
    /// Helper class for Windows Management Instrumentation (WMI) queries.
    /// Provides methods for querying hardware and system information.
    /// </summary>
    public static class WmiHelper
    {
        /// <summary>
        /// Execute a WMI query and return results as list of dictionaries
        /// </summary>
        /// <param name="query">WMI query string</param>
        /// <param name="properties">Properties to retrieve</param>
        /// <returns>List of dictionaries containing property values</returns>
        public static List<Dictionary<string, object>> ExecuteQuery(string query, params string[] properties)
        {
            var results = new List<Dictionary<string, object>>();

            try
            {
                using (var searcher = new ManagementObjectSearcher(query))
                using (var collection = searcher.Get())
                {
                    foreach (ManagementObject obj in collection)
                    {
                        var item = new Dictionary<string, object>();
                        
                        foreach (var prop in properties)
                        {
                            try
                            {
                                item[prop] = obj[prop];
                            }
                            catch
                            {
                                item[prop] = null;
                            }
                        }
                        
                        results.Add(item);
                        obj.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"WMI query failed: {query}", ex);
            }

            return results;
        }

        /// <summary>
        /// Execute a WMI query in a specific namespace
        /// </summary>
        public static List<Dictionary<string, object>> ExecuteQueryInNamespace(string namespacePath, string query, params string[] properties)
        {
            var results = new List<Dictionary<string, object>>();

            try
            {
                var scope = new ManagementScope(namespacePath);
                scope.Connect();

                using (var searcher = new ManagementObjectSearcher(scope, new ObjectQuery(query)))
                using (var collection = searcher.Get())
                {
                    foreach (ManagementObject obj in collection)
                    {
                        var item = new Dictionary<string, object>();
                        
                        foreach (var prop in properties)
                        {
                            try
                            {
                                item[prop] = obj[prop];
                            }
                            catch
                            {
                                item[prop] = null;
                            }
                        }
                        
                        results.Add(item);
                        obj.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"WMI query failed in namespace {namespacePath}: {query}", ex);
            }

            return results;
        }

        /// <summary>
        /// Get single value from WMI query
        /// </summary>
        public static T GetValue<T>(string query, string property, T defaultValue = default)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(query))
                using (var collection = searcher.Get())
                {
                    foreach (ManagementObject obj in collection)
                    {
                        var value = obj[property];
                        obj.Dispose();
                        
                        if (value != null)
                        {
                            return (T)Convert.ChangeType(value, typeof(T));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug($"WMI GetValue failed: {query}.{property}", ex);
            }

            return defaultValue;
        }

        /// <summary>
        /// Get computer system information
        /// </summary>
        public static Dictionary<string, object> GetComputerSystemInfo()
        {
            var results = ExecuteQuery(
                "SELECT * FROM Win32_ComputerSystem",
                "Manufacturer", "Model", "Name", "Domain", "TotalPhysicalMemory", 
                "NumberOfProcessors", "NumberOfLogicalProcessors", "SystemType");

            return results.Count > 0 ? results[0] : new Dictionary<string, object>();
        }

        /// <summary>
        /// Get operating system information
        /// </summary>
        public static Dictionary<string, object> GetOperatingSystemInfo()
        {
            var results = ExecuteQuery(
                "SELECT * FROM Win32_OperatingSystem",
                "Caption", "Version", "BuildNumber", "OSArchitecture", 
                "LastBootUpTime", "LocalDateTime", "FreePhysicalMemory");

            return results.Count > 0 ? results[0] : new Dictionary<string, object>();
        }

        /// <summary>
        /// Get processor information
        /// </summary>
        public static List<Dictionary<string, object>> GetProcessorInfo()
        {
            return ExecuteQuery(
                "SELECT * FROM Win32_Processor",
                "Name", "Manufacturer", "ProcessorId", "NumberOfCores", 
                "NumberOfLogicalProcessors", "MaxClockSpeed", "CurrentClockSpeed",
                "LoadPercentage", "DeviceID");
        }

        /// <summary>
        /// Get mainboard information
        /// </summary>
        public static Dictionary<string, object> GetMainboardInfo()
        {
            var results = ExecuteQuery(
                "SELECT * FROM Win32_BaseBoard",
                "Manufacturer", "Product", "SerialNumber", "Version");

            return results.Count > 0 ? results[0] : new Dictionary<string, object>();
        }

        /// <summary>
        /// Get monitor information from WMI
        /// </summary>
        public static List<Dictionary<string, object>> GetMonitorInfo()
        {
            return ExecuteQuery(
                "SELECT * FROM Win32_DesktopMonitor",
                "DeviceID", "Name", "MonitorManufacturer", "MonitorType",
                "ScreenHeight", "ScreenWidth", "PixelsPerXLogicalInch", "PixelsPerYLogicalInch");
        }

        /// <summary>
        /// Get physical disk information
        /// </summary>
        public static List<Dictionary<string, object>> GetDiskInfo()
        {
            return ExecuteQuery(
                "SELECT * FROM Win32_DiskDrive",
                "DeviceID", "Model", "Manufacturer", "SerialNumber", "FirmwareRevision",
                "Size", "MediaType", "InterfaceType", "Index", "Partitions", "Status");
        }

        /// <summary>
        /// Get video controller (GPU) information
        /// </summary>
        public static List<Dictionary<string, object>> GetVideoControllerInfo()
        {
            return ExecuteQuery(
                "SELECT * FROM Win32_VideoController",
                "Name", "AdapterCompatibility", "AdapterRAM", "DriverVersion",
                "VideoModeDescription", "CurrentHorizontalResolution", 
                "CurrentVerticalResolution", "CurrentRefreshRate", "Status");
        }

        /// <summary>
        /// Get memory module information
        /// </summary>
        public static List<Dictionary<string, object>> GetMemoryInfo()
        {
            return ExecuteQuery(
                "SELECT * FROM Win32_PhysicalMemory",
                "Manufacturer", "PartNumber", "Capacity", "Speed", "DeviceLocator");
        }

        /// <summary>
        /// Parse WMI datetime string to DateTime
        /// </summary>
        public static DateTime ParseWmiDateTime(string wmiDateTime)
        {
            try
            {
                if (string.IsNullOrEmpty(wmiDateTime))
                    return DateTime.MinValue;

                // WMI datetime format: yyyyMMddHHmmss.ffffff+zzz
                return ManagementDateTimeConverter.ToDateTime(wmiDateTime);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Convert bytes to string representation (KB, MB, GB, TB)
        /// </summary>
        public static string BytesToString(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:F2} {sizes[order]}";
        }
    }
}
