// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2026 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using WinEcoSensor.Common.Models;
using WinEcoSensor.Common.Utilities;

namespace WinEcoSensor.Common.Monitoring
{
    /// <summary>
    /// Monitors user activity including login status, idle time, and input activity.
    /// </summary>
    public class UserActivityMonitor
    {
        // Win32 API imports for idle time detection
        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        // For screen saver detection
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool SystemParametersInfo(int uAction, int uParam, ref bool lpvParam, int flags);

        private const int SPI_GETSCREENSAVERRUNNING = 114;

        // For workstation lock detection
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        // WTS API for querying real user sessions (works from services)
        [DllImport("wtsapi32.dll", SetLastError = true)]
        private static extern bool WTSEnumerateSessions(IntPtr hServer, int Reserved, int Version, ref IntPtr ppSessionInfo, ref int pCount);

        [DllImport("wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr pMemory);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        private static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out IntPtr ppBuffer, out int pBytesReturned);

        private static readonly IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;

        private enum WTS_INFO_CLASS
        {
            WTSUserName = 5,
            WTSDomainName = 7,
            WTSConnectState = 8,
            WTSClientName = 10,
            WTSSessionInfo = 24
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

        [StructLayout(LayoutKind.Sequential)]
        private struct WTS_SESSION_INFO
        {
            public int SessionId;
            public IntPtr pWinStationName;
            public WTS_CONNECTSTATE_CLASS State;
        }

        private int _idleThresholdMinutes;
        private DateTime? _firstActivityToday;
        private DateTime _todayDate;

        /// <summary>
        /// Current activity info
        /// </summary>
        public UserActivityInfo CurrentActivity { get; private set; }

        /// <summary>
        /// Create new user activity monitor
        /// </summary>
        /// <param name="idleThresholdMinutes">Minutes of inactivity before considered idle</param>
        public UserActivityMonitor(int idleThresholdMinutes = 5)
        {
            _idleThresholdMinutes = idleThresholdMinutes;
            CurrentActivity = new UserActivityInfo
            {
                IdleThresholdMinutes = idleThresholdMinutes
            };
            _todayDate = DateTime.Today;
        }

        /// <summary>
        /// Update all activity information
        /// </summary>
        public UserActivityInfo Update()
        {
            var activity = new UserActivityInfo
            {
                IdleThresholdMinutes = _idleThresholdMinutes,
                CollectedAtUtc = DateTime.UtcNow
            };

            try
            {
                // Get basic client info
                activity.ClientName = Environment.MachineName;

                // Get real user session info using WTS API (works from services)
                GetActiveUserSession(activity);

                // Get system boot time
                activity.SystemBootTimeUtc = GetSystemBootTime();

                // Get idle time - only works if we're in user context
                // For services, we estimate based on session state
                if (_isRunningAsService)
                {
                    // When running as service, use session state for idle detection
                    activity.IsIdle = activity.SessionState == SessionState.Disconnected ||
                                      activity.SessionState == SessionState.Idle;
                    activity.IdleTime = activity.IsIdle ? TimeSpan.FromMinutes(_idleThresholdMinutes + 1) : TimeSpan.Zero;
                    activity.LastInputActivityUtc = activity.IsIdle ? DateTime.UtcNow.AddMinutes(-_idleThresholdMinutes - 1) : DateTime.UtcNow;
                }
                else
                {
                    var idleTime = GetIdleTime();
                    activity.LastInputActivityUtc = DateTime.UtcNow - idleTime;
                    activity.IdleTime = idleTime;
                    activity.IsIdle = idleTime.TotalMinutes >= _idleThresholdMinutes;
                }

                // Check screen saver (may not work from service)
                activity.IsScreenSaverActive = IsScreenSaverRunning();

                // Track first activity of the day
                TrackFirstActivityToday(activity);

                // Get process count
                activity.RunningProcessCount = Process.GetProcesses().Length;

                // Update computed fields
                activity.UpdateComputedFields();

                CurrentActivity = activity;
            }
            catch (Exception ex)
            {
                Logger.Error("Error updating user activity", ex);
            }

            return activity;
        }

        /// <summary>
        /// Get active user session info using WTS API
        /// </summary>
        private void GetActiveUserSession(UserActivityInfo activity)
        {
            IntPtr pSessionInfo = IntPtr.Zero;
            int sessionCount = 0;

            Logger.Debug($"GetActiveUserSession: Running as service = {_isRunningAsService}, Current SessionId = {Process.GetCurrentProcess().SessionId}");

            try
            {
                if (WTSEnumerateSessions(WTS_CURRENT_SERVER_HANDLE, 0, 1, ref pSessionInfo, ref sessionCount))
                {
                    Logger.Debug($"WTSEnumerateSessions: Found {sessionCount} sessions");
                    int sessionInfoSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
                    IntPtr current = pSessionInfo;

                    for (int i = 0; i < sessionCount; i++)
                    {
                        WTS_SESSION_INFO sessionInfo = (WTS_SESSION_INFO)Marshal.PtrToStructure(current, typeof(WTS_SESSION_INFO));
                        current = IntPtr.Add(current, sessionInfoSize);

                        string stationName = sessionInfo.pWinStationName != IntPtr.Zero
                            ? Marshal.PtrToStringAnsi(sessionInfo.pWinStationName)
                            : "null";
                        Logger.Debug($"Session {sessionInfo.SessionId}: Station={stationName}, State={sessionInfo.State}");

                        // Skip Session 0 (services) and console session without user
                        if (sessionInfo.SessionId == 0)
                            continue;

                        // Get username for this session
                        string userName = GetSessionString(sessionInfo.SessionId, WTS_INFO_CLASS.WTSUserName);
                        Logger.Debug($"Session {sessionInfo.SessionId}: UserName={userName ?? "(null)"}");

                        if (string.IsNullOrEmpty(userName))
                            continue;

                        // Found an active user session
                        activity.LoggedInUser = userName;
                        activity.DomainName = GetSessionString(sessionInfo.SessionId, WTS_INFO_CLASS.WTSDomainName);
                        activity.IsUserLoggedIn = true;

                        // Map WTS state to our session state
                        switch (sessionInfo.State)
                        {
                            case WTS_CONNECTSTATE_CLASS.WTSActive:
                                activity.SessionState = SessionState.Active;
                                activity.IsWorkstationLocked = false;
                                break;
                            case WTS_CONNECTSTATE_CLASS.WTSDisconnected:
                                activity.SessionState = SessionState.Disconnected;
                                activity.IsWorkstationLocked = true;
                                break;
                            case WTS_CONNECTSTATE_CLASS.WTSConnected:
                                activity.SessionState = SessionState.Connected;
                                activity.IsWorkstationLocked = false;
                                break;
                            case WTS_CONNECTSTATE_CLASS.WTSIdle:
                                activity.SessionState = SessionState.Idle;
                                activity.IsWorkstationLocked = false;
                                break;
                            default:
                                activity.SessionState = SessionState.Unknown;
                                break;
                        }

                        Logger.Debug($"Found user session: {userName} (Session {sessionInfo.SessionId}, State: {sessionInfo.State})");
                        return; // Found active user, exit
                    }
                }

                // No active user session found
                activity.IsUserLoggedIn = false;
                activity.SessionState = SessionState.Disconnected;
                Logger.Debug("No active user session found");
            }
            catch (Exception ex)
            {
                Logger.Error("Error querying user sessions", ex);
                // Fallback to Environment values
                activity.DomainName = Environment.UserDomainName;
                activity.LoggedInUser = Environment.UserName;
                activity.IsUserLoggedIn = !string.IsNullOrEmpty(activity.LoggedInUser) &&
                                          activity.LoggedInUser.ToUpperInvariant() != "SYSTEM";
            }
            finally
            {
                if (pSessionInfo != IntPtr.Zero)
                {
                    WTSFreeMemory(pSessionInfo);
                }
            }
        }

        /// <summary>
        /// Get string value from WTS session
        /// </summary>
        private string GetSessionString(int sessionId, WTS_INFO_CLASS infoClass)
        {
            IntPtr buffer = IntPtr.Zero;
            int bytesReturned = 0;

            try
            {
                if (WTSQuerySessionInformation(WTS_CURRENT_SERVER_HANDLE, sessionId, infoClass, out buffer, out bytesReturned))
                {
                    return Marshal.PtrToStringAnsi(buffer);
                }
            }
            catch { }
            finally
            {
                if (buffer != IntPtr.Zero)
                {
                    WTSFreeMemory(buffer);
                }
            }

            return null;
        }

        /// <summary>
        /// Check if running as a Windows Service
        /// </summary>
        private bool _isRunningAsService
        {
            get
            {
                try
                {
                    // Services run in session 0
                    return Process.GetCurrentProcess().SessionId == 0;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Get system idle time
        /// </summary>
        public TimeSpan GetIdleTime()
        {
            try
            {
                LASTINPUTINFO lastInput = new LASTINPUTINFO();
                lastInput.cbSize = (uint)Marshal.SizeOf(lastInput);

                if (GetLastInputInfo(ref lastInput))
                {
                    uint ticksSinceLastInput = unchecked((uint)Environment.TickCount - lastInput.dwTime);
                    return TimeSpan.FromMilliseconds(ticksSinceLastInput);
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("Error getting idle time", ex);
            }

            return TimeSpan.Zero;
        }

        /// <summary>
        /// Check if screen saver is running
        /// </summary>
        public bool IsScreenSaverRunning()
        {
            try
            {
                bool isRunning = false;
                SystemParametersInfo(SPI_GETSCREENSAVERRUNNING, 0, ref isRunning, 0);
                return isRunning;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if workstation is locked
        /// </summary>
        public bool IsWorkstationLocked()
        {
            try
            {
                // Check if the foreground window is the lock screen
                IntPtr hwnd = GetForegroundWindow();
                if (hwnd == IntPtr.Zero)
                    return true; // No foreground window likely means locked

                var sb = new System.Text.StringBuilder(256);
                GetWindowText(hwnd, sb, sb.Capacity);
                string windowTitle = sb.ToString().ToLowerInvariant();

                // Common lock screen indicators
                if (windowTitle.Contains("windows logon") ||
                    windowTitle.Contains("lock screen") ||
                    windowTitle.Contains("windows security"))
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get system boot time
        /// </summary>
        private DateTime GetSystemBootTime()
        {
            try
            {
                using (var uptime = new PerformanceCounter("System", "System Up Time"))
                {
                    uptime.NextValue();
                    return DateTime.UtcNow.AddSeconds(-uptime.NextValue());
                }
            }
            catch
            {
                // Fallback using Environment.TickCount
                return DateTime.UtcNow.AddMilliseconds(-Environment.TickCount);
            }
        }


        /// <summary>
        /// Track first user activity of the day
        /// </summary>
        private void TrackFirstActivityToday(UserActivityInfo activity)
        {
            // Reset if new day
            if (DateTime.Today != _todayDate)
            {
                _firstActivityToday = null;
                _todayDate = DateTime.Today;
            }

            // Record first activity when user is active
            if (!_firstActivityToday.HasValue &&
                activity.IsUserLoggedIn &&
                !activity.IsIdle &&
                !activity.IsWorkstationLocked)
            {
                // Store as UTC with explicit Kind for proper conversion later
                _firstActivityToday = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
                // Log in local time for readability
                Logger.Info($"First user activity today recorded at {_firstActivityToday.Value.ToLocalTime():HH:mm:ss} (local time)");
            }

            activity.FirstActivityTodayUtc = _firstActivityToday;
        }

        /// <summary>
        /// Get first activity time today
        /// </summary>
        public DateTime? GetFirstActivityToday()
        {
            return _firstActivityToday;
        }

        /// <summary>
        /// Get duration since first activity today
        /// </summary>
        public TimeSpan GetTimeSinceFirstActivity()
        {
            if (!_firstActivityToday.HasValue)
                return TimeSpan.Zero;

            return DateTime.UtcNow - _firstActivityToday.Value;
        }

        /// <summary>
        /// Check if current user is administrator
        /// </summary>
        public bool IsUserAdministrator()
        {
            try
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get current activity info (alias for CurrentActivity property)
        /// </summary>
        public UserActivityInfo GetCurrentActivity()
        {
            return CurrentActivity ?? Update();
        }

        /// <summary>
        /// Record first activity of the day
        /// </summary>
        public void RecordFirstActivity()
        {
            if (!_firstActivityToday.HasValue)
            {
                // Store as UTC with explicit Kind for proper conversion later
                _firstActivityToday = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
                Logger.Info($"First user activity recorded at {_firstActivityToday.Value.ToLocalTime():HH:mm:ss} (local time)");
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            // No unmanaged resources to dispose
        }
    }
}
