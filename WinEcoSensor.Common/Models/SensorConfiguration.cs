// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2026 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WinEcoSensor.Common.Models
{
    [DataContract]
    public class SensorConfiguration
    {
        [DataMember(Name = "backendUrl")]
        public string BackendUrl { get; set; }

        [DataMember(Name = "reportIntervalSeconds")]
        public int ReportIntervalSeconds { get; set; }

        [DataMember(Name = "hardwareReportIntervalSeconds")]
        public int HardwareReportIntervalSeconds { get; set; }

        [DataMember(Name = "autoStart")]
        public bool AutoStart { get; set; }

        [DataMember(Name = "monitoringEnabled")]
        public bool MonitoringEnabled { get; set; }

        [DataMember(Name = "idleThresholdMinutes")]
        public int IdleThresholdMinutes { get; set; }

        [DataMember(Name = "monitorCpu")]
        public bool MonitorCpu { get; set; }

        [DataMember(Name = "monitorDisplays")]
        public bool MonitorDisplays { get; set; }

        [DataMember(Name = "monitorDisks")]
        public bool MonitorDisks { get; set; }

        [DataMember(Name = "monitorUserActivity")]
        public bool MonitorUserActivity { get; set; }

        [DataMember(Name = "detectRemoteSessions")]
        public bool DetectRemoteSessions { get; set; }

        [DataMember(Name = "logLevelValue")]
        public int LogLevelValue { get; set; }

        [DataMember(Name = "maxLogFileSizeMb")]
        public int MaxLogFileSizeMb { get; set; }

        [DataMember(Name = "logFileRetentionCount")]
        public int LogFileRetentionCount { get; set; }

        [DataMember(Name = "showTrayIcon")]
        public bool ShowTrayIcon { get; set; }

        [DataMember(Name = "showNotifications")]
        public bool ShowNotifications { get; set; }

        [DataMember(Name = "psuEfficiency")]
        public double PsuEfficiency { get; set; }

        [DataMember(Name = "electricityCostPerKwh")]
        public decimal ElectricityCostPerKwh { get; set; }

        [DataMember(Name = "currencyCode")]
        public string CurrencyCode { get; set; }

        [DataMember(Name = "machineId")]
        public string MachineId { get; set; }

        [DataMember(Name = "customMachineName")]
        public string CustomMachineName { get; set; }

        [DataMember(Name = "httpTimeoutSeconds")]
        public int HttpTimeoutSeconds { get; set; }

        [DataMember(Name = "httpRetryCount")]
        public int HttpRetryCount { get; set; }

        [DataMember(Name = "queueEventsWhenOffline")]
        public bool QueueEventsWhenOffline { get; set; }

        [DataMember(Name = "maxQueuedEvents")]
        public int MaxQueuedEvents { get; set; }

        [DataMember(Name = "configFilePath")]
        public string ConfigFilePath { get; set; }

        [DataMember(Name = "lastModifiedUtc")]
        public DateTime LastModifiedUtc { get; set; }

        [DataMember(Name = "eprelMappings")]
        public List<EprelMapping> EprelMappings { get; set; }

        public SensorConfiguration()
        {
            BackendUrl = "http://localhost:8080/event/batch";
            ReportIntervalSeconds = 60;
            HardwareReportIntervalSeconds = 3600;
            AutoStart = true;
            MonitoringEnabled = true;
            IdleThresholdMinutes = 5;
            MonitorCpu = true;
            MonitorDisplays = true;
            MonitorDisks = true;
            MonitorUserActivity = true;
            DetectRemoteSessions = true;
            LogLevelValue = 3;
            MaxLogFileSizeMb = 10;
            LogFileRetentionCount = 5;
            ShowTrayIcon = true;
            ShowNotifications = true;
            PsuEfficiency = 0.85;
            ElectricityCostPerKwh = 0.30m;
            CurrencyCode = "EUR";
            HttpTimeoutSeconds = 30;
            HttpRetryCount = 3;
            QueueEventsWhenOffline = true;
            MaxQueuedEvents = 1000;
            LastModifiedUtc = DateTime.UtcNow;
            EprelMappings = new List<EprelMapping>();
        }

        // Backward-compatible alias properties
        public int StatusIntervalSeconds
        {
            get { return ReportIntervalSeconds; }
            set { ReportIntervalSeconds = value; }
        }

        public int HeartbeatIntervalSeconds
        {
            get { return HardwareReportIntervalSeconds; }
            set { HardwareReportIntervalSeconds = value; }
        }

        public bool MonitorRemoteSessions
        {
            get { return DetectRemoteSessions; }
            set { DetectRemoteSessions = value; }
        }

        public int IdleThresholdSeconds
        {
            get { return IdleThresholdMinutes * 60; }
            set { IdleThresholdMinutes = value / 60; }
        }

        public bool LogToFile
        {
            get { return true; }
            set { /* ignored */ }
        }

        public string LogLevel
        {
            get
            {
                switch (LogLevelValue)
                {
                    case 0: return "None";
                    case 1: return "Error";
                    case 2: return "Warning";
                    case 3: return "Info";
                    case 4: return "Debug";
                    default: return "Info";
                }
            }
            set
            {
                if (value == null) { LogLevelValue = 3; return; }
                switch (value.ToLower())
                {
                    case "none": LogLevelValue = 0; break;
                    case "error": LogLevelValue = 1; break;
                    case "warning": LogLevelValue = 2; break;
                    case "info": LogLevelValue = 3; break;
                    case "debug": LogLevelValue = 4; break;
                    default: LogLevelValue = 3; break;
                }
            }
        }

        public void EnsureMachineId()
        {
            if (string.IsNullOrEmpty(MachineId))
            {
                string hostname = Environment.MachineName;
                string domain = Environment.UserDomainName;
                string combined = domain + "\\" + hostname;

                using (var sha = System.Security.Cryptography.SHA256.Create())
                {
                    byte[] hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(combined));
                    MachineId = BitConverter.ToString(hash).Replace("-", "").Substring(0, 16).ToLowerInvariant();
                }
            }
        }

        public bool Validate(out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(BackendUrl))
            {
                errorMessage = "Backend URL is required";
                return false;
            }

            Uri uri;
            if (!Uri.TryCreate(BackendUrl, UriKind.Absolute, out uri))
            {
                errorMessage = "Backend URL is not a valid URI";
                return false;
            }

            if (ReportIntervalSeconds < 10 || ReportIntervalSeconds > 3600)
            {
                errorMessage = "Report interval must be between 10 and 3600 seconds";
                return false;
            }

            if (PsuEfficiency < 0.5 || PsuEfficiency > 1.0)
            {
                errorMessage = "PSU efficiency must be between 0.5 and 1.0";
                return false;
            }

            errorMessage = null;
            return true;
        }
    }
}
