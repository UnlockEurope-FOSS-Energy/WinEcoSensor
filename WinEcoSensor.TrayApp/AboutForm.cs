// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2026 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace WinEcoSensor.TrayApp
{
    /// <summary>
    /// About dialog with clickable links
    /// </summary>
    public class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            this.Text = "About WinEcoSensor";
            this.Size = new Size(450, 420);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            int yPos = 15;

            // Title
            var titleLabel = new Label
            {
                Text = "WinEcoSensor",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 215),
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            panel.Controls.Add(titleLabel);
            yPos += 40;

            // Subtitle
            var subtitleLabel = new Label
            {
                Text = "Windows Eco Energy Sensor",
                Font = new Font("Segoe UI", 11f),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            panel.Controls.Add(subtitleLabel);
            yPos += 30;

            // Version
            var versionLabel = new Label
            {
                Text = $"Version {version}",
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(150, 150, 150),
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            panel.Controls.Add(versionLabel);
            yPos += 35;

            // Description
            var descLabel = new Label
            {
                Text = "Monitors user presence, display activity, and energy-relevant\nstates on Windows systems to support energy-efficient operations.",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(200, 200, 200),
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            panel.Controls.Add(descLabel);
            yPos += 50;

            // Copyright
            var copyrightLabel = new Label
            {
                Text = "Copyright © 2026 Unlock Europe\nFOSS Energy Initiative",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(180, 180, 180),
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            panel.Controls.Add(copyrightLabel);
            yPos += 45;

            // License link
            var licenseLabel = new Label
            {
                Text = "Licensed under:",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(150, 150, 150),
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            panel.Controls.Add(licenseLabel);

            var licenseLink = new LinkLabel
            {
                Text = "European Union Public License (EUPL-1.2)",
                Font = new Font("Segoe UI", 9f),
                AutoSize = true,
                Location = new Point(115, yPos),
                LinkColor = Color.FromArgb(100, 180, 255),
                ActiveLinkColor = Color.FromArgb(150, 200, 255),
                VisitedLinkColor = Color.FromArgb(100, 180, 255)
            };
            licenseLink.LinkClicked += (s, e) => OpenUrl("https://eupl.eu/");
            panel.Controls.Add(licenseLink);
            yPos += 30;

            // GitHub link
            var githubLabel = new Label
            {
                Text = "GitHub:",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(150, 150, 150),
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            panel.Controls.Add(githubLabel);

            var githubLink = new LinkLabel
            {
                Text = "https://github.com/UnlockEurope-FOSS-Energy/WinEcoSensor",
                Font = new Font("Segoe UI", 9f),
                AutoSize = true,
                Location = new Point(70, yPos),
                LinkColor = Color.FromArgb(100, 180, 255),
                ActiveLinkColor = Color.FromArgb(150, 200, 255),
                VisitedLinkColor = Color.FromArgb(100, 180, 255)
            };
            githubLink.LinkClicked += (s, e) => OpenUrl("https://github.com/UnlockEurope-FOSS-Energy/WinEcoSensor");
            panel.Controls.Add(githubLink);
            yPos += 35;

            // Help Desk section
            var helpDeskTitleLabel = new Label
            {
                Text = "Help Desk / Service Management:",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(200, 200, 200),
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            panel.Controls.Add(helpDeskTitleLabel);
            yPos += 22;

            var emailLink = new LinkLabel
            {
                Text = "ServiceManagement@procon.co.at",
                Font = new Font("Segoe UI", 9f),
                AutoSize = true,
                Location = new Point(20, yPos),
                LinkColor = Color.FromArgb(100, 180, 255),
                ActiveLinkColor = Color.FromArgb(150, 200, 255),
                VisitedLinkColor = Color.FromArgb(100, 180, 255)
            };
            emailLink.LinkClicked += (s, e) => OpenUrl("mailto:ServiceManagement@procon.co.at");
            panel.Controls.Add(emailLink);
            yPos += 40;

            // OK button
            var okButton = new Button
            {
                Text = "OK",
                Size = new Size(80, 30),
                Location = new Point(175, yPos),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };
            okButton.FlatAppearance.BorderColor = Color.FromArgb(0, 100, 180);
            panel.Controls.Add(okButton);

            this.Controls.Add(panel);
            this.AcceptButton = okButton;
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not open link:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
