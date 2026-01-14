// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2024 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.Collections.Generic;
using System.Text;
using WinEcoSensor.Common.Models;
using WinEcoSensor.Common.Monitoring;
using WinEcoSensor.Common.Utilities;

namespace WinEcoSensor.Common.Communication
{
    /// <summary>
    /// Builds CloudEvent messages conforming to CloudEvents 1.0 specification.
    /// Creates structured event messages for backend communication.
    /// </summary>
    public class CloudEventBuilder
    {
        // CloudEvents 1.0 specification version
        private const string SpecVersion = "1.0";

        // Event type namespace
        private const string EventTypeBase = "eu.unlock.winecosensor";

        // Default source pattern
        private const string SourcePattern = "/winecosensor/{0}";

        private readonly string _hardwareId;
        private readonly string _source;

        /// <summary>
        /// Create CloudEvent builder with default hardware ID
        /// </summary>
        public CloudEventBuilder() : this(Environment.MachineName)
        {
        }

        /// <summary>
        /// Create CloudEvent builder with hardware ID
        /// </summary>
        /// <param name="hardwareId">Unique hardware identifier</param>
        public CloudEventBuilder(string hardwareId)
        {
            _hardwareId = hardwareId ?? "unknown";
            _source = string.Format(SourcePattern, _hardwareId);
        }

        /// <summary>
        /// Create a status event (simplified overload for testing)
        /// </summary>
        public CloudEventMessage CreateStatusEvent(HardwareInfo hardwareInfo, UserActivityInfo userActivity, EnergyState energyState)
        {
            return BuildStatusEvent(hardwareInfo, userActivity, energyState, null);
        }

        /// <summary>
        /// Create a heartbeat event (simplified overload for testing)
        /// </summary>
        public CloudEventMessage CreateHeartbeatEvent()
        {
            return BuildHeartbeatEvent(null);
        }

        /// <summary>
        /// Build a status update event containing all sensor data
        /// </summary>
        /// <param name="hardwareInfo">Hardware information</param>
        /// <param name="userActivity">User activity information</param>
        /// <param name="energyState">Current energy state</param>
        /// <param name="displayTimeStats">Display time statistics</param>
        /// <returns>CloudEvent message ready for transmission</returns>
        public CloudEventMessage BuildStatusEvent(
            HardwareInfo hardwareInfo,
            UserActivityInfo userActivity,
            EnergyState energyState,
            DisplayTimeStats displayTimeStats)
        {
            var eventId = GenerateEventId();
            var timestamp = DateTime.UtcNow;

            var message = new CloudEventMessage
            {
                SpecVersion = SpecVersion,
                Type = $"{EventTypeBase}.status",
                Source = _source,
                Id = eventId,
                Time = timestamp.ToString("o"),
                DataContentType = "application/json",
                Subject = _hardwareId,
                Data = new CloudEventData
                {
                    HardwareId = _hardwareId,
                    Timestamp = timestamp,
                    Hardware = CreateHardwareData(hardwareInfo),
                    UserActivity = CreateUserActivityData(userActivity),
                    Energy = CreateEnergyData(energyState),
                    DisplayTime = CreateDisplayTimeData(displayTimeStats)
                }
            };

            return message;
        }

        /// <summary>
        /// Build a heartbeat event (lightweight status ping)
        /// </summary>
        /// <param name="energyState">Current energy state</param>
        /// <returns>CloudEvent message</returns>
        public CloudEventMessage BuildHeartbeatEvent(EnergyState energyState)
        {
            var eventId = GenerateEventId();
            var timestamp = DateTime.UtcNow;

            var message = new CloudEventMessage
            {
                SpecVersion = SpecVersion,
                Type = $"{EventTypeBase}.heartbeat",
                Source = _source,
                Id = eventId,
                Time = timestamp.ToString("o"),
                DataContentType = "application/json",
                Subject = _hardwareId,
                Data = new CloudEventData
                {
                    HardwareId = _hardwareId,
                    Timestamp = timestamp,
                    Energy = CreateEnergyData(energyState)
                }
            };

            return message;
        }

        /// <summary>
        /// Build a hardware registration event (sent on startup)
        /// </summary>
        /// <param name="hardwareInfo">Hardware information</param>
        /// <returns>CloudEvent message</returns>
        public CloudEventMessage BuildRegistrationEvent(HardwareInfo hardwareInfo)
        {
            var eventId = GenerateEventId();
            var timestamp = DateTime.UtcNow;

            var message = new CloudEventMessage
            {
                SpecVersion = SpecVersion,
                Type = $"{EventTypeBase}.registration",
                Source = _source,
                Id = eventId,
                Time = timestamp.ToString("o"),
                DataContentType = "application/json",
                Subject = _hardwareId,
                Data = new CloudEventData
                {
                    HardwareId = _hardwareId,
                    Timestamp = timestamp,
                    Hardware = CreateHardwareData(hardwareInfo),
                    ServiceVersion = GetServiceVersion()
                }
            };

            return message;
        }

