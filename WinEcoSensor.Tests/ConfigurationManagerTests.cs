// WinEcoSensor - Windows Eco Energy Sensor
// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) 2025 WinEcoSensor Contributors

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WinEcoSensor.Common.Configuration;
using WinEcoSensor.Common.Models;

namespace WinEcoSensor.Tests
{
    /// <summary>
    /// Unit tests for ConfigurationManager class
    /// </summary>
    [TestClass]
    public class ConfigurationManagerTests
    {
        private ConfigurationManager _manager;
        private string _tempConfigPath;

        [TestInitialize]
        public void Setup()
        {
            _manager = new ConfigurationManager();
            _tempConfigPath = Path.Combine(Path.GetTempPath(), "WinEcoSensorTest_" + Guid.NewGuid() + ".config");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_tempConfigPath))
            {
                File.Delete(_tempConfigPath);
            }
        }

        [TestMethod]
        public void Constructor_CreatesDefaultConfiguration()
        {
            Assert.IsNotNull(_manager.Configuration);
        }

        [TestMethod]
        public void Configuration_DefaultValues_AreSet()
        {
            var config = _manager.Configuration;
            Assert.IsTrue(config.MonitorCpu);
            Assert.IsTrue(config.MonitorDisplays);
            Assert.IsTrue(config.MonitorUserActivity);
        }

        [TestMethod]
        public void IsDirty_InitiallyFalse()
        {
            Assert.IsFalse(_manager.IsDirty);
        }

        [TestMethod]
        public void Configuration_WhenSet_SetsDirtyFlag()
        {
            _manager.Configuration = new SensorConfiguration();

            Assert.IsTrue(_manager.IsDirty);
        }

        [TestMethod]
        public void Load_WhenFileNotExists_ReturnsDefaultConfiguration()
        {
            var config = _manager.Load();

            Assert.IsNotNull(config);
        }

        [TestMethod]
        public void ResetToDefaults_ResetsConfiguration()
        {
            _manager.Configuration.BackendUrl = "http://test.com";
            _manager.Configuration.MonitorCpu = false;

            _manager.ResetToDefaults();

            Assert.IsTrue(_manager.Configuration.MonitorCpu);
            Assert.IsTrue(_manager.IsDirty);
        }

        [TestMethod]
        public void ExportTo_CreatesFile()
        {
            _manager.Configuration.BackendUrl = "http://export-test.com";

            bool result = _manager.ExportTo(_tempConfigPath);

            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(_tempConfigPath));
        }

        [TestMethod]
        public void ExportTo_CreatesValidXml()
        {
            _manager.Configuration.BackendUrl = "http://export-test.com";
            _manager.ExportTo(_tempConfigPath);

            string content = File.ReadAllText(_tempConfigPath);

            Assert.IsTrue(content.Contains("<?xml"));
            Assert.IsTrue(content.Contains("WinEcoSensorConfig"));
            Assert.IsTrue(content.Contains("http://export-test.com"));
        }

        [TestMethod]
        public void ImportFrom_WithValidFile_LoadsConfiguration()
        {
            _manager.Configuration.BackendUrl = "http://original.com";
            _manager.ExportTo(_tempConfigPath);

            var newManager = new ConfigurationManager();

            bool result = newManager.ImportFrom(_tempConfigPath);

            Assert.IsTrue(result);
            Assert.AreEqual("http://original.com", newManager.Configuration.BackendUrl);
        }

        [TestMethod]
        public void ImportFrom_WithNonExistentFile_ReturnsFalse()
        {
            bool result = _manager.ImportFrom("C:\\NonExistent\\Path\\config.xml");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Configuration_EprelMappings_IsInitialized()
        {
            Assert.IsNotNull(_manager.Configuration.EprelMappings);
        }

        [TestMethod]
        public void ExportImport_PreservesEprelMappings()
        {
            var mapping = new EprelMapping();
            mapping.EprelNumber = "123456";
            mapping.ModelName = "Test Monitor";
            mapping.Manufacturer = "Test Corp";
            mapping.EnergyClass = "A";
            mapping.PowerOnWatts = 25.5;
            mapping.HardwareType = EprelHardwareType.Monitor;

            _manager.Configuration.EprelMappings.Add(mapping);
            _manager.ExportTo(_tempConfigPath);

            var newManager = new ConfigurationManager();

            newManager.ImportFrom(_tempConfigPath);

            Assert.AreEqual(1, newManager.Configuration.EprelMappings.Count);
            Assert.AreEqual("123456", newManager.Configuration.EprelMappings[0].EprelNumber);
            Assert.AreEqual("Test Monitor", newManager.Configuration.EprelMappings[0].ModelName);
            Assert.AreEqual(25.5, newManager.Configuration.EprelMappings[0].PowerOnWatts, 0.01);
        }

        [TestMethod]
        public void ExportImport_PreservesAllSettings()
        {
            _manager.Configuration.BackendUrl = "http://test-backend.com";
            _manager.Configuration.StatusIntervalSeconds = 30;
            _manager.Configuration.HeartbeatIntervalSeconds = 120;
            _manager.Configuration.MonitorCpu = false;
            _manager.Configuration.MonitorDisplays = false;
            _manager.Configuration.MonitorRemoteSessions = false;
            _manager.Configuration.IdleThresholdSeconds = 600;
            _manager.Configuration.PsuEfficiency = 0.92;
            _manager.Configuration.LogLevel = "Debug";
            _manager.ExportTo(_tempConfigPath);

            var newManager = new ConfigurationManager();

            newManager.ImportFrom(_tempConfigPath);

            Assert.AreEqual("http://test-backend.com", newManager.Configuration.BackendUrl);
            Assert.AreEqual(30, newManager.Configuration.StatusIntervalSeconds);
            Assert.AreEqual(120, newManager.Configuration.HeartbeatIntervalSeconds);
            Assert.IsFalse(newManager.Configuration.MonitorCpu);
            Assert.IsFalse(newManager.Configuration.MonitorDisplays);
            Assert.IsFalse(newManager.Configuration.MonitorRemoteSessions);
            Assert.AreEqual(0.92, newManager.Configuration.PsuEfficiency, 0.001);
            Assert.AreEqual("Debug", newManager.Configuration.LogLevel);
        }

        [TestMethod]
        public void ConfigPath_ReturnsValidPath()
        {
            Assert.IsFalse(string.IsNullOrEmpty(_manager.ConfigPath));
            Assert.IsTrue(_manager.ConfigPath.EndsWith(".config"));
        }
    }
}
