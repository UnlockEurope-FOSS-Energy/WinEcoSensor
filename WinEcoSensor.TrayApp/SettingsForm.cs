// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2026 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinEcoSensor.Common.Configuration;
using WinEcoSensor.Common.Models;
using WinEcoSensor.Common.Monitoring;
using WinEcoSensor.Common.Utilities;

namespace WinEcoSensor.TrayApp
{
    /// <summary>
    /// Settings form for WinEcoSensor configuration.
    /// Allows configuration of autostart, EPREL mappings, and backend URL.
    /// </summary>
    public partial class SettingsForm : Form
    {
        private ConfigurationManager _configManager;
        private SensorConfiguration _configuration;
        private HardwareMonitor _hardwareMonitor;
        private DisplayMonitor _displayMonitor;

        /// <summary>
        /// Create settings form
        /// </summary>
        public SettingsForm()
        {
            InitializeComponent();
            LoadConfiguration();
            LoadHardwareInfo();
        }

        /// <summary>
        /// Load current configuration
        /// </summary>
        private void LoadConfiguration()
        {
            try
            {
                _configManager = new ConfigurationManager();
                _configuration = _configManager.Load();

                // Set control values from configuration
                chkAutoStart.Checked = _configuration.AutoStart;
                txtBackendUrl.Text = _configuration.BackendUrl ?? "";
                numMonitorInterval.Value = Math.Max(1, Math.Min(3600, _configuration.StatusIntervalSeconds));
                numReportInterval.Value = Math.Max(10, Math.Min(86400, _configuration.ReportIntervalSeconds));
                chkMonitorCpu.Checked = _configuration.MonitorCpu;
                chkMonitorDisplay.Checked = _configuration.MonitorDisplays;
                chkMonitorRemote.Checked = _configuration.MonitorRemoteSessions;
                cmbLogLevel.SelectedItem = _configuration.LogLevel;

                Logger.Debug("Configuration loaded into settings form");
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading configuration", ex);
                MessageBox.Show(
                    "Failed to load configuration. Default values will be used.",
                    "Settings",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                _configManager = new ConfigurationManager();
                _configuration = new SensorConfiguration();
            }
        }

        /// <summary>
        /// Load hardware information and display in list
        /// </summary>
        private void LoadHardwareInfo()
        {
            try
            {
                lstHardware.Items.Clear();

                // Get hardware info
                _hardwareMonitor = new HardwareMonitor();
                _displayMonitor = new DisplayMonitor();

                var info = _hardwareMonitor.CollectHardwareInfo();

                // Add mainboard
                AddHardwareItem("Mainboard", info.MainboardManufacturer, info.MainboardProduct,
                    "N/A", $"Base: {DefaultPowerValues.SystemBasePower}W");

                // Add CPU
                AddHardwareItem("CPU", info.Processor?.Manufacturer, info.CpuName,
                    "N/A", $"TDP: {info.CpuTdpWatts}W");

                // Add memory
                AddHardwareItem("Memory", $"{info.TotalMemoryMB / 1024.0:F1} GB", "System RAM",
                    "N/A", "~3-5W");

                // Add GPUs
                foreach (var gpu in info.GraphicsCards)
                {
                    AddHardwareItem("GPU", gpu.Manufacturer, gpu.Name,
                        "N/A", $"Est: {gpu.EstimatedTdpWatts}W");
                }

                // Add monitors with EPREL support
                foreach (var monitor in _displayMonitor.Monitors)
                {
                    var eprelText = !string.IsNullOrEmpty(monitor.EprelNumber) 
                        ? monitor.EprelNumber 
                        : "Click to set...";
                    
                    AddHardwareItem("Monitor", 
                        $"Monitor {monitor.MonitorIndex + 1}", 
                        $"{monitor.HorizontalResolution}x{monitor.VerticalResolution} ({monitor.ScreenSizeInches}\")",
                        eprelText,
                        $"{monitor.PowerConsumptionOnWatts}W on, {monitor.PowerConsumptionStandbyWatts}W standby");
                }

                // Add disks
                foreach (var disk in info.Disks)
                {
                    AddHardwareItem("Disk", disk.Model, 
                        $"{disk.Type} - {disk.SizeGB:F0} GB",
                        "N/A", 
                        $"{disk.PowerActiveWatts}W active, {disk.PowerIdleWatts}W idle");
                }

                Logger.Debug($"Loaded {lstHardware.Items.Count} hardware items");
            }
            catch (Exception ex)
            {
                Logger.Error("Error loading hardware info", ex);
                MessageBox.Show(
                    "Failed to load hardware information.",
                    "Settings",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Add hardware item to the list view
        /// </summary>
        private void AddHardwareItem(string type, string manufacturer, string model, string eprel, string power)
        {
            var item = new ListViewItem(type);
            item.SubItems.Add(manufacturer ?? "Unknown");
            item.SubItems.Add(model ?? "Unknown");
            item.SubItems.Add(eprel ?? "N/A");
            item.SubItems.Add(power ?? "Unknown");
            item.Tag = type; // For later reference
            lstHardware.Items.Add(item);
        }

        /// <summary>
        /// Handle hardware list double-click for EPREL editing
        /// </summary>
        private void lstHardware_DoubleClick(object sender, EventArgs e)
        {
            if (lstHardware.SelectedItems.Count == 0)
                return;

            var item = lstHardware.SelectedItems[0];
            if (item.Tag?.ToString() != "Monitor")
            {
                MessageBox.Show(
                    "EPREL energy labels are only applicable to monitors.",
                    "EPREL Mapping",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // Show EPREL input dialog
            var currentEprel = item.SubItems[3].Text;
            if (currentEprel == "Click to set..." || currentEprel == "N/A")
                currentEprel = "";

            var eprelNumber = ShowInputDialog("EPREL Number", 
                "Enter the EU EPREL number for this monitor:\n\n" +
                "You can find this on the energy label or at:\n" +
                "https://eprel.ec.europa.eu/",
                currentEprel);

            if (eprelNumber != null)
            {
                item.SubItems[3].Text = string.IsNullOrEmpty(eprelNumber) ? "Click to set..." : eprelNumber;

                // TODO: Look up EPREL data and update power values
                if (!string.IsNullOrEmpty(eprelNumber))
                {
                    Logger.Info($"EPREL number set for monitor: {eprelNumber}");
                }
            }
        }

        /// <summary>
        /// Show simple input dialog
        /// </summary>
        private string ShowInputDialog(string title, string prompt, string defaultValue)
        {
            using (var form = new Form())
            {
                form.Text = title;
                form.Size = new System.Drawing.Size(400, 200);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                var lblPrompt = new Label
                {
                    Text = prompt,
                    Location = new System.Drawing.Point(10, 10),
                    Size = new System.Drawing.Size(360, 80)
                };

                var txtInput = new TextBox
                {
                    Text = defaultValue,
                    Location = new System.Drawing.Point(10, 100),
                    Size = new System.Drawing.Size(360, 23)
                };

                var btnOk = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Location = new System.Drawing.Point(210, 130),
                    Size = new System.Drawing.Size(75, 23)
                };

                var btnCancel = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    Location = new System.Drawing.Point(295, 130),
                    Size = new System.Drawing.Size(75, 23)
                };

                form.Controls.Add(lblPrompt);
                form.Controls.Add(txtInput);
                form.Controls.Add(btnOk);
                form.Controls.Add(btnCancel);
                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

                return form.ShowDialog(this) == DialogResult.OK ? txtInput.Text : null;
            }
        }

        /// <summary>
        /// Test backend connection
        /// </summary>
        private async void btnTestBackend_Click(object sender, EventArgs e)
        {
            var url = txtBackendUrl.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show(
                    "Please enter a backend URL first.",
                    "Test Backend",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            btnTestBackend.Enabled = false;
            btnTestBackend.Text = "Testing...";

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);

                    // Try a simple HEAD request first, then GET
                    HttpResponseMessage response;
                    try
                    {
                        var request = new HttpRequestMessage(HttpMethod.Head, url);
                        response = await client.SendAsync(request);
                    }
                    catch
                    {
                        // HEAD not supported, try GET
                        response = await client.GetAsync(url);
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show(
                            $"Connection successful!\n\nStatus: {response.StatusCode}\nServer: {response.Headers.Server}",
                            "Test Backend",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        Logger.Info($"Backend test successful: {url}");
                    }
                    else
                    {
                        MessageBox.Show(
                            $"Server responded with error:\n\nStatus: {(int)response.StatusCode} {response.ReasonPhrase}",
                            "Test Backend",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        Logger.Warning($"Backend test returned {response.StatusCode}: {url}");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    $"Connection failed:\n\n{ex.Message}",
                    "Test Backend",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Logger.Warning($"Backend test failed: {ex.Message}");
            }
            catch (TaskCanceledException)
            {
                MessageBox.Show(
                    "Connection timed out after 10 seconds.",
                    "Test Backend",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Logger.Warning("Backend test timed out");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unexpected error:\n\n{ex.Message}",
                    "Test Backend",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Logger.Error("Backend test error", ex);
            }
            finally
            {
                btnTestBackend.Enabled = true;
                btnTestBackend.Text = "Test Connection";
            }
        }

        /// <summary>
        /// Save settings and close
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate URL
                if (!string.IsNullOrEmpty(txtBackendUrl.Text))
                {
                    if (!Uri.TryCreate(txtBackendUrl.Text, UriKind.Absolute, out var uri) ||
                        (uri.Scheme != "http" && uri.Scheme != "https"))
                    {
                        MessageBox.Show(
                            "Please enter a valid HTTP or HTTPS URL for the backend server.",
                            "Validation Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        txtBackendUrl.Focus();
                        return;
                    }
                }

                // Update configuration
                _configuration.AutoStart = chkAutoStart.Checked;
                _configuration.BackendUrl = txtBackendUrl.Text.Trim();
                _configuration.StatusIntervalSeconds = (int)numMonitorInterval.Value;
                _configuration.ReportIntervalSeconds = (int)numReportInterval.Value;
                _configuration.MonitorCpu = chkMonitorCpu.Checked;
                _configuration.MonitorDisplays = chkMonitorDisplay.Checked;
                _configuration.MonitorRemoteSessions = chkMonitorRemote.Checked;

                if (Enum.TryParse<Logger.Level>(cmbLogLevel.SelectedItem?.ToString(), out var logLevel))
                {
                    _configuration.LogLevelValue = (int)logLevel;
                }

                // Collect EPREL mappings from list
                _configuration.EprelMappings.Clear();
                int monitorIndex = 0;
                foreach (ListViewItem item in lstHardware.Items)
                {
                    if (item.Tag?.ToString() == "Monitor")
                    {
                        var eprel = item.SubItems[3].Text;
                        if (!string.IsNullOrEmpty(eprel) && eprel != "Click to set..." && eprel != "N/A")
                        {
                            _configuration.EprelMappings.Add(new EprelMapping
                            {
                                EprelNumber = eprel,
                                HardwareType = EprelHardwareType.Monitor,
                                Id = $"monitor_{monitorIndex}"
                            });
                        }
                        monitorIndex++;
                    }
                }

                // Save configuration
                _configManager.Configuration = _configuration;
                _configManager.Save();

                // Update autostart registry
                UpdateAutoStart(_configuration.AutoStart);

                Logger.Info("Settings saved successfully");
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                Logger.Error("Error saving settings", ex);
                MessageBox.Show(
                    $"Failed to save settings:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Update Windows autostart registry
        /// </summary>
        private void UpdateAutoStart(bool enabled)
        {
            try
            {
                const string keyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
                const string valueName = "WinEcoSensor";

                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(keyName, true))
                {
                    if (key == null)
                        return;

                    if (enabled)
                    {
                        var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        key.SetValue(valueName, $"\"{exePath}\"");
                        Logger.Info("Autostart enabled in registry");
                    }
                    else
                    {
                        key.DeleteValue(valueName, false);
                        Logger.Info("Autostart disabled in registry");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"Failed to update autostart: {ex.Message}");
            }
        }

        /// <summary>
        /// Cancel and close
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Refresh hardware list
        /// </summary>
        private void btnRefreshHardware_Click(object sender, EventArgs e)
        {
            _hardwareMonitor = null;
            _displayMonitor?.Dispose();
            LoadHardwareInfo();
        }

        /// <summary>
        /// Clean up
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _hardwareMonitor = null;
            _displayMonitor?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
