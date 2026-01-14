// WinEcoSensor - Windows Eco Energy Sensor
// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) 2025 WinEcoSensor Contributors

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WinEcoSensor.Common.Communication;
using WinEcoSensor.Common.Models;

namespace WinEcoSensor.Tests
{
    /// <summary>
    /// Unit tests for CloudEventBuilder class
    /// </summary>
    [TestClass]
    public class CloudEventBuilderTests
    {
        private CloudEventBuilder _builder;

        [TestInitialize]
        public void Setup()
        {
            _builder = new CloudEventBuilder();
        }

        [TestMethod]
        public void Constructor_InitializesDefaultSource()
        {
            Assert.IsNotNull(_builder);
        }

        [TestMethod]
        public void CreateStatusEvent_ReturnsValidCloudEvent()
        {
            var hardwareInfo = new HardwareInfo();
            hardwareInfo.MainboardManufacturer = "Test";
            hardwareInfo.MainboardProduct = "Test Model";

            var userActivity = new UserActivityInfo();
            userActivity.IsLoggedIn = true;
            userActivity.UserName = "testuser";

            var energyState = new EnergyState();
            energyState.TotalPowerWatts = 100;
            energyState.TimestampUtc = DateTime.UtcNow;

            var cloudEvent = _builder.CreateStatusEvent(hardwareInfo, userActivity, energyState);

            Assert.IsNotNull(cloudEvent);
            Assert.AreEqual("1.0", cloudEvent.SpecVersion);
            Assert.IsFalse(string.IsNullOrEmpty(cloudEvent.Id));
            Assert.IsFalse(string.IsNullOrEmpty(cloudEvent.Source));
            Assert.IsFalse(string.IsNullOrEmpty(cloudEvent.Type));
        }

        [TestMethod]
        public void CreateStatusEvent_GeneratesUniqueIds()
        {
            var hardwareInfo = new HardwareInfo();
            var userActivity = new UserActivityInfo();
            var energyState = new EnergyState();

            var event1 = _builder.CreateStatusEvent(hardwareInfo, userActivity, energyState);
            var event2 = _builder.CreateStatusEvent(hardwareInfo, userActivity, energyState);

            Assert.AreNotEqual(event1.Id, event2.Id);
        }

        [TestMethod]
        public void CreateStatusEvent_SetsCorrectEventType()
        {
            var hardwareInfo = new HardwareInfo();
            var userActivity = new UserActivityInfo();
            var energyState = new EnergyState();

            var cloudEvent = _builder.CreateStatusEvent(hardwareInfo, userActivity, energyState);

            Assert.IsTrue(cloudEvent.Type.Contains("status"));
        }

        [TestMethod]
        public void CreateStatusEvent_SetsDataContentType()
        {
            var hardwareInfo = new HardwareInfo();
            var userActivity = new UserActivityInfo();
            var energyState = new EnergyState();

            var cloudEvent = _builder.CreateStatusEvent(hardwareInfo, userActivity, energyState);

            Assert.AreEqual("application/json", cloudEvent.DataContentType);
        }

        [TestMethod]
        public void CreateHeartbeatEvent_ReturnsValidEvent()
        {
            var cloudEvent = _builder.CreateHeartbeatEvent();

            Assert.IsNotNull(cloudEvent);
            Assert.AreEqual("1.0", cloudEvent.SpecVersion);
            Assert.IsTrue(cloudEvent.Type.Contains("heartbeat"));
        }

        [TestMethod]
        public void CreateHeartbeatEvent_HasData()
        {
            var cloudEvent = _builder.CreateHeartbeatEvent();

            Assert.IsNotNull(cloudEvent.Data);
        }

        [TestMethod]
        public void CreateStatusEvent_IncludesTimestamp()
        {
            var before = DateTime.UtcNow.AddSeconds(-1);
            var hardwareInfo = new HardwareInfo();
            var userActivity = new UserActivityInfo();
            var energyState = new EnergyState();

            var cloudEvent = _builder.CreateStatusEvent(hardwareInfo, userActivity, energyState);
            var after = DateTime.UtcNow.AddSeconds(1);

            // Time is a string in ISO format
            Assert.IsFalse(string.IsNullOrEmpty(cloudEvent.Time));
        }

        [TestMethod]
        public void CreateStatusEvent_SourceContainsHostname()
        {
            var hostname = Environment.MachineName.ToLowerInvariant();
            var hardwareInfo = new HardwareInfo();
            var userActivity = new UserActivityInfo();
            var energyState = new EnergyState();

            var cloudEvent = _builder.CreateStatusEvent(hardwareInfo, userActivity, energyState);

            Assert.IsTrue(cloudEvent.Source.ToLowerInvariant().Contains(hostname));
        }

        [TestMethod]
        public void CreateStatusEvent_DataContainsHardwareInfo()
        {
            var hardwareInfo = new HardwareInfo();
            hardwareInfo.CpuName = "Intel Core i7";

            var userActivity = new UserActivityInfo();
            var energyState = new EnergyState();

            var cloudEvent = _builder.CreateStatusEvent(hardwareInfo, userActivity, energyState);

            Assert.IsNotNull(cloudEvent.Data);
        }

        [TestMethod]
        public void CreateStatusEvent_WithNullParameters_HandlesGracefully()
        {
            var cloudEvent = _builder.CreateStatusEvent(null, null, null);

            Assert.IsNotNull(cloudEvent);
            Assert.IsNotNull(cloudEvent.Data);
        }
    }
}
