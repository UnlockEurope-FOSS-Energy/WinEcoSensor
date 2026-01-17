// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2026 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System.Drawing;

namespace WinEcoSensor.TrayApp
{
    partial class StatusForm
    {
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            // Dark theme colors
            Color bgColor = Color.FromArgb(30, 30, 30);
            Color panelBgColor = Color.FromArgb(40, 40, 40);
            Color labelColor = Color.FromArgb(200, 200, 200);
            Color valueColor = Color.White;
            Color accentColor = Color.FromArgb(0, 120, 215);

            this.grpService = new System.Windows.Forms.GroupBox();
            this.lblServiceStatusLabel = new System.Windows.Forms.Label();
            this.lblServiceStatus = new System.Windows.Forms.Label();
            this.grpActivity = new System.Windows.Forms.GroupBox();
            this.lblLoginStatusLabel = new System.Windows.Forms.Label();
            this.lblLoginStatus = new System.Windows.Forms.Label();
            this.lblUserNameLabel = new System.Windows.Forms.Label();
            this.lblUserName = new System.Windows.Forms.Label();
            this.lblSessionStateLabel = new System.Windows.Forms.Label();
            this.lblSessionState = new System.Windows.Forms.Label();
            this.lblFirstActivityLabel = new System.Windows.Forms.Label();
            this.lblFirstActivity = new System.Windows.Forms.Label();
            this.lblDurationLabel = new System.Windows.Forms.Label();
            this.lblDuration = new System.Windows.Forms.Label();
            this.lblIdleTimeLabel = new System.Windows.Forms.Label();
            this.lblIdleTime = new System.Windows.Forms.Label();
            this.lblScreenSaverLabel = new System.Windows.Forms.Label();
            this.lblScreenSaver = new System.Windows.Forms.Label();
            this.lblWorkstationLockedLabel = new System.Windows.Forms.Label();
            this.lblWorkstationLocked = new System.Windows.Forms.Label();
            this.grpDisplay = new System.Windows.Forms.GroupBox();
            this.lblMonitorCountLabel = new System.Windows.Forms.Label();
            this.lblMonitorCount = new System.Windows.Forms.Label();
            this.lblDisplayStateLabel = new System.Windows.Forms.Label();
            this.lblDisplayState = new System.Windows.Forms.Label();
            this.lblStateDurationLabel = new System.Windows.Forms.Label();
            this.lblStateDuration = new System.Windows.Forms.Label();
            this.lblOnTimeLabel = new System.Windows.Forms.Label();
            this.lblOnTime = new System.Windows.Forms.Label();
            this.lblOffTimeLabel = new System.Windows.Forms.Label();
            this.lblOffTime = new System.Windows.Forms.Label();
            this.lblIdleDisplayTimeLabel = new System.Windows.Forms.Label();
            this.lblIdleDisplayTime = new System.Windows.Forms.Label();
            this.lblDisplayPowerLabel = new System.Windows.Forms.Label();
            this.lblDisplayPower = new System.Windows.Forms.Label();
            this.grpEnergy = new System.Windows.Forms.GroupBox();
            this.lblCpuUsageLabel = new System.Windows.Forms.Label();
            this.lblCpuUsage = new System.Windows.Forms.Label();
            this.progressCpu = new System.Windows.Forms.ProgressBar();
            this.lblCpuPowerLabel = new System.Windows.Forms.Label();
            this.lblCpuPower = new System.Windows.Forms.Label();
            this.lblTotalPowerLabel = new System.Windows.Forms.Label();
            this.lblTotalPower = new System.Windows.Forms.Label();
            this.lblEnergyTodayLabel = new System.Windows.Forms.Label();
            this.lblEnergyToday = new System.Windows.Forms.Label();
            this.lblCostEstimateLabel = new System.Windows.Forms.Label();
            this.lblCostEstimate = new System.Windows.Forms.Label();
            this.grpRemote = new System.Windows.Forms.GroupBox();
            this.lblRdpStatusLabel = new System.Windows.Forms.Label();
            this.lblRdpStatus = new System.Windows.Forms.Label();
            this.lblRdpClientLabel = new System.Windows.Forms.Label();
            this.lblRdpClient = new System.Windows.Forms.Label();
            this.lblTeamViewerLabel = new System.Windows.Forms.Label();
            this.lblTeamViewerStatus = new System.Windows.Forms.Label();
            this.lblAnyDeskLabel = new System.Windows.Forms.Label();
            this.lblAnyDeskStatus = new System.Windows.Forms.Label();
            this.lblOtherRemoteLabel = new System.Windows.Forms.Label();
            this.lblOtherRemote = new System.Windows.Forms.Label();
            this.grpService.SuspendLayout();
            this.grpActivity.SuspendLayout();
            this.grpDisplay.SuspendLayout();
            this.grpEnergy.SuspendLayout();
            this.grpRemote.SuspendLayout();
            this.SuspendLayout();
            //
            // grpService
            //
            this.grpService.Controls.Add(this.lblServiceStatusLabel);
            this.grpService.Controls.Add(this.lblServiceStatus);
            this.grpService.Location = new System.Drawing.Point(12, 12);
            this.grpService.Name = "grpService";
            this.grpService.Size = new System.Drawing.Size(280, 50);
            this.grpService.TabIndex = 0;
            this.grpService.TabStop = false;
            this.grpService.Text = "Service";
            this.grpService.ForeColor = accentColor;
            this.grpService.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            //
            // lblServiceStatusLabel
            //
            this.lblServiceStatusLabel.AutoSize = true;
            this.lblServiceStatusLabel.Location = new System.Drawing.Point(10, 22);
            this.lblServiceStatusLabel.Name = "lblServiceStatusLabel";
            this.lblServiceStatusLabel.Size = new System.Drawing.Size(40, 13);
            this.lblServiceStatusLabel.TabIndex = 0;
            this.lblServiceStatusLabel.Text = "Status:";
            this.lblServiceStatusLabel.ForeColor = labelColor;
            this.lblServiceStatusLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblServiceStatus
            //
            this.lblServiceStatus.AutoSize = true;
            this.lblServiceStatus.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.lblServiceStatus.Location = new System.Drawing.Point(100, 22);
            this.lblServiceStatus.Name = "lblServiceStatus";
            this.lblServiceStatus.Size = new System.Drawing.Size(70, 13);
            this.lblServiceStatus.TabIndex = 1;
            this.lblServiceStatus.Text = "Checking...";
            this.lblServiceStatus.ForeColor = valueColor;
            //
            // grpActivity
            //
            this.grpActivity.Controls.Add(this.lblLoginStatusLabel);
            this.grpActivity.Controls.Add(this.lblLoginStatus);
            this.grpActivity.Controls.Add(this.lblUserNameLabel);
            this.grpActivity.Controls.Add(this.lblUserName);
            this.grpActivity.Controls.Add(this.lblSessionStateLabel);
            this.grpActivity.Controls.Add(this.lblSessionState);
            this.grpActivity.Controls.Add(this.lblFirstActivityLabel);
            this.grpActivity.Controls.Add(this.lblFirstActivity);
            this.grpActivity.Controls.Add(this.lblDurationLabel);
            this.grpActivity.Controls.Add(this.lblDuration);
            this.grpActivity.Controls.Add(this.lblIdleTimeLabel);
            this.grpActivity.Controls.Add(this.lblIdleTime);
            this.grpActivity.Controls.Add(this.lblScreenSaverLabel);
            this.grpActivity.Controls.Add(this.lblScreenSaver);
            this.grpActivity.Controls.Add(this.lblWorkstationLockedLabel);
            this.grpActivity.Controls.Add(this.lblWorkstationLocked);
            this.grpActivity.Location = new System.Drawing.Point(12, 68);
            this.grpActivity.Name = "grpActivity";
            this.grpActivity.Size = new System.Drawing.Size(280, 215);
            this.grpActivity.TabIndex = 1;
            this.grpActivity.TabStop = false;
            this.grpActivity.Text = "User Activity";
            this.grpActivity.ForeColor = accentColor;
            this.grpActivity.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            //
            // lblLoginStatusLabel
            //
            this.lblLoginStatusLabel.AutoSize = true;
            this.lblLoginStatusLabel.Location = new System.Drawing.Point(10, 25);
            this.lblLoginStatusLabel.Name = "lblLoginStatusLabel";
            this.lblLoginStatusLabel.Size = new System.Drawing.Size(70, 13);
            this.lblLoginStatusLabel.TabIndex = 0;
            this.lblLoginStatusLabel.Text = "Login Status:";
            this.lblLoginStatusLabel.ForeColor = labelColor;
            this.lblLoginStatusLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblLoginStatus
            //
            this.lblLoginStatus.AutoSize = true;
            this.lblLoginStatus.Location = new System.Drawing.Point(130, 25);
            this.lblLoginStatus.Name = "lblLoginStatus";
            this.lblLoginStatus.Size = new System.Drawing.Size(16, 13);
            this.lblLoginStatus.TabIndex = 1;
            this.lblLoginStatus.Text = "--";
            this.lblLoginStatus.ForeColor = valueColor;
            this.lblLoginStatus.Font = new Font("Segoe UI", 9f);
            //
            // lblUserNameLabel
            //
            this.lblUserNameLabel.AutoSize = true;
            this.lblUserNameLabel.Location = new System.Drawing.Point(10, 48);
            this.lblUserNameLabel.Name = "lblUserNameLabel";
            this.lblUserNameLabel.Size = new System.Drawing.Size(63, 13);
            this.lblUserNameLabel.TabIndex = 2;
            this.lblUserNameLabel.Text = "User Name:";
            this.lblUserNameLabel.ForeColor = labelColor;
            this.lblUserNameLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblUserName
            //
            this.lblUserName.AutoSize = true;
            this.lblUserName.Location = new System.Drawing.Point(130, 48);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(16, 13);
            this.lblUserName.TabIndex = 3;
            this.lblUserName.Text = "--";
            this.lblUserName.ForeColor = valueColor;
            this.lblUserName.Font = new Font("Segoe UI", 9f);
            //
            // lblSessionStateLabel
            //
            this.lblSessionStateLabel.AutoSize = true;
            this.lblSessionStateLabel.Location = new System.Drawing.Point(10, 71);
            this.lblSessionStateLabel.Name = "lblSessionStateLabel";
            this.lblSessionStateLabel.Size = new System.Drawing.Size(75, 13);
            this.lblSessionStateLabel.TabIndex = 4;
            this.lblSessionStateLabel.Text = "Session State:";
            this.lblSessionStateLabel.ForeColor = labelColor;
            this.lblSessionStateLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblSessionState
            //
            this.lblSessionState.AutoSize = true;
            this.lblSessionState.Location = new System.Drawing.Point(130, 71);
            this.lblSessionState.Name = "lblSessionState";
            this.lblSessionState.Size = new System.Drawing.Size(16, 13);
            this.lblSessionState.TabIndex = 5;
            this.lblSessionState.Text = "--";
            this.lblSessionState.ForeColor = valueColor;
            this.lblSessionState.Font = new Font("Segoe UI", 9f);
            //
            // lblFirstActivityLabel
            //
            this.lblFirstActivityLabel.AutoSize = true;
            this.lblFirstActivityLabel.Location = new System.Drawing.Point(10, 94);
            this.lblFirstActivityLabel.Name = "lblFirstActivityLabel";
            this.lblFirstActivityLabel.Size = new System.Drawing.Size(100, 13);
            this.lblFirstActivityLabel.TabIndex = 6;
            this.lblFirstActivityLabel.Text = "First Activity Today:";
            this.lblFirstActivityLabel.ForeColor = labelColor;
            this.lblFirstActivityLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblFirstActivity
            //
            this.lblFirstActivity.AutoSize = true;
            this.lblFirstActivity.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.lblFirstActivity.Location = new System.Drawing.Point(130, 94);
            this.lblFirstActivity.Name = "lblFirstActivity";
            this.lblFirstActivity.Size = new System.Drawing.Size(16, 13);
            this.lblFirstActivity.TabIndex = 7;
            this.lblFirstActivity.Text = "--";
            this.lblFirstActivity.ForeColor = Color.FromArgb(100, 200, 255);
            //
            // lblDurationLabel
            //
            this.lblDurationLabel.AutoSize = true;
            this.lblDurationLabel.Location = new System.Drawing.Point(10, 117);
            this.lblDurationLabel.Name = "lblDurationLabel";
            this.lblDurationLabel.Size = new System.Drawing.Size(110, 13);
            this.lblDurationLabel.TabIndex = 8;
            this.lblDurationLabel.Text = "Duration Since Start:";
            this.lblDurationLabel.ForeColor = labelColor;
            this.lblDurationLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblDuration
            //
            this.lblDuration.AutoSize = true;
            this.lblDuration.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.lblDuration.Location = new System.Drawing.Point(130, 117);
            this.lblDuration.Name = "lblDuration";
            this.lblDuration.Size = new System.Drawing.Size(16, 13);
            this.lblDuration.TabIndex = 9;
            this.lblDuration.Text = "--";
            this.lblDuration.ForeColor = Color.FromArgb(100, 200, 255);
            //
            // lblIdleTimeLabel
            //
            this.lblIdleTimeLabel.AutoSize = true;
            this.lblIdleTimeLabel.Location = new System.Drawing.Point(10, 140);
            this.lblIdleTimeLabel.Name = "lblIdleTimeLabel";
            this.lblIdleTimeLabel.Size = new System.Drawing.Size(55, 13);
            this.lblIdleTimeLabel.TabIndex = 10;
            this.lblIdleTimeLabel.Text = "Idle Time:";
            this.lblIdleTimeLabel.ForeColor = labelColor;
            this.lblIdleTimeLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblIdleTime
            //
            this.lblIdleTime.AutoSize = true;
            this.lblIdleTime.Location = new System.Drawing.Point(130, 140);
            this.lblIdleTime.Name = "lblIdleTime";
            this.lblIdleTime.Size = new System.Drawing.Size(16, 13);
            this.lblIdleTime.TabIndex = 11;
            this.lblIdleTime.Text = "--";
            this.lblIdleTime.ForeColor = valueColor;
            this.lblIdleTime.Font = new Font("Segoe UI", 9f);
            //
            // lblScreenSaverLabel
            //
            this.lblScreenSaverLabel.AutoSize = true;
            this.lblScreenSaverLabel.Location = new System.Drawing.Point(10, 163);
            this.lblScreenSaverLabel.Name = "lblScreenSaverLabel";
            this.lblScreenSaverLabel.Size = new System.Drawing.Size(75, 13);
            this.lblScreenSaverLabel.TabIndex = 12;
            this.lblScreenSaverLabel.Text = "Screen Saver:";
            this.lblScreenSaverLabel.ForeColor = labelColor;
            this.lblScreenSaverLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblScreenSaver
            //
            this.lblScreenSaver.AutoSize = true;
            this.lblScreenSaver.Location = new System.Drawing.Point(130, 163);
            this.lblScreenSaver.Name = "lblScreenSaver";
            this.lblScreenSaver.Size = new System.Drawing.Size(16, 13);
            this.lblScreenSaver.TabIndex = 13;
            this.lblScreenSaver.Text = "--";
            this.lblScreenSaver.ForeColor = valueColor;
            this.lblScreenSaver.Font = new Font("Segoe UI", 9f);
            //
            // lblWorkstationLockedLabel
            //
            this.lblWorkstationLockedLabel.AutoSize = true;
            this.lblWorkstationLockedLabel.Location = new System.Drawing.Point(10, 186);
            this.lblWorkstationLockedLabel.Name = "lblWorkstationLockedLabel";
            this.lblWorkstationLockedLabel.Size = new System.Drawing.Size(65, 13);
            this.lblWorkstationLockedLabel.TabIndex = 14;
            this.lblWorkstationLockedLabel.Text = "Workstation:";
            this.lblWorkstationLockedLabel.ForeColor = labelColor;
            this.lblWorkstationLockedLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblWorkstationLocked
            //
            this.lblWorkstationLocked.AutoSize = true;
            this.lblWorkstationLocked.Location = new System.Drawing.Point(130, 186);
            this.lblWorkstationLocked.Name = "lblWorkstationLocked";
            this.lblWorkstationLocked.Size = new System.Drawing.Size(16, 13);
            this.lblWorkstationLocked.TabIndex = 15;
            this.lblWorkstationLocked.Text = "--";
            this.lblWorkstationLocked.ForeColor = valueColor;
            this.lblWorkstationLocked.Font = new Font("Segoe UI", 9f);
            //
            // grpDisplay
            //
            this.grpDisplay.Controls.Add(this.lblMonitorCountLabel);
            this.grpDisplay.Controls.Add(this.lblMonitorCount);
            this.grpDisplay.Controls.Add(this.lblDisplayStateLabel);
            this.grpDisplay.Controls.Add(this.lblDisplayState);
            this.grpDisplay.Controls.Add(this.lblStateDurationLabel);
            this.grpDisplay.Controls.Add(this.lblStateDuration);
            this.grpDisplay.Controls.Add(this.lblOnTimeLabel);
            this.grpDisplay.Controls.Add(this.lblOnTime);
            this.grpDisplay.Controls.Add(this.lblOffTimeLabel);
            this.grpDisplay.Controls.Add(this.lblOffTime);
            this.grpDisplay.Controls.Add(this.lblIdleDisplayTimeLabel);
            this.grpDisplay.Controls.Add(this.lblIdleDisplayTime);
            this.grpDisplay.Controls.Add(this.lblDisplayPowerLabel);
            this.grpDisplay.Controls.Add(this.lblDisplayPower);
            this.grpDisplay.Location = new System.Drawing.Point(298, 12);
            this.grpDisplay.Name = "grpDisplay";
            this.grpDisplay.Size = new System.Drawing.Size(280, 190);
            this.grpDisplay.TabIndex = 2;
            this.grpDisplay.TabStop = false;
            this.grpDisplay.Text = "Display Status";
            this.grpDisplay.ForeColor = accentColor;
            this.grpDisplay.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            //
            // lblMonitorCountLabel
            //
            this.lblMonitorCountLabel.AutoSize = true;
            this.lblMonitorCountLabel.Location = new System.Drawing.Point(10, 25);
            this.lblMonitorCountLabel.Name = "lblMonitorCountLabel";
            this.lblMonitorCountLabel.Size = new System.Drawing.Size(50, 13);
            this.lblMonitorCountLabel.TabIndex = 0;
            this.lblMonitorCountLabel.Text = "Monitors:";
            this.lblMonitorCountLabel.ForeColor = labelColor;
            this.lblMonitorCountLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblMonitorCount
            //
            this.lblMonitorCount.AutoSize = true;
            this.lblMonitorCount.Location = new System.Drawing.Point(130, 25);
            this.lblMonitorCount.Name = "lblMonitorCount";
            this.lblMonitorCount.Size = new System.Drawing.Size(16, 13);
            this.lblMonitorCount.TabIndex = 1;
            this.lblMonitorCount.Text = "--";
            this.lblMonitorCount.ForeColor = valueColor;
            this.lblMonitorCount.Font = new Font("Segoe UI", 9f);
            //
            // lblDisplayStateLabel
            //
            this.lblDisplayStateLabel.AutoSize = true;
            this.lblDisplayStateLabel.Location = new System.Drawing.Point(10, 48);
            this.lblDisplayStateLabel.Name = "lblDisplayStateLabel";
            this.lblDisplayStateLabel.Size = new System.Drawing.Size(35, 13);
            this.lblDisplayStateLabel.TabIndex = 2;
            this.lblDisplayStateLabel.Text = "State:";
            this.lblDisplayStateLabel.ForeColor = labelColor;
            this.lblDisplayStateLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblDisplayState
            //
            this.lblDisplayState.AutoSize = true;
            this.lblDisplayState.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.lblDisplayState.Location = new System.Drawing.Point(130, 48);
            this.lblDisplayState.Name = "lblDisplayState";
            this.lblDisplayState.Size = new System.Drawing.Size(16, 13);
            this.lblDisplayState.TabIndex = 3;
            this.lblDisplayState.Text = "--";
            this.lblDisplayState.ForeColor = valueColor;
            //
            // lblStateDurationLabel
            //
            this.lblStateDurationLabel.AutoSize = true;
            this.lblStateDurationLabel.Location = new System.Drawing.Point(10, 71);
            this.lblStateDurationLabel.Name = "lblStateDurationLabel";
            this.lblStateDurationLabel.Size = new System.Drawing.Size(80, 13);
            this.lblStateDurationLabel.TabIndex = 4;
            this.lblStateDurationLabel.Text = "State Duration:";
            this.lblStateDurationLabel.ForeColor = labelColor;
            this.lblStateDurationLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblStateDuration
            //
            this.lblStateDuration.AutoSize = true;
            this.lblStateDuration.Location = new System.Drawing.Point(130, 71);
            this.lblStateDuration.Name = "lblStateDuration";
            this.lblStateDuration.Size = new System.Drawing.Size(16, 13);
            this.lblStateDuration.TabIndex = 5;
            this.lblStateDuration.Text = "--";
            this.lblStateDuration.ForeColor = valueColor;
            this.lblStateDuration.Font = new Font("Segoe UI", 9f);
            //
            // lblOnTimeLabel
            //
            this.lblOnTimeLabel.AutoSize = true;
            this.lblOnTimeLabel.Location = new System.Drawing.Point(10, 94);
            this.lblOnTimeLabel.Name = "lblOnTimeLabel";
            this.lblOnTimeLabel.Size = new System.Drawing.Size(80, 13);
            this.lblOnTimeLabel.TabIndex = 6;
            this.lblOnTimeLabel.Text = "On Time Today:";
            this.lblOnTimeLabel.ForeColor = labelColor;
            this.lblOnTimeLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblOnTime
            //
            this.lblOnTime.AutoSize = true;
            this.lblOnTime.ForeColor = System.Drawing.Color.FromArgb(100, 200, 100);
            this.lblOnTime.Location = new System.Drawing.Point(130, 94);
            this.lblOnTime.Name = "lblOnTime";
            this.lblOnTime.Size = new System.Drawing.Size(16, 13);
            this.lblOnTime.TabIndex = 7;
            this.lblOnTime.Text = "--";
            this.lblOnTime.Font = new Font("Segoe UI", 9f);
            //
            // lblOffTimeLabel
            //
            this.lblOffTimeLabel.AutoSize = true;
            this.lblOffTimeLabel.Location = new System.Drawing.Point(10, 117);
            this.lblOffTimeLabel.Name = "lblOffTimeLabel";
            this.lblOffTimeLabel.Size = new System.Drawing.Size(82, 13);
            this.lblOffTimeLabel.TabIndex = 8;
            this.lblOffTimeLabel.Text = "Off Time Today:";
            this.lblOffTimeLabel.ForeColor = labelColor;
            this.lblOffTimeLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblOffTime
            //
            this.lblOffTime.AutoSize = true;
            this.lblOffTime.ForeColor = System.Drawing.Color.FromArgb(150, 150, 150);
            this.lblOffTime.Location = new System.Drawing.Point(130, 117);
            this.lblOffTime.Name = "lblOffTime";
            this.lblOffTime.Size = new System.Drawing.Size(16, 13);
            this.lblOffTime.TabIndex = 9;
            this.lblOffTime.Text = "--";
            this.lblOffTime.Font = new Font("Segoe UI", 9f);
            //
            // lblIdleDisplayTimeLabel
            //
            this.lblIdleDisplayTimeLabel.AutoSize = true;
            this.lblIdleDisplayTimeLabel.Location = new System.Drawing.Point(10, 140);
            this.lblIdleDisplayTimeLabel.Name = "lblIdleDisplayTimeLabel";
            this.lblIdleDisplayTimeLabel.Size = new System.Drawing.Size(86, 13);
            this.lblIdleDisplayTimeLabel.TabIndex = 10;
            this.lblIdleDisplayTimeLabel.Text = "Idle Time Today:";
            this.lblIdleDisplayTimeLabel.ForeColor = labelColor;
            this.lblIdleDisplayTimeLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblIdleDisplayTime
            //
            this.lblIdleDisplayTime.AutoSize = true;
            this.lblIdleDisplayTime.ForeColor = System.Drawing.Color.FromArgb(255, 180, 100);
            this.lblIdleDisplayTime.Location = new System.Drawing.Point(130, 140);
            this.lblIdleDisplayTime.Name = "lblIdleDisplayTime";
            this.lblIdleDisplayTime.Size = new System.Drawing.Size(16, 13);
            this.lblIdleDisplayTime.TabIndex = 11;
            this.lblIdleDisplayTime.Text = "--";
            this.lblIdleDisplayTime.Font = new Font("Segoe UI", 9f);
            //
            // lblDisplayPowerLabel
            //
            this.lblDisplayPowerLabel.AutoSize = true;
            this.lblDisplayPowerLabel.Location = new System.Drawing.Point(10, 163);
            this.lblDisplayPowerLabel.Name = "lblDisplayPowerLabel";
            this.lblDisplayPowerLabel.Size = new System.Drawing.Size(75, 13);
            this.lblDisplayPowerLabel.TabIndex = 12;
            this.lblDisplayPowerLabel.Text = "Display Power:";
            this.lblDisplayPowerLabel.ForeColor = labelColor;
            this.lblDisplayPowerLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblDisplayPower
            //
            this.lblDisplayPower.AutoSize = true;
            this.lblDisplayPower.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.lblDisplayPower.Location = new System.Drawing.Point(130, 163);
            this.lblDisplayPower.Name = "lblDisplayPower";
            this.lblDisplayPower.Size = new System.Drawing.Size(16, 13);
            this.lblDisplayPower.TabIndex = 13;
            this.lblDisplayPower.Text = "--";
            this.lblDisplayPower.ForeColor = Color.FromArgb(100, 255, 100);
            //
            // grpEnergy
            //
            this.grpEnergy.Controls.Add(this.lblCpuUsageLabel);
            this.grpEnergy.Controls.Add(this.lblCpuUsage);
            this.grpEnergy.Controls.Add(this.progressCpu);
            this.grpEnergy.Controls.Add(this.lblCpuPowerLabel);
            this.grpEnergy.Controls.Add(this.lblCpuPower);
            this.grpEnergy.Controls.Add(this.lblTotalPowerLabel);
            this.grpEnergy.Controls.Add(this.lblTotalPower);
            this.grpEnergy.Controls.Add(this.lblEnergyTodayLabel);
            this.grpEnergy.Controls.Add(this.lblEnergyToday);
            this.grpEnergy.Controls.Add(this.lblCostEstimateLabel);
            this.grpEnergy.Controls.Add(this.lblCostEstimate);
            this.grpEnergy.Location = new System.Drawing.Point(298, 208);
            this.grpEnergy.Name = "grpEnergy";
            this.grpEnergy.Size = new System.Drawing.Size(280, 155);
            this.grpEnergy.TabIndex = 3;
            this.grpEnergy.TabStop = false;
            this.grpEnergy.Text = "Energy Consumption";
            this.grpEnergy.ForeColor = accentColor;
            this.grpEnergy.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            //
            // lblCpuUsageLabel
            //
            this.lblCpuUsageLabel.AutoSize = true;
            this.lblCpuUsageLabel.Location = new System.Drawing.Point(10, 25);
            this.lblCpuUsageLabel.Name = "lblCpuUsageLabel";
            this.lblCpuUsageLabel.Size = new System.Drawing.Size(63, 13);
            this.lblCpuUsageLabel.TabIndex = 0;
            this.lblCpuUsageLabel.Text = "CPU Usage:";
            this.lblCpuUsageLabel.ForeColor = labelColor;
            this.lblCpuUsageLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblCpuUsage
            //
            this.lblCpuUsage.AutoSize = true;
            this.lblCpuUsage.Location = new System.Drawing.Point(130, 25);
            this.lblCpuUsage.Name = "lblCpuUsage";
            this.lblCpuUsage.Size = new System.Drawing.Size(16, 13);
            this.lblCpuUsage.TabIndex = 1;
            this.lblCpuUsage.Text = "--";
            this.lblCpuUsage.ForeColor = valueColor;
            this.lblCpuUsage.Font = new Font("Segoe UI", 9f);
            //
            // progressCpu
            //
            this.progressCpu.Location = new System.Drawing.Point(180, 22);
            this.progressCpu.Name = "progressCpu";
            this.progressCpu.Size = new System.Drawing.Size(90, 18);
            this.progressCpu.TabIndex = 2;
            this.progressCpu.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            //
            // lblCpuPowerLabel
            //
            this.lblCpuPowerLabel.AutoSize = true;
            this.lblCpuPowerLabel.Location = new System.Drawing.Point(10, 48);
            this.lblCpuPowerLabel.Name = "lblCpuPowerLabel";
            this.lblCpuPowerLabel.Size = new System.Drawing.Size(62, 13);
            this.lblCpuPowerLabel.TabIndex = 3;
            this.lblCpuPowerLabel.Text = "CPU Power:";
            this.lblCpuPowerLabel.ForeColor = labelColor;
            this.lblCpuPowerLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblCpuPower
            //
            this.lblCpuPower.AutoSize = true;
            this.lblCpuPower.Location = new System.Drawing.Point(130, 48);
            this.lblCpuPower.Name = "lblCpuPower";
            this.lblCpuPower.Size = new System.Drawing.Size(16, 13);
            this.lblCpuPower.TabIndex = 4;
            this.lblCpuPower.Text = "--";
            this.lblCpuPower.ForeColor = valueColor;
            this.lblCpuPower.Font = new Font("Segoe UI", 9f);
            //
            // lblTotalPowerLabel
            //
            this.lblTotalPowerLabel.AutoSize = true;
            this.lblTotalPowerLabel.Location = new System.Drawing.Point(10, 71);
            this.lblTotalPowerLabel.Name = "lblTotalPowerLabel";
            this.lblTotalPowerLabel.Size = new System.Drawing.Size(95, 13);
            this.lblTotalPowerLabel.TabIndex = 5;
            this.lblTotalPowerLabel.Text = "Total System Power:";
            this.lblTotalPowerLabel.ForeColor = labelColor;
            this.lblTotalPowerLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblTotalPower
            //
            this.lblTotalPower.AutoSize = true;
            this.lblTotalPower.Font = new System.Drawing.Font("Segoe UI", 14f, System.Drawing.FontStyle.Bold);
            this.lblTotalPower.ForeColor = System.Drawing.Color.FromArgb(100, 255, 100);
            this.lblTotalPower.Location = new System.Drawing.Point(130, 66);
            this.lblTotalPower.Name = "lblTotalPower";
            this.lblTotalPower.Size = new System.Drawing.Size(16, 16);
            this.lblTotalPower.TabIndex = 6;
            this.lblTotalPower.Text = "--";
            //
            // lblEnergyTodayLabel
            //
            this.lblEnergyTodayLabel.AutoSize = true;
            this.lblEnergyTodayLabel.Location = new System.Drawing.Point(10, 100);
            this.lblEnergyTodayLabel.Name = "lblEnergyTodayLabel";
            this.lblEnergyTodayLabel.Size = new System.Drawing.Size(100, 13);
            this.lblEnergyTodayLabel.TabIndex = 7;
            this.lblEnergyTodayLabel.Text = "Energy Today (est):";
            this.lblEnergyTodayLabel.ForeColor = labelColor;
            this.lblEnergyTodayLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblEnergyToday
            //
            this.lblEnergyToday.AutoSize = true;
            this.lblEnergyToday.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            this.lblEnergyToday.Location = new System.Drawing.Point(130, 100);
            this.lblEnergyToday.Name = "lblEnergyToday";
            this.lblEnergyToday.Size = new System.Drawing.Size(16, 13);
            this.lblEnergyToday.TabIndex = 8;
            this.lblEnergyToday.Text = "--";
            this.lblEnergyToday.ForeColor = Color.FromArgb(255, 200, 100);
            //
            // lblCostEstimateLabel
            //
            this.lblCostEstimateLabel.AutoSize = true;
            this.lblCostEstimateLabel.Location = new System.Drawing.Point(10, 123);
            this.lblCostEstimateLabel.Name = "lblCostEstimateLabel";
            this.lblCostEstimateLabel.Size = new System.Drawing.Size(110, 13);
            this.lblCostEstimateLabel.TabIndex = 9;
            this.lblCostEstimateLabel.Text = "Cost (25 ct/kWh):";
            this.lblCostEstimateLabel.ForeColor = labelColor;
            this.lblCostEstimateLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblCostEstimate
            //
            this.lblCostEstimate.AutoSize = true;
            this.lblCostEstimate.Location = new System.Drawing.Point(130, 123);
            this.lblCostEstimate.Name = "lblCostEstimate";
            this.lblCostEstimate.Size = new System.Drawing.Size(16, 13);
            this.lblCostEstimate.TabIndex = 10;
            this.lblCostEstimate.Text = "--";
            this.lblCostEstimate.ForeColor = Color.FromArgb(255, 200, 100);
            this.lblCostEstimate.Font = new Font("Segoe UI", 9f);
            //
            // grpRemote
            //
            this.grpRemote.Controls.Add(this.lblRdpStatusLabel);
            this.grpRemote.Controls.Add(this.lblRdpStatus);
            this.grpRemote.Controls.Add(this.lblRdpClientLabel);
            this.grpRemote.Controls.Add(this.lblRdpClient);
            this.grpRemote.Controls.Add(this.lblTeamViewerLabel);
            this.grpRemote.Controls.Add(this.lblTeamViewerStatus);
            this.grpRemote.Controls.Add(this.lblAnyDeskLabel);
            this.grpRemote.Controls.Add(this.lblAnyDeskStatus);
            this.grpRemote.Controls.Add(this.lblOtherRemoteLabel);
            this.grpRemote.Controls.Add(this.lblOtherRemote);
            this.grpRemote.Location = new System.Drawing.Point(12, 289);
            this.grpRemote.Name = "grpRemote";
            this.grpRemote.Size = new System.Drawing.Size(280, 145);
            this.grpRemote.TabIndex = 4;
            this.grpRemote.TabStop = false;
            this.grpRemote.Text = "Remote Access";
            this.grpRemote.ForeColor = accentColor;
            this.grpRemote.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            //
            // lblRdpStatusLabel
            //
            this.lblRdpStatusLabel.AutoSize = true;
            this.lblRdpStatusLabel.Location = new System.Drawing.Point(10, 25);
            this.lblRdpStatusLabel.Name = "lblRdpStatusLabel";
            this.lblRdpStatusLabel.Size = new System.Drawing.Size(32, 13);
            this.lblRdpStatusLabel.TabIndex = 0;
            this.lblRdpStatusLabel.Text = "RDP:";
            this.lblRdpStatusLabel.ForeColor = labelColor;
            this.lblRdpStatusLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblRdpStatus
            //
            this.lblRdpStatus.AutoSize = true;
            this.lblRdpStatus.Location = new System.Drawing.Point(100, 25);
            this.lblRdpStatus.Name = "lblRdpStatus";
            this.lblRdpStatus.Size = new System.Drawing.Size(16, 13);
            this.lblRdpStatus.TabIndex = 1;
            this.lblRdpStatus.Text = "--";
            this.lblRdpStatus.ForeColor = valueColor;
            this.lblRdpStatus.Font = new Font("Segoe UI", 9f);
            //
            // lblRdpClientLabel
            //
            this.lblRdpClientLabel.AutoSize = true;
            this.lblRdpClientLabel.Location = new System.Drawing.Point(10, 48);
            this.lblRdpClientLabel.Name = "lblRdpClientLabel";
            this.lblRdpClientLabel.Size = new System.Drawing.Size(62, 13);
            this.lblRdpClientLabel.TabIndex = 2;
            this.lblRdpClientLabel.Text = "RDP Client:";
            this.lblRdpClientLabel.ForeColor = labelColor;
            this.lblRdpClientLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblRdpClient
            //
            this.lblRdpClient.AutoSize = true;
            this.lblRdpClient.Location = new System.Drawing.Point(100, 48);
            this.lblRdpClient.Name = "lblRdpClient";
            this.lblRdpClient.Size = new System.Drawing.Size(16, 13);
            this.lblRdpClient.TabIndex = 3;
            this.lblRdpClient.Text = "--";
            this.lblRdpClient.ForeColor = valueColor;
            this.lblRdpClient.Font = new Font("Segoe UI", 9f);
            //
            // lblTeamViewerLabel
            //
            this.lblTeamViewerLabel.AutoSize = true;
            this.lblTeamViewerLabel.Location = new System.Drawing.Point(10, 71);
            this.lblTeamViewerLabel.Name = "lblTeamViewerLabel";
            this.lblTeamViewerLabel.Size = new System.Drawing.Size(68, 13);
            this.lblTeamViewerLabel.TabIndex = 4;
            this.lblTeamViewerLabel.Text = "TeamViewer:";
            this.lblTeamViewerLabel.ForeColor = labelColor;
            this.lblTeamViewerLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblTeamViewerStatus
            //
            this.lblTeamViewerStatus.AutoSize = true;
            this.lblTeamViewerStatus.Location = new System.Drawing.Point(100, 71);
            this.lblTeamViewerStatus.Name = "lblTeamViewerStatus";
            this.lblTeamViewerStatus.Size = new System.Drawing.Size(16, 13);
            this.lblTeamViewerStatus.TabIndex = 5;
            this.lblTeamViewerStatus.Text = "--";
            this.lblTeamViewerStatus.ForeColor = valueColor;
            this.lblTeamViewerStatus.Font = new Font("Segoe UI", 9f);
            //
            // lblAnyDeskLabel
            //
            this.lblAnyDeskLabel.AutoSize = true;
            this.lblAnyDeskLabel.Location = new System.Drawing.Point(10, 94);
            this.lblAnyDeskLabel.Name = "lblAnyDeskLabel";
            this.lblAnyDeskLabel.Size = new System.Drawing.Size(52, 13);
            this.lblAnyDeskLabel.TabIndex = 6;
            this.lblAnyDeskLabel.Text = "AnyDesk:";
            this.lblAnyDeskLabel.ForeColor = labelColor;
            this.lblAnyDeskLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblAnyDeskStatus
            //
            this.lblAnyDeskStatus.AutoSize = true;
            this.lblAnyDeskStatus.Location = new System.Drawing.Point(100, 94);
            this.lblAnyDeskStatus.Name = "lblAnyDeskStatus";
            this.lblAnyDeskStatus.Size = new System.Drawing.Size(16, 13);
            this.lblAnyDeskStatus.TabIndex = 7;
            this.lblAnyDeskStatus.Text = "--";
            this.lblAnyDeskStatus.ForeColor = valueColor;
            this.lblAnyDeskStatus.Font = new Font("Segoe UI", 9f);
            //
            // lblOtherRemoteLabel
            //
            this.lblOtherRemoteLabel.AutoSize = true;
            this.lblOtherRemoteLabel.Location = new System.Drawing.Point(10, 117);
            this.lblOtherRemoteLabel.Name = "lblOtherRemoteLabel";
            this.lblOtherRemoteLabel.Size = new System.Drawing.Size(65, 13);
            this.lblOtherRemoteLabel.TabIndex = 8;
            this.lblOtherRemoteLabel.Text = "Other Tools:";
            this.lblOtherRemoteLabel.ForeColor = labelColor;
            this.lblOtherRemoteLabel.Font = new Font("Segoe UI", 9f);
            //
            // lblOtherRemote
            //
            this.lblOtherRemote.AutoSize = true;
            this.lblOtherRemote.Location = new System.Drawing.Point(100, 117);
            this.lblOtherRemote.Name = "lblOtherRemote";
            this.lblOtherRemote.Size = new System.Drawing.Size(16, 13);
            this.lblOtherRemote.TabIndex = 9;
            this.lblOtherRemote.Text = "--";
            this.lblOtherRemote.ForeColor = valueColor;
            this.lblOtherRemote.Font = new Font("Segoe UI", 9f);
            //
            // StatusForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(590, 446);
            this.Controls.Add(this.grpRemote);
            this.Controls.Add(this.grpEnergy);
            this.Controls.Add(this.grpDisplay);
            this.Controls.Add(this.grpActivity);
            this.Controls.Add(this.grpService);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "StatusForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WinEcoSensor - Status";
            this.BackColor = bgColor;
            this.ForeColor = valueColor;
            this.grpService.ResumeLayout(false);
            this.grpService.PerformLayout();
            this.grpActivity.ResumeLayout(false);
            this.grpActivity.PerformLayout();
            this.grpDisplay.ResumeLayout(false);
            this.grpDisplay.PerformLayout();
            this.grpEnergy.ResumeLayout(false);
            this.grpEnergy.PerformLayout();
            this.grpRemote.ResumeLayout(false);
            this.grpRemote.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox grpService;
        private System.Windows.Forms.Label lblServiceStatusLabel;
        private System.Windows.Forms.Label lblServiceStatus;
        private System.Windows.Forms.GroupBox grpActivity;
        private System.Windows.Forms.Label lblLoginStatusLabel;
        private System.Windows.Forms.Label lblLoginStatus;
        private System.Windows.Forms.Label lblUserNameLabel;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.Label lblSessionStateLabel;
        private System.Windows.Forms.Label lblSessionState;
        private System.Windows.Forms.Label lblFirstActivityLabel;
        private System.Windows.Forms.Label lblFirstActivity;
        private System.Windows.Forms.Label lblDurationLabel;
        private System.Windows.Forms.Label lblDuration;
        private System.Windows.Forms.Label lblIdleTimeLabel;
        private System.Windows.Forms.Label lblIdleTime;
        private System.Windows.Forms.Label lblScreenSaverLabel;
        private System.Windows.Forms.Label lblScreenSaver;
        private System.Windows.Forms.Label lblWorkstationLockedLabel;
        private System.Windows.Forms.Label lblWorkstationLocked;
        private System.Windows.Forms.GroupBox grpDisplay;
        private System.Windows.Forms.Label lblMonitorCountLabel;
        private System.Windows.Forms.Label lblMonitorCount;
        private System.Windows.Forms.Label lblDisplayStateLabel;
        private System.Windows.Forms.Label lblDisplayState;
        private System.Windows.Forms.Label lblStateDurationLabel;
        private System.Windows.Forms.Label lblStateDuration;
        private System.Windows.Forms.Label lblOnTimeLabel;
        private System.Windows.Forms.Label lblOnTime;
        private System.Windows.Forms.Label lblOffTimeLabel;
        private System.Windows.Forms.Label lblOffTime;
        private System.Windows.Forms.Label lblIdleDisplayTimeLabel;
        private System.Windows.Forms.Label lblIdleDisplayTime;
        private System.Windows.Forms.Label lblDisplayPowerLabel;
        private System.Windows.Forms.Label lblDisplayPower;
        private System.Windows.Forms.GroupBox grpEnergy;
        private System.Windows.Forms.Label lblCpuUsageLabel;
        private System.Windows.Forms.Label lblCpuUsage;
        private System.Windows.Forms.ProgressBar progressCpu;
        private System.Windows.Forms.Label lblCpuPowerLabel;
        private System.Windows.Forms.Label lblCpuPower;
        private System.Windows.Forms.Label lblTotalPowerLabel;
        private System.Windows.Forms.Label lblTotalPower;
        private System.Windows.Forms.Label lblEnergyTodayLabel;
        private System.Windows.Forms.Label lblEnergyToday;
        private System.Windows.Forms.Label lblCostEstimateLabel;
        private System.Windows.Forms.Label lblCostEstimate;
        private System.Windows.Forms.GroupBox grpRemote;
        private System.Windows.Forms.Label lblRdpStatusLabel;
        private System.Windows.Forms.Label lblRdpStatus;
        private System.Windows.Forms.Label lblRdpClientLabel;
        private System.Windows.Forms.Label lblRdpClient;
        private System.Windows.Forms.Label lblTeamViewerLabel;
        private System.Windows.Forms.Label lblTeamViewerStatus;
        private System.Windows.Forms.Label lblAnyDeskLabel;
        private System.Windows.Forms.Label lblAnyDeskStatus;
        private System.Windows.Forms.Label lblOtherRemoteLabel;
        private System.Windows.Forms.Label lblOtherRemote;
    }
}
