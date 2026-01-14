// WinEcoSensor - Windows Eco Energy Sensor
// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) 2025 WinEcoSensor Contributors

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WinEcoSensor.Common.Utilities;

namespace WinEcoSensor.Tests
{
    /// <summary>
    /// Unit tests for EnergyCalculator class
    /// </summary>
    [TestClass]
    public class EnergyCalculatorTests
    {
        private EnergyCalculator _calculator;

        [TestInitialize]
        public void Setup()
        {
            _calculator = new EnergyCalculator();
        }

        [TestMethod]
        public void Constructor_SetsDefaultPsuEfficiency()
        {
            // Assert
            Assert.AreEqual(0.85, _calculator.PsuEfficiency, 0.001);
        }

        [TestMethod]
        public void Constructor_InitializesSessionStart()
        {
            // Assert
            Assert.IsTrue(_calculator.SessionStartUtc <= DateTime.UtcNow);
            Assert.IsTrue(_calculator.SessionStartUtc > DateTime.UtcNow.AddMinutes(-1));
        }

        [TestMethod]
        public void Constructor_InitializesEnergyCountersToZero()
        {
            // Assert
            Assert.AreEqual(0, _calculator.SessionEnergyWh);
            Assert.AreEqual(0, _calculator.DailyEnergyWh);
        }

        [TestMethod]
        public void PsuEfficiency_CanBeChanged()
        {
            // Arrange
            double newEfficiency = 0.90;

            // Act
            _calculator.PsuEfficiency = newEfficiency;

            // Assert
            Assert.AreEqual(newEfficiency, _calculator.PsuEfficiency);
        }

        [TestMethod]
        public void StartNewSession_ResetsSessionCounters()
        {
            // Arrange - simulate some energy accumulation
            var beforeStart = _calculator.SessionStartUtc;
            System.Threading.Thread.Sleep(10);

            // Act
            _calculator.StartNewSession();

            // Assert
            Assert.IsTrue(_calculator.SessionStartUtc > beforeStart);
            Assert.AreEqual(0, _calculator.SessionEnergyWh);
        }

        [TestMethod]
        public void Reset_ResetsAllCounters()
        {
            // Act
            _calculator.Reset();

            // Assert
            Assert.AreEqual(0, _calculator.SessionEnergyWh);
            Assert.AreEqual(0, _calculator.DailyEnergyWh);
            Assert.AreEqual(0, _calculator.LastPowerWatts);
        }

        [TestMethod]
        public void GetEstimatedDailyKwh_ConvertsWhToKwh()
        {
            // Act
            double kwh = _calculator.GetEstimatedDailyKwh();

            // Assert
            Assert.AreEqual(0, kwh); // Initially zero
        }

        [TestMethod]
        public void GetEstimatedMonthlyKwh_Calculates22WorkingDays()
        {
            // Act
            double monthly = _calculator.GetEstimatedMonthlyKwh();
            double daily = _calculator.GetEstimatedDailyKwh();

            // Assert
            Assert.AreEqual(daily * 22, monthly, 0.001);
        }

        [TestMethod]
        public void GetEstimatedAnnualKwh_Calculates260WorkingDays()
        {
            // Act
            double annual = _calculator.GetEstimatedAnnualKwh();
            double daily = _calculator.GetEstimatedDailyKwh();

            // Assert
            Assert.AreEqual(daily * 260, annual, 0.001);
        }

        [TestMethod]
        public void CalculateCo2Emissions_UsesDefaultEuAverage()
        {
            // Arrange
            double energyKwh = 100;

            // Act
            double co2 = EnergyCalculator.CalculateCo2Emissions(energyKwh);

            // Assert - EU average is 0.276 kg/kWh
            Assert.AreEqual(27.6, co2, 0.001);
        }

        [TestMethod]
        public void CalculateCo2Emissions_UsesCustomFactor()
        {
            // Arrange
            double energyKwh = 100;
            double customFactor = 0.5;

            // Act
            double co2 = EnergyCalculator.CalculateCo2Emissions(energyKwh, customFactor);

            // Assert
            Assert.AreEqual(50, co2, 0.001);
        }

        [TestMethod]
        public void CalculateEnergyCost_UsesDefaultPrice()
        {
            // Arrange
            double energyKwh = 100;

            // Act
            double cost = EnergyCalculator.CalculateEnergyCost(energyKwh);

            // Assert - Default is 0.25 EUR/kWh
            Assert.AreEqual(25, cost, 0.001);
        }

        [TestMethod]
        public void CalculateEnergyCost_UsesCustomPrice()
        {
            // Arrange
            double energyKwh = 100;
            double customPrice = 0.30;

            // Act
            double cost = EnergyCalculator.CalculateEnergyCost(energyKwh, customPrice);

            // Assert
            Assert.AreEqual(30, cost, 0.001);
        }

        [TestMethod]
        public void CalculateCurrentState_WithNullMonitors_ReturnsValidState()
        {
            // Act
            var state = _calculator.CalculateCurrentState(null, null, null);

            // Assert
            Assert.IsNotNull(state);
            Assert.IsTrue(state.TimestampUtc <= DateTime.UtcNow);
        }

        [TestMethod]
        public void CalculateCurrentState_IncludesBasePower()
        {
            // Act
            var state = _calculator.CalculateCurrentState(null, null, null);

            // Assert - Base system power should be included
            Assert.IsTrue(state.BasePowerWatts > 0);
        }

        [TestMethod]
        public void CalculateCurrentState_AppliesPsuEfficiency()
        {
            // Arrange
            _calculator.PsuEfficiency = 0.80;

            // Act
            var state = _calculator.CalculateCurrentState(null, null, null);

            // Assert
            Assert.AreEqual(0.80, state.PsuEfficiency);
            // Total power should be higher than DC power due to PSU losses
            Assert.IsTrue(state.TotalPowerWatts > state.BasePowerWatts);
        }
    }
}
