// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2026 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using WinEcoSensor.Common.Utilities;

namespace WinEcoSensor.TrayApp
{
    /// <summary>
    /// User data management form for storing personal and usage information
    /// </summary>
    public class UserDataForm : Form
    {
        // User ID
        private Label _userIdValueLabel;
        private Label _usernameValueLabel;

        // Owner fields
        private TextBox _firstNameTextBox;
        private TextBox _lastNameTextBox;
        private TextBox _emailTextBox;
        private TextBox _mobileTextBox;

        // Location fields
        private TextBox _roomDescriptionTextBox;
        private TextBox _addressTextBox;
        private ComboBox _countryComboBox;
        private ComboBox _holidayCalendarComboBox;

        // Usage time fields (from/to times per day)
        private DateTimePicker _mondayFrom;
        private DateTimePicker _mondayTo;
        private DateTimePicker _tuesdayFrom;
        private DateTimePicker _tuesdayTo;
        private DateTimePicker _wednesdayFrom;
        private DateTimePicker _wednesdayTo;
        private DateTimePicker _thursdayFrom;
        private DateTimePicker _thursdayTo;
        private DateTimePicker _fridayFrom;
        private DateTimePicker _fridayTo;
        private DateTimePicker _saturdayFrom;
        private DateTimePicker _saturdayTo;
        private DateTimePicker _sundayFrom;
        private DateTimePicker _sundayTo;
        private Label _weeklyTotalLabel;

        // Buttons
        private Button _saveButton;
        private Button _cancelButton;
        private Button _sendReportButton;

        // Data
        private UserData _userData;
        private static readonly string DataFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WinEcoSensor", "UserData.xml");

        // Countries list
        private readonly string[] _countries = new[]
        {
            "Austria", "Germany", "Switzerland", "Belgium", "Bulgaria", "Croatia",
            "Cyprus", "Czech Republic", "Denmark", "Estonia", "Finland", "France",
            "Greece", "Hungary", "Ireland", "Italy", "Latvia", "Lithuania",
            "Luxembourg", "Malta", "Netherlands", "Poland", "Portugal", "Romania",
            "Slovakia", "Slovenia", "Spain", "Sweden", "United Kingdom", "Norway"
        };

        // Holiday calendars
        private readonly string[] _holidayCalendars = new[]
        {
            "Austria - National",
            "Austria - Vienna",
            "Austria - Salzburg",
            "Austria - Tyrol",
            "Germany - National",
            "Germany - Bavaria",
            "Germany - Berlin",
            "Germany - NRW",
            "Switzerland - National",
            "Switzerland - Zurich",
            "EU - Common Holidays"
        };

        public UserDataForm()
        {
            LoadUserData();
            InitializeComponent();
            PopulateForm();
        }

        private void InitializeComponent()
        {
            this.Text = "WinEcoSensor - User Profile";
            this.Size = new Size(600, 780);
            this.MinimumSize = new Size(550, 740);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                AutoScroll = true
            };

            int yPos = 10;

            // Title
            var titleLabel = new Label
            {
                Text = "User Profile",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 215),
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            mainPanel.Controls.Add(titleLabel);
            yPos += 50;

            // === User Identification Section ===
            yPos = AddSectionHeader(mainPanel, "User Identification", yPos);

            AddLabel(mainPanel, "User ID:", 20, yPos);
            _userIdValueLabel = new Label
            {
                Text = GenerateUserId(),
                Font = new Font("Consolas", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 200, 255),
                Location = new Point(140, yPos),
                AutoSize = true
            };
            mainPanel.Controls.Add(_userIdValueLabel);
            yPos += 28;

            AddLabel(mainPanel, "Username:", 20, yPos);
            _usernameValueLabel = new Label
            {
                Text = GetFullUsername(),
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(180, 180, 180),
                Location = new Point(140, yPos),
                AutoSize = true
            };
            mainPanel.Controls.Add(_usernameValueLabel);
            yPos += 35;

