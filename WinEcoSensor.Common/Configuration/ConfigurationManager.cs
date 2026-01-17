// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2026 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Win32;
using WinEcoSensor.Common.Models;
using WinEcoSensor.Common.Utilities;

namespace WinEcoSensor.Common.Configuration
{
    /// <summary>
    /// Manages loading and saving of WinEcoSensor configuration.
    /// Stores settings in XML file and registry for autostart.
    /// </summary>
    public class ConfigurationManager
    {
        // Configuration file name
        private const string ConfigFileName = "WinEcoSensor.config";

        // Registry path for autostart
        private const string AutostartRegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private const string AutostartValueName = "WinEcoSensor";

        // Application paths
        private static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "WinEcoSensor");

        private static readonly string ConfigFilePath = Path.Combine(AppDataPath, ConfigFileName);

        private SensorConfiguration _configuration;
        private bool _isDirty;

        /// <summary>
        /// Current configuration
        /// </summary>
        public SensorConfiguration Configuration
        {
            get => _configuration;
            set
            {
                _configuration = value;
                _isDirty = true;
            }
        }

        /// <summary>
        /// Whether configuration has unsaved changes
        /// </summary>
        public bool IsDirty => _isDirty;

        /// <summary>
        /// Configuration file path
        /// </summary>
        public string ConfigPath => ConfigFilePath;

        /// <summary>
        /// Event raised when configuration changes
        /// </summary>
        public event EventHandler ConfigurationChanged;

        /// <summary>
        /// Create new configuration manager
        /// </summary>
        public ConfigurationManager()
        {
            _configuration = new SensorConfiguration();
        }

        /// <summary>
        /// Load configuration from file
        /// </summary>
        /// <returns>Loaded configuration</returns>
        public SensorConfiguration Load()
        {
            try
            {
                if (!File.Exists(ConfigFilePath))
                {
                    Logger.Info($"Configuration file not found, using defaults: {ConfigFilePath}");
                    _configuration = new SensorConfiguration();
                    Save(); // Create default config
                    return _configuration;
                }

                var doc = new XmlDocument();
                doc.Load(ConfigFilePath);

                _configuration = ParseConfiguration(doc);
                _isDirty = false;

                Logger.Info($"Configuration loaded from: {ConfigFilePath}");
                return _configuration;
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading configuration", ex);
                _configuration = new SensorConfiguration();
                return _configuration;
            }
        }

        /// <summary>
        /// Save configuration to file
        /// </summary>
        /// <returns>True if saved successfully</returns>
        public bool Save()
        {
            try
            {
                // Ensure directory exists
                if (!Directory.Exists(AppDataPath))
                {
                    Directory.CreateDirectory(AppDataPath);
                }

                var doc = CreateConfigurationXml(_configuration);
                doc.Save(ConfigFilePath);

                // Update autostart setting in registry
                UpdateAutostartRegistry(_configuration.AutoStart);

                _isDirty = false;
                Logger.Info($"Configuration saved to: {ConfigFilePath}");

                ConfigurationChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Error saving configuration", ex);
                return false;
            }
        }

