// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2026 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

namespace WinEcoSensor.TrayApp
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.grpGeneral = new System.Windows.Forms.GroupBox();
            this.chkAutoStart = new System.Windows.Forms.CheckBox();
            this.lblMonitorInterval = new System.Windows.Forms.Label();
            this.numMonitorInterval = new System.Windows.Forms.NumericUpDown();
            this.lblMonitorIntervalUnit = new System.Windows.Forms.Label();
            this.lblReportInterval = new System.Windows.Forms.Label();
            this.numReportInterval = new System.Windows.Forms.NumericUpDown();
            this.lblReportIntervalUnit = new System.Windows.Forms.Label();
            this.grpMonitoring = new System.Windows.Forms.GroupBox();
            this.chkMonitorCpu = new System.Windows.Forms.CheckBox();
            this.chkMonitorDisplay = new System.Windows.Forms.CheckBox();
            this.chkMonitorRemote = new System.Windows.Forms.CheckBox();
            this.grpLogging = new System.Windows.Forms.GroupBox();
            this.lblLogLevel = new System.Windows.Forms.Label();
            this.cmbLogLevel = new System.Windows.Forms.ComboBox();
            this.tabHardware = new System.Windows.Forms.TabPage();
            this.lstHardware = new System.Windows.Forms.ListView();
            this.colType = new System.Windows.Forms.ColumnHeader();
            this.colManufacturer = new System.Windows.Forms.ColumnHeader();
            this.colModel = new System.Windows.Forms.ColumnHeader();
            this.colEprel = new System.Windows.Forms.ColumnHeader();
            this.colPower = new System.Windows.Forms.ColumnHeader();
            this.lblHardwareInfo = new System.Windows.Forms.Label();
            this.btnRefreshHardware = new System.Windows.Forms.Button();
            this.tabBackend = new System.Windows.Forms.TabPage();
            this.grpBackend = new System.Windows.Forms.GroupBox();
            this.lblBackendUrl = new System.Windows.Forms.Label();
            this.txtBackendUrl = new System.Windows.Forms.TextBox();
            this.btnTestBackend = new System.Windows.Forms.Button();
            this.lblBackendInfo = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.grpGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMonitorInterval)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numReportInterval)).BeginInit();
            this.grpMonitoring.SuspendLayout();
            this.grpLogging.SuspendLayout();
            this.tabHardware.SuspendLayout();
            this.tabBackend.SuspendLayout();
            this.grpBackend.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabGeneral);
            this.tabControl.Controls.Add(this.tabHardware);
            this.tabControl.Controls.Add(this.tabBackend);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(560, 380);
            this.tabControl.TabIndex = 0;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.grpGeneral);
            this.tabGeneral.Controls.Add(this.grpMonitoring);
            this.tabGeneral.Controls.Add(this.grpLogging);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(552, 354);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // grpGeneral
            // 
            this.grpGeneral.Controls.Add(this.chkAutoStart);
            this.grpGeneral.Controls.Add(this.lblMonitorInterval);
            this.grpGeneral.Controls.Add(this.numMonitorInterval);
            this.grpGeneral.Controls.Add(this.lblMonitorIntervalUnit);
            this.grpGeneral.Controls.Add(this.lblReportInterval);
            this.grpGeneral.Controls.Add(this.numReportInterval);
            this.grpGeneral.Controls.Add(this.lblReportIntervalUnit);
            this.grpGeneral.Location = new System.Drawing.Point(6, 6);
            this.grpGeneral.Name = "grpGeneral";
            this.grpGeneral.Size = new System.Drawing.Size(540, 120);
            this.grpGeneral.TabIndex = 0;
            this.grpGeneral.TabStop = false;
            this.grpGeneral.Text = "General Settings";
            // 
            // chkAutoStart
            // 
            this.chkAutoStart.AutoSize = true;
            this.chkAutoStart.Location = new System.Drawing.Point(15, 25);
            this.chkAutoStart.Name = "chkAutoStart";
            this.chkAutoStart.Size = new System.Drawing.Size(200, 17);
            this.chkAutoStart.TabIndex = 0;
            this.chkAutoStart.Text = "Start automatically with Windows";
            this.chkAutoStart.UseVisualStyleBackColor = true;
            // 
            // lblMonitorInterval
            // 
            this.lblMonitorInterval.AutoSize = true;
            this.lblMonitorInterval.Location = new System.Drawing.Point(15, 55);
            this.lblMonitorInterval.Name = "lblMonitorInterval";
            this.lblMonitorInterval.Size = new System.Drawing.Size(90, 13);
            this.lblMonitorInterval.TabIndex = 1;
            this.lblMonitorInterval.Text = "Monitor interval:";
            // 
            // numMonitorInterval
            // 
            this.numMonitorInterval.Location = new System.Drawing.Point(150, 53);
            this.numMonitorInterval.Maximum = new decimal(new int[] { 3600, 0, 0, 0 });
            this.numMonitorInterval.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numMonitorInterval.Name = "numMonitorInterval";
            this.numMonitorInterval.Size = new System.Drawing.Size(80, 20);
            this.numMonitorInterval.TabIndex = 2;
            this.numMonitorInterval.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // lblMonitorIntervalUnit
            // 
            this.lblMonitorIntervalUnit.AutoSize = true;
            this.lblMonitorIntervalUnit.Location = new System.Drawing.Point(236, 55);
            this.lblMonitorIntervalUnit.Name = "lblMonitorIntervalUnit";
            this.lblMonitorIntervalUnit.Size = new System.Drawing.Size(47, 13);
            this.lblMonitorIntervalUnit.TabIndex = 3;
            this.lblMonitorIntervalUnit.Text = "seconds";
            // 
            // lblReportInterval
            // 
            this.lblReportInterval.AutoSize = true;
            this.lblReportInterval.Location = new System.Drawing.Point(15, 85);
            this.lblReportInterval.Name = "lblReportInterval";
            this.lblReportInterval.Size = new System.Drawing.Size(80, 13);
            this.lblReportInterval.TabIndex = 4;
            this.lblReportInterval.Text = "Report interval:";
            // 
            // numReportInterval
            // 
            this.numReportInterval.Location = new System.Drawing.Point(150, 83);
            this.numReportInterval.Maximum = new decimal(new int[] { 86400, 0, 0, 0 });
            this.numReportInterval.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            this.numReportInterval.Name = "numReportInterval";
            this.numReportInterval.Size = new System.Drawing.Size(80, 20);
            this.numReportInterval.TabIndex = 5;
            this.numReportInterval.Value = new decimal(new int[] { 60, 0, 0, 0 });
            // 
            // lblReportIntervalUnit
            // 
            this.lblReportIntervalUnit.AutoSize = true;
            this.lblReportIntervalUnit.Location = new System.Drawing.Point(236, 85);
            this.lblReportIntervalUnit.Name = "lblReportIntervalUnit";
            this.lblReportIntervalUnit.Size = new System.Drawing.Size(47, 13);
            this.lblReportIntervalUnit.TabIndex = 6;
            this.lblReportIntervalUnit.Text = "seconds";
            // 
            // grpMonitoring
            // 
            this.grpMonitoring.Controls.Add(this.chkMonitorCpu);
            this.grpMonitoring.Controls.Add(this.chkMonitorDisplay);
            this.grpMonitoring.Controls.Add(this.chkMonitorRemote);
            this.grpMonitoring.Location = new System.Drawing.Point(6, 132);
            this.grpMonitoring.Name = "grpMonitoring";
            this.grpMonitoring.Size = new System.Drawing.Size(540, 100);
            this.grpMonitoring.TabIndex = 1;
            this.grpMonitoring.TabStop = false;
            this.grpMonitoring.Text = "Monitoring Options";
            // 
            // chkMonitorCpu
            // 
            this.chkMonitorCpu.AutoSize = true;
            this.chkMonitorCpu.Checked = true;
            this.chkMonitorCpu.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMonitorCpu.Location = new System.Drawing.Point(15, 25);
            this.chkMonitorCpu.Name = "chkMonitorCpu";
            this.chkMonitorCpu.Size = new System.Drawing.Size(150, 17);
            this.chkMonitorCpu.TabIndex = 0;
            this.chkMonitorCpu.Text = "Monitor CPU usage";
            this.chkMonitorCpu.UseVisualStyleBackColor = true;
            // 
            // chkMonitorDisplay
            // 
            this.chkMonitorDisplay.AutoSize = true;
            this.chkMonitorDisplay.Checked = true;
            this.chkMonitorDisplay.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMonitorDisplay.Location = new System.Drawing.Point(15, 50);
            this.chkMonitorDisplay.Name = "chkMonitorDisplay";
            this.chkMonitorDisplay.Size = new System.Drawing.Size(180, 17);
            this.chkMonitorDisplay.TabIndex = 1;
            this.chkMonitorDisplay.Text = "Monitor display state";
            this.chkMonitorDisplay.UseVisualStyleBackColor = true;
            // 
            // chkMonitorRemote
            // 
            this.chkMonitorRemote.AutoSize = true;
            this.chkMonitorRemote.Checked = true;
            this.chkMonitorRemote.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMonitorRemote.Location = new System.Drawing.Point(15, 75);
            this.chkMonitorRemote.Name = "chkMonitorRemote";
            this.chkMonitorRemote.Size = new System.Drawing.Size(230, 17);
            this.chkMonitorRemote.TabIndex = 2;
            this.chkMonitorRemote.Text = "Monitor remote access (RDP, TeamViewer)";
            this.chkMonitorRemote.UseVisualStyleBackColor = true;
            // 
            // grpLogging
            // 
            this.grpLogging.Controls.Add(this.lblLogLevel);
            this.grpLogging.Controls.Add(this.cmbLogLevel);
            this.grpLogging.Location = new System.Drawing.Point(6, 238);
            this.grpLogging.Name = "grpLogging";
            this.grpLogging.Size = new System.Drawing.Size(540, 60);
            this.grpLogging.TabIndex = 2;
            this.grpLogging.TabStop = false;
            this.grpLogging.Text = "Logging";
            // 
            // lblLogLevel
            // 
            this.lblLogLevel.AutoSize = true;
            this.lblLogLevel.Location = new System.Drawing.Point(15, 25);
            this.lblLogLevel.Name = "lblLogLevel";
            this.lblLogLevel.Size = new System.Drawing.Size(55, 13);
            this.lblLogLevel.TabIndex = 0;
            this.lblLogLevel.Text = "Log level:";
            // 
            // cmbLogLevel
            // 
            this.cmbLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLogLevel.FormattingEnabled = true;
            this.cmbLogLevel.Items.AddRange(new object[] { "Error", "Warning", "Info", "Debug" });
            this.cmbLogLevel.Location = new System.Drawing.Point(150, 22);
            this.cmbLogLevel.Name = "cmbLogLevel";
            this.cmbLogLevel.Size = new System.Drawing.Size(120, 21);
            this.cmbLogLevel.TabIndex = 1;
            // 
            // tabHardware
            // 
            this.tabHardware.Controls.Add(this.lstHardware);
            this.tabHardware.Controls.Add(this.lblHardwareInfo);
            this.tabHardware.Controls.Add(this.btnRefreshHardware);
            this.tabHardware.Location = new System.Drawing.Point(4, 22);
            this.tabHardware.Name = "tabHardware";
            this.tabHardware.Padding = new System.Windows.Forms.Padding(3);
            this.tabHardware.Size = new System.Drawing.Size(552, 354);
            this.tabHardware.TabIndex = 1;
            this.tabHardware.Text = "Hardware & EPREL";
            this.tabHardware.UseVisualStyleBackColor = true;
            // 
            // lstHardware
            // 
            this.lstHardware.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstHardware.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colType,
            this.colManufacturer,
            this.colModel,
            this.colEprel,
            this.colPower});
            this.lstHardware.FullRowSelect = true;
            this.lstHardware.GridLines = true;
            this.lstHardware.HideSelection = false;
            this.lstHardware.Location = new System.Drawing.Point(6, 6);
            this.lstHardware.MultiSelect = false;
            this.lstHardware.Name = "lstHardware";
            this.lstHardware.Size = new System.Drawing.Size(540, 275);
            this.lstHardware.TabIndex = 0;
            this.lstHardware.UseCompatibleStateImageBehavior = false;
            this.lstHardware.View = System.Windows.Forms.View.Details;
            this.lstHardware.DoubleClick += new System.EventHandler(this.lstHardware_DoubleClick);
            // 
            // colType
            // 
            this.colType.Text = "Type";
            this.colType.Width = 70;
            // 
            // colManufacturer
            // 
            this.colManufacturer.Text = "Manufacturer";
            this.colManufacturer.Width = 100;
            // 
            // colModel
            // 
            this.colModel.Text = "Model";
            this.colModel.Width = 150;
            // 
            // colEprel
            // 
            this.colEprel.Text = "EPREL";
            this.colEprel.Width = 100;
            // 
            // colPower
            // 
            this.colPower.Text = "Power";
            this.colPower.Width = 100;
            // 
            // lblHardwareInfo
            // 
            this.lblHardwareInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblHardwareInfo.Location = new System.Drawing.Point(6, 287);
            this.lblHardwareInfo.Name = "lblHardwareInfo";
            this.lblHardwareInfo.Size = new System.Drawing.Size(440, 30);
            this.lblHardwareInfo.TabIndex = 1;
            this.lblHardwareInfo.Text = "Double-click on a monitor to set its EU EPREL energy label number for accurate power data.";
            // 
            // btnRefreshHardware
            // 
            this.btnRefreshHardware.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefreshHardware.Location = new System.Drawing.Point(471, 320);
            this.btnRefreshHardware.Name = "btnRefreshHardware";
            this.btnRefreshHardware.Size = new System.Drawing.Size(75, 23);
            this.btnRefreshHardware.TabIndex = 2;
            this.btnRefreshHardware.Text = "Refresh";
            this.btnRefreshHardware.UseVisualStyleBackColor = true;
            this.btnRefreshHardware.Click += new System.EventHandler(this.btnRefreshHardware_Click);
            // 
            // tabBackend
            // 
            this.tabBackend.Controls.Add(this.grpBackend);
            this.tabBackend.Location = new System.Drawing.Point(4, 22);
            this.tabBackend.Name = "tabBackend";
            this.tabBackend.Padding = new System.Windows.Forms.Padding(3);
            this.tabBackend.Size = new System.Drawing.Size(552, 354);
            this.tabBackend.TabIndex = 2;
            this.tabBackend.Text = "Backend Server";
            this.tabBackend.UseVisualStyleBackColor = true;
            // 
            // grpBackend
            // 
            this.grpBackend.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpBackend.Controls.Add(this.lblBackendUrl);
            this.grpBackend.Controls.Add(this.txtBackendUrl);
            this.grpBackend.Controls.Add(this.btnTestBackend);
            this.grpBackend.Controls.Add(this.lblBackendInfo);
            this.grpBackend.Location = new System.Drawing.Point(6, 6);
            this.grpBackend.Name = "grpBackend";
            this.grpBackend.Size = new System.Drawing.Size(540, 150);
            this.grpBackend.TabIndex = 0;
            this.grpBackend.TabStop = false;
            this.grpBackend.Text = "Backend Server Configuration";
            // 
            // lblBackendUrl
            // 
            this.lblBackendUrl.AutoSize = true;
            this.lblBackendUrl.Location = new System.Drawing.Point(15, 30);
            this.lblBackendUrl.Name = "lblBackendUrl";
            this.lblBackendUrl.Size = new System.Drawing.Size(75, 13);
            this.lblBackendUrl.TabIndex = 0;
            this.lblBackendUrl.Text = "Backend URL:";
            // 
            // txtBackendUrl
            // 
            this.txtBackendUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBackendUrl.Location = new System.Drawing.Point(100, 27);
            this.txtBackendUrl.Name = "txtBackendUrl";
            this.txtBackendUrl.Size = new System.Drawing.Size(330, 20);
            this.txtBackendUrl.TabIndex = 1;
            // 
            // btnTestBackend
            // 
            this.btnTestBackend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTestBackend.Location = new System.Drawing.Point(436, 25);
            this.btnTestBackend.Name = "btnTestBackend";
            this.btnTestBackend.Size = new System.Drawing.Size(90, 23);
            this.btnTestBackend.TabIndex = 2;
            this.btnTestBackend.Text = "Test Connection";
            this.btnTestBackend.UseVisualStyleBackColor = true;
            this.btnTestBackend.Click += new System.EventHandler(this.btnTestBackend_Click);
            // 
            // lblBackendInfo
            // 
            this.lblBackendInfo.Location = new System.Drawing.Point(15, 60);
            this.lblBackendInfo.Name = "lblBackendInfo";
            this.lblBackendInfo.Size = new System.Drawing.Size(510, 80);
            this.lblBackendInfo.TabIndex = 3;
            this.lblBackendInfo.Text = "Enter the URL of the WinEcoSensor backend server where energy monitoring data will be sent.\n\n" +
                "The backend receives CloudEvent messages containing system status, energy consumption, and user activity data.\n\n" +
                "Leave empty to disable backend reporting.";
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(416, 405);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(497, 405);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // SettingsForm
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(584, 441);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WinEcoSensor Settings";
            this.tabControl.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.grpGeneral.ResumeLayout(false);
            this.grpGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMonitorInterval)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numReportInterval)).EndInit();
            this.grpMonitoring.ResumeLayout(false);
            this.grpMonitoring.PerformLayout();
            this.grpLogging.ResumeLayout(false);
            this.grpLogging.PerformLayout();
            this.tabHardware.ResumeLayout(false);
            this.tabBackend.ResumeLayout(false);
            this.grpBackend.ResumeLayout(false);
            this.grpBackend.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.GroupBox grpGeneral;
        private System.Windows.Forms.CheckBox chkAutoStart;
        private System.Windows.Forms.Label lblMonitorInterval;
        private System.Windows.Forms.NumericUpDown numMonitorInterval;
        private System.Windows.Forms.Label lblMonitorIntervalUnit;
        private System.Windows.Forms.Label lblReportInterval;
        private System.Windows.Forms.NumericUpDown numReportInterval;
        private System.Windows.Forms.Label lblReportIntervalUnit;
        private System.Windows.Forms.GroupBox grpMonitoring;
        private System.Windows.Forms.CheckBox chkMonitorCpu;
        private System.Windows.Forms.CheckBox chkMonitorDisplay;
        private System.Windows.Forms.CheckBox chkMonitorRemote;
        private System.Windows.Forms.GroupBox grpLogging;
        private System.Windows.Forms.Label lblLogLevel;
        private System.Windows.Forms.ComboBox cmbLogLevel;
        private System.Windows.Forms.TabPage tabHardware;
        private System.Windows.Forms.ListView lstHardware;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.ColumnHeader colManufacturer;
        private System.Windows.Forms.ColumnHeader colModel;
        private System.Windows.Forms.ColumnHeader colEprel;
        private System.Windows.Forms.ColumnHeader colPower;
        private System.Windows.Forms.Label lblHardwareInfo;
        private System.Windows.Forms.Button btnRefreshHardware;
        private System.Windows.Forms.TabPage tabBackend;
        private System.Windows.Forms.GroupBox grpBackend;
        private System.Windows.Forms.Label lblBackendUrl;
        private System.Windows.Forms.TextBox txtBackendUrl;
        private System.Windows.Forms.Button btnTestBackend;
        private System.Windows.Forms.Label lblBackendInfo;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}
