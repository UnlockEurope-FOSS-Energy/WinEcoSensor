// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2024 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.IO;
using System.Text;

namespace WinEcoSensor.Common.Utilities
{
    /// <summary>
    /// Simple logging utility for WinEcoSensor.
    /// Supports file and console output with configurable log levels.
    /// </summary>
    public static class Logger
    {
        private static readonly object LockObject = new object();
        private static string _logFilePath;
        private static int _logLevel = 3; // Default: Info
        private static int _maxFileSizeMb = 10;
        private static int _retentionCount = 5;
        private static bool _initialized;
        private static bool _logToConsole = true;

        /// <summary>
        /// Log levels
        /// </summary>
        public enum Level
        {
            None = 0,
            Error = 1,
            Warning = 2,
            Info = 3,
            Debug = 4
        }

        /// <summary>
        /// Initialize the logger with configuration (with console option)
        /// </summary>
        public static void Initialize(string logFilePath, int logLevel = 3, int maxFileSizeMb = 10, int retentionCount = 5, bool logToConsole = true)
        {
            lock (LockObject)
            {
                _logFilePath = logFilePath;
                _logLevel = logLevel;
                _maxFileSizeMb = maxFileSizeMb;
                _retentionCount = retentionCount;
                _logToConsole = logToConsole;
                _initialized = true;

                // Ensure log directory exists
                var logDir = Path.GetDirectoryName(_logFilePath);
                if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
            }
        }

        /// <summary>
        /// Set log level
        /// </summary>
        public static void SetLogLevel(int level)
        {
            _logLevel = level;
        }

        /// <summary>
        /// Log an error message
        /// </summary>
        public static void Error(string message, Exception ex = null)
        {
            Log(Level.Error, message, ex);
        }

        /// <summary>
        /// Log a warning message
        /// </summary>
        public static void Warning(string message, Exception ex = null)
        {
            Log(Level.Warning, message, ex);
        }

        /// <summary>
        /// Log an info message
        /// </summary>
        public static void Info(string message)
        {
            Log(Level.Info, message);
        }

        /// <summary>
        /// Log a debug message
        /// </summary>
        public static void Debug(string message, Exception ex = null)
        {
            Log(Level.Debug, message, ex);
        }

        /// <summary>
        /// Log a message with specified level
        /// </summary>
        public static void Log(Level level, string message, Exception ex = null)
        {
            if ((int)level > _logLevel)
                return;

            var sb = new StringBuilder();
            sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            sb.Append(" [");
            sb.Append(level.ToString().ToUpper().PadRight(7));
            sb.Append("] ");
            sb.Append(message);

            if (ex != null)
            {
                sb.AppendLine();
                sb.Append("           Exception: ");
                sb.Append(ex.GetType().Name);
                sb.Append(": ");
                sb.Append(ex.Message);
                
                if (ex.StackTrace != null)
                {
                    sb.AppendLine();
                    sb.Append("           StackTrace: ");
                    sb.Append(ex.StackTrace);
                }

                if (ex.InnerException != null)
                {
                    sb.AppendLine();
                    sb.Append("           InnerException: ");
                    sb.Append(ex.InnerException.Message);
                }
            }

            string logEntry = sb.ToString();

            // Write to console
            WriteToConsole(level, logEntry);

            // Write to file if initialized
            if (_initialized)
            {
                WriteToFile(logEntry);
            }
        }

        /// <summary>
        /// Write log entry to console with color coding
        /// </summary>
        private static void WriteToConsole(Level level, string entry)
        {
            if (!_logToConsole)
                return;

            try
            {
                ConsoleColor originalColor = Console.ForegroundColor;

                switch (level)
                {
                    case Level.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case Level.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case Level.Info:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case Level.Debug:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                }

                Console.WriteLine(entry);
                Console.ForegroundColor = originalColor;
            }
            catch
            {
                // Ignore console errors (may not have console)
            }
        }

        /// <summary>
        /// Write log entry to file with rotation
        /// </summary>
        private static void WriteToFile(string entry)
        {
            lock (LockObject)
            {
                try
                {
                    // Check for log rotation
                    RotateLogIfNeeded();

                    // Append to log file
                    File.AppendAllText(_logFilePath, entry + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    // Fallback to console if file write fails
                    Console.WriteLine($"Failed to write to log file: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Rotate log file if it exceeds maximum size
        /// </summary>
        private static void RotateLogIfNeeded()
        {
            try
            {
                if (!File.Exists(_logFilePath))
                    return;

                var fileInfo = new FileInfo(_logFilePath);
                if (fileInfo.Length < _maxFileSizeMb * 1024 * 1024)
                    return;

                // Rotate existing log files
                for (int i = _retentionCount - 1; i >= 1; i--)
                {
                    string oldPath = $"{_logFilePath}.{i}";
                    string newPath = $"{_logFilePath}.{i + 1}";

                    if (File.Exists(newPath))
                        File.Delete(newPath);

                    if (File.Exists(oldPath))
                        File.Move(oldPath, newPath);
                }

                // Move current log to .1
                string rotatedPath = $"{_logFilePath}.1";
                if (File.Exists(rotatedPath))
                    File.Delete(rotatedPath);

                File.Move(_logFilePath, rotatedPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to rotate log file: {ex.Message}");
            }
        }

        /// <summary>
        /// Flush any pending log entries
        /// </summary>
        public static void Flush()
        {
            // Currently writes are synchronous, but this can be used
            // if async logging is implemented later
        }

        /// <summary>
        /// Get the current log file path
        /// </summary>
        public static string GetLogFilePath()
        {
            return _logFilePath;
        }
    }
}
