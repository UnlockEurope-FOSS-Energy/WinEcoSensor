// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2024 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.Threading;
using System.Windows.Forms;
using WinEcoSensor.Common.Utilities;

namespace WinEcoSensor.TrayApp
{
    /// <summary>
    /// Main entry point for the WinEcoSensor tray application.
    /// Manages application lifecycle and single instance enforcement.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// Mutex name for single instance enforcement
        /// </summary>
        private const string MutexName = "WinEcoSensor.TrayApp.SingleInstance";

        /// <summary>
        /// Application entry point
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Ensure single instance
            bool createdNew;
            using (var mutex = new Mutex(true, MutexName, out createdNew))
            {
                if (!createdNew)
                {
                    // Another instance is already running
                    MessageBox.Show(
                        "WinEcoSensor Tray Application is already running.",
                        "WinEcoSensor",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                try
                {
                    // Initialize logging
                    string logPath = System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        "WinEcoSensor", "Logs", "TrayApp.log");
                    Logger.Initialize(logPath, logLevel: 3, logToConsole: false);
                    Logger.Info("WinEcoSensor Tray Application starting...");

                    // Enable visual styles
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    // Set exception handlers
                    Application.ThreadException += OnThreadException;
                    AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

                    // Run application with tray context
                    using (var context = new TrayApplicationContext())
                    {
                        Application.Run(context);
                    }

                    Logger.Info("WinEcoSensor Tray Application exiting normally");
                }
                catch (Exception ex)
                {
                    Logger.Error("Fatal error in tray application", ex);
                    MessageBox.Show(
                        $"A fatal error occurred:\n\n{ex.Message}",
                        "WinEcoSensor Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Handle UI thread exceptions
        /// </summary>
        private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Logger.Error("Unhandled UI thread exception", e.Exception);
            ShowErrorDialog(e.Exception);
        }

        /// <summary>
        /// Handle non-UI thread exceptions
        /// </summary>
        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            Logger.Error("Unhandled domain exception", exception);
            
            if (e.IsTerminating)
            {
                ShowErrorDialog(exception);
            }
        }

        /// <summary>
        /// Display error dialog to user
        /// </summary>
        private static void ShowErrorDialog(Exception ex)
        {
            try
            {
                var message = ex != null 
                    ? $"An unexpected error occurred:\n\n{ex.Message}\n\nPlease check the log file for details."
                    : "An unexpected error occurred. Please check the log file for details.";

                MessageBox.Show(
                    message,
                    "WinEcoSensor Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch
            {
                // Fail silently if we can't even show the error dialog
            }
        }
    }
}
