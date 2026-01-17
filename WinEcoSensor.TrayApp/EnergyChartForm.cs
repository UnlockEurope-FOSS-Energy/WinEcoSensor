// ============================================================================
// WinEcoSensor - Windows Eco Energy Sensor
// Copyright (c) 2026 Unlock Europe - FOSS Energy Initiative
// Licensed under the European Union Public License (EUPL-1.2)
// ============================================================================

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WinEcoSensor.Common.Models;

namespace WinEcoSensor.TrayApp
{
    /// <summary>
    /// Energy consumption chart form with statistics and energy tracking
    /// </summary>
    public class EnergyChartForm : Form
    {
        private System.Windows.Forms.Timer _updateTimer;
        private DoubleBufferedPanel _chartPanel;
        private Panel _legendPanel;
        private Panel _statsPanel;
        private Label _statsLabel;
        private Label _energyLabel;

        // Data storage
        private const int MaxDataPoints = 60; // 1 minute of data at 1s intervals
        private Queue<EnergyDataPoint> _dataPoints = new Queue<EnergyDataPoint>();

        // Statistics since app start
        private double _totalPowerSum = 0;
        private int _totalPowerCount = 0;
        private double _totalPowerMin = double.MaxValue;
        private double _totalPowerMax = double.MinValue;
        private double _totalEnergyWh = 0; // Cumulative energy in Watt-hours
        private DateTime _startTime;

        // Energy price in ct/kWh (Euro cents per kilowatt-hour)
        private const double EnergyPriceCentPerKwh = 25.0; // Default Austrian/German average

        // Colors for components
        private readonly Color _totalColor = Color.FromArgb(0, 120, 215);      // Blue
        private readonly Color _cpuColor = Color.FromArgb(232, 17, 35);        // Red
        private readonly Color _displayColor = Color.FromArgb(255, 185, 0);    // Yellow/Orange
        private readonly Color _gpuColor = Color.FromArgb(0, 153, 51);         // Green
        private readonly Color _diskColor = Color.FromArgb(142, 68, 173);      // Purple
        private readonly Color _memoryColor = Color.FromArgb(0, 188, 212);     // Cyan
        private readonly Color _baseColor = Color.FromArgb(158, 158, 158);     // Gray
        private readonly Color _avgColor = Color.FromArgb(255, 255, 255);      // White for average line
        private readonly Color _bandColor = Color.FromArgb(40, 0, 120, 215);   // Semi-transparent blue for band

        // Current data source
        private Func<EnergyState> _getEnergyState;

        public EnergyChartForm()
        {
            _startTime = DateTime.Now;
            InitializeComponent();
        }

        /// <summary>
        /// Set the data source function
        /// </summary>
        public void SetDataSource(Func<EnergyState> getEnergyState)
        {
            _getEnergyState = getEnergyState;
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

        private void InitializeComponent()
        {
            this.Text = "WinEcoSensor - Power Pointer";
            this.Size = new Size(950, 620);
            this.MinimumSize = new Size(700, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.DoubleBuffered = true;

            // Chart panel with double buffering
            _chartPanel = new DoubleBufferedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(40, 40, 40),
                Padding = new Padding(10)
            };
            _chartPanel.Paint += ChartPanel_Paint;
            _chartPanel.Resize += (s, e) => _chartPanel.Invalidate();

            // Energy consumption panel (bottom)
            var energyPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(25, 50, 25),
                Padding = new Padding(10, 5, 10, 5)
            };

            _energyLabel = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 255, 100),
                Text = "Total Energy: 0.000 Wh"
            };
            energyPanel.Controls.Add(_energyLabel);

