using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing.Imaging;
using Timer = System.Windows.Forms.Timer;

namespace EmployeeManagementSystem.Forms
{
    public partial class AttendanceForm : Form
    {
        private readonly ApplicationDbContext _context;
        private readonly DataGridView dgvAttendance;
        private readonly Panel formPanel;
        private readonly DateTimePicker dtpDate;
        private readonly ComboBox cmbEmployee;
        private readonly DateTimePicker dtpCheckIn;
        private readonly DateTimePicker dtpCheckOut;
        private readonly ComboBox cmbStatus;
        private readonly User _currentUser;
        private int? selectedAttendanceId;

        public AttendanceForm(User currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            _context = new ApplicationDbContext();

            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(37, 37, 38);

            // Create main controls
            dgvAttendance = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                GridColor = Color.FromArgb(60, 60, 60),
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true
            };

            formPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 300,
                BackColor = Color.FromArgb(45, 45, 48),
                Padding = new Padding(10)
            };

            // Initialize date/time pickers
            dtpDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Dock = DockStyle.Top,
                Value = DateTime.Today,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };

            dtpCheckIn = new DateTimePicker
            {
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Dock = DockStyle.Top,
                Value = DateTime.Today.AddHours(9), // Default 9 AM
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };

            dtpCheckOut = new DateTimePicker
            {
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Dock = DockStyle.Top,
                Value = DateTime.Today.AddHours(17), // Default 5 PM
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White
            };

            cmbEmployee = new ComboBox
            {
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbStatus = new ComboBox
            {
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbStatus.Items.AddRange(new string[] { 
                "Present", 
                "Absent", 
                "Late", 
                "Half Day",
                "On Leave" 
            });
            cmbStatus.SelectedIndex = 0;

            SetupFormControls();
            SetupDataGridView();
            LoadAttendance();
            LoadEmployees();

            // Add cell click handler
            dgvAttendance.CellClick += DgvAttendance_CellClick;
        }

        private void SetupFormControls()
        {
            var lblTitle = new Label
            {
                Text = "Attendance Management",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Height = 40
            };

            // Create buttons based on user role
            var btnCheckIn = CreateButton("Check In", Color.FromArgb(40, 167, 69));
            var btnCheckOut = CreateButton("Check Out", Color.FromArgb(0, 123, 255));
            var btnManualEntry = CreateButton("Add Manual Entry", Color.FromArgb(255, 193, 7));
            var btnUpdate = CreateButton("Update Entry", Color.FromArgb(23, 162, 184));
            var btnDelete = CreateButton("Delete Entry", Color.FromArgb(220, 53, 69));
            var btnDownloadReport = CreateButton("Download Monthly Report", Color.FromArgb(75, 123, 236));

            // Show/hide controls based on role
            if (_currentUser.Role == UserRole.Administrator)
            {
                formPanel.Controls.AddRange(new Control[] {
                    btnDelete,
                    btnUpdate,
                    btnManualEntry,
                    btnCheckOut,
                    btnCheckIn,
                    btnDownloadReport,
                    CreateLabel("Status"),
                    cmbStatus,
                    CreateLabel("Check Out Time"),
                    dtpCheckOut,
                    CreateLabel("Check In Time"),
                    dtpCheckIn,
                    CreateLabel("Date"),
                    dtpDate,
                    CreateLabel("Employee"),
                    cmbEmployee,
                    lblTitle
                });

                btnManualEntry.Visible = true;
                btnUpdate.Visible = true;
                btnDelete.Visible = true;
                btnDownloadReport.Visible = true;
            }
            else // Employee view
            {
                formPanel.Controls.AddRange(new Control[] {
                    btnCheckOut,
                    btnCheckIn,
                    CreateLabel("Date"),
                    dtpDate,
                    CreateLabel("Employee"),
                    cmbEmployee,
                    lblTitle
                });

                // Disable date editing for employees
                dtpDate.Enabled = false;
            }

            // Add event handlers
            btnCheckIn.Click += BtnCheckIn_Click;
            btnCheckOut.Click += BtnCheckOut_Click;
            btnManualEntry.Click += BtnManualEntry_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnDownloadReport.Click += BtnDownloadReport_Click;
        }

        private void SetupDataGridView()
        {
            dgvAttendance.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 50 },
                new DataGridViewTextBoxColumn { Name = "EmployeeName", HeaderText = "Employee Name", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Date", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "CheckIn", HeaderText = "Check In", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "CheckOut", HeaderText = "Check Out", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 100 }
            });

            // Add these styling properties
            dgvAttendance.EnableHeadersVisualStyles = false;
            dgvAttendance.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
            dgvAttendance.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvAttendance.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(45, 45, 48);
            dgvAttendance.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            dgvAttendance.DefaultCellStyle.BackColor = Color.FromArgb(37, 37, 38);
            dgvAttendance.DefaultCellStyle.ForeColor = Color.White;
            dgvAttendance.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
            dgvAttendance.DefaultCellStyle.SelectionForeColor = Color.White;

            // Add month view checkbox to a top panel
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(45, 45, 48),
                Padding = new Padding(10)
            };

            var chkMonthView = new CheckBox
            {
                Text = "Show Monthly View",
                ForeColor = Color.White,
                AutoSize = true,
                Name = "chkMonthView",
                Location = new Point(10, 10)
            };
            chkMonthView.CheckedChanged += (s, e) => LoadAttendance();

            topPanel.Controls.Add(chkMonthView);

            // Create container for grid and form
            var mainContainer = new Panel { Dock = DockStyle.Fill };
            mainContainer.Controls.Add(dgvAttendance);
            mainContainer.Controls.Add(formPanel);
            mainContainer.Controls.Add(topPanel);

            this.Controls.Add(mainContainer);
        }

        private void LoadEmployees()
        {
            try
            {
                // For regular employees, only show themselves
                if (_currentUser.Role == UserRole.Employee)
                {
                    var employee = _context.Employees
                        .Include(e => e.User)
                        .Where(e => e.UserId == _currentUser.UserId)
                        .Select(e => new { 
                            EmployeeId = e.EmployeeId, 
                            Name = e.User.Name  // Use Name for display
                        })
                        .ToList();

                    cmbEmployee.DisplayMember = "Name";  // Use Name for display
                    cmbEmployee.ValueMember = "EmployeeId";
                    cmbEmployee.DataSource = employee;
                    cmbEmployee.Enabled = false; // Disable selection for employees
                }
                // For admin, show all employees
                else
                {
                    var employees = _context.Employees
                        .Include(e => e.User)
                        .OrderBy(e => e.User.Name)
                        .Select(e => new { 
                            EmployeeId = e.EmployeeId, 
                            Name = e.User.Name  // Add Name for display
                        })
                        .ToList();

                    cmbEmployee.DisplayMember = "Name";  // Use Name for display
                    cmbEmployee.ValueMember = "EmployeeId";
                    cmbEmployee.DataSource = employees;
                    cmbEmployee.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading employees: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadAttendance()
        {
            try
            {
                dgvAttendance.Rows.Clear();
                var query = _context.Attendances
                    .Include(a => a.Employee)
                    .ThenInclude(e => e.User)
                    .AsQueryable();

                // Get checkbox reference
                var chkMonthView = Controls.Find("chkMonthView", true).FirstOrDefault() as CheckBox;
                bool isMonthView = chkMonthView?.Checked ?? false;

                // Set date range based on view type
                if (isMonthView)
                {
                    // Show whole month
                    var startOfMonth = new DateTime(dtpDate.Value.Year, dtpDate.Value.Month, 1);
                    var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
                    query = query.Where(a => a.Date >= startOfMonth && a.Date <= endOfMonth);
                }
                else
                {
                    // Show single day
                    query = query.Where(a => a.Date.Date == dtpDate.Value.Date);
                }

                // For employees, only show their own attendance
                if (_currentUser.Role == UserRole.Employee)
                {
                    query = query.Where(a => a.Employee.UserId == _currentUser.UserId);
                }

                var attendanceRecords = query
                    .OrderBy(a => a.Date)
                    .ThenBy(a => a.Employee.User.Name)
                    .ToList();

                foreach (var record in attendanceRecords)
                {
                    dgvAttendance.Rows.Add(
                        record.AttendanceId,
                        record.Employee.User.Name,
                        record.Date.ToShortDateString(),
                        record.CheckInTime?.ToString(@"hh\:mm") ?? "-",
                        record.CheckOutTime?.ToString(@"hh\:mm") ?? "-",
                        record.Status
                    );

                    // Color weekends
                    var lastRow = dgvAttendance.Rows[dgvAttendance.Rows.Count - 1];
                    if (record.Date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                    {
                        lastRow.DefaultCellStyle.BackColor = Color.FromArgb(60, 60, 60);
                    }
                }

                // Add summary if in month view
                if (isMonthView)
                {
                    var summary = new Label
                    {
                        Text = $"Total Records: {attendanceRecords.Count}\n" +
                              $"Present: {attendanceRecords.Count(r => r.Status == "Present")}\n" +
                              $"Late: {attendanceRecords.Count(r => r.Status == "Late")}\n" +
                              $"Absent: {attendanceRecords.Count(r => r.Status == "Absent")}",
                        Dock = DockStyle.Bottom,
                        ForeColor = Color.White,
                        BackColor = Color.FromArgb(45, 45, 48),
                        Height = 80,
                        Padding = new Padding(10)
                    };

                    // Remove existing summary if any
                    var existingSummary = Controls.OfType<Label>().FirstOrDefault(l => l.Name == "summaryLabel");
                    existingSummary?.Dispose();

                    summary.Name = "summaryLabel";
                    Controls.Add(summary);
                }
                else
                {
                    // Remove summary when not in month view
                    var existingSummary = Controls.OfType<Label>().FirstOrDefault(l => l.Name == "summaryLabel");
                    existingSummary?.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading attendance: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Bitmap AddWatermark(Bitmap original, string location)
        {
            try
            {
                using (Graphics g = Graphics.FromImage(original))
                {
                    // Setup font and brush for watermark
                    using (Font font = new Font("Arial", 12, FontStyle.Bold))
                    using (SolidBrush brush = new SolidBrush(Color.Yellow))
                    {
                        // Timestamp and location text
                        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        string watermarkText = $"Time: {timestamp}\nLocation: {location}";

                        // Position watermark in buttom right
                        var measure = g.MeasureString(watermarkText, font);
                        float x = original.Width - measure.Width - 10;
                        float y = original.Height - measure.Height - 10;

                        // add dark background semi-transparent for text
                        using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(128, 0, 0, 0)))
                        {
                            g.FillRectangle(bgBrush, x - 5, y - 5, measure.Width + 10, measure.Height + 10);
                        }

                        // Draw text
                        g.DrawString(watermarkText, font, brush, x, y);
                    }
                }
                return original;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding watermark: {ex.Message}");
                return original;
            }
        }

        private async Task<byte[]> CapturePhoto()
        {
            try
            {
                var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count == 0)
                {
                    MessageBox.Show("No camera found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }

                using (var captureForm = new Form())
                {
                    captureForm.Width = 640;
                    captureForm.Height = 580; // add height for label
                    captureForm.Text = "Camera Capture";
                    captureForm.StartPosition = FormStartPosition.CenterScreen;
                    captureForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    captureForm.BackColor = Color.FromArgb(45, 45, 48);

                    // Info Panel on top
                    Panel infoPanel = new Panel
                    {
                        Dock = DockStyle.Top,
                        Height = 60,
                        BackColor = Color.FromArgb(30, 30, 30)
                    };

                    // Label for date and time
                    Label timeLabel = new Label
                    {
                        Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                        ForeColor = Color.Yellow,
                        Font = new Font("Arial", 12, FontStyle.Bold),
                        AutoSize = true,
                        Location = new Point(10, 10)
                    };

                    // Label for location
                    Label locationLabel = new Label
                    {
                        Text = "Getting location...",
                        ForeColor = Color.Yellow,
                        Font = new Font("Arial", 12, FontStyle.Bold),
                        AutoSize = true,
                        Location = new Point(10, 35)
                    };

                    // Timer for update time - specify System.Windows.Forms.Timer
                    var timer = new System.Windows.Forms.Timer { Interval = 1000 };
                    timer.Tick += (s, e) => timeLabel.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    timer.Start();

                    // Update location
                    _ = Task.Run(async () =>
                    {
                        var location = await GetCurrentLocation();
                        if (infoPanel.IsHandleCreated)
                        {
                            infoPanel.Invoke(() => locationLabel.Text = $"Location: {location}");
                        }
                    });

                    // Camera preview
                    PictureBox preview = new PictureBox
                    {
                        Width = 640,
                        Height = 480,
                        Dock = DockStyle.Fill,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        BorderStyle = BorderStyle.Fixed3D
                    };

                    // Button panel
                    TableLayoutPanel buttonPanel = new TableLayoutPanel
                    {
                        Dock = DockStyle.Bottom,
                        Height = 40,
                        ColumnCount = 2,
                        BackColor = Color.FromArgb(45, 45, 48)
                    };

                    Button captureBtn = new Button
                    {
                        Text = "Capture",
                        Height = 35,
                        ForeColor = Color.White,
                        BackColor = Color.FromArgb(0, 122, 204),
                        FlatStyle = FlatStyle.Flat,
                        Dock = DockStyle.Fill
                    };

                    Button cancelBtn = new Button
                    {
                        Text = "Cancel",
                        Height = 35,
                        ForeColor = Color.White,
                        BackColor = Color.FromArgb(171, 23, 48),
                        FlatStyle = FlatStyle.Flat,
                        Dock = DockStyle.Fill
                    };

                    // Setup camera
                    var videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                    videoSource.VideoResolution = videoSource.VideoCapabilities
                        .OrderByDescending(c => c.FrameSize.Width * c.FrameSize.Height)
                        .FirstOrDefault();

                    byte[] photoBytes = null;
                    var tcs = new TaskCompletionSource<byte[]>();

                    captureBtn.Click += async (s, e) =>
                    {
                        if (preview.Image != null)
                        {
                            var location = locationLabel.Text.Replace("Location: ", "");
                            var bitmap = new Bitmap(preview.Image);
                            bitmap = AddWatermark(bitmap, location);

                            using (var ms = new MemoryStream())
                            {
                                bitmap.Save(ms, ImageFormat.Jpeg);
                                photoBytes = ms.ToArray();
                                await Task.Run(() => tcs.SetResult(photoBytes));
                                captureForm.Close();
                            }
                        }
                    };

                    cancelBtn.Click += (s, e) =>
                    {
                        tcs.SetResult(null);
                        captureForm.Close();
                    };

                    // Add controls
                    infoPanel.Controls.AddRange(new Control[] { timeLabel, locationLabel });
                    buttonPanel.Controls.Add(captureBtn, 0, 0);
                    buttonPanel.Controls.Add(cancelBtn, 1, 0);

                    captureForm.Controls.Add(preview);
                    captureForm.Controls.Add(buttonPanel);
                    captureForm.Controls.Add(infoPanel);

                    // Start camera
                    videoSource.Start();
                    videoSource.NewFrame += (s, e) =>
                    {
                        preview.Image?.Dispose();
                        preview.Image = (Bitmap)e.Frame.Clone();
                    };

                    captureForm.FormClosing += (s, e) =>
                    {
                        timer.Stop();
                        timer.Dispose();
                        videoSource.SignalToStop();
                        preview.Image?.Dispose();
                    };

                    captureForm.ShowDialog();
                    return await tcs.Task;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Camera error: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private async void BtnCheckIn_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbEmployee.SelectedValue == null)
                {
                    MessageBox.Show("Please select an employee", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var employeeId = (int)cmbEmployee.SelectedValue;
                var currentTime = DateTime.Now;

                // Verify employee is checking in for themselves
                if (_currentUser.Role == UserRole.Employee)
                {
                    var employee = _context.Employees.FirstOrDefault(e => e.UserId == _currentUser.UserId);
                    if (employee.EmployeeId != employeeId)
                    {
                        MessageBox.Show("You can only check in for yourself.", 
                            "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                var existingRecord = _context.Attendances
                    .FirstOrDefault(a => 
                        a.EmployeeId == employeeId && 
                        a.Date.Date == currentTime.Date);

                if (existingRecord != null && existingRecord.CheckInTime.HasValue)
                {
                    MessageBox.Show("Employee has already checked in for today!", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                byte[] photo = null;
                try
                {
                    photo = await CapturePhoto();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Camera not available. Check-in will continue without photo.",
                        "Camera Warning",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                var location = await GetCurrentLocation();

                if (existingRecord == null)
                {
                    var status = "Present";
                    // Check if check-in time is after 9 AM
                    if (currentTime.TimeOfDay > new TimeSpan(9, 0, 0))
                    {
                        status = "Late";
                    }

                    existingRecord = new Attendance
                    {
                        EmployeeId = employeeId,
                        Date = currentTime.Date,
                        Status = status,
                        CheckInTime = currentTime.TimeOfDay,
                        CheckInPhoto = photo,
                        CheckInLocation = location
                    };
                    _context.Attendances.Add(existingRecord);
                }
                else
                {
                    existingRecord.CheckInTime = currentTime.TimeOfDay;
                    existingRecord.CheckInPhoto = photo;
                    existingRecord.CheckInLocation = location;
                    
                    // Update status if checking in late
                    if (currentTime.TimeOfDay > new TimeSpan(9, 0, 0))
                    {
                        existingRecord.Status = "Late";
                    }
                }

                await _context.SaveChangesAsync();
                LoadAttendance();

                string message = photo != null 
                    ? "Check-in recorded successfully with photo!" 
                    : "Check-in recorded successfully without photo.";
                    
                MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error recording check-in: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnCheckOut_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbEmployee.SelectedValue == null)
                {
                    MessageBox.Show("Please select an employee", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var employeeId = (int)cmbEmployee.SelectedValue;
                var currentTime = DateTime.Now;

                var existingRecord = _context.Attendances
                    .FirstOrDefault(a => 
                        a.EmployeeId == employeeId && 
                        a.Date.Date == currentTime.Date);

                if (existingRecord == null || !existingRecord.CheckInTime.HasValue)
                {
                    MessageBox.Show("No check-in record found for today!", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (existingRecord.CheckOutTime.HasValue)
                {
                    MessageBox.Show("Employee has already checked out for today!", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                byte[] photo = null;
                try
                {
                    photo = await CapturePhoto();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Camera not available. Check-out will continue without photo.",
                        "Camera Warning",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                var location = await GetCurrentLocation();

                existingRecord.CheckOutTime = currentTime.TimeOfDay;
                existingRecord.CheckOutPhoto = photo;
                existingRecord.CheckOutLocation = location;

                // Calculate work duration and update status
                var workDuration = currentTime.TimeOfDay - existingRecord.CheckInTime.Value;
                var checkOutTime = currentTime.TimeOfDay;
                var normalEndTime = new TimeSpan(17, 0, 0); // 5 PM

                if (workDuration.TotalHours < 8)
                {
                    existingRecord.Status = "Half Day";
                }
                else if (existingRecord.Status != "Late") // Don't override Late status
                {
                    if (checkOutTime > normalEndTime)
                    {
                        existingRecord.Status = "OT"; // Overtime
                    }
                    else
                    {
                        existingRecord.Status = "Present";
                    }
                }

                await _context.SaveChangesAsync();
                LoadAttendance();

                string message = photo != null 
                    ? "Check-out recorded successfully with photo!" 
                    : "Check-out recorded successfully without photo.";
                    
                MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error recording check-out: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnManualEntry_Click(object sender, EventArgs e)
        {
            try
            {
                if (_currentUser.Role != UserRole.Administrator)
                {
                    MessageBox.Show("Only administrators can add manual entries.", 
                        "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (cmbEmployee.SelectedValue == null)
                {
                    MessageBox.Show("Please select an employee", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var employeeId = (int)cmbEmployee.SelectedValue;
                var existingRecord = _context.Attendances
                    .FirstOrDefault(a => 
                        a.EmployeeId == employeeId && 
                        a.Date.Date == dtpDate.Value.Date);

                if (existingRecord != null)
                {
                    MessageBox.Show("Attendance record already exists for this date!", 
                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Determine status based on check-in and check-out times
                var checkInTime = dtpCheckIn.Value.TimeOfDay;
                var checkOutTime = dtpCheckOut.Value.TimeOfDay;
                var workDuration = checkOutTime - checkInTime;
                var normalStartTime = new TimeSpan(9, 0, 0); // 9 AM
                var normalEndTime = new TimeSpan(17, 0, 0); // 5 PM

                string status;
                if (checkInTime > normalStartTime)
                {
                    status = "Late";
                }
                else if (workDuration.TotalHours < 8)
                {
                    status = "Half Day";
                }
                else if (checkOutTime > normalEndTime)
                {
                    status = "OT";
                }
                else
                {
                    status = "Present";
                }

                var attendance = new Attendance
                {
                    EmployeeId = employeeId,
                    Date = dtpDate.Value.Date,
                    CheckInTime = checkInTime,
                    CheckOutTime = checkOutTime,
                    Status = status
                };

                _context.Attendances.Add(attendance);
                _context.SaveChanges();
                LoadAttendance();
                ClearForm();

                MessageBox.Show("Manual attendance entry added successfully!", 
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding manual entry: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (_currentUser.Role != UserRole.Administrator)
                {
                    MessageBox.Show("Only administrators can update attendance records.", 
                        "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!selectedAttendanceId.HasValue)
                {
                    MessageBox.Show("Please select an attendance record to update", 
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var attendance = _context.Attendances.Find(selectedAttendanceId.Value);
                if (attendance == null) return;

                // Determine status based on check-in and check-out times
                var checkInTime = dtpCheckIn.Value.TimeOfDay;
                var checkOutTime = dtpCheckOut.Value.TimeOfDay;
                var workDuration = checkOutTime - checkInTime;
                var normalStartTime = new TimeSpan(9, 0, 0); // 9 AM
                var normalEndTime = new TimeSpan(17, 0, 0); // 5 PM

                string status;
                if (checkInTime > normalStartTime)
                {
                    status = "Late";
                }
                else if (workDuration.TotalHours < 8)
                {
                    status = "Half Day";
                }
                else if (checkOutTime > normalEndTime)
                {
                    status = "OT";
                }
                else
                {
                    status = "Present";
                }

                attendance.Date = dtpDate.Value.Date;
                attendance.CheckInTime = checkInTime;
                attendance.CheckOutTime = checkOutTime;
                attendance.Status = status;

                _context.SaveChanges();
                LoadAttendance();
                ClearForm();

                MessageBox.Show("Attendance record updated successfully!", 
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating attendance: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (_currentUser.Role != UserRole.Administrator)
                {
                    MessageBox.Show("Only administrators can delete attendance records.", 
                        "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!selectedAttendanceId.HasValue)
                {
                    MessageBox.Show("Please select an attendance record to delete", 
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (MessageBox.Show("Are you sure you want to delete this attendance record?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var attendance = _context.Attendances.Find(selectedAttendanceId.Value);
                    if (attendance != null)
                    {
                        _context.Attendances.Remove(attendance);
                        _context.SaveChanges();
                        LoadAttendance();
                        ClearForm();

                        MessageBox.Show("Attendance record deleted successfully!", 
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting attendance: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvAttendance_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            selectedAttendanceId = (int)dgvAttendance.Rows[e.RowIndex].Cells["Id"].Value;
            var attendance = _context.Attendances
                .Include(a => a.Employee)
                .FirstOrDefault(a => a.AttendanceId == selectedAttendanceId);

            if (attendance == null) return;

            cmbEmployee.SelectedValue = attendance.EmployeeId;
            dtpDate.Value = attendance.Date;
            
            // Handle CheckInTime
            DateTime baseDate = attendance.Date.Date;
            if (attendance.CheckInTime.HasValue)
            {
                dtpCheckIn.Value = baseDate + attendance.CheckInTime.Value;
            }
            else
            {
                dtpCheckIn.Value = baseDate.AddHours(9); // Default to 9 AM
            }

            // Handle CheckOutTime
            if (attendance.CheckOutTime.HasValue)
            {
                dtpCheckOut.Value = baseDate + attendance.CheckOutTime.Value;
            }
            else
            {
                dtpCheckOut.Value = baseDate.AddHours(17); // Default to 5 PM
            }
            
            cmbStatus.Text = attendance.Status;
        }

        private void ClearForm()
        {
            selectedAttendanceId = null;
            if (cmbEmployee.Items.Count > 0) cmbEmployee.SelectedIndex = 0;
            dtpDate.Value = DateTime.Today;
            dtpCheckIn.Value = DateTime.Today.AddHours(9);
            dtpCheckOut.Value = DateTime.Today.AddHours(17);
            cmbStatus.SelectedIndex = 0;
        }

        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Top,
                ForeColor = Color.White,
                Height = 20,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0, 10, 0, 5)
            };
        }

        private Button CreateButton(string text, Color backColor)
        {
            return new Button
            {
                Text = text,
                Dock = DockStyle.Top,
                Height = 35,
                Margin = new Padding(0, 5, 0, 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatAppearance = { BorderSize = 0 }
            };
        }

        private void BtnDownloadReport_Click(object sender, EventArgs e)
        {
            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "CSV files (*.csv)|*.csv";
                    saveDialog.FileName = $"Attendance_Report_{dtpDate.Value:yyyyMM}.csv";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Get the month's data
                        var startOfMonth = new DateTime(dtpDate.Value.Year, dtpDate.Value.Month, 1);
                        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                        var attendanceData = _context.Attendances
                            .Include(a => a.Employee)
                            .ThenInclude(e => e.User)
                            .Where(a => a.Date >= startOfMonth && a.Date <= endOfMonth)
                            .OrderBy(a => a.Employee.User.Name)
                            .ThenBy(a => a.Date)
                            .ToList();

                        // Calculate statistics for each employee
                        var employeeStats = attendanceData
                            .GroupBy(a => a.Employee)
                            .Select(g => new
                            {
                                Employee = g.Key,
                                TotalDays = g.Count(),
                                Present = g.Count(a => a.Status == "Present"),
                                Late = g.Count(a => a.Status == "Late"),
                                Absent = g.Count(a => a.Status == "Absent"),
                                AverageCheckInTime = g.Where(a => a.CheckInTime.HasValue)
                                    .Select(a => a.CheckInTime.Value)
                                    .DefaultIfEmpty(TimeSpan.Zero)
                                    .Average(t => t.TotalMinutes)
                            })
                            .ToList();

                        using (var writer = new StreamWriter(saveDialog.FileName))
                        {
                            // Write header
                            writer.WriteLine("Report Period: {0:MMMM yyyy}", dtpDate.Value);
                            writer.WriteLine();
                            
                            // Write summary section
                            writer.WriteLine("ATTENDANCE SUMMARY");
                            writer.WriteLine("Employee,Total Days,Present,Late,Absent,Average Check-In Time");
                            
                            foreach (var stat in employeeStats)
                            {
                                writer.WriteLine(
                                    $"{stat.Employee.User.Name}," +
                                    $"{stat.TotalDays}," +
                                    $"{stat.Present}," +
                                    $"{stat.Late}," +
                                    $"{stat.Absent}," +
                                    $"{TimeSpan.FromMinutes(stat.AverageCheckInTime):hh\\:mm}"
                                );
                            }

                            writer.WriteLine();
                            writer.WriteLine("DETAILED ATTENDANCE RECORDS");
                            writer.WriteLine("Date,Employee,Check In,Check Out,Status,Duration");

                            // Write detailed records
                            foreach (var record in attendanceData)
                            {
                                var duration = record.CheckInTime.HasValue && record.CheckOutTime.HasValue
                                    ? (record.CheckOutTime.Value - record.CheckInTime.Value).ToString(@"hh\:mm")
                                    : "-";

                                writer.WriteLine(
                                    $"{record.Date:yyyy-MM-dd}," +
                                    $"{record.Employee.User.Name}," +
                                    $"{record.CheckInTime?.ToString(@"hh\:mm") ?? "-"}," +
                                    $"{record.CheckOutTime?.ToString(@"hh\:mm") ?? "-"}," +
                                    $"{record.Status}," +
                                    $"{duration}"
                                );
                            }
                        }

                        MessageBox.Show(
                            "Report generated successfully!\n" +
                            $"Location: {saveDialog.FileName}",
                            "Success",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error generating report: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task<string> GetCurrentLocation()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    var response = await client.GetAsync("http://ip-api.com/json/");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        // Parse JSON response
                        using (JsonDocument document = JsonDocument.Parse(json))
                        {
                            var root = document.RootElement;
                            var city = root.GetProperty("city").GetString();
                            var region = root.GetProperty("regionName").GetString();
                            var country = root.GetProperty("country").GetString();
                            
                            return $"{city}, {region}, {country}";
                        }
                    }
                    return "Location not available";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting location: {ex.Message}");
                return "Location service error";
            }
        }
    }
} 