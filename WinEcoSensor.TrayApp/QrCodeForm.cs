// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2026 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinEcoSensor.TrayApp
{
    /// <summary>
    /// Form displaying QR code with encrypted device information
    /// </summary>
    public class QrCodeForm : Form
    {
        private PictureBox _qrCodePictureBox;
        private Label _titleLabel;
        private Label _computerLabel;
        private Label _userLabel;
        private Label _deviceIdLabel;
        private Label _timestampLabel;
        private Label _infoLabel;
        private Button _copyButton;
        private Button _refreshButton;
        private string _deviceId;
        private string _encryptedData;

        public QrCodeForm()
        {
            InitializeComponent();
            GenerateQrCode();
        }

        private void InitializeComponent()
        {
            this.Text = "WinEcoSensor - Device QR Code";
            this.Size = new Size(400, 560);
            this.MinimumSize = new Size(350, 520);
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            // Title
            _titleLabel = new Label
            {
                Text = "Device Identification",
                Font = new Font("Segoe UI Semibold", 14f),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(0, 120, 215)
            };

            // QR Code container
            var qrPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 250,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(30, 30, 30)
            };

            _qrCodePictureBox = new PictureBox
            {
                Size = new Size(220, 220),
                Location = new Point((400 - 220) / 2 - 8, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            qrPanel.Controls.Add(_qrCodePictureBox);

            // Info panel
            var infoPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 10, 20, 10),
                BackColor = Color.FromArgb(40, 40, 40)
            };

            _computerLabel = new Label
            {
                Text = "Computer: " + Environment.MachineName,
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(200, 200, 200),
                AutoSize = false,
                Size = new Size(350, 22),
                Location = new Point(15, 10)
            };

            _userLabel = new Label
            {
                Text = "User: " + Environment.UserName,
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(200, 200, 200),
                AutoSize = false,
                Size = new Size(350, 22),
                Location = new Point(15, 32)
            };

            _deviceIdLabel = new Label
            {
                Text = "Device ID: ---",
                Font = new Font("Consolas", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 200, 255),
                AutoSize = false,
                Size = new Size(350, 22),
                Location = new Point(15, 54)
            };

            _timestampLabel = new Label
            {
                Text = "Generated: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"),
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(150, 150, 150),
                AutoSize = false,
                Size = new Size(350, 20),
                Location = new Point(15, 80)
            };

            _infoLabel = new Label
            {
                Text = "Scan with MGIP Management Tools\nDevice ID can be entered manually if scanning fails.",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(120, 120, 120),
                AutoSize = false,
                Size = new Size(350, 40),
                Location = new Point(15, 108)
            };

            // Buttons
            _copyButton = new Button
            {
                Text = "Copy Device ID",
                Size = new Size(120, 32),
                Location = new Point(50, 158),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _copyButton.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            _copyButton.Click += CopyButton_Click;

            _refreshButton = new Button
            {
                Text = "Refresh",
                Size = new Size(100, 32),
                Location = new Point(190, 158),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _refreshButton.FlatAppearance.BorderColor = Color.FromArgb(0, 100, 180);
            _refreshButton.Click += RefreshButton_Click;

            infoPanel.Controls.Add(_computerLabel);
            infoPanel.Controls.Add(_userLabel);
            infoPanel.Controls.Add(_deviceIdLabel);
            infoPanel.Controls.Add(_timestampLabel);
            infoPanel.Controls.Add(_infoLabel);
            infoPanel.Controls.Add(_copyButton);
            infoPanel.Controls.Add(_refreshButton);

            this.Controls.Add(infoPanel);
            this.Controls.Add(qrPanel);
            this.Controls.Add(_titleLabel);
        }

        /// <summary>
        /// Set the form icon
        /// </summary>
        public void SetIcon(Icon icon)
        {
            if (icon != null)
            {
                this.Icon = icon;
            }
        }

        private void GenerateQrCode()
        {
            try
            {
                string computerName = Environment.MachineName;
                string userName = Environment.UserName;

                // Create device ID
                _deviceId = QrCodeGenerator.CreateDeviceId(computerName, userName);
                _encryptedData = QrCodeGenerator.EncryptDeviceInfo(computerName, userName);

                // Generate QR code with device ID
                var qrBitmap = QrCodeGenerator.Generate(computerName, userName, 8);

                // Dispose old image if exists
                if (_qrCodePictureBox.Image != null)
                {
                    _qrCodePictureBox.Image.Dispose();
                }

                _qrCodePictureBox.Image = qrBitmap;

                // Update labels
                _computerLabel.Text = "Computer: " + computerName;
                _userLabel.Text = "User: " + userName;
                _deviceIdLabel.Text = "Device ID: " + _deviceId;
                _timestampLabel.Text = "Generated: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error generating QR code:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void CopyButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_deviceId))
            {
                try
                {
                    Clipboard.SetText(_deviceId);

                    // Visual feedback
                    var originalText = _copyButton.Text;
                    _copyButton.Text = "Copied!";
                    _copyButton.BackColor = Color.FromArgb(0, 153, 51);

                    var timer = new Timer { Interval = 1500 };
                    timer.Tick += (s, args) =>
                    {
                        _copyButton.Text = originalText;
                        _copyButton.BackColor = Color.FromArgb(60, 60, 60);
                        timer.Stop();
                        timer.Dispose();
                    };
                    timer.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Could not copy to clipboard:\n\n" + ex.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            GenerateQrCode();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_qrCodePictureBox?.Image != null)
                {
                    _qrCodePictureBox.Image.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}