            // === Owner Section ===
            yPos = AddSectionHeader(mainPanel, "Owner Information", yPos);

            AddLabel(mainPanel, "First Name:", 20, yPos);
            _firstNameTextBox = AddTextBox(mainPanel, 140, yPos, 380);
            yPos += 32;

            AddLabel(mainPanel, "Last Name:", 20, yPos);
            _lastNameTextBox = AddTextBox(mainPanel, 140, yPos, 380);
            yPos += 32;

            AddLabel(mainPanel, "Email:", 20, yPos);
            _emailTextBox = AddTextBox(mainPanel, 140, yPos, 380);
            yPos += 32;

            AddLabel(mainPanel, "Mobile Phone:", 20, yPos);
            _mobileTextBox = AddTextBox(mainPanel, 140, yPos, 380);
            yPos += 40;

            // === Location Section ===
            yPos = AddSectionHeader(mainPanel, "Location", yPos);

            AddLabel(mainPanel, "Room:", 20, yPos);
            _roomDescriptionTextBox = AddTextBox(mainPanel, 140, yPos, 380);
            yPos += 32;

            AddLabel(mainPanel, "Address:", 20, yPos);
            _addressTextBox = AddTextBox(mainPanel, 140, yPos, 380);
            var addressHint = new Label
            {
                Text = "(Optional)",
                Font = new Font("Segoe UI", 8f, FontStyle.Italic),
                ForeColor = Color.FromArgb(120, 120, 120),
                Location = new Point(525, yPos + 3),
                AutoSize = true
            };
            mainPanel.Controls.Add(addressHint);
            yPos += 32;

            AddLabel(mainPanel, "Country:", 20, yPos);
            _countryComboBox = new ComboBox
            {
                Location = new Point(140, yPos),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _countryComboBox.Items.AddRange(_countries);
            _countryComboBox.SelectedIndex = 0; // Austria
            mainPanel.Controls.Add(_countryComboBox);
            yPos += 32;

            AddLabel(mainPanel, "Holiday Calendar:", 20, yPos);
            _holidayCalendarComboBox = new ComboBox
            {
                Location = new Point(140, yPos),
                Size = new Size(250, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _holidayCalendarComboBox.Items.AddRange(_holidayCalendars);
            _holidayCalendarComboBox.SelectedIndex = 0;
            mainPanel.Controls.Add(_holidayCalendarComboBox);
            yPos += 45;

            // === Usage Schedule Section ===
            yPos = AddSectionHeader(mainPanel, "Regular Working Hours", yPos);

            // Headers
            var fromHeader = new Label
            {
                Text = "From",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(180, 180, 180),
                Location = new Point(120, yPos),
                AutoSize = true
            };
            mainPanel.Controls.Add(fromHeader);

            var toHeader = new Label
            {
                Text = "To",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(180, 180, 180),
                Location = new Point(210, yPos),
                AutoSize = true
            };
            mainPanel.Controls.Add(toHeader);
            yPos += 22;

            // Weekday time pickers
            (_mondayFrom, _mondayTo) = AddTimeRow(mainPanel, "Monday", 20, yPos, false);
            yPos += 28;
            (_tuesdayFrom, _tuesdayTo) = AddTimeRow(mainPanel, "Tuesday", 20, yPos, false);
            yPos += 28;
            (_wednesdayFrom, _wednesdayTo) = AddTimeRow(mainPanel, "Wednesday", 20, yPos, false);
            yPos += 28;
            (_thursdayFrom, _thursdayTo) = AddTimeRow(mainPanel, "Thursday", 20, yPos, false);
            yPos += 28;
            (_fridayFrom, _fridayTo) = AddTimeRow(mainPanel, "Friday", 20, yPos, false);
            yPos += 28;
            (_saturdayFrom, _saturdayTo) = AddTimeRow(mainPanel, "Saturday", 20, yPos, true);
            yPos += 28;
            (_sundayFrom, _sundayTo) = AddTimeRow(mainPanel, "Sunday", 20, yPos, true);
            yPos += 35;

            // Weekly total
            var totalLabel = new Label
            {
                Text = "Max Weekly Usage:",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(200, 200, 200),
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            mainPanel.Controls.Add(totalLabel);

            _weeklyTotalLabel = new Label
            {
                Text = "0.0 hours",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 255, 100),
                AutoSize = true,
                Location = new Point(170, yPos - 2)
            };
            mainPanel.Controls.Add(_weeklyTotalLabel);
            yPos += 50;

            // === Buttons ===
            _saveButton = new Button
            {
                Text = "Save",
                Location = new Point(100, yPos),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            _saveButton.FlatAppearance.BorderColor = Color.FromArgb(0, 100, 180);
            _saveButton.Click += SaveButton_Click;
            mainPanel.Controls.Add(_saveButton);

            _sendReportButton = new Button
            {
                Text = "Send Report",
                Location = new Point(220, yPos),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(0, 153, 51),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f),
                Cursor = Cursors.Hand
            };
            _sendReportButton.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 40);
            _sendReportButton.Click += SendReportButton_Click;
            mainPanel.Controls.Add(_sendReportButton);

            _cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(360, yPos),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10f),
                Cursor = Cursors.Hand
            };
            _cancelButton.FlatAppearance.BorderColor = Color.FromArgb(80, 80, 80);
            _cancelButton.Click += (s, e) => this.Close();
            mainPanel.Controls.Add(_cancelButton);

