// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2024 Unlock Europe - FOSS Energy Initiative
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
                activity.DomainName = Environment.UserDomainName;
                activity.LoggedInUser = Environment.UserName;
                activity.IsUserLoggedIn = !string.IsNullOrEmpty(activity.LoggedInUser);

                // Get system boot time
                activity.SystemBootTimeUtc = GetSystemBootTime();

                // Get idle time
                var idleTime = GetIdleTime();
                activity.LastInputActivityUtc = DateTime.UtcNow - idleTime;
                activity.IdleTime = idleTime;
                activity.IsIdle = idleTime.TotalMinutes >= _idleThresholdMinutes;

                // Check screen saver
                activity.IsScreenSaverActive = IsScreenSaverRunning();

                // Check workstation lock
                activity.IsWorkstationLocked = IsWorkstationLocked();

                // Determine session state
                activity.SessionState = DetermineSessionState(activity);

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
        /// Determine session state based on activity info
        /// </summary>
        private SessionState DetermineSessionState(UserActivityInfo activity)
        {
            if (!activity.IsUserLoggedIn)
                return SessionState.Disconnected;

            if (activity.IsWorkstationLocked)
                return SessionState.Idle;

            if (activity.IsScreenSaverActive)
                return SessionState.Idle;

            if (activity.IsIdle)
                return SessionState.Idle;

            return SessionState.Active;
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
                _firstActivityToday = DateTime.UtcNow;
                Logger.Info($"First user activity today recorded at {_firstActivityToday.Value:HH:mm:ss}");
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
                _firstActivityToday = DateTime.UtcNow;
                Logger.Info($"First user activity recorded at {_firstActivityToday.Value:HH:mm:ss}");
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
