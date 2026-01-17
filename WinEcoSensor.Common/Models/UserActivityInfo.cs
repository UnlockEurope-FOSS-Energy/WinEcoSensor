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
    public class UserActivityInfo
    {
        [DataMember(Name = "clientName")]
        public string ClientName { get; set; }

        [DataMember(Name = "domainName")]
        public string DomainName { get; set; }

        [DataMember(Name = "loggedInUser")]
        public string LoggedInUser { get; set; }

        [DataMember(Name = "isUserLoggedIn")]
        public bool IsUserLoggedIn { get; set; }

        [DataMember(Name = "sessionState")]
        public SessionState SessionState { get; set; }

        [DataMember(Name = "loginTimeUtc")]
        public DateTime? LoginTimeUtc { get; set; }

        [DataMember(Name = "sessionDuration")]
        public TimeSpan SessionDuration { get; set; }

        [DataMember(Name = "firstActivityTodayUtc")]
        public DateTime? FirstActivityTodayUtc { get; set; }

        [DataMember(Name = "timeSinceFirstActivity")]
        public TimeSpan TimeSinceFirstActivity { get; set; }

        [DataMember(Name = "lastInputActivityUtc")]
        public DateTime LastInputActivityUtc { get; set; }

        [DataMember(Name = "idleTime")]
        public TimeSpan IdleTime { get; set; }

        [DataMember(Name = "isIdle")]
        public bool IsIdle { get; set; }

        [DataMember(Name = "idleThresholdMinutes")]
        public int IdleThresholdMinutes { get; set; }

        [DataMember(Name = "isScreenSaverActive")]
        public bool IsScreenSaverActive { get; set; }

        [DataMember(Name = "isWorkstationLocked")]
        public bool IsWorkstationLocked { get; set; }

        [DataMember(Name = "isRdpSessionActive")]
        public bool IsRdpSessionActive { get; set; }

        [DataMember(Name = "rdpSession")]
        public RemoteSessionInfo RdpSession { get; set; }

        [DataMember(Name = "remoteAccessSessions")]
        public List<RemoteSessionInfo> RemoteAccessSessions { get; set; }

        [DataMember(Name = "isRemoteAccessActive")]
        public bool IsRemoteAccessActive { get; set; }

        [DataMember(Name = "activeMonitorCount")]
        public int ActiveMonitorCount { get; set; }

        [DataMember(Name = "runningProcessCount")]
        public int RunningProcessCount { get; set; }

        [DataMember(Name = "activeWindowCount")]
        public int ActiveWindowCount { get; set; }

        [DataMember(Name = "displayState")]
        public DisplayActivityState DisplayState { get; set; }

        [DataMember(Name = "activityLevel")]
        public ActivityLevel ActivityLevel { get; set; }

        [DataMember(Name = "systemBootTimeUtc")]
        public DateTime SystemBootTimeUtc { get; set; }

        [DataMember(Name = "systemUptime")]
        public TimeSpan SystemUptime { get; set; }

        [DataMember(Name = "collectedAtUtc")]
        public DateTime CollectedAtUtc { get; set; }

        public UserActivityInfo()
        {
            RemoteAccessSessions = new List<RemoteSessionInfo>();
            CollectedAtUtc = DateTime.UtcNow;
            IdleThresholdMinutes = 5;
            SessionState = SessionState.Unknown;
            DisplayState = DisplayActivityState.Unknown;
            ActivityLevel = ActivityLevel.Unknown;
        }

        // Backward-compatible alias properties
        public bool IsLoggedIn
        {
            get { return IsUserLoggedIn; }
            set { IsUserLoggedIn = value; }
        }

        public string UserName
        {
            get { return LoggedInUser; }
            set { LoggedInUser = value; }
        }

        public int IdleTimeSeconds
        {
            get { return (int)IdleTime.TotalSeconds; }
        }

        public bool IsScreenSaverRunning
        {
            get { return IsScreenSaverActive; }
            set { IsScreenSaverActive = value; }
        }

        public bool IsRdpSession
        {
            get { return IsRdpSessionActive; }
            set { IsRdpSessionActive = value; }
        }

        public bool IsRemoteSession
        {
            get { return IsRemoteAccessActive; }
            set { IsRemoteAccessActive = value; }
        }

        public string RemoteToolName
        {
            get
            {
                if (IsRdpSessionActive) return "RDP";
                if (RemoteAccessSessions != null && RemoteAccessSessions.Count > 0)
                    return RemoteAccessSessions[0].SessionType.ToString();
                return null;
            }
            set
            {
                // Read-only property, setter for compatibility only
            }
        }

        public DateTime? FirstActivityToday
        {
            get { return FirstActivityTodayUtc.HasValue ? FirstActivityTodayUtc.Value.ToLocalTime() : (DateTime?)null; }
        }

        /// <summary>
        /// RDP client name (from RdpSession)
        /// </summary>
        public string RdpClientName
        {
            get { return RdpSession != null ? RdpSession.ClientName : null; }
            set { if (RdpSession != null) RdpSession.ClientName = value; }
        }

        /// <summary>
        /// RDP client address (from RdpSession)
        /// </summary>
        public string RdpClientAddress
        {
            get { return RdpSession != null ? RdpSession.ClientAddress : null; }
            set { if (RdpSession != null) RdpSession.ClientAddress = value; }
        }

        public void UpdateComputedFields()
        {
            IdleTime = DateTime.UtcNow - LastInputActivityUtc;
            IsIdle = IdleTime.TotalMinutes >= IdleThresholdMinutes;

            if (LoginTimeUtc.HasValue)
            {
                SessionDuration = DateTime.UtcNow - LoginTimeUtc.Value;
            }

            if (FirstActivityTodayUtc.HasValue)
            {
                TimeSinceFirstActivity = DateTime.UtcNow - FirstActivityTodayUtc.Value;
            }

            SystemUptime = DateTime.UtcNow - SystemBootTimeUtc;
            IsRemoteAccessActive = IsRdpSessionActive || (RemoteAccessSessions != null && RemoteAccessSessions.Count > 0);
            ActivityLevel = DetermineActivityLevel();
            UpdateDisplayState();
        }

        private ActivityLevel DetermineActivityLevel()
        {
            if (!IsUserLoggedIn) return ActivityLevel.None;
            if (IsWorkstationLocked) return ActivityLevel.Locked;
            if (IdleTime.TotalMinutes >= 30) return ActivityLevel.None;
            if (IdleTime.TotalMinutes >= 15) return ActivityLevel.VeryLow;
            if (IdleTime.TotalMinutes >= 5) return ActivityLevel.Low;
            if (IdleTime.TotalMinutes >= 1) return ActivityLevel.Medium;
            return ActivityLevel.High;
        }

        private void UpdateDisplayState()
        {
            if (ActiveMonitorCount == 0) DisplayState = DisplayActivityState.Off;
            else if (IsScreenSaverActive) DisplayState = DisplayActivityState.ScreenSaver;
            else if (IsIdle) DisplayState = DisplayActivityState.Idle;
            else DisplayState = DisplayActivityState.Active;
        }
    }

    [DataContract]
    public class RemoteSessionInfo
    {
        [DataMember(Name = "sessionType")]
        public RemoteSessionType SessionType { get; set; }

        [DataMember(Name = "clientName")]
        public string ClientName { get; set; }

        [DataMember(Name = "clientAddress")]
        public string ClientAddress { get; set; }

        [DataMember(Name = "userName")]
        public string UserName { get; set; }

        [DataMember(Name = "sessionId")]
        public int SessionId { get; set; }

        [DataMember(Name = "isActive")]
        public bool IsActive { get; set; }

        [DataMember(Name = "connectionTimeUtc")]
        public DateTime ConnectionTimeUtc { get; set; }

        [DataMember(Name = "processName")]
        public string ProcessName { get; set; }
    }

    public enum RemoteSessionType { Unknown = 0, Rdp = 1, TeamViewer = 2, AnyDesk = 3, VNC = 4, LogMeIn = 5, RemoteDesktopPlus = 6, Other = 99 }
    public enum SessionState { Unknown = 0, Active = 1, Connected = 2, ConnectQuery = 3, Shadow = 4, Disconnected = 5, Idle = 6, Listen = 7, Reset = 8, Down = 9, Init = 10 }
    public enum DisplayActivityState { Unknown = 0, Active = 1, Idle = 2, ScreenSaver = 3, Off = 4 }
    public enum ActivityLevel { Unknown = 0, None = 1, VeryLow = 2, Low = 3, Medium = 4, High = 5, Locked = 6 }
}