            this.Controls.Add(mainPanel);
            this.AcceptButton = _saveButton;
            this.CancelButton = _cancelButton;

            // Wire up change events for total calculation
            _mondayFrom.ValueChanged += (s, e) => UpdateWeeklyTotal();
            _mondayTo.ValueChanged += (s, e) => UpdateWeeklyTotal();
            _tuesdayFrom.ValueChanged += (s, e) => UpdateWeeklyTotal();
            _tuesdayTo.ValueChanged += (s, e) => UpdateWeeklyTotal();
            _wednesdayFrom.ValueChanged += (s, e) => UpdateWeeklyTotal();
            _wednesdayTo.ValueChanged += (s, e) => UpdateWeeklyTotal();
            _thursdayFrom.ValueChanged += (s, e) => UpdateWeeklyTotal();
            _thursdayTo.ValueChanged += (s, e) => UpdateWeeklyTotal();
            _fridayFrom.ValueChanged += (s, e) => UpdateWeeklyTotal();
            _fridayTo.ValueChanged += (s, e) => UpdateWeeklyTotal();
            _saturdayFrom.ValueChanged += (s, e) => UpdateWeeklyTotal();
            _saturdayTo.ValueChanged += (s, e) => UpdateWeeklyTotal();
            _sundayFrom.ValueChanged += (s, e) => UpdateWeeklyTotal();
            _sundayTo.ValueChanged += (s, e) => UpdateWeeklyTotal();
        }

        private (DateTimePicker from, DateTimePicker to) AddTimeRow(Panel panel, string dayName, int x, int y, bool isWeekend)
        {
            var dayLabel = new Label
            {
                Text = dayName + ":",
                Font = new Font("Segoe UI", 9f),
                ForeColor = isWeekend ? Color.FromArgb(100, 100, 100) : Color.FromArgb(200, 200, 200),
                AutoSize = true,
                Location = new Point(x, y + 3)
            };
            panel.Controls.Add(dayLabel);

            var fromPicker = new DateTimePicker
            {
                Location = new Point(x + 100, y),
                Size = new Size(80, 25),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "HH:mm",
                ShowUpDown = true,
                Value = isWeekend ? DateTime.Today : DateTime.Today.AddHours(8), // 08:00 or 00:00
                CalendarForeColor = Color.White,
                CalendarMonthBackground = Color.FromArgb(50, 50, 50)
            };
            panel.Controls.Add(fromPicker);

            var toPicker = new DateTimePicker
            {
                Location = new Point(x + 190, y),
                Size = new Size(80, 25),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "HH:mm",
                ShowUpDown = true,
                Value = isWeekend ? DateTime.Today : DateTime.Today.AddHours(18), // 18:00 or 00:00
                CalendarForeColor = Color.White,
                CalendarMonthBackground = Color.FromArgb(50, 50, 50)
            };
            panel.Controls.Add(toPicker);

            return (fromPicker, toPicker);
        }

