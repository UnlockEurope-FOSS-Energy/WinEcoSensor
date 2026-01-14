// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2024 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace WinEcoSensor.Common.Models
{
    [DataContract]
    public class EnergyState
    {
        [DataMember(Name = "stateId")]
        public string StateId { get; set; }

        [DataMember(Name = "machineId")]
        public string MachineId { get; set; }

        [DataMember(Name = "hostname")]
        public string Hostname { get; set; }

        [DataMember(Name = "systemPowerState")]
        public SystemPowerState SystemPowerState { get; set; }

        [DataMember(Name = "cpuUsagePercent")]
        public double CpuUsagePercent { get; set; }

        [DataMember(Name = "cpuPowerWatts")]
        public double CpuPowerWatts { get; set; }

        [DataMember(Name = "memoryUsagePercent")]
        public double MemoryUsagePercent { get; set; }

        [DataMember(Name = "memoryPowerWatts")]
        public double MemoryPowerWatts { get; set; }

        [DataMember(Name = "monitorPowerWatts")]
        public double MonitorPowerWatts { get; set; }

        [DataMember(Name = "activeMonitorCount")]
        public int ActiveMonitorCount { get; set; }

        [DataMember(Name = "diskPowerWatts")]
        public double DiskPowerWatts { get; set; }

        [DataMember(Name = "gpuPowerWatts")]
        public double GpuPowerWatts { get; set; }

        [DataMember(Name = "systemBasePowerWatts")]
        public double SystemBasePowerWatts { get; set; }

        [DataMember(Name = "peripheralPowerWatts")]
        public double PeripheralPowerWatts { get; set; }

        [DataMember(Name = "totalPowerWatts")]
        public double TotalPowerWatts { get; set; }

        [DataMember(Name = "psuEfficiency")]
        public double PsuEfficiency { get; set; }

        [DataMember(Name = "wallPowerWatts")]
        public double WallPowerWatts { get; set; }

        [DataMember(Name = "energyTodayWh")]
        public double EnergyTodayWh { get; set; }

        [DataMember(Name = "energySessionWh")]
        public double EnergySessionWh { get; set; }

        [DataMember(Name = "estimatedMonthlyKwh")]
        public double EstimatedMonthlyKwh { get; set; }

        [DataMember(Name = "powerBreakdown")]
        public Dictionary<string, double> PowerBreakdown { get; set; }

        [DataMember(Name = "activityState")]
        public DisplayActivityState ActivityState { get; set; }

        [DataMember(Name = "efficiencyRating")]
        public EnergyEfficiencyRating EfficiencyRating { get; set; }

        [DataMember(Name = "currentStateDuration")]
        public TimeSpan CurrentStateDuration { get; set; }

        [DataMember(Name = "activeTimeToday")]
        public TimeSpan ActiveTimeToday { get; set; }

        [DataMember(Name = "idleTimeToday")]
        public TimeSpan IdleTimeToday { get; set; }

        [DataMember(Name = "displayOffTimeToday")]
        public TimeSpan DisplayOffTimeToday { get; set; }

        [DataMember(Name = "timestampUtc")]
        public DateTime TimestampUtc { get; set; }

        [DataMember(Name = "previousStateTimestampUtc")]
        public DateTime PreviousStateTimestampUtc { get; set; }

        public EnergyState()
        {
            StateId = Guid.NewGuid().ToString();
            TimestampUtc = DateTime.UtcNow;
            PowerBreakdown = new Dictionary<string, double>();
            PsuEfficiency = 0.85;
            SystemBasePowerWatts = 15;
            PeripheralPowerWatts = 5;
        }

        // Backward-compatible alias properties
        public double DisplayPowerWatts
        {
            get { return MonitorPowerWatts; }
            set { MonitorPowerWatts = value; }
        }

        public double BasePowerWatts
        {
            get { return SystemBasePowerWatts; }
            set { SystemBasePowerWatts = value; }
        }

        public double DailyEnergyWh
        {
            get { return EnergyTodayWh; }
            set { EnergyTodayWh = value; }
        }

        public double SessionEnergyWh
        {
            get { return EnergySessionWh; }
            set { EnergySessionWh = value; }
        }

        public TimeSpan SessionDuration
        {
            get { return CurrentStateDuration; }
            set { CurrentStateDuration = value; }
        }

        public void CalculateTotalPower()
        {
            TotalPowerWatts = CpuPowerWatts + MemoryPowerWatts + MonitorPowerWatts +
                             DiskPowerWatts + GpuPowerWatts + SystemBasePowerWatts + PeripheralPowerWatts;

            WallPowerWatts = PsuEfficiency > 0 ? TotalPowerWatts / PsuEfficiency : TotalPowerWatts;

            PowerBreakdown["CPU"] = CpuPowerWatts;
            PowerBreakdown["Memory"] = MemoryPowerWatts;
            PowerBreakdown["Monitors"] = MonitorPowerWatts;
            PowerBreakdown["Storage"] = DiskPowerWatts;
            PowerBreakdown["GPU"] = GpuPowerWatts;
            PowerBreakdown["System"] = SystemBasePowerWatts;
            PowerBreakdown["Peripherals"] = PeripheralPowerWatts;

            CalculateEfficiencyRating();
        }

        private void CalculateEfficiencyRating()
        {
            if (ActivityState == DisplayActivityState.Off)
            {
                EfficiencyRating = TotalPowerWatts < 30 ? EnergyEfficiencyRating.Excellent :
                                   TotalPowerWatts < 50 ? EnergyEfficiencyRating.Good : EnergyEfficiencyRating.NeedsImprovement;
            }
            else if (ActivityState == DisplayActivityState.Idle)
            {
                EfficiencyRating = TotalPowerWatts < 50 ? EnergyEfficiencyRating.Excellent :
                                   TotalPowerWatts < 80 ? EnergyEfficiencyRating.Good :
                                   TotalPowerWatts < 120 ? EnergyEfficiencyRating.Average : EnergyEfficiencyRating.NeedsImprovement;
            }
            else
            {
                double powerPerCpuPercent = CpuUsagePercent > 0 ? CpuPowerWatts / CpuUsagePercent : 0;
                EfficiencyRating = powerPerCpuPercent < 0.5 ? EnergyEfficiencyRating.Excellent :
                                   powerPerCpuPercent < 1.0 ? EnergyEfficiencyRating.Good :
                                   powerPerCpuPercent < 2.0 ? EnergyEfficiencyRating.Average : EnergyEfficiencyRating.NeedsImprovement;
            }
        }

        public void UpdateEnergyCounters(double durationHours)
        {
            double energyIncrementWh = WallPowerWatts * durationHours;
            EnergyTodayWh += energyIncrementWh;
            EnergySessionWh += energyIncrementWh;

            double hoursToday = (DateTime.UtcNow - DateTime.UtcNow.Date).TotalHours;
            if (hoursToday > 1)
            {
                double dailyKwh = EnergyTodayWh / 1000.0 / hoursToday * 24;
                EstimatedMonthlyKwh = dailyKwh * 30;
            }
        }
    }

    public enum SystemPowerState { Unknown = 0, Running = 1, Idle = 2, Standby = 3, Hibernate = 4, Shutdown = 5 }
    public enum EnergyEfficiencyRating { Unknown = 0, Excellent = 1, Good = 2, Average = 3, NeedsImprovement = 4 }
}