            // Stats panel
            _statsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 35,
                BackColor = Color.FromArgb(35, 35, 35),
                Padding = new Padding(10, 5, 10, 5)
            };

            _statsLabel = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                Text = "Statistics: Waiting for data..."
            };
            _statsPanel.Controls.Add(_statsLabel);

            // Legend panel with colored boxes
            _legendPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(30, 30, 30),
                Padding = new Padding(10)
            };
            _legendPanel.Paint += LegendPanel_Paint;

            // Title label with start time
            var titleLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 45,
                Text = string.Format("Power Pointer (Watts) - Started: {0:dd.MM.yyyy HH:mm:ss}", _startTime),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Semibold", 13f),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(30, 30, 30)
            };

            this.Controls.Add(_chartPanel);
            this.Controls.Add(energyPanel);
            this.Controls.Add(_statsPanel);
            this.Controls.Add(_legendPanel);
            this.Controls.Add(titleLabel);

            // Update timer
            _updateTimer = new System.Windows.Forms.Timer
            {
                Interval = 1000
            };
            _updateTimer.Tick += UpdateTimer_Tick;
        }

        /// <summary>
        /// Paint the legend with colored boxes
        /// </summary>
        private void LegendPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (var font = new Font("Segoe UI", 9f))
            {
                // Stacked bar legend - components only (stacked from bottom to top)
                var legendItems = new[]
                {
                    new { Color = _baseColor, Label = "Base" },
                    new { Color = _memoryColor, Label = "Memory" },
                    new { Color = _diskColor, Label = "Disk" },
                    new { Color = _gpuColor, Label = "GPU" },
                    new { Color = _displayColor, Label = "Display" },
                    new { Color = _cpuColor, Label = "CPU" },
                    new { Color = _avgColor, Label = "Avg" }
                };

                int totalWidth = 0;
                foreach (var item in legendItems)
                {
                    totalWidth += 14 + (int)g.MeasureString(item.Label, font).Width + 15;
                }

                int startX = (_legendPanel.Width - totalWidth) / 2;
                int y = (_legendPanel.Height - 14) / 2;

                foreach (var item in legendItems)
                {
                    // Draw colored box
                    using (var brush = new SolidBrush(item.Color))
                    {
                        g.FillRectangle(brush, startX, y, 12, 12);
                    }
                    using (var pen = new Pen(Color.FromArgb(100, 100, 100), 1))
                    {
                        g.DrawRectangle(pen, startX, y, 12, 12);
                    }

                    // Draw label
                    using (var brush = new SolidBrush(Color.White))
                    {
                        g.DrawString(item.Label, font, brush, startX + 14, y - 2);
                    }

                    startX += 14 + (int)g.MeasureString(item.Label, font).Width + 15;
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _updateTimer.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                _updateTimer?.Stop();
                _updateTimer?.Dispose();
            }
            base.OnFormClosing(e);
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                var energyState = _getEnergyState?.Invoke();
                if (energyState != null)
                {
                    var dataPoint = new EnergyDataPoint
                    {
                        Timestamp = DateTime.Now,
                        TotalPower = energyState.TotalPowerWatts,
                        CpuPower = energyState.CpuPowerWatts,
                        DisplayPower = energyState.DisplayPowerWatts,
                        GpuPower = energyState.GpuPowerWatts,
                        DiskPower = energyState.DiskPowerWatts,
                        MemoryPower = energyState.MemoryPowerWatts,
                        BasePower = energyState.BasePowerWatts
                    };

                    _dataPoints.Enqueue(dataPoint);
                    while (_dataPoints.Count > MaxDataPoints)
                    {
                        _dataPoints.Dequeue();
                    }

                    // Update statistics and energy
                    UpdateStatistics(dataPoint.TotalPower);

                    // Only invalidate the chart panel, not the whole form
                    _chartPanel.Invalidate();
                }
            }
            catch { }
        }

        /// <summary>
        /// Update running statistics and cumulative energy
        /// </summary>
        private void UpdateStatistics(double totalPower)
        {
            _totalPowerSum += totalPower;
            _totalPowerCount++;

            // Accumulate energy: Power (W) * Time (h) = Wh
            // 1 second = 1/3600 hours
            _totalEnergyWh += totalPower / 3600.0;

            if (totalPower < _totalPowerMin)
                _totalPowerMin = totalPower;
            if (totalPower > _totalPowerMax)
                _totalPowerMax = totalPower;

            // Update stats label
            double avg = _totalPowerSum / _totalPowerCount;
            var duration = DateTime.Now - _startTime;

            _statsLabel.Text = string.Format(
                "Avg: {0:F1}W | Min: {1:F1}W | Max: {2:F1}W | Range: {3:F1}W | Samples: {4} | Duration: {5}",
                avg,
                _totalPowerMin == double.MaxValue ? 0 : _totalPowerMin,
                _totalPowerMax == double.MinValue ? 0 : _totalPowerMax,
                (_totalPowerMax == double.MinValue || _totalPowerMin == double.MaxValue) ? 0 : _totalPowerMax - _totalPowerMin,
                _totalPowerCount,
                FormatDuration(duration));

            // Calculate cost in cents
            double costTodayCent = (_totalEnergyWh / 1000.0) * EnergyPriceCentPerKwh;
            double estDailyCostCent = EstimateDailyEnergy() * EnergyPriceCentPerKwh;

            // Update energy label with cost
            string energyText;
            if (_totalEnergyWh >= 1000)
            {
                energyText = string.Format("Energy: {0:F3} kWh", _totalEnergyWh / 1000.0);
            }
            else
            {
                energyText = string.Format("Energy: {0:F3} Wh", _totalEnergyWh);
            }
            _energyLabel.Text = string.Format("{0} | Cost: {1:F2} ct | Est. Daily: {2:F2} kWh / {3:F1} ct | Price: {4:F1} ct/kWh",
                energyText, costTodayCent, EstimateDailyEnergy(), estDailyCostCent, EnergyPriceCentPerKwh);
        }

        /// <summary>
        /// Estimate daily energy consumption based on current average
        /// </summary>
        private double EstimateDailyEnergy()
        {
            if (_totalPowerCount == 0) return 0;
            double avgPower = _totalPowerSum / _totalPowerCount;
            // 24 hours * avgPower = daily Wh, convert to kWh
            return (avgPower * 24) / 1000.0;
        }

        private string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalHours >= 1)
                return string.Format("{0:D2}:{1:D2}:{2:D2}", (int)duration.TotalHours, duration.Minutes, duration.Seconds);
            return string.Format("{0:D2}:{1:D2}", duration.Minutes, duration.Seconds);
        }

        private void ChartPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = _chartPanel.ClientRectangle;
            var chartRect = new Rectangle(60, 20, rect.Width - 80, rect.Height - 60);

            if (chartRect.Width < 100 || chartRect.Height < 100)
                return;

            // Draw background
            using (var brush = new SolidBrush(Color.FromArgb(25, 25, 25)))
            {
                g.FillRectangle(brush, chartRect);
            }

            // Get data as array
            var data = _dataPoints.ToArray();
            if (data.Length == 0)
            {
                DrawNoDataMessage(g, chartRect);
                return;
            }

            // Calculate scale based on total power (stacked components)
            double maxValue = 10;

            foreach (var dp in data)
            {
                maxValue = Math.Max(maxValue, dp.TotalPower);
            }

            // Include historical max in scale calculation
            if (_totalPowerMax != double.MinValue)
                maxValue = Math.Max(maxValue, _totalPowerMax);

            // Round up to nice value
            maxValue = RoundUpToNice(maxValue);

            // Draw grid
            DrawGrid(g, chartRect);

            // Draw min/max band for total power
            if (_totalPowerMin != double.MaxValue && _totalPowerMax != double.MinValue)
            {
                DrawMinMaxBand(g, chartRect, maxValue);
            }

            // Draw Y-axis labels (single axis)
            DrawYAxisLabels(g, chartRect, maxValue);

            // Draw X-axis labels
            DrawXAxisLabels(g, chartRect, data);

            // Draw stacked bars
            DrawStackedBars(g, chartRect, data, maxValue);

            // Draw average line on top of bars
            if (_totalPowerCount > 0)
            {
                DrawAverageLine(g, chartRect, maxValue);
            }

            // Draw current values at bottom right
            if (data.Length > 0)
            {
                DrawCurrentValues(g, chartRect, data[data.Length - 1]);
            }

            // Draw border
            using (var pen = new Pen(Color.FromArgb(80, 80, 80), 1f))
            {
                g.DrawRectangle(pen, chartRect);
            }
        }

        /// <summary>
        /// Draw the min/max band for total power
        /// </summary>
        private void DrawMinMaxBand(Graphics g, Rectangle chartRect, double maxScale)
        {
            int minY = chartRect.Bottom - (int)(chartRect.Height * _totalPowerMin / maxScale);
            int maxY = chartRect.Bottom - (int)(chartRect.Height * _totalPowerMax / maxScale);

            minY = Math.Max(chartRect.Top, Math.Min(chartRect.Bottom, minY));
            maxY = Math.Max(chartRect.Top, Math.Min(chartRect.Bottom, maxY));

            // Draw filled band
            using (var brush = new SolidBrush(_bandColor))
            {
                g.FillRectangle(brush, chartRect.Left, maxY, chartRect.Width, minY - maxY);
            }

            // Draw min/max lines with dashes
            using (var pen = new Pen(Color.FromArgb(100, 0, 120, 215), 1f))
            {
                pen.DashStyle = DashStyle.Dash;
                g.DrawLine(pen, chartRect.Left, minY, chartRect.Right, minY);
                g.DrawLine(pen, chartRect.Left, maxY, chartRect.Right, maxY);
            }

            // Draw min/max labels
            using (var font = new Font("Segoe UI", 7f))
            using (var brush = new SolidBrush(Color.FromArgb(150, 150, 150)))
            {
                g.DrawString(string.Format("Min: {0:F1}W", _totalPowerMin), font, brush, chartRect.Left + 5, minY - 12);
                g.DrawString(string.Format("Max: {0:F1}W", _totalPowerMax), font, brush, chartRect.Left + 5, maxY + 2);
            }
        }

        /// <summary>
        /// Draw the average line
        /// </summary>
        private void DrawAverageLine(Graphics g, Rectangle chartRect, double maxScale)
        {
            double avg = _totalPowerSum / _totalPowerCount;
            int avgY = chartRect.Bottom - (int)(chartRect.Height * avg / maxScale);
            avgY = Math.Max(chartRect.Top, Math.Min(chartRect.Bottom, avgY));

            using (var pen = new Pen(_avgColor, 2f))
            {
                pen.DashStyle = DashStyle.DashDot;
                g.DrawLine(pen, chartRect.Left, avgY, chartRect.Right, avgY);
            }

            // Draw avg label
            using (var font = new Font("Segoe UI", 8f, FontStyle.Bold))
            using (var brush = new SolidBrush(_avgColor))
            {
                string label = string.Format("Avg: {0:F1}W", avg);
                var size = g.MeasureString(label, font);
                g.DrawString(label, font, brush, chartRect.Right - size.Width - 5, avgY - size.Height - 2);
            }
        }

        private void DrawNoDataMessage(Graphics g, Rectangle chartRect)
        {
            using (var font = new Font("Segoe UI", 12f))
            using (var brush = new SolidBrush(Color.Gray))
            {
                var text = "Waiting for data...";
                var size = g.MeasureString(text, font);
                g.DrawString(text, font, brush,
                    chartRect.X + (chartRect.Width - size.Width) / 2,
                    chartRect.Y + (chartRect.Height - size.Height) / 2);
            }
        }

        private void DrawGrid(Graphics g, Rectangle chartRect)
        {
            using (var pen = new Pen(Color.FromArgb(50, 50, 50), 1f))
            {
                pen.DashStyle = DashStyle.Dot;

                // Horizontal grid lines (5 lines)
                for (int i = 1; i < 5; i++)
                {
                    int y = chartRect.Bottom - (int)(chartRect.Height * i / 5.0);
                    g.DrawLine(pen, chartRect.Left, y, chartRect.Right, y);
                }

                // Vertical grid lines
                int numVerticalLines = Math.Min(10, MaxDataPoints / 6);
                for (int i = 1; i < numVerticalLines; i++)
                {
                    int x = chartRect.Left + (int)(chartRect.Width * i / (double)numVerticalLines);
                    g.DrawLine(pen, x, chartRect.Top, x, chartRect.Bottom);
                }
            }
        }

        private void DrawYAxisLabels(Graphics g, Rectangle chartRect, double maxValue)
        {
            using (var font = new Font("Segoe UI", 8f))
            using (var brush = new SolidBrush(Color.White))
            {
                for (int i = 0; i <= 5; i++)
                {
                    double value = maxValue * i / 5.0;
                    int y = chartRect.Bottom - (int)(chartRect.Height * i / 5.0);
                    string label = value.ToString("F0") + "W";
                    var size = g.MeasureString(label, font);
                    g.DrawString(label, font, brush, chartRect.Left - size.Width - 5, y - size.Height / 2);
                }
            }
        }

        private void DrawXAxisLabels(Graphics g, Rectangle chartRect, EnergyDataPoint[] data)
        {
            if (data.Length == 0) return;

            using (var font = new Font("Segoe UI", 8f))
            using (var brush = new SolidBrush(Color.Gray))
            {
                // Start time
                string startLabel = data[0].Timestamp.ToString("HH:mm:ss");
                g.DrawString(startLabel, font, brush, chartRect.Left, chartRect.Bottom + 5);

                // End time
                string endLabel = data[data.Length - 1].Timestamp.ToString("HH:mm:ss");
                var size = g.MeasureString(endLabel, font);
                g.DrawString(endLabel, font, brush, chartRect.Right - size.Width, chartRect.Bottom + 5);

                // Middle time
                if (data.Length > 2)
                {
                    string midLabel = data[data.Length / 2].Timestamp.ToString("HH:mm:ss");
                    size = g.MeasureString(midLabel, font);
                    g.DrawString(midLabel, font, brush, chartRect.Left + (chartRect.Width - size.Width) / 2, chartRect.Bottom + 5);
                }
            }
        }

        /// <summary>
        /// Draw stacked bars showing component breakdown
        /// The total bar height corresponds to TotalPower, with components proportionally scaled
        /// </summary>
        private void DrawStackedBars(Graphics g, Rectangle chartRect, EnergyDataPoint[] data, double maxValue)
        {
            if (data.Length == 0 || maxValue <= 0) return;

            // Calculate bar width with small gap between bars
            float totalWidth = chartRect.Width;
            float barWidth = Math.Max(2, (totalWidth / MaxDataPoints) - 1);

            for (int i = 0; i < data.Length; i++)
            {
                var dp = data[i];
                float x = chartRect.Left + (float)(chartRect.Width * i / (double)MaxDataPoints);

                // Total bar height based on TotalPower
                float totalBarHeight = (float)(chartRect.Height * dp.TotalPower / maxValue);
                totalBarHeight = Math.Max(0, Math.Min(chartRect.Height, totalBarHeight));

                if (totalBarHeight <= 0) continue;

                // Sum of component powers
                double componentSum = dp.BasePower + dp.MemoryPower + dp.DiskPower +
                                      dp.GpuPower + dp.DisplayPower + dp.CpuPower;

                // If components don't add up, use TotalPower for scaling
                if (componentSum <= 0) componentSum = dp.TotalPower;

                // Scale factor to make components fit within TotalPower bar height
                double scaleFactor = componentSum > 0 ? dp.TotalPower / componentSum : 1.0;

                // Stack the components from bottom to top: Base, Memory, Disk, GPU, Display, CPU
                float currentY = chartRect.Bottom;

                // Draw each component segment proportionally
                var components = new[]
                {
                    new { Power = dp.BasePower, Color = _baseColor },
                    new { Power = dp.MemoryPower, Color = _memoryColor },
                    new { Power = dp.DiskPower, Color = _diskColor },
                    new { Power = dp.GpuPower, Color = _gpuColor },
                    new { Power = dp.DisplayPower, Color = _displayColor },
                    new { Power = dp.CpuPower, Color = _cpuColor }
                };

                foreach (var component in components)
                {
                    if (component.Power > 0)
                    {
                        // Scale component power proportionally to fit within total bar
                        double scaledPower = component.Power * scaleFactor;
                        float segmentHeight = (float)(chartRect.Height * scaledPower / maxValue);
                        segmentHeight = Math.Max(0, Math.Min(currentY - chartRect.Top, segmentHeight));

                        if (segmentHeight > 0)
                        {
                            using (var brush = new SolidBrush(component.Color))
                            {
                                g.FillRectangle(brush, x, currentY - segmentHeight, barWidth, segmentHeight);
                            }
                            currentY -= segmentHeight;
                        }
                    }
                }
            }
        }

        private void DrawCurrentValues(Graphics g, Rectangle chartRect, EnergyDataPoint current)
        {
            int lineHeight = 16;
            int boxWidth = 130;

            // Draw large total power display at top right - the eye-catcher
            using (var largeFont = new Font("Segoe UI", 28f, FontStyle.Bold))
            using (var labelFont = new Font("Segoe UI", 10f))
            {
                string totalText = string.Format("{0:F1}", current.TotalPower);
                var totalSize = g.MeasureString(totalText, largeFont);

                int totalX = chartRect.Right - (int)totalSize.Width - 20;
                int totalY = chartRect.Top + 10;

                // Background for total power
                using (var bgBrush = new SolidBrush(Color.FromArgb(220, 0, 60, 120)))
                {
                    g.FillRectangle(bgBrush, totalX - 10, totalY - 5, totalSize.Width + 55, totalSize.Height + 20);
                }
                using (var pen = new Pen(Color.FromArgb(0, 120, 215), 2f))
                {
                    g.DrawRectangle(pen, totalX - 10, totalY - 5, totalSize.Width + 55, totalSize.Height + 20);
                }

                // Draw total value
                using (var brush = new SolidBrush(Color.White))
                {
                    g.DrawString(totalText, largeFont, brush, totalX, totalY);
                }
                // Draw "W" unit
                using (var brush = new SolidBrush(Color.FromArgb(200, 200, 200)))
                {
                    g.DrawString("W", labelFont, brush, totalX + totalSize.Width + 2, totalY + totalSize.Height - 18);
                }
                // Draw "TOTAL" label
                using (var brush = new SolidBrush(Color.FromArgb(150, 150, 150)))
                {
                    g.DrawString("TOTAL", labelFont, brush, totalX, totalY + totalSize.Height + 2);
                }
            }

            // Component breakdown at bottom right
            using (var font = new Font("Segoe UI", 9f, FontStyle.Bold))
            {
                int boxHeight = lineHeight * 6 + 6;
                int x = chartRect.Right - boxWidth - 5;
                int y = chartRect.Bottom - boxHeight - 5;

                // Semi-transparent background for readability
                using (var bgBrush = new SolidBrush(Color.FromArgb(200, 25, 25, 25)))
                {
                    g.FillRectangle(bgBrush, x - 5, y - 3, boxWidth + 5, boxHeight);
                }
                using (var pen = new Pen(Color.FromArgb(80, 80, 80), 1f))
                {
                    g.DrawRectangle(pen, x - 5, y - 3, boxWidth + 5, boxHeight);
                }

                DrawValueLabel(g, font, string.Format("CPU: {0:F1}W", current.CpuPower), _cpuColor, x, y);
                y += lineHeight;
                DrawValueLabel(g, font, string.Format("Display: {0:F1}W", current.DisplayPower), _displayColor, x, y);
                y += lineHeight;
                DrawValueLabel(g, font, string.Format("GPU: {0:F1}W", current.GpuPower), _gpuColor, x, y);
                y += lineHeight;
                DrawValueLabel(g, font, string.Format("Disk: {0:F1}W", current.DiskPower), _diskColor, x, y);
                y += lineHeight;
                DrawValueLabel(g, font, string.Format("Memory: {0:F1}W", current.MemoryPower), _memoryColor, x, y);
                y += lineHeight;
                DrawValueLabel(g, font, string.Format("Base: {0:F1}W", current.BasePower), _baseColor, x, y);
            }
        }

        private void DrawValueLabel(Graphics g, Font font, string text, Color color, int x, int y)
        {
            using (var brush = new SolidBrush(color))
            {
                g.DrawString(text, font, brush, x, y);
            }
        }

        private double RoundUpToNice(double value)
        {
            if (value <= 0) return 10;

            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(value)));
            double normalized = value / magnitude;

            if (normalized <= 1) return magnitude;
            if (normalized <= 2) return 2 * magnitude;
            if (normalized <= 5) return 5 * magnitude;
            return 10 * magnitude;
        }

        /// <summary>
        /// Clear all data points and reset statistics
        /// </summary>
        public void ClearData()
        {
            _dataPoints.Clear();
            _totalPowerSum = 0;
            _totalPowerCount = 0;
            _totalPowerMin = double.MaxValue;
            _totalPowerMax = double.MinValue;
            _totalEnergyWh = 0;
            _startTime = DateTime.Now;
            _statsLabel.Text = "Statistics: Waiting for data...";
            _energyLabel.Text = "Total Energy: 0.000 Wh";
            _chartPanel.Invalidate();
        }

        private class EnergyDataPoint
        {
            public DateTime Timestamp { get; set; }
            public double TotalPower { get; set; }
            public double CpuPower { get; set; }
            public double DisplayPower { get; set; }
            public double GpuPower { get; set; }
            public double DiskPower { get; set; }
            public double MemoryPower { get; set; }
            public double BasePower { get; set; }
        }

        /// <summary>
        /// Double-buffered panel for flicker-free drawing
        /// </summary>
        private class DoubleBufferedPanel : Panel
        {
            public DoubleBufferedPanel()
            {
                this.DoubleBuffered = true;
                this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
                this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                this.SetStyle(ControlStyles.UserPaint, true);
                this.UpdateStyles();
            }
        }
    }
}