        private int AddSectionHeader(Panel panel, string text, int yPos)
        {
            var line1 = new Panel
            {
                BackColor = Color.FromArgb(60, 60, 60),
                Location = new Point(20, yPos + 8),
                Size = new Size(520, 1)
            };
            panel.Controls.Add(line1);

            var header = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 215),
                BackColor = Color.FromArgb(30, 30, 30),
                AutoSize = true,
                Location = new Point(30, yPos)
            };
            panel.Controls.Add(header);
            header.BringToFront();

            return yPos + 30;
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

        private TextBox AddTextBox(Panel panel, int x, int y, int width)
        {
            var textBox = new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 25),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            panel.Controls.Add(textBox);
            return textBox;
        }

        private string GetFullUsername()
        {
            try
            {
                // Try to get Azure AD / domain username
                string domainName = Environment.UserDomainName;
                string userName = Environment.UserName;

                // Check if it's an Azure AD user
                if (domainName.Equals("AzureAD", StringComparison.OrdinalIgnoreCase))
                {
                    return $"AzureAD\\{userName}";
                }

                // For local or domain users
                return $"{domainName}\\{userName}";
            }
            catch
            {
                return Environment.UserName;
            }
        }

        private string GenerateUserId()
        {
            string input = $"{Environment.MachineName}|{Environment.UserName}";
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                return $"USR-{hash[0]:X2}{hash[1]:X2}{hash[2]:X2}{hash[3]:X2}";
            }
        }

        private double CalculateDayHours(DateTimePicker from, DateTimePicker to)
        {
            var fromTime = from.Value.TimeOfDay;
            var toTime = to.Value.TimeOfDay;

            if (toTime <= fromTime)
                return 0;

            return (toTime - fromTime).TotalHours;
        }

        private void UpdateWeeklyTotal()
        {
            double total =
                CalculateDayHours(_mondayFrom, _mondayTo) +
                CalculateDayHours(_tuesdayFrom, _tuesdayTo) +
                CalculateDayHours(_wednesdayFrom, _wednesdayTo) +
                CalculateDayHours(_thursdayFrom, _thursdayTo) +
                CalculateDayHours(_fridayFrom, _fridayTo) +
                CalculateDayHours(_saturdayFrom, _saturdayTo) +
                CalculateDayHours(_sundayFrom, _sundayTo);

            _weeklyTotalLabel.Text = $"{total:F1} hours";
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

        private void LoadUserData()
        {
            try
            {
                if (File.Exists(DataFilePath))
                {
                    var serializer = new XmlSerializer(typeof(UserData));
                    using (var reader = new StreamReader(DataFilePath))
                    {
                        _userData = (UserData)serializer.Deserialize(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"Could not load user data: {ex.Message}");
            }

            if (_userData == null)
            {
                _userData = new UserData
                {
                    UserId = GenerateUserId(),
                    // Weekday defaults: 08:00 - 18:00
                    MondayFromTime = "08:00",
                    MondayToTime = "18:00",
                    TuesdayFromTime = "08:00",
                    TuesdayToTime = "18:00",
                    WednesdayFromTime = "08:00",
                    WednesdayToTime = "18:00",
                    ThursdayFromTime = "08:00",
                    ThursdayToTime = "18:00",
                    FridayFromTime = "08:00",
                    FridayToTime = "18:00",
                    // Weekend defaults: 00:00 - 00:00
                    SaturdayFromTime = "00:00",
                    SaturdayToTime = "00:00",
                    SundayFromTime = "00:00",
                    SundayToTime = "00:00",
                    Country = "Austria",
                    HolidayCalendar = "Austria - National"
                };
            }
        }

        private DateTime ParseTimeString(string timeStr)
        {
            if (string.IsNullOrEmpty(timeStr))
                return DateTime.Today;

            if (TimeSpan.TryParse(timeStr, out var ts))
                return DateTime.Today.Add(ts);

            return DateTime.Today;
        }

        private string TimeToString(DateTimePicker picker)
        {
            return picker.Value.ToString("HH:mm");
        }

        private void PopulateForm()
        {
            _userIdValueLabel.Text = _userData.UserId ?? GenerateUserId();
            _firstNameTextBox.Text = _userData.FirstName ?? "";
            _lastNameTextBox.Text = _userData.LastName ?? "";
            _emailTextBox.Text = _userData.Email ?? "";
            _mobileTextBox.Text = _userData.MobilePhone ?? "";
            _roomDescriptionTextBox.Text = _userData.RoomDescription ?? "";
            _addressTextBox.Text = _userData.Address ?? "";

            // Country
            int countryIndex = Array.IndexOf(_countries, _userData.Country);
            _countryComboBox.SelectedIndex = countryIndex >= 0 ? countryIndex : 0;

            // Holiday calendar
            int calendarIndex = Array.IndexOf(_holidayCalendars, _userData.HolidayCalendar);
            _holidayCalendarComboBox.SelectedIndex = calendarIndex >= 0 ? calendarIndex : 0;

            // Usage times
            _mondayFrom.Value = ParseTimeString(_userData.MondayFromTime);
            _mondayTo.Value = ParseTimeString(_userData.MondayToTime);
            _tuesdayFrom.Value = ParseTimeString(_userData.TuesdayFromTime);
            _tuesdayTo.Value = ParseTimeString(_userData.TuesdayToTime);
            _wednesdayFrom.Value = ParseTimeString(_userData.WednesdayFromTime);
            _wednesdayTo.Value = ParseTimeString(_userData.WednesdayToTime);
            _thursdayFrom.Value = ParseTimeString(_userData.ThursdayFromTime);
            _thursdayTo.Value = ParseTimeString(_userData.ThursdayToTime);
            _fridayFrom.Value = ParseTimeString(_userData.FridayFromTime);
            _fridayTo.Value = ParseTimeString(_userData.FridayToTime);
            _saturdayFrom.Value = ParseTimeString(_userData.SaturdayFromTime);
            _saturdayTo.Value = ParseTimeString(_userData.SaturdayToTime);
            _sundayFrom.Value = ParseTimeString(_userData.SundayFromTime);
            _sundayTo.Value = ParseTimeString(_userData.SundayToTime);

            UpdateWeeklyTotal();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            // Update data object
            _userData.UserId = _userIdValueLabel.Text;
            _userData.FirstName = _firstNameTextBox.Text.Trim();
            _userData.LastName = _lastNameTextBox.Text.Trim();
            _userData.Email = _emailTextBox.Text.Trim();
            _userData.MobilePhone = _mobileTextBox.Text.Trim();
            _userData.RoomDescription = _roomDescriptionTextBox.Text.Trim();
            _userData.Address = _addressTextBox.Text.Trim();
            _userData.Country = _countryComboBox.SelectedItem?.ToString() ?? "Austria";
            _userData.HolidayCalendar = _holidayCalendarComboBox.SelectedItem?.ToString() ?? "";

            // Save times
            _userData.MondayFromTime = TimeToString(_mondayFrom);
            _userData.MondayToTime = TimeToString(_mondayTo);
            _userData.TuesdayFromTime = TimeToString(_tuesdayFrom);
            _userData.TuesdayToTime = TimeToString(_tuesdayTo);
            _userData.WednesdayFromTime = TimeToString(_wednesdayFrom);
            _userData.WednesdayToTime = TimeToString(_wednesdayTo);
            _userData.ThursdayFromTime = TimeToString(_thursdayFrom);
            _userData.ThursdayToTime = TimeToString(_thursdayTo);
            _userData.FridayFromTime = TimeToString(_fridayFrom);
            _userData.FridayToTime = TimeToString(_fridayTo);
            _userData.SaturdayFromTime = TimeToString(_saturdayFrom);
            _userData.SaturdayToTime = TimeToString(_saturdayTo);
            _userData.SundayFromTime = TimeToString(_sundayFrom);
            _userData.SundayToTime = TimeToString(_sundayTo);

            _userData.LastModified = DateTime.Now;

            // Save to file
            try
            {
                string directory = Path.GetDirectoryName(DataFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var serializer = new XmlSerializer(typeof(UserData));
                using (var writer = new StreamWriter(DataFilePath))
                {
                    serializer.Serialize(writer, _userData);
                }

                Logger.Info($"User data saved for {_userData.Email}");

                MessageBox.Show(
                    "User profile saved successfully.",
                    "Saved",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.Error("Error saving user data", ex);
                MessageBox.Show(
                    $"Error saving user data:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void SendReportButton_Click(object sender, EventArgs e)
        {
            // First save the data
            SaveButton_Click(sender, e);

            // Prepare consumption report
            var report = CreateConsumptionReport();

            // Show confirmation
            var result = MessageBox.Show(
                "This will send your consumption report to the backend service.\n\n" +
                $"From: {_userData.Email}\n" +
                $"User: {_userData.FirstName} {_userData.LastName}\n\n" +
                "Do you want to continue?",
                "Send Report",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                SendConsumptionReport(report);
            }
        }

        private string CreateConsumptionReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== WinEcoSensor Consumption Report ===");
            sb.AppendLine();
            sb.AppendLine($"Report Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"User ID: {_userData.UserId}");
            sb.AppendLine($"Computer: {Environment.MachineName}");
            sb.AppendLine($"Username: {GetFullUsername()}");
            sb.AppendLine();
            sb.AppendLine("--- User Information ---");
            sb.AppendLine($"Name: {_userData.FirstName} {_userData.LastName}");
            sb.AppendLine($"Email: {_userData.Email}");
            sb.AppendLine($"Mobile: {_userData.MobilePhone}");
            sb.AppendLine();
            sb.AppendLine("--- Location ---");
            sb.AppendLine($"Room: {_userData.RoomDescription}");
            sb.AppendLine($"Address: {_userData.Address}");
            sb.AppendLine($"Country: {_userData.Country}");
            sb.AppendLine($"Holiday Calendar: {_userData.HolidayCalendar}");
            sb.AppendLine();
            sb.AppendLine("--- Weekly Working Hours ---");
            sb.AppendLine($"Monday:    {_userData.MondayFromTime} - {_userData.MondayToTime} ({CalculateDayHours(_mondayFrom, _mondayTo):F1}h)");
            sb.AppendLine($"Tuesday:   {_userData.TuesdayFromTime} - {_userData.TuesdayToTime} ({CalculateDayHours(_tuesdayFrom, _tuesdayTo):F1}h)");
            sb.AppendLine($"Wednesday: {_userData.WednesdayFromTime} - {_userData.WednesdayToTime} ({CalculateDayHours(_wednesdayFrom, _wednesdayTo):F1}h)");
            sb.AppendLine($"Thursday:  {_userData.ThursdayFromTime} - {_userData.ThursdayToTime} ({CalculateDayHours(_thursdayFrom, _thursdayTo):F1}h)");
            sb.AppendLine($"Friday:    {_userData.FridayFromTime} - {_userData.FridayToTime} ({CalculateDayHours(_fridayFrom, _fridayTo):F1}h)");
            sb.AppendLine($"Saturday:  {_userData.SaturdayFromTime} - {_userData.SaturdayToTime} ({CalculateDayHours(_saturdayFrom, _saturdayTo):F1}h)");
            sb.AppendLine($"Sunday:    {_userData.SundayFromTime} - {_userData.SundayToTime} ({CalculateDayHours(_sundayFrom, _sundayTo):F1}h)");
            sb.AppendLine();

            double weeklyTotal =
                CalculateDayHours(_mondayFrom, _mondayTo) +
                CalculateDayHours(_tuesdayFrom, _tuesdayTo) +
                CalculateDayHours(_wednesdayFrom, _wednesdayTo) +
                CalculateDayHours(_thursdayFrom, _thursdayTo) +
                CalculateDayHours(_fridayFrom, _fridayTo) +
                CalculateDayHours(_saturdayFrom, _saturdayTo) +
                CalculateDayHours(_sundayFrom, _sundayTo);

            sb.AppendLine($"Total Weekly Hours: {weeklyTotal:F1}h");
            sb.AppendLine();

            return sb.ToString();
        }

        private void SendConsumptionReport(string report)
        {
            try
            {
                // Save report locally first
                string reportFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "WinEcoSensor", "Reports");
                Directory.CreateDirectory(reportFolder);

                string reportFile = Path.Combine(reportFolder, $"ConsumptionReport_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                File.WriteAllText(reportFile, report);

                // Prepare email (using mailto: for now as SMTP requires configuration)
                string subject = Uri.EscapeDataString($"WinEcoSensor Consumption Report - {_userData.UserId}");
                string body = Uri.EscapeDataString(report);
                string mailto = $"mailto:ServiceManagement@procon.co.at?subject={subject}&body={body}";

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = mailto,
                    UseShellExecute = true
                });

                Logger.Info($"Consumption report prepared for {_userData.Email}");

                MessageBox.Show(
                    "Your email client has been opened with the report.\n\n" +
                    $"Report also saved to:\n{reportFile}",
                    "Report Ready",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.Error("Error sending consumption report", ex);
                MessageBox.Show(
                    $"Error preparing report:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Get the current user data (static method for other forms to access)
        /// </summary>
        public static UserData GetCurrentUserData()
        {
            try
            {
                if (File.Exists(DataFilePath))
                {
                    var serializer = new XmlSerializer(typeof(UserData));
                    using (var reader = new StreamReader(DataFilePath))
                    {
                        return (UserData)serializer.Deserialize(reader);
                    }
                }
            }
            catch { }

            return null;
        }
    }

    /// <summary>
    /// User data structure for serialization
    /// </summary>
    [Serializable]
    public class UserData
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MobilePhone { get; set; }
        public string RoomDescription { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string HolidayCalendar { get; set; }

        // Time-based schedule (stored as "HH:mm" strings)
        public string MondayFromTime { get; set; }
        public string MondayToTime { get; set; }
        public string TuesdayFromTime { get; set; }
        public string TuesdayToTime { get; set; }
        public string WednesdayFromTime { get; set; }
        public string WednesdayToTime { get; set; }
        public string ThursdayFromTime { get; set; }
        public string ThursdayToTime { get; set; }
        public string FridayFromTime { get; set; }
        public string FridayToTime { get; set; }
        public string SaturdayFromTime { get; set; }
        public string SaturdayToTime { get; set; }
        public string SundayFromTime { get; set; }
        public string SundayToTime { get; set; }

        public DateTime LastModified { get; set; }

        public double GetWeeklyTotalHours()
        {
            double total = 0;
            total += GetDayHours(MondayFromTime, MondayToTime);
            total += GetDayHours(TuesdayFromTime, TuesdayToTime);
            total += GetDayHours(WednesdayFromTime, WednesdayToTime);
            total += GetDayHours(ThursdayFromTime, ThursdayToTime);
            total += GetDayHours(FridayFromTime, FridayToTime);
            total += GetDayHours(SaturdayFromTime, SaturdayToTime);
            total += GetDayHours(SundayFromTime, SundayToTime);
            return total;
        }

        private double GetDayHours(string fromTime, string toTime)
        {
            if (string.IsNullOrEmpty(fromTime) || string.IsNullOrEmpty(toTime))
                return 0;

            if (TimeSpan.TryParse(fromTime, out var from) && TimeSpan.TryParse(toTime, out var to))
            {
                if (to > from)
                    return (to - from).TotalHours;
            }
            return 0;
        }
    }
}