        /// <summary>
        /// Parse configuration from XML document
        /// </summary>
        private SensorConfiguration ParseConfiguration(XmlDocument doc)
        {
            var config = new SensorConfiguration();

            try
            {
                var root = doc.DocumentElement;
                if (root == null) return config;

                // Backend settings
                var backendNode = root.SelectSingleNode("Backend");
                if (backendNode != null)
                {
                    config.BackendUrl = GetNodeValue(backendNode, "Url", config.BackendUrl);
                    config.StatusIntervalSeconds = GetNodeValueInt(backendNode, "StatusIntervalSeconds", config.StatusIntervalSeconds);
                    config.HeartbeatIntervalSeconds = GetNodeValueInt(backendNode, "HeartbeatIntervalSeconds", config.HeartbeatIntervalSeconds);
                }

                // Monitoring settings
                var monitoringNode = root.SelectSingleNode("Monitoring");
                if (monitoringNode != null)
                {
                    config.MonitorCpu = GetNodeValueBool(monitoringNode, "MonitorCpu", config.MonitorCpu);
                    config.MonitorDisplays = GetNodeValueBool(monitoringNode, "MonitorDisplays", config.MonitorDisplays);
                    config.MonitorDisks = GetNodeValueBool(monitoringNode, "MonitorDisks", config.MonitorDisks);
                    config.MonitorUserActivity = GetNodeValueBool(monitoringNode, "MonitorUserActivity", config.MonitorUserActivity);
                    config.MonitorRemoteSessions = GetNodeValueBool(monitoringNode, "MonitorRemoteSessions", config.MonitorRemoteSessions);
                    config.IdleThresholdSeconds = GetNodeValueInt(monitoringNode, "IdleThresholdSeconds", config.IdleThresholdSeconds);
                }

                // Energy settings
                var energyNode = root.SelectSingleNode("Energy");
                if (energyNode != null)
                {
                    config.PsuEfficiency = GetNodeValueDouble(energyNode, "PsuEfficiency", config.PsuEfficiency);
                }

                // General settings
                var generalNode = root.SelectSingleNode("General");
                if (generalNode != null)
                {
                    config.AutoStart = GetNodeValueBool(generalNode, "AutoStart", config.AutoStart);
                    config.LogLevel = GetNodeValue(generalNode, "LogLevel", config.LogLevel);
                    config.LogToFile = GetNodeValueBool(generalNode, "LogToFile", config.LogToFile);
                }

                // EPREL mappings
                var eprelNode = root.SelectSingleNode("EprelMappings");
                if (eprelNode != null)
                {
                    config.EprelMappings.Clear();
                    foreach (XmlNode mappingNode in eprelNode.SelectNodes("Mapping"))
                    {
                        var mapping = ParseEprelMapping(mappingNode);
                        if (mapping != null)
                        {
                            config.EprelMappings.Add(mapping);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error parsing configuration", ex);
            }

            return config;
        }

        /// <summary>
        /// Create XML document from configuration
        /// </summary>
        private XmlDocument CreateConfigurationXml(SensorConfiguration config)
        {
            var doc = new XmlDocument();
            var declaration = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(declaration);

            var root = doc.CreateElement("WinEcoSensorConfig");
            root.SetAttribute("version", "1.0");
            doc.AppendChild(root);

            // Backend settings
            var backendNode = doc.CreateElement("Backend");
            AddElement(doc, backendNode, "Url", config.BackendUrl);
            AddElement(doc, backendNode, "StatusIntervalSeconds", config.StatusIntervalSeconds.ToString());
            AddElement(doc, backendNode, "HeartbeatIntervalSeconds", config.HeartbeatIntervalSeconds.ToString());
            root.AppendChild(backendNode);

            // Monitoring settings
            var monitoringNode = doc.CreateElement("Monitoring");
            AddElement(doc, monitoringNode, "MonitorCpu", config.MonitorCpu.ToString().ToLower());
            AddElement(doc, monitoringNode, "MonitorDisplays", config.MonitorDisplays.ToString().ToLower());
            AddElement(doc, monitoringNode, "MonitorDisks", config.MonitorDisks.ToString().ToLower());
            AddElement(doc, monitoringNode, "MonitorUserActivity", config.MonitorUserActivity.ToString().ToLower());
            AddElement(doc, monitoringNode, "MonitorRemoteSessions", config.MonitorRemoteSessions.ToString().ToLower());
            AddElement(doc, monitoringNode, "IdleThresholdSeconds", config.IdleThresholdSeconds.ToString());
            root.AppendChild(monitoringNode);

            // Energy settings
            var energyNode = doc.CreateElement("Energy");
            AddElement(doc, energyNode, "PsuEfficiency", config.PsuEfficiency.ToString(System.Globalization.CultureInfo.InvariantCulture));
            root.AppendChild(energyNode);

            // General settings
            var generalNode = doc.CreateElement("General");
            AddElement(doc, generalNode, "AutoStart", config.AutoStart.ToString().ToLower());
            AddElement(doc, generalNode, "LogLevel", config.LogLevel);
            AddElement(doc, generalNode, "LogToFile", config.LogToFile.ToString().ToLower());
            root.AppendChild(generalNode);

            // EPREL mappings
            var eprelNode = doc.CreateElement("EprelMappings");
            foreach (var mapping in config.EprelMappings)
            {
                var mappingNode = CreateEprelMappingNode(doc, mapping);
                eprelNode.AppendChild(mappingNode);
            }
            root.AppendChild(eprelNode);

            return doc;
        }

        /// <summary>
        /// Parse EPREL mapping from XML node
        /// </summary>
        private EprelMapping ParseEprelMapping(XmlNode node)
        {
            try
            {
                var mapping = new EprelMapping
                {
                    EprelNumber = GetNodeValue(node, "EprelNumber", ""),
                    ModelName = GetNodeValue(node, "ModelName", ""),
                    Manufacturer = GetNodeValue(node, "Manufacturer", ""),
                    EnergyClass = GetNodeValue(node, "EnergyClass", ""),
                    PowerOnWatts = GetNodeValueDouble(node, "PowerOnWatts", 0),
                    PowerStandbyWatts = GetNodeValueDouble(node, "PowerStandbyWatts", 0),
                    PowerOffWatts = GetNodeValueDouble(node, "PowerOffWatts", 0),
                    AnnualEnergyKwh = GetNodeValueDouble(node, "AnnualEnergyKwh", 0)
                };

                var hardwareType = GetNodeValue(node, "HardwareType", "Monitor");
                mapping.HardwareType = (EprelHardwareType)Enum.Parse(typeof(EprelHardwareType), hardwareType, true);

                return mapping;
            }
            catch (Exception ex)
            {
                Logger.Warning($"Error parsing EPREL mapping: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create EPREL mapping XML node
        /// </summary>
        private XmlElement CreateEprelMappingNode(XmlDocument doc, EprelMapping mapping)
        {
            var node = doc.CreateElement("Mapping");
            AddElement(doc, node, "EprelNumber", mapping.EprelNumber);
            AddElement(doc, node, "ModelName", mapping.ModelName);
            AddElement(doc, node, "Manufacturer", mapping.Manufacturer);
            AddElement(doc, node, "HardwareType", mapping.HardwareType.ToString());
            AddElement(doc, node, "EnergyClass", mapping.EnergyClass);
            AddElement(doc, node, "PowerOnWatts", mapping.PowerOnWatts.ToString(System.Globalization.CultureInfo.InvariantCulture));
            AddElement(doc, node, "PowerStandbyWatts", mapping.PowerStandbyWatts.ToString(System.Globalization.CultureInfo.InvariantCulture));
            AddElement(doc, node, "PowerOffWatts", mapping.PowerOffWatts.ToString(System.Globalization.CultureInfo.InvariantCulture));
            AddElement(doc, node, "AnnualEnergyKwh", mapping.AnnualEnergyKwh.ToString(System.Globalization.CultureInfo.InvariantCulture));
            return node;
        }

        /// <summary>
        /// Update autostart registry entry
        /// </summary>
        private void UpdateAutostartRegistry(bool enable)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(AutostartRegistryPath, true))
                {
                    if (key == null)
                    {
                        Logger.Warning("Could not open autostart registry key");
                        return;
                    }

                    if (enable)
                    {
                        // Get tray app path
                        string exePath = GetTrayAppPath();
                        if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                        {
                            key.SetValue(AutostartValueName, $"\"{exePath}\"");
                            Logger.Info("Autostart enabled in registry");
                        }
                    }
                    else
                    {
                        if (key.GetValue(AutostartValueName) != null)
                        {
                            key.DeleteValue(AutostartValueName, false);
                            Logger.Info("Autostart disabled in registry");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"Could not update autostart registry: {ex.Message}");
            }
        }

        /// <summary>
        /// Get path to tray application
        /// </summary>
        private string GetTrayAppPath()
        {
            // Try to find in program files
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string path = Path.Combine(programFiles, "WinEcoSensor", "WinEcoSensor.TrayApp.exe");
            if (File.Exists(path)) return path;

            // Try current directory
            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WinEcoSensor.TrayApp.exe");
            if (File.Exists(path)) return path;

            return null;
        }

        /// <summary>
        /// Check if autostart is enabled in registry
        /// </summary>
        public bool IsAutostartEnabled()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(AutostartRegistryPath, false))
                {
                    if (key == null) return false;
                    return key.GetValue(AutostartValueName) != null;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Add XML element with text content
        /// </summary>
        private void AddElement(XmlDocument doc, XmlElement parent, string name, string value)
        {
            var element = doc.CreateElement(name);
            element.InnerText = value ?? "";
            parent.AppendChild(element);
        }

        /// <summary>
        /// Get string value from child node
        /// </summary>
        private string GetNodeValue(XmlNode parent, string name, string defaultValue)
        {
            var node = parent.SelectSingleNode(name);
            return node?.InnerText ?? defaultValue;
        }

        /// <summary>
        /// Get integer value from child node
        /// </summary>
        private int GetNodeValueInt(XmlNode parent, string name, int defaultValue)
        {
            var text = GetNodeValue(parent, name, null);
            if (string.IsNullOrEmpty(text)) return defaultValue;
            return int.TryParse(text, out int value) ? value : defaultValue;
        }

        /// <summary>
        /// Get double value from child node
        /// </summary>
        private double GetNodeValueDouble(XmlNode parent, string name, double defaultValue)
        {
            var text = GetNodeValue(parent, name, null);
            if (string.IsNullOrEmpty(text)) return defaultValue;
            return double.TryParse(text, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double value) ? value : defaultValue;
        }

        /// <summary>
        /// Get boolean value from child node
        /// </summary>
        private bool GetNodeValueBool(XmlNode parent, string name, bool defaultValue)
        {
            var text = GetNodeValue(parent, name, null);
            if (string.IsNullOrEmpty(text)) return defaultValue;
            return bool.TryParse(text, out bool value) ? value : defaultValue;
        }

        /// <summary>
        /// Export configuration to specified path
        /// </summary>
        public bool ExportTo(string path)
        {
            try
            {
                var doc = CreateConfigurationXml(_configuration);
                doc.Save(path);
                Logger.Info($"Configuration exported to: {path}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Error exporting configuration", ex);
                return false;
            }
        }

        /// <summary>
        /// Import configuration from specified path
        /// </summary>
        public bool ImportFrom(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    Logger.Warning($"Configuration file not found: {path}");
                    return false;
                }

                var doc = new XmlDocument();
                doc.Load(path);
                _configuration = ParseConfiguration(doc);
                _isDirty = true;

                Logger.Info($"Configuration imported from: {path}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Error importing configuration", ex);
                return false;
            }
        }

        /// <summary>
        /// Reset configuration to defaults
        /// </summary>
        public void ResetToDefaults()
        {
            _configuration = new SensorConfiguration();
            _isDirty = true;
            Logger.Info("Configuration reset to defaults");
        }
    }
}
