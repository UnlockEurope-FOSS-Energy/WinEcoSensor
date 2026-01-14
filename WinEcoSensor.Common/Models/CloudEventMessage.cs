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
    /// <summary>
    /// CloudEvents specification compliant message structure.
    /// See https://cloudevents.io/ for specification details.
    /// </summary>
    [DataContract]
    public class CloudEventMessage
    {
        /// <summary>
        /// CloudEvents specification version (required)
        /// </summary>
        [DataMember(Name = "specversion")]
        public string SpecVersion { get; set; }

        /// <summary>
        /// Event type identifier (required)
        /// </summary>
        [DataMember(Name = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Event source URI (required)
        /// </summary>
        [DataMember(Name = "source")]
        public string Source { get; set; }

        /// <summary>
        /// Unique event identifier (required)
        /// </summary>
        [DataMember(Name = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Timestamp of event occurrence (optional but recommended)
        /// </summary>
        [DataMember(Name = "time")]
        public string Time { get; set; }

        /// <summary>
        /// Content type of data payload (optional)
        /// </summary>
        [DataMember(Name = "datacontenttype")]
        public string DataContentType { get; set; }

        /// <summary>
        /// Schema URI for data validation (optional)
        /// </summary>
        [DataMember(Name = "dataschema")]
        public string DataSchema { get; set; }

        /// <summary>
        /// Subject of the event (optional)
        /// </summary>
        [DataMember(Name = "subject")]
        public string Subject { get; set; }

        /// <summary>
        /// Event data payload
        /// </summary>
        [DataMember(Name = "data")]
        public object Data { get; set; }

        /// <summary>
        /// Extension attributes
        /// </summary>
        [DataMember(Name = "extensions")]
        public Dictionary<string, object> Extensions { get; set; }

        public CloudEventMessage()
        {
            SpecVersion = "1.0";
            Id = Guid.NewGuid().ToString();
            Time = DateTime.UtcNow.ToString("o"); // ISO 8601 format
            DataContentType = "application/json";
            Extensions = new Dictionary<string, object>();
        }

        /// <summary>
        /// Create a new CloudEvent for energy state update
        /// </summary>
        public static CloudEventMessage CreateEnergyStateEvent(string machineId, EnergyState energyState)
        {
            return new CloudEventMessage
            {
                Type = "eu.unlockeurope.winecosensor.energystate.v1",
                Source = $"urn:winecosensor:{machineId}",
                Subject = $"machine/{machineId}/energy",
                Data = energyState,
                Extensions = new Dictionary<string, object>
                {
                    { "machineid", machineId },
                    { "eventcategory", "energy" }
                }
            };
        }

        /// <summary>
        /// Create a new CloudEvent for hardware info report
        /// </summary>
        public static CloudEventMessage CreateHardwareInfoEvent(string machineId, HardwareInfo hardwareInfo)
        {
            return new CloudEventMessage
            {
                Type = "eu.unlockeurope.winecosensor.hardwareinfo.v1",
                Source = $"urn:winecosensor:{machineId}",
                Subject = $"machine/{machineId}/hardware",
                Data = hardwareInfo,
                Extensions = new Dictionary<string, object>
                {
                    { "machineid", machineId },
                    { "eventcategory", "hardware" }
                }
            };
        }

        /// <summary>
        /// Create a new CloudEvent for user activity update
        /// </summary>
        public static CloudEventMessage CreateUserActivityEvent(string machineId, UserActivityInfo activityInfo)
        {
            return new CloudEventMessage
            {
                Type = "eu.unlockeurope.winecosensor.useractivity.v1",
                Source = $"urn:winecosensor:{machineId}",
                Subject = $"machine/{machineId}/activity",
                Data = activityInfo,
                Extensions = new Dictionary<string, object>
                {
                    { "machineid", machineId },
                    { "eventcategory", "activity" }
                }
            };
        }

        /// <summary>
        /// Create a new CloudEvent for service status (heartbeat)
        /// </summary>
        public static CloudEventMessage CreateHeartbeatEvent(string machineId, ServiceStatus status)
        {
            return new CloudEventMessage
            {
                Type = "eu.unlockeurope.winecosensor.heartbeat.v1",
                Source = $"urn:winecosensor:{machineId}",
                Subject = $"machine/{machineId}/heartbeat",
                Data = status,
                Extensions = new Dictionary<string, object>
                {
                    { "machineid", machineId },
                    { "eventcategory", "heartbeat" }
                }
            };
        }
    }

    /// <summary>
    /// Service status information for heartbeat events
    /// </summary>
    [DataContract]
    public class ServiceStatus
    {
        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "version")]
        public string Version { get; set; }

        [DataMember(Name = "uptimeSeconds")]
        public long UptimeSeconds { get; set; }

        [DataMember(Name = "lastEventTimeUtc")]
        public DateTime LastEventTimeUtc { get; set; }

        [DataMember(Name = "eventsSentCount")]
        public long EventsSentCount { get; set; }

        [DataMember(Name = "errorCount")]
        public long ErrorCount { get; set; }

        [DataMember(Name = "monitoringEnabled")]
        public bool MonitoringEnabled { get; set; }

        public ServiceStatus()
        {
            Status = "running";
            Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0.0";
            LastEventTimeUtc = DateTime.UtcNow;
        }
    }
}