        /// <summary>
        /// Build a session start event
        /// </summary>
        /// <param name="userActivity">User activity information</param>
        /// <returns>CloudEvent message</returns>
        public CloudEventMessage BuildSessionStartEvent(UserActivityInfo userActivity)
        {
            var eventId = GenerateEventId();
            var timestamp = DateTime.UtcNow;

            var message = new CloudEventMessage
            {
                SpecVersion = SpecVersion,
                Type = $"{EventTypeBase}.session.start",
                Source = _source,
                Id = eventId,
                Time = timestamp.ToString("o"),
                DataContentType = "application/json",
                Subject = _hardwareId,
                Data = new CloudEventData
                {
                    HardwareId = _hardwareId,
                    Timestamp = timestamp,
                    UserActivity = CreateUserActivityData(userActivity)
                }
            };

            return message;
        }

        /// <summary>
        /// Build a session end event
        /// </summary>
        /// <param name="energyState">Final energy state</param>
        /// <param name="displayTimeStats">Display time statistics</param>
        /// <returns>CloudEvent message</returns>
        public CloudEventMessage BuildSessionEndEvent(EnergyState energyState, DisplayTimeStats displayTimeStats)
        {
            var eventId = GenerateEventId();
            var timestamp = DateTime.UtcNow;

            var message = new CloudEventMessage
            {
                SpecVersion = SpecVersion,
                Type = $"{EventTypeBase}.session.end",
                Source = _source,
                Id = eventId,
                Time = timestamp.ToString("o"),
                DataContentType = "application/json",
                Subject = _hardwareId,
                Data = new CloudEventData
                {
                    HardwareId = _hardwareId,
                    Timestamp = timestamp,
                    Energy = CreateEnergyData(energyState),
                    DisplayTime = CreateDisplayTimeData(displayTimeStats)
                }
            };

            return message;
        }

        /// <summary>
        /// Build a daily summary event
        /// </summary>
        /// <param name="energyState">Daily energy totals</param>
        /// <param name="displayTimeStats">Display time statistics</param>
        /// <returns>CloudEvent message</returns>
        public CloudEventMessage BuildDailySummaryEvent(EnergyState energyState, DisplayTimeStats displayTimeStats)
        {
            var eventId = GenerateEventId();
            var timestamp = DateTime.UtcNow;

            var message = new CloudEventMessage
            {
                SpecVersion = SpecVersion,
                Type = $"{EventTypeBase}.summary.daily",
                Source = _source,
                Id = eventId,
                Time = timestamp.ToString("o"),
                DataContentType = "application/json",
                Subject = _hardwareId,
                Data = new CloudEventData
                {
                    HardwareId = _hardwareId,
                    Timestamp = timestamp,
                    Energy = CreateEnergyData(energyState),
                    DisplayTime = CreateDisplayTimeData(displayTimeStats),
                    SummaryDate = DateTime.Today.ToString("yyyy-MM-dd")
                }
            };

            return message;
        }

        /// <summary>
        /// Create hardware data section
        /// </summary>
        private Dictionary<string, object> CreateHardwareData(HardwareInfo info)
        {
            if (info == null) return null;

            var data = new Dictionary<string, object>
            {
                ["machineName"] = info.MachineName,
                ["operatingSystem"] = info.OperatingSystem,
                ["mainboardManufacturer"] = info.MainboardManufacturer,
                ["mainboardProduct"] = info.MainboardProduct,
                ["cpuName"] = info.CpuName,
                ["cpuCores"] = info.CpuCores,
                ["cpuLogicalProcessors"] = info.CpuLogicalProcessors,
                ["cpuMaxClockSpeedMhz"] = info.CpuMaxClockSpeedMhz,
                ["cpuTdpWatts"] = info.CpuTdpWatts,
                ["totalMemoryMB"] = info.TotalMemoryMB,
                ["monitorCount"] = info.MonitorCount,
                ["diskCount"] = info.DiskCount,
                ["gpuCount"] = info.GpuCount
            };

            // Add monitor details
            if (info.Monitors != null && info.Monitors.Count > 0)
            {
                var monitors = new List<Dictionary<string, object>>();
                foreach (var monitor in info.Monitors)
                {
                    monitors.Add(new Dictionary<string, object>
                    {
                        ["isPrimary"] = monitor.IsPrimary,
                        ["screenSize"] = monitor.ScreenSizeInches,
                        ["resolution"] = $"{monitor.HorizontalResolution}x{monitor.VerticalResolution}",
                        ["powerOnWatts"] = monitor.PowerConsumptionOnWatts,
                        ["eprelNumber"] = monitor.EprelNumber,
                        ["energyClass"] = monitor.EnergyClass
                    });
                }
                data["monitors"] = monitors;
            }

            // Add disk details
            if (info.Disks != null && info.Disks.Count > 0)
            {
                var disks = new List<Dictionary<string, object>>();
                foreach (var disk in info.Disks)
                {
                    disks.Add(new Dictionary<string, object>
                    {
                        ["model"] = disk.Model,
                        ["type"] = disk.Type.ToString(),
                        ["sizeGB"] = disk.SizeGB,
                        ["powerActiveWatts"] = disk.PowerActiveWatts
                    });
                }
                data["disks"] = disks;
            }

            // Add GPU names
            if (info.Gpus != null && info.Gpus.Count > 0)
            {
                data["gpus"] = info.Gpus;
            }

            return data;
        }

