// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2024 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.ServiceProcess;
using WinEcoSensor.Common.Utilities;

namespace WinEcoSensor.Service
{
    /// <summary>
    /// Service entry point.
    /// Handles service startup and console mode for debugging.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Main entry point for the service.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        static void Main(string[] args)
        {
            // Check for console mode (debugging)
            bool consoleMode = false;
            foreach (var arg in args)
            {
                if (arg.Equals("-console", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("/console", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("--console", StringComparison.OrdinalIgnoreCase))
                {
                    consoleMode = true;
                    break;
                }
            }

            if (consoleMode || Environment.UserInteractive)
            {
                // Run in console mode for debugging
                RunInConsoleMode();
            }
            else
            {
                // Run as Windows Service
                RunAsService();
            }
        }

        /// <summary>
        /// Run the service in console mode for debugging
        /// </summary>
        private static void RunInConsoleMode()
        {
            Console.WriteLine("============================================");
            Console.WriteLine("WinEcoSensor - Windows Eco Energy Sensor");
            Console.WriteLine("Console Mode (for debugging)");
            Console.WriteLine("============================================");
            Console.WriteLine();

            // Initialize logger for console output
            string logPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "WinEcoSensor", "Logs", "Service.log");
            Logger.Initialize(
                logFilePath: logPath,
                logLevel: 4, // Debug
                logToConsole: true
            );

            Logger.Info("Starting WinEcoSensor in console mode...");

            using (var service = new WinEcoSensorService())
            {
                // Simulate service start
                service.StartService(null);

                Console.WriteLine();
                Console.WriteLine("Service is running. Press any key to stop...");
                Console.ReadKey(true);

                // Simulate service stop
                service.StopService();
            }

            Logger.Info("WinEcoSensor stopped.");
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }

        /// <summary>
        /// Run as Windows Service
        /// </summary>
        private static void RunAsService()
        {
            ServiceBase[] servicesToRun = new ServiceBase[]
            {
                new WinEcoSensorService()
            };

            ServiceBase.Run(servicesToRun);
        }
    }
}
