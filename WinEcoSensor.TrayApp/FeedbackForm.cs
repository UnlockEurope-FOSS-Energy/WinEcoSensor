// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2026 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using WinEcoSensor.Common.Utilities;

namespace WinEcoSensor.TrayApp
{
    /// <summary>
    /// Feedback form for user feedback submission
    /// </summary>
    public class FeedbackForm : Form
    {
        // Form controls
        private TextBox _usernameTextBox;
        private TextBox _emailTextBox;
        private TextBox _mobileTextBox;
        private TextBox _firstNameTextBox;
        private TextBox _lastNameTextBox;
        private ComboBox _feedbackTypeComboBox;
        private TextBox _feedbackTextBox;
        private ComboBox _fileTypeComboBox;
        private ListBox _attachmentsListBox;
        private Button _attachButton;
        private Button _removeAttachmentButton;
        private Button _submitButton;
        private Button _cancelButton;

        // Attached files
        private List<AttachedFile> _attachedFiles = new List<AttachedFile>();

        // Feedback types
        private readonly string[] _feedbackTypes = new[]
        {
            "Praise",
            "Service Request",
            "Change Request",
            "Bug",
            "Cosmetic",
            "Nice to have"
        };

        // File types
        private readonly string[] _fileTypes = new[]
        {
            "Energy Invoice",
            "Hardware Specification",
            "QR Code",
            "Other"
        };

        public FeedbackForm()
        {
            InitializeComponent();
            LoadUserInfo();
        }

        private void InitializeComponent()
        {
            this.Text = "WinEcoSensor - Feedback";
            this.Size = new Size(500, 680);
            this.MinimumSize = new Size(450, 600);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                AutoScroll = true
            };

            int yPos = 10;
            int labelWidth = 120;
            int controlWidth = 320;
            int rowHeight = 32;

            // Title
            var titleLabel = new Label
            {
                Text = "Submit Feedback",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 215),
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            panel.Controls.Add(titleLabel);
            yPos += 45;

            // Username (readonly)
            AddLabel(panel, "Username:", 20, yPos);
            _usernameTextBox = AddTextBox(panel, labelWidth + 20, yPos, controlWidth, true);
            yPos += rowHeight;

            // Email
            AddLabel(panel, "Email:", 20, yPos);
            _emailTextBox = AddTextBox(panel, labelWidth + 20, yPos, controlWidth, false);
            yPos += rowHeight;

            // Mobile
            AddLabel(panel, "Mobile:", 20, yPos);
            _mobileTextBox = AddTextBox(panel, labelWidth + 20, yPos, controlWidth, false);
            // Use tooltip for placeholder hint (PlaceholderText not available in .NET Framework 4.8)
            var toolTip = new ToolTip();
            toolTip.SetToolTip(_mobileTextBox, "e.g. +43 664 1234567");
            yPos += rowHeight;

            // First Name
            AddLabel(panel, "First Name:", 20, yPos);
            _firstNameTextBox = AddTextBox(panel, labelWidth + 20, yPos, controlWidth, false);
            yPos += rowHeight;

            // Last Name
            AddLabel(panel, "Last Name:", 20, yPos);
            _lastNameTextBox = AddTextBox(panel, labelWidth + 20, yPos, controlWidth, false);
            yPos += rowHeight + 10;

