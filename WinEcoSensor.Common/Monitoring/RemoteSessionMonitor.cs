// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2024 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using WinEcoSensor.Common.Models;
using WinEcoSensor.Common.Utilities;

namespace WinEcoSensor.Common.Monitoring
{
    /// <summary>
    /// Monitors remote desktop and remote access sessions.
    /// Detects RDP, TeamViewer, AnyDesk, VNC, and other remote access tools.
    /// </summary>
    public class RemoteSessionMonitor
    {
        // WTS API imports for terminal services session info
        [DllImport("wtsapi32.dll")]
        private static extern IntPtr WTSOpenServer(string serverName);

        [DllImport("wtsapi32.dll")]
        private static extern void WTSCloseServer(IntPtr hServer);

        [DllImport("wtsapi32.dll")]
        private static extern bool WTSEnumerateSessions(
            IntPtr hServer,
            int reserved,
            int version,
            ref IntPtr ppSessionInfo,
            ref int pCount);

        [DllImport("wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr pMemory);

        [DllImport("wtsapi32.dll")]
        private static extern bool WTSQuerySessionInformation(
            IntPtr hServer,
            int sessionId,
            WTS_INFO_CLASS wtsInfoClass,
            out IntPtr ppBuffer,
            out int pBytesReturned);

        [DllImport("kernel32.dll")]
        private static extern int GetCurrentProcessId();

        [DllImport("kernel32.dll")]
        private static extern bool ProcessIdToSessionId(int dwProcessId, out int pSessionId);

        [StructLayout(LayoutKind.Sequential)]
        private struct WTS_SESSION_INFO
        {
            public int SessionID;
            public IntPtr pWinStationName;
            public WTS_CONNECTSTATE_CLASS State;
        }

        private enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,
            WTSConnected,
            WTSConnectQuery,
            WTSShadow,
            WTSDisconnected,
            WTSIdle,
            WTSListen,
            WTSReset,
            WTSDown,
            WTSInit
        }

        private enum WTS_INFO_CLASS
        {
            WTSClientName = 10,
            WTSClientAddress = 14,
            WTSSessionInfo = 24
        }

        // Known remote access tool process names
        private static readonly Dictionary<string, RemoteSessionType> RemoteToolProcesses = new Dictionary<string, RemoteSessionType>(StringComparer.OrdinalIgnoreCase)
        {
            { "TeamViewer", RemoteSessionType.TeamViewer },
            { "TeamViewer_Service", RemoteSessionType.TeamViewer },
            { "tv_w32", RemoteSessionType.TeamViewer },
            { "tv_x64", RemoteSessionType.TeamViewer },
            { "AnyDesk", RemoteSessionType.AnyDesk },
            { "AnyDesk.exe", RemoteSessionType.AnyDesk },
            { "vncserver", RemoteSessionType.VNC },
            { "winvnc", RemoteSessionType.VNC },
            { "tvnserver", RemoteSessionType.VNC },
            { "ultravnc", RemoteSessionType.VNC },
            { "LogMeIn", RemoteSessionType.LogMeIn },
            { "LMIGuardianSvc", RemoteSessionType.LogMeIn },
            { "RemoteDesktopPlus", RemoteSessionType.RemoteDesktopPlus },
            { "rdpclip", RemoteSessionType.Rdp },
            { "mstsc", RemoteSessionType.Rdp },
            { "TermService", RemoteSessionType.Rdp }
        };

        /// <summary>
        /// Current RDP session info
        /// </summary>
        public RemoteSessionInfo RdpSession { get; private set; }

        /// <summary>
        /// List of detected remote access sessions
        /// </summary>
        public List<RemoteSessionInfo> RemoteAccessSessions { get; private set; }

        /// <summary>
        /// Whether any remote session is active
        /// </summary>
        public bool IsAnyRemoteSessionActive { get; private set; }

        /// <summary>
        /// Whether RDP session is active
        /// </summary>
        public bool IsRdpActive { get; private set; }

        /// <summary>
        /// Create new remote session monitor
        /// </summary>
        public RemoteSessionMonitor()
        {
            RemoteAccessSessions = new List<RemoteSessionInfo>();
        }

        /// <summary>
        /// Update all remote session information
        /// </summary>
        public void Update()
        {
            try
            {
                RemoteAccessSessions.Clear();
                
                // Check RDP sessions
                CheckRdpSessions();

                // Check for remote access tools
                CheckRemoteAccessTools();

                // Update flags
                IsRdpActive = RdpSession != null && RdpSession.IsActive;
                IsAnyRemoteSessionActive = IsRdpActive || RemoteAccessSessions.Any(s => s.IsActive);
            }
            catch (Exception ex)
            {
                Logger.Error("Error updating remote session info", ex);
            }
        }

        /// <summary>
        /// Check for active RDP sessions
        /// </summary>
        private void CheckRdpSessions()
        {
            RdpSession = null;
            
            try
            {
                // Get current session ID
                int currentSessionId;
                ProcessIdToSessionId(GetCurrentProcessId(), out currentSessionId);

                IntPtr serverHandle = WTSOpenServer(Environment.MachineName);
                if (serverHandle == IntPtr.Zero)
                {
                    serverHandle = IntPtr.Zero; // Use local server
                }

                try
                {
                    IntPtr sessionInfoPtr = IntPtr.Zero;
                    int sessionCount = 0;

                    if (WTSEnumerateSessions(serverHandle, 0, 1, ref sessionInfoPtr, ref sessionCount))
                    {
                        int dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
                        IntPtr current = sessionInfoPtr;

                        for (int i = 0; i < sessionCount; i++)
                        {
                            WTS_SESSION_INFO sessionInfo = (WTS_SESSION_INFO)Marshal.PtrToStructure(current, typeof(WTS_SESSION_INFO));
                            current = IntPtr.Add(current, dataSize);

                            string stationName = Marshal.PtrToStringAnsi(sessionInfo.pWinStationName);

                            // Check if this is an RDP session
                            if (stationName != null && 
                                (stationName.StartsWith("RDP", StringComparison.OrdinalIgnoreCase) ||
                                 stationName.Contains("-Tcp")))
                            {
                                // This is an RDP session
                                var rdpInfo = new RemoteSessionInfo
                                {
                                    SessionType = RemoteSessionType.Rdp,
                                    SessionId = sessionInfo.SessionID,
                                    IsActive = sessionInfo.State == WTS_CONNECTSTATE_CLASS.WTSActive,
                                    ConnectionTimeUtc = DateTime.UtcNow
                                };

                                // Get client name
                                string clientName = GetSessionClientName(serverHandle, sessionInfo.SessionID);
                                rdpInfo.ClientName = clientName;

                                // Get client address
                                string clientAddress = GetSessionClientAddress(serverHandle, sessionInfo.SessionID);
                                rdpInfo.ClientAddress = clientAddress;

                                if (rdpInfo.IsActive || sessionInfo.State == WTS_CONNECTSTATE_CLASS.WTSConnected)
                                {
                                    RdpSession = rdpInfo;
                                    RemoteAccessSessions.Add(rdpInfo);
                                    Logger.Debug($"Active RDP session detected: {clientName} ({clientAddress})");
                                }
                            }
                        }

                        WTSFreeMemory(sessionInfoPtr);
                    }
                }
                finally
                {
                    if (serverHandle != IntPtr.Zero)
                    {
                        WTSCloseServer(serverHandle);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("Error checking RDP sessions", ex);
            }
        }

        /// <summary>
        /// Get session client name
        /// </summary>
        private string GetSessionClientName(IntPtr serverHandle, int sessionId)
        {
            try
            {
                IntPtr buffer;
                int bytesReturned;

                if (WTSQuerySessionInformation(serverHandle, sessionId, WTS_INFO_CLASS.WTSClientName, out buffer, out bytesReturned))
                {
                    string clientName = Marshal.PtrToStringAnsi(buffer);
                    WTSFreeMemory(buffer);
                    return clientName;
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Get session client address
        /// </summary>
        private string GetSessionClientAddress(IntPtr serverHandle, int sessionId)
        {
            // This is a simplified implementation
            // Full implementation would parse the WTS_CLIENT_ADDRESS structure
            return null;
        }

        /// <summary>
        /// Check for running remote access tools
        /// </summary>
        private void CheckRemoteAccessTools()
        {
            try
            {
                var processes = Process.GetProcesses();
                var detectedTools = new HashSet<RemoteSessionType>();

                foreach (var process in processes)
                {
                    try
                    {
                        string processName = process.ProcessName;
                        
                        if (RemoteToolProcesses.TryGetValue(processName, out RemoteSessionType sessionType))
                        {
                            // Only add each type once
                            if (!detectedTools.Contains(sessionType))
                            {
                                detectedTools.Add(sessionType);
                                
                                var sessionInfo = new RemoteSessionInfo
                                {
                                    SessionType = sessionType,
                                    ProcessName = processName,
                                    IsActive = true,
                                    ConnectionTimeUtc = DateTime.UtcNow
                                };

                                // Skip RDP tools if we already have RDP session info
                                if (sessionType != RemoteSessionType.Rdp)
                                {
                                    RemoteAccessSessions.Add(sessionInfo);
                                    Logger.Debug($"Remote access tool detected: {sessionType} ({processName})");
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Process may have exited
                    }
                }

                foreach (var process in processes)
                {
                    try { process.Dispose(); } catch { }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error checking remote access tools", ex);
            }
        }

        /// <summary>
        /// Check if a specific remote access tool is running
        /// </summary>
        public bool IsRemoteToolRunning(RemoteSessionType toolType)
        {
            return RemoteAccessSessions.Any(s => s.SessionType == toolType && s.IsActive);
        }

        /// <summary>
        /// Get summary of active remote sessions
        /// </summary>
        public string GetRemoteSessionSummary()
        {
            if (!IsAnyRemoteSessionActive)
                return "No remote sessions";

            var parts = new List<string>();

            if (IsRdpActive && RdpSession != null)
            {
                parts.Add($"RDP: {RdpSession.ClientName ?? "Unknown client"}");
            }

            foreach (var session in RemoteAccessSessions.Where(s => s.SessionType != RemoteSessionType.Rdp))
            {
                parts.Add($"{session.SessionType}: Active");
            }

            return string.Join(", ", parts);
        }

        // Backward-compatible alias properties
        /// <summary>
        /// Alias for IsRdpActive
        /// </summary>
        public bool IsRdpSession
        {
            get { return IsRdpActive; }
        }

        /// <summary>
        /// RDP client name
        /// </summary>
        public string RdpClientName
        {
            get { return RdpSession != null ? RdpSession.ClientName : null; }
        }

        /// <summary>
        /// RDP client address
        /// </summary>
        public string RdpClientAddress
        {
            get { return RdpSession != null ? RdpSession.ClientAddress : null; }
        }

        /// <summary>
        /// Alias for IsAnyRemoteSessionActive
        /// </summary>
        public bool IsRemoteAccessActive
        {
            get { return IsAnyRemoteSessionActive; }
        }

        /// <summary>
        /// Name of currently active remote tool
        /// </summary>
        public string ActiveRemoteToolName
        {
            get
            {
                if (IsRdpActive) return "RDP";
                var activeSession = RemoteAccessSessions.FirstOrDefault(s => s.IsActive);
                return activeSession != null ? activeSession.SessionType.ToString() : null;
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            RemoteAccessSessions.Clear();
        }
    }
}