        /// <summary>
        /// Create user activity data section
        /// </summary>
        private Dictionary<string, object> CreateUserActivityData(UserActivityInfo info)
        {
            if (info == null) return null;

            return new Dictionary<string, object>
            {
                ["isUserLoggedIn"] = info.IsUserLoggedIn,
                ["userName"] = info.UserName,
                ["domainName"] = info.DomainName,
                ["loginTimeUtc"] = info.LoginTimeUtc?.ToString("o"),
                ["idleTimeSeconds"] = info.IdleTimeSeconds,
                ["isScreenSaverRunning"] = info.IsScreenSaverRunning,
                ["isWorkstationLocked"] = info.IsWorkstationLocked,
                ["isRdpSession"] = info.IsRdpSession,
                ["isRemoteSession"] = info.IsRemoteSession,
                ["remoteToolName"] = info.RemoteToolName,
                ["firstActivityTodayUtc"] = info.FirstActivityTodayUtc?.ToString("o"),
                ["sessionState"] = info.SessionState.ToString()
            };
        }

        /// <summary>
        /// Create energy data section
        /// </summary>
        private Dictionary<string, object> CreateEnergyData(EnergyState state)
        {
            if (state == null) return null;

            return new Dictionary<string, object>
            {
                ["timestampUtc"] = state.TimestampUtc.ToString("o"),
                ["totalPowerWatts"] = Math.Round(state.TotalPowerWatts, 2),
                ["cpuPowerWatts"] = Math.Round(state.CpuPowerWatts, 2),
                ["displayPowerWatts"] = Math.Round(state.DisplayPowerWatts, 2),
                ["diskPowerWatts"] = Math.Round(state.DiskPowerWatts, 2),
                ["gpuPowerWatts"] = Math.Round(state.GpuPowerWatts, 2),
                ["memoryPowerWatts"] = Math.Round(state.MemoryPowerWatts, 2),
                ["basePowerWatts"] = Math.Round(state.BasePowerWatts, 2),
                ["psuEfficiency"] = state.PsuEfficiency,
                ["dailyEnergyWh"] = Math.Round(state.DailyEnergyWh, 2),
                ["sessionEnergyWh"] = Math.Round(state.SessionEnergyWh, 2),
                ["sessionDurationMinutes"] = Math.Round(state.SessionDuration.TotalMinutes, 1),
                ["efficiencyRating"] = state.EfficiencyRating
            };
        }

        /// <summary>
        /// Create display time data section
        /// </summary>
        private Dictionary<string, object> CreateDisplayTimeData(DisplayTimeStats stats)
        {
            if (stats == null) return null;

            return new Dictionary<string, object>
            {
                ["onTimeMinutes"] = Math.Round(stats.OnTimeToday.TotalMinutes, 1),
                ["offTimeMinutes"] = Math.Round(stats.OffTimeToday.TotalMinutes, 1),
                ["idleTimeMinutes"] = Math.Round(stats.IdleTimeToday.TotalMinutes, 1),
                ["currentState"] = stats.CurrentState.ToString(),
                ["currentStateDurationMinutes"] = Math.Round(stats.CurrentStateDuration.TotalMinutes, 1)
            };
        }

        /// <summary>
        /// Generate unique event ID (UUID format required by KurrentDB)
        /// </summary>
        private string GenerateEventId()
        {
            return Guid.NewGuid().ToString();
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
    }

    /// <summary>
    /// Extension data structure for CloudEvent payload
    /// </summary>
    public class CloudEventData
    {
        /// <summary>
        /// Hardware identifier
        /// </summary>
        public string HardwareId { get; set; }

        /// <summary>
        /// Event timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Hardware information
        /// </summary>
        public Dictionary<string, object> Hardware { get; set; }

        /// <summary>
        /// User activity information
        /// </summary>
        public Dictionary<string, object> UserActivity { get; set; }

        /// <summary>
        /// Energy state information
        /// </summary>
        public Dictionary<string, object> Energy { get; set; }

        /// <summary>
        /// Display time statistics
        /// </summary>
        public Dictionary<string, object> DisplayTime { get; set; }

        /// <summary>
        /// Service version (for registration events)
        /// </summary>
        public string ServiceVersion { get; set; }

        /// <summary>
        /// Summary date (for daily summary events)
        /// </summary>
        public string SummaryDate { get; set; }
    }
}