            // Feedback Type
            AddLabel(panel, "Feedback Type:", 20, yPos);
            _feedbackTypeComboBox = new ComboBox
            {
                Location = new Point(labelWidth + 20, yPos),
                Size = new Size(controlWidth, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _feedbackTypeComboBox.Items.AddRange(_feedbackTypes);
            _feedbackTypeComboBox.SelectedIndex = 0;
            panel.Controls.Add(_feedbackTypeComboBox);
            yPos += rowHeight + 10;

            // Feedback Text
            AddLabel(panel, "Description:", 20, yPos);
            yPos += 22;
            _feedbackTextBox = new TextBox
            {
                Location = new Point(20, yPos),
                Size = new Size(controlWidth + labelWidth, 100),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            panel.Controls.Add(_feedbackTextBox);
            yPos += 110;

            // File Attachments Section
            var attachLabel = new Label
            {
                Text = "Attachments",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(200, 200, 200),
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            panel.Controls.Add(attachLabel);
            yPos += 28;

            // File Type
            AddLabel(panel, "File Type:", 20, yPos);
            _fileTypeComboBox = new ComboBox
            {
                Location = new Point(labelWidth + 20, yPos),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _fileTypeComboBox.Items.AddRange(_fileTypes);
            _fileTypeComboBox.SelectedIndex = 0;
            panel.Controls.Add(_fileTypeComboBox);

            // Attach Button
            _attachButton = new Button
            {
                Text = "Attach File...",
                Location = new Point(labelWidth + 180, yPos),
                Size = new Size(100, 25),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _attachButton.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            _attachButton.Click += AttachButton_Click;
            panel.Controls.Add(_attachButton);
            yPos += rowHeight;

            // Attachments List
            _attachmentsListBox = new ListBox
            {
                Location = new Point(20, yPos),
                Size = new Size(controlWidth + labelWidth - 90, 80),
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            panel.Controls.Add(_attachmentsListBox);

            // Remove Attachment Button
            _removeAttachmentButton = new Button
            {
                Text = "Remove",
                Location = new Point(controlWidth + labelWidth - 60, yPos),
                Size = new Size(80, 25),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _removeAttachmentButton.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            _removeAttachmentButton.Click += RemoveAttachmentButton_Click;
            panel.Controls.Add(_removeAttachmentButton);
            yPos += 95;

            // Buttons
            _submitButton = new Button
            {
                Text = "Submit",
                Location = new Point(120, yPos),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };
            _submitButton.FlatAppearance.BorderColor = Color.FromArgb(0, 100, 180);
            _submitButton.Click += SubmitButton_Click;
            panel.Controls.Add(_submitButton);

            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(240, yPos),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10f)
            };
            _cancelButton.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            _cancelButton.Click += (s, e) => this.Close();
            panel.Controls.Add(_cancelButton);

            this.Controls.Add(panel);
            this.AcceptButton = _submitButton;
            this.CancelButton = _cancelButton;
        }

        private Label AddLabel(Panel panel, string text, int x, int y)
        {
            var label = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(200, 200, 200),
                AutoSize = true,
                Location = new Point(x, y + 3)
            };
            panel.Controls.Add(label);
            return label;
        }

        private TextBox AddTextBox(Panel panel, int x, int y, int width, bool readOnly)
        {
            var textBox = new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 25),
                BackColor = readOnly ? Color.FromArgb(40, 40, 40) : Color.FromArgb(50, 50, 50),
                ForeColor = readOnly ? Color.FromArgb(150, 150, 150) : Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = readOnly
            };
            panel.Controls.Add(textBox);
            return textBox;
        }

        private void LoadUserInfo()
        {
            _usernameTextBox.Text = $"{Environment.UserDomainName}\\{Environment.UserName}";

            // Try to load from saved user data
            var userData = UserDataForm.GetCurrentUserData();
            if (userData != null)
            {
                if (!string.IsNullOrEmpty(userData.Email))
                    _emailTextBox.Text = userData.Email;
                if (!string.IsNullOrEmpty(userData.MobilePhone))
                    _mobileTextBox.Text = userData.MobilePhone;
                if (!string.IsNullOrEmpty(userData.FirstName))
                    _firstNameTextBox.Text = userData.FirstName;
                if (!string.IsNullOrEmpty(userData.LastName))
                    _lastNameTextBox.Text = userData.LastName;
            }
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

        private void AttachButton_Click(object sender, EventArgs e)
        {
            using (var openDialog = new OpenFileDialog())
            {
                openDialog.Title = "Select File to Attach";
                openDialog.Filter = "All Files (*.*)|*.*|" +
                                   "Images (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp|" +
                                   "Documents (*.pdf;*.doc;*.docx;*.txt)|*.pdf;*.doc;*.docx;*.txt|" +
                                   "Spreadsheets (*.xls;*.xlsx;*.csv)|*.xls;*.xlsx;*.csv";
                openDialog.Multiselect = false;

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    var fileInfo = new FileInfo(openDialog.FileName);

                    // Check file size (max 10 MB)
                    if (fileInfo.Length > 10 * 1024 * 1024)
                    {
                        MessageBox.Show(
                            "File size exceeds 10 MB limit.",
                            "File Too Large",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }

                    var attachedFile = new AttachedFile
                    {
                        FilePath = openDialog.FileName,
                        FileName = Path.GetFileName(openDialog.FileName),
                        FileType = _fileTypeComboBox.SelectedItem?.ToString() ?? "Other",
                        FileSize = fileInfo.Length
                    };

                    _attachedFiles.Add(attachedFile);
                    UpdateAttachmentsList();
                }
            }
        }

        private void RemoveAttachmentButton_Click(object sender, EventArgs e)
        {
            if (_attachmentsListBox.SelectedIndex >= 0)
            {
                _attachedFiles.RemoveAt(_attachmentsListBox.SelectedIndex);
                UpdateAttachmentsList();
            }
        }

        private void UpdateAttachmentsList()
        {
            _attachmentsListBox.Items.Clear();
            foreach (var file in _attachedFiles)
            {
                string sizeStr = file.FileSize < 1024
                    ? $"{file.FileSize} B"
                    : file.FileSize < 1024 * 1024
                        ? $"{file.FileSize / 1024.0:F1} KB"
                        : $"{file.FileSize / (1024.0 * 1024.0):F1} MB";

                _attachmentsListBox.Items.Add($"[{file.FileType}] {file.FileName} ({sizeStr})");
            }
        }

        private void SubmitButton_Click(object sender, EventArgs e)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(_emailTextBox.Text))
            {
                MessageBox.Show("Please enter your email address.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _emailTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(_firstNameTextBox.Text))
            {
                MessageBox.Show("Please enter your first name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _firstNameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(_lastNameTextBox.Text))
            {
                MessageBox.Show("Please enter your last name.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _lastNameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(_feedbackTextBox.Text))
            {
                MessageBox.Show("Please enter your feedback description.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _feedbackTextBox.Focus();
                return;
            }

            // Create feedback data
            var feedback = new FeedbackData
            {
                Username = _usernameTextBox.Text,
                Email = _emailTextBox.Text,
                Mobile = _mobileTextBox.Text,
                FirstName = _firstNameTextBox.Text,
                LastName = _lastNameTextBox.Text,
                FeedbackType = _feedbackTypeComboBox.SelectedItem?.ToString() ?? "Other",
                Description = _feedbackTextBox.Text,
                Attachments = new List<AttachedFile>(_attachedFiles),
                SubmittedAt = DateTime.Now,
                ComputerName = Environment.MachineName
            };

            // Save feedback locally (in real app, would send to server)
            SaveFeedback(feedback);

            MessageBox.Show(
                "Thank you for your feedback!\n\n" +
                "Your feedback has been saved and will be reviewed by our team.",
                "Feedback Submitted",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void SaveFeedback(FeedbackData feedback)
        {
            try
            {
                // Save to local feedback folder
                string feedbackFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "WinEcoSensor", "Feedback");

                Directory.CreateDirectory(feedbackFolder);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string feedbackFile = Path.Combine(feedbackFolder, $"Feedback_{timestamp}.txt");

                using (var writer = new StreamWriter(feedbackFile))
                {
                    writer.WriteLine("=== WinEcoSensor Feedback ===");
                    writer.WriteLine($"Submitted: {feedback.SubmittedAt:yyyy-MM-dd HH:mm:ss}");
                    writer.WriteLine($"Computer: {feedback.ComputerName}");
                    writer.WriteLine($"Username: {feedback.Username}");
                    writer.WriteLine();
                    writer.WriteLine("--- Contact Information ---");
                    writer.WriteLine($"Name: {feedback.FirstName} {feedback.LastName}");
                    writer.WriteLine($"Email: {feedback.Email}");
                    writer.WriteLine($"Mobile: {feedback.Mobile}");
                    writer.WriteLine();
                    writer.WriteLine("--- Feedback ---");
                    writer.WriteLine($"Type: {feedback.FeedbackType}");
                    writer.WriteLine();
                    writer.WriteLine("Description:");
                    writer.WriteLine(feedback.Description);
                    writer.WriteLine();

                    if (feedback.Attachments.Count > 0)
                    {
                        writer.WriteLine("--- Attachments ---");
                        foreach (var att in feedback.Attachments)
                        {
                            writer.WriteLine($"- [{att.FileType}] {att.FileName} ({att.FilePath})");
                        }
                    }
                }

                // Copy attachments
                if (feedback.Attachments.Count > 0)
                {
                    string attachFolder = Path.Combine(feedbackFolder, $"Attachments_{timestamp}");
                    Directory.CreateDirectory(attachFolder);

                    foreach (var att in feedback.Attachments)
                    {
                        string destPath = Path.Combine(attachFolder, att.FileName);
                        File.Copy(att.FilePath, destPath, true);
                    }
                }

                Logger.Info($"Feedback submitted: {feedback.FeedbackType} from {feedback.Email}");
            }
            catch (Exception ex)
            {
                Logger.Error("Error saving feedback", ex);
            }
        }

        /// <summary>
        /// Attached file information
        /// </summary>
        private class AttachedFile
        {
            public string FilePath { get; set; }
            public string FileName { get; set; }
            public string FileType { get; set; }
            public long FileSize { get; set; }
        }

        /// <summary>
        /// Feedback data structure
        /// </summary>
        private class FeedbackData
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public string Mobile { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string FeedbackType { get; set; }
            public string Description { get; set; }
            public List<AttachedFile> Attachments { get; set; }
            public DateTime SubmittedAt { get; set; }
            public string ComputerName { get; set; }
        }
    }
}
