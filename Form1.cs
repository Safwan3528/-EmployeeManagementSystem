using System;
using System.Drawing;
using System.Windows.Forms;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using System.Linq;
using EmployeeManagementSystem.Forms;
using System.IO;
using System.Collections.Generic;
using EmployeeManagementSystem.Extensions;
using Microsoft.EntityFrameworkCore;
using Timer = System.Windows.Forms.Timer;

namespace EmployeeManagementSystem
{
    public partial class Form1 : Form
    {
        private ApplicationDbContext _context;
        private Color primaryColor = Color.FromArgb(0, 123, 255);
        private Color hoverColor = Color.FromArgb(0, 105, 217);
        private readonly User _currentUser;

        public Form1(User user)
        {
            _currentUser = user;
            InitializeComponent();
            _context = new ApplicationDbContext();
            _context.Database.EnsureCreated();
            
            // Set appearance
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            
            // Initialize navigation
            InitializeNavigation();
            SetupNavigationButtons();
            
            // Show dashboard by default
            ShowDashboard();
            
            // Setup access control
            SetupAccessControl();
        }

        private void InitializeNavigation()
        {
            // Setup header
            lblTitle.Text = "HR Management System";
            lblTitle.ForeColor = Color.White;
            lblTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            
            // Setup close and maximize buttons
            btnClose.Click += (s, e) => Application.Exit();
            btnMaximize.Click += (s, e) => {
                if (this.WindowState == FormWindowState.Maximized)
                {
                    this.WindowState = FormWindowState.Normal;
                    btnMaximize.Text = "□";
                }
                else
                {
                    this.WindowState = FormWindowState.Maximized;
                    btnMaximize.Text = "❐";
                }
            };
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            
            // Make form draggable
            headerPanel.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left)
                {
                    NativeMethods.ReleaseCapture();
                    NativeMethods.SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };
            
            btnDashboard.Click += (s, e) => ShowDashboard();
            btnAttendance.Click += (s, e) => ShowAttendanceTracking();
            btnEmployees.Click += (s, e) => ShowEmployeeManagement();
            btnLeave.Click += (s, e) => ShowLeaveManagement();
            btnPayroll.Click += (s, e) => ShowPayrollManagement();
            btnReports.Click += (s, e) => ShowPerformanceReview();
            btnSettings.Click += (s, e) => ShowSettings();
            btnAbout.Click += (s, e) => ShowAbout();

            // Add Logout button
            var btnLogout = new Button
            {
                Text = "🔓 Logout",
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Height = 45,
                Dock = DockStyle.Bottom,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand
            };

            // add event handler for Logout
            btnLogout.Click += (s, e) => {
                if (MessageBox.Show("Are you sure you want to logout?", 
                    "Confirm Logout", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    // Closed current form
                    this.Close();
                    
                    // open new login form
                    Application.Restart();
                    Environment.Exit(0);  // Pastikan aplikasi ditutup sepenuhnya
                }
            };

            // add hover effect
            btnLogout.MouseEnter += (s, e) => btnLogout.BackColor = hoverColor;
            btnLogout.MouseLeave += (s, e) => btnLogout.BackColor = Color.Transparent;

            // btnExit used DockStyle.Bottom
            btnExit.Dock = DockStyle.Bottom;
            btnExit.BackColor = Color.FromArgb(220, 53, 69);  // Warna merah
            btnExit.Text = "🚪 Exit";  // Guna icon pintu
            btnExit.FlatStyle = FlatStyle.Flat;
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Font = new Font("Segoe UI", 10);
            btnExit.ForeColor = Color.White;
            btnExit.TextAlign = ContentAlignment.MiddleLeft;
            btnExit.Padding = new Padding(15, 0, 0, 0);
            btnExit.Cursor = Cursors.Hand;
            
            // add hover effect for Exit button
            btnExit.MouseEnter += (s, e) => btnExit.BackColor = Color.FromArgb(200, 35, 51);  // Merah lebih gelap
            btnExit.MouseLeave += (s, e) => btnExit.BackColor = Color.FromArgb(220, 53, 69);  // Kembali ke warna asal

            // add event handler for Exit button
            btnExit.Click -= (s, e) => Application.Exit();  // Buang handler lama jika ada
            btnExit.Click += (s, e) => {
                if (MessageBox.Show(
                    "Are you sure you want to exit the application?\nAll unsaved changes will be lost.", 
                    "Confirm Exit", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    // Closed database connection
                    if (_context != null)
                    {
                        _context.Dispose();
                    }

                    // cloed all form
                    foreach (Form form in Application.OpenForms)
                    {
                        form.Close();
                    }

                    // Exit from application
                    Application.Exit();
                    Environment.Exit(0);  // exit enviroment
                }
            };

            // Tambah butang-butang lain
            sidebarPanel.Controls.Add(btnExit);     // Exit in buttom
            sidebarPanel.Controls.Add(btnLogout);   // Logout on top of Exit
            
            // add button
            sidebarPanel.Controls.Add(btnAbout);
            sidebarPanel.Controls.Add(btnSettings);
            sidebarPanel.Controls.Add(btnReports);
            sidebarPanel.Controls.Add(btnPayroll);
            sidebarPanel.Controls.Add(btnLeave);
            sidebarPanel.Controls.Add(btnEmployees);
            sidebarPanel.Controls.Add(btnAttendance);
            sidebarPanel.Controls.Add(btnDashboard);
        }

        private void SetupNavigationButtons()
        {
            // Setup style for all navigation buttons
            foreach (Control ctrl in sidebarPanel.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                    btn.Height = 45;
                    btn.Dock = DockStyle.Top;
                    btn.Font = new Font("Segoe UI", 10);
                    btn.ForeColor = Color.White;
                    btn.TextAlign = ContentAlignment.MiddleLeft;
                    btn.Padding = new Padding(15, 0, 0, 0);
                    
                    btn.MouseEnter += (s, e) => btn.BackColor = hoverColor;
                    btn.MouseLeave += (s, e) => btn.BackColor = Color.Transparent;
                }
            }
        }

        private void ShowDashboard()
        {
            try 
            {
                contentPanel.Controls.Clear();
                var dashboardPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(37, 37, 38),
                    Padding = new Padding(25)
                };

                // For admin, show admin dashboard
                if (_currentUser.Role == UserRole.Administrator)
                {
                    var adminDashboard = CreateAdminDashboard();
                    contentPanel.Controls.Add(adminDashboard);
                    lblTitle.Text = "Admin Dashboard";
                    return;
                }

                // Employee Information
                var employee = _context.Employees
                    .Include(e => e.User)
                    .Include(e => e.Leaves)
                    .FirstOrDefault(e => e.UserId == _currentUser.UserId);

                if (employee == null)
                {
                    MessageBox.Show("Error: Employee data not found.", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Employee Info Card
                var infoCard = CreateDashboardCard("Employee Information", 500, 160);
                infoCard.Location = new Point(25, 25);

                var nameLabel = new Label
                {
                    Text = employee.User.Name.ToUpper(),
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 123, 255),
                    AutoSize = false,
                    Height = 35,
                    Location = new Point(20, 40),
                    Width = 460,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                var detailsLabel = new Label
                {
                    Text = $"Department: {employee.Department}\n" +
                          $"Position: {employee.Position}",
                    ForeColor = Color.LightGray,
                    Font = new Font("Segoe UI", 11),
                    AutoSize = false,
                    Height = 50,
                    Location = new Point(20, 80),
                    Width = 460
                };

                infoCard.Controls.AddRange(new Control[] { detailsLabel, nameLabel });

                // Leave Balance Card
                var leaveCard = CreateDashboardCard("Leave Balance", 500, 160);
                leaveCard.Location = new Point(550, 25);

                var currentYear = DateTime.Now.Year;
                var usedLeaves = employee.Leaves
                    .Count(l => l.Status == LeaveStatus.Approved && 
                               l.StartDate.Year == currentYear);
                var remainingLeaves = Math.Max(0, 14 - usedLeaves);

                var leaveBalancePanel = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 2,
                    Padding = new Padding(20)
                };

                var remainingLabel = CreateStatItem("📅", $"Remaining Leaves\n{remainingLeaves} days", Color.FromArgb(40, 167, 69));
                var usedLabel = CreateStatItem("✓", $"Used Leaves\n{usedLeaves} days", Color.FromArgb(0, 123, 255));

                leaveBalancePanel.Controls.Add(remainingLabel, 0, 0);
                leaveBalancePanel.Controls.Add(usedLabel, 1, 0);

                leaveCard.Controls.Add(leaveBalancePanel);

                // Coworkers on Leave Card
                var onLeaveCard = CreateDashboardCard("Coworkers On Leave Today", 500, 160);
                onLeaveCard.Location = new Point(25, 210);

                var today = DateTime.Today;
                var coworkersOnLeave = _context.Leaves
                    .Include(l => l.Employee)
                    .ThenInclude(e => e.User)
                    .Where(l => l.Status == LeaveStatus.Approved &&
                               l.StartDate.Date <= today &&
                               l.EndDate.Date >= today &&
                               l.EmployeeId != employee.EmployeeId)
                    .Select(l => l.Employee)
                    .ToList();

                if (coworkersOnLeave.Any())
                {
                    var coworkersPanel = new FlowLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        FlowDirection = FlowDirection.TopDown,
                        Padding = new Padding(20),
                        AutoScroll = true
                    };

                    foreach (var coworker in coworkersOnLeave)
                    {
                        var coworkerLabel = new Label
                        {
                            Text = $"• {coworker.User.Name} ({coworker.Position})",
                            ForeColor = Color.White,
                            Font = new Font("Segoe UI", 10),
                            AutoSize = true,
                            Margin = new Padding(0, 5, 0, 5)
                        };
                        coworkersPanel.Controls.Add(coworkerLabel);
                    }

                    onLeaveCard.Controls.Add(coworkersPanel);
                }
                else
                {
                    onLeaveCard.Controls.Add(new Label
                    {
                        Text = "No coworkers on leave today",
                        ForeColor = Color.LightGray,
                        Font = new Font("Segoe UI", 11),
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter
                    });
                }

                // Latest Payslip Card
                var payslipCard = CreateDashboardCard("Latest Payslip", 500, 160);
                payslipCard.Location = new Point(550, 210);

                var latestPayslip = _context.Payrolls
                    .Where(p => p.EmployeeId == employee.EmployeeId)
                    .OrderByDescending(p => p.PayPeriodEnd)
                    .FirstOrDefault();

                if (latestPayslip != null)
                {
                    var payslipPanel = new TableLayoutPanel
                    {
                        Dock = DockStyle.Fill,
                        ColumnCount = 1,
                        RowCount = 3,
                        Padding = new Padding(20)
                    };

                    var amountLabel = new Label
                    {
                        Text = $"RM {latestPayslip.NetSalary:N2}",
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        ForeColor = Color.White,
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter
                    };

                    var periodLabel = new Label
                    {
                        Text = $"For {latestPayslip.PayPeriodStart:MMMM yyyy}",
                        Font = new Font("Segoe UI", 7),
                        ForeColor = Color.LightGray,
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.TopCenter
                    };

                    var downloadButton = new Button
                    {
                        Text = "Download Payslip",
                        Height = 35,
                        Dock = DockStyle.Bottom,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.FromArgb(0, 123, 255),
                        ForeColor = Color.White,
                        Cursor = Cursors.Hand
                    };
                    downloadButton.FlatAppearance.BorderSize = 0;
                    downloadButton.Click += (s, e) => DownloadPayslip(latestPayslip);

                    payslipPanel.Controls.Add(periodLabel, 0, 0);
                    payslipPanel.Controls.Add(amountLabel, 0, 2);
                    payslipPanel.Controls.Add(downloadButton, 0, 2);

                    payslipCard.Controls.Add(payslipPanel);
                }
                else
                {
                    payslipCard.Controls.Add(new Label
                    {
                        Text = "No payslip available",
                        ForeColor = Color.LightGray,
                        Font = new Font("Segoe UI", 11),
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter
                    });
                }

                // Add all cards to dashboard
                dashboardPanel.Controls.AddRange(new Control[] { 
                    payslipCard,
                    onLeaveCard,
                    leaveCard,
                    infoCard
                });

                contentPanel.Controls.Add(dashboardPanel);
                lblTitle.Text = "Dashboard";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard: {ex.Message}\n{ex.StackTrace}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Panel CreateAdminDashboard()
        {
            var adminPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(37, 37, 38),
                Padding = new Padding(25)
            };

            // Admin Info Card
            var infoCard = CreateDashboardCard("Administrator Information", 500, 160);
            infoCard.Location = new Point(25, 25);

            var nameLabel = new Label
            {
                Text = _currentUser.Name.ToUpper(),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 123, 255),
                AutoSize = false,
                Height = 35,
                Dock = DockStyle.None,
                Location = new Point(20, 40),
                Width = 460,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var roleLabel = new Label
            {
                Text = "Role: System Administrator",
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 11),
                AutoSize = false,
                Height = 30,
                Location = new Point(20, 80),
                Width = 460,
                TextAlign = ContentAlignment.MiddleLeft
            };

            infoCard.Controls.AddRange(new Control[] { roleLabel, nameLabel });

            // Today's Statistics Card
            var statsCard = CreateDashboardCard("Today's Overview", 500, 160);
            statsCard.Location = new Point(550, 25);

            var today = DateTime.Today;
            var totalEmployees = _context.Employees.Count();
            var presentToday = _context.Attendances
                .Count(a => a.Date.Date == today && 
                           (a.Status == "Present" || a.Status == "Late"));
            var onLeaveToday = _context.Leaves
                .Count(l => l.StartDate.Date <= today && 
                           l.EndDate.Date >= today && 
                           l.Status == LeaveStatus.Approved);
            var pendingLeaves = _context.Leaves
                .Count(l => l.Status == LeaveStatus.Pending);

            var statsPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(20),
            };

            // Create stat items with icons
            var presentLabel = CreateStatItem("👥", $"Present Today\n{presentToday}/{totalEmployees}", Color.FromArgb(40, 167, 69));
            var leaveLabel = CreateStatItem("��", $"On Leave Today\n{onLeaveToday}", Color.FromArgb(255, 193, 7));
            var pendingLabel = CreateStatItem("⏳", $"Pending Leaves\n{pendingLeaves}", Color.FromArgb(220, 53, 69));
            var totalLabel = CreateStatItem("📊", $"Total Employees\n{totalEmployees}", Color.FromArgb(0, 123, 255));

            statsPanel.Controls.Add(presentLabel, 0, 0);
            statsPanel.Controls.Add(leaveLabel, 1, 0);
            statsPanel.Controls.Add(pendingLabel, 0, 1);
            statsPanel.Controls.Add(totalLabel, 1, 1);

            statsCard.Controls.Add(statsPanel);

            // Recent Leave Requests Card
            var leaveRequestsCard = CreateDashboardCard("Recent Leave Requests", 1025, 200);
            leaveRequestsCard.Location = new Point(25, 210);

            var recentLeaves = _context.Leaves
                .Include(l => l.Employee)
                .ThenInclude(e => e.User)
                .Where(l => l.Status == LeaveStatus.Pending)
                .OrderByDescending(l => l.RequestDate)
                .Take(5)
                .ToList();

            if (recentLeaves.Any())
            {
                var leavePanel = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 4,
                    RowCount = recentLeaves.Count + 1,
                    Padding = new Padding(20, 10, 20, 10)
                };

                // Add headers
                var headers = new[] { "Employee", "Leave Type", "Duration", "Status" };
                for (int i = 0; i < headers.Length; i++)
                {
                    leavePanel.Controls.Add(new Label
                    {
                        Text = headers[i],
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        ForeColor = Color.FromArgb(0, 123, 255),
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft
                    }, i, 0);
                }

                // Add leave requests
                int row = 1;
                foreach (var leave in recentLeaves)
                {
                    var duration = (leave.EndDate - leave.StartDate).Days + 1;
                    
                    leavePanel.Controls.Add(new Label
                    {
                        Text = leave.Employee.User.Name,
                        ForeColor = Color.White,
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft
                    }, 0, row);

                    leavePanel.Controls.Add(new Label
                    {
                        Text = leave.LeaveType,
                        ForeColor = Color.White,
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft
                    }, 1, row);

                    leavePanel.Controls.Add(new Label
                    {
                        Text = $"{leave.StartDate:dd/MM/yyyy} - {leave.EndDate:dd/MM/yyyy} ({duration} days)",
                        ForeColor = Color.White,
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft
                    }, 2, row);

                    var statusLabel = new Label
                    {
                        Text = leave.Status.ToString(),
                        ForeColor = Color.FromArgb(255, 193, 7),
                        Font = new Font("Segoe UI", 9, FontStyle.Bold),
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleLeft
                    };

                    leavePanel.Controls.Add(statusLabel, 3, row);
                    row++;
                }

                leaveRequestsCard.Controls.Add(leavePanel);
            }
            else
            {
                leaveRequestsCard.Controls.Add(new Label
                {
                    Text = "No pending leave requests",
                    ForeColor = Color.LightGray,
                    Font = new Font("Segoe UI", 11),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                });
            }

            // Add all cards to admin panel
            adminPanel.Controls.AddRange(new Control[] { 
                leaveRequestsCard,
                statsCard, 
                infoCard 
            });

            return adminPanel;
        }

        private Panel CreateStatItem(string icon, string text, Color color)
        {
            var panel = new Panel
            {
                Margin = new Padding(5),
                Height = 45,
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(50, 50, 53)
            };

            var iconLabel = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 20),
                ForeColor = color,
                Location = new Point(10, 5),
                Size = new Size(40, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var textLabel = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White,
                Location = new Point(55, 5),
                Size = new Size(150, 40),
                TextAlign = ContentAlignment.MiddleLeft
            };

            panel.Controls.AddRange(new Control[] { iconLabel, textLabel });
            return panel;
        }

        private Label CreateStatsLabel(string text)
        {
            return new Label
            {
                Text = text,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12),
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(0, 5, 0, 5)
            };
        }

        private Label CreateEmployeeLabel(string name)
        {
            return new Label
            {
                Text = name,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11),
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(0, 5, 0, 5)
            };
        }

        private Label CreateInfoLabel(string text)
        {
            return new Label
            {
                Text = text,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                AutoSize = false
            };
        }

        private Button CreatePayslipButton(Payroll payslip)
        {
            var btn = new Button
            {
                Text = $"Payslip {payslip.PayPeriodStart:MMM yyyy}",
                Dock = DockStyle.Top,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                Margin = new Padding(0, 5, 0, 5),
                Cursor = Cursors.Hand,
                Tag = payslip
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => DownloadPayslip(payslip);

            // Add hover effect
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(0, 105, 217);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(0, 123, 255);

            return btn;
        }

        private Panel CreateDashboardCard(string title, int width, int height)
        {
            var card = new Panel
            {
                Width = width,
                Height = height,
                BackColor = Color.FromArgb(45, 45, 48),
                Padding = new Padding(0)
            };

            if (!string.IsNullOrEmpty(title))
            {
                var titleLabel = new Label
                {
                    Text = title,
                    ForeColor = Color.FromArgb(0, 123, 255),
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    Dock = DockStyle.Top,
                    Height = 30,
                    Padding = new Padding(15, 10, 15, 0)
                };
                card.Controls.Add(titleLabel);
            }

            return card;
        }

        private void DownloadPayslip(Payroll payroll)
        {
            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "PDF files (*.pdf)|*.pdf";
                    saveDialog.FileName = $"Payslip_{payroll.PayPeriodStart:yyyyMM}.pdf";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Generate payslip content
                        var employee = _context.Employees
                            .Include(e => e.User)
                            .FirstOrDefault(e => e.EmployeeId == payroll.EmployeeId);

                        if (employee == null) return;

                        // Here you would generate the PDF
                        // For now, we'll just show a success message
                        MessageBox.Show(
                            $"Payslip downloaded successfully!\n" +
                            $"Employee: {employee.User.Name}\n" +
                            $"Period: {payroll.PayPeriodStart:MMM yyyy}\n" +
                            $"Net Salary: {payroll.NetSalary:C2}",
                            "Download Success",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error downloading payslip: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowEmployeeManagement()
        {
            // Debug message for called function
            Console.WriteLine("Opening Employee Management...");

            if (_currentUser.Role != UserRole.Administrator)
            {
                MessageBox.Show("Access denied. Only administrators can access Employee Management.",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            try 
            {
                contentPanel.Controls.Clear();
                var employeeForm = new EmployeeForm(_currentUser);
                employeeForm.TopLevel = false;
                employeeForm.FormBorderStyle = FormBorderStyle.None;
                employeeForm.Dock = DockStyle.Fill;
                employeeForm.Visible = true;  // Tambah ini
                
                contentPanel.Controls.Add(employeeForm);
                employeeForm.Show();
                lblTitle.Text = "Employee Management";

                // Debug message for added form
                Console.WriteLine("Employee Management form added to panel");
            }
            catch (Exception ex)
            {
                // add error handling
                MessageBox.Show($"Error loading Employee Management: {ex.Message}\n\n{ex.StackTrace}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowAttendanceTracking()
        {
            contentPanel.Controls.Clear();
            var attendanceForm = new AttendanceForm(_currentUser);  // Pass currentUser
            attendanceForm.TopLevel = false;
            attendanceForm.FormBorderStyle = FormBorderStyle.None;
            attendanceForm.Dock = DockStyle.Fill;
            attendanceForm.Visible = true;  // Tambah ini untuk pastikan form visible
            
            contentPanel.Controls.Add(attendanceForm);
            attendanceForm.Show();
            lblTitle.Text = "Attendance Tracking";

            // Debug message
            Console.WriteLine("Attendance form loaded");
        }

        private void ShowLeaveManagement()
        {
            contentPanel.Controls.Clear();
            var leaveForm = new LeaveForm(_currentUser);
            leaveForm.TopLevel = false;
            leaveForm.FormBorderStyle = FormBorderStyle.None;
            leaveForm.Dock = DockStyle.Fill;
            
            contentPanel.Controls.Add(leaveForm);
            leaveForm.Show();
            lblTitle.Text = "Leave Management";
        }

        private void ShowPayrollManagement()
        {
            if (_currentUser.Role != UserRole.Administrator)
            {
                MessageBox.Show("Access denied. Only administrators can access Payroll Management.",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            contentPanel.Controls.Clear();
            var payrollForm = new PayrollForm(_currentUser);
            payrollForm.TopLevel = false;
            payrollForm.FormBorderStyle = FormBorderStyle.None;
            payrollForm.Dock = DockStyle.Fill;
            
            contentPanel.Controls.Add(payrollForm);
            payrollForm.Show();
            lblTitle.Text = "Payroll Management";
        }

        private void ShowPerformanceReview()
        {
            if (_currentUser.Role != UserRole.Administrator)
            {
                MessageBox.Show("Access denied. Only administrators can manage Performance Reviews.",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            contentPanel.Controls.Clear();
            var performanceReviewForm = new PerformanceReviewForm(_currentUser);
            performanceReviewForm.TopLevel = false;
            performanceReviewForm.FormBorderStyle = FormBorderStyle.None;
            performanceReviewForm.Dock = DockStyle.Fill;
            
            contentPanel.Controls.Add(performanceReviewForm);
            performanceReviewForm.Show();
            lblTitle.Text = "Performance Review";
        }

        private void ShowAbout()
        {
            // Main container panel with dark background
            contentPanel.Controls.Clear();
            var aboutPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(37, 37, 38),
                Padding = new Padding(20)
            };

            // Define developers array
            var developers = new[]
            {
                new { Name = "SAFWAN RAHIMI BIN SUHAILI", Matric = "B24070037" },
                new { Name = "NORUL AZWA BINTI HASSAN", Matric = "B24070014" },
                new { Name = "AHMAD FAHMIE AIZZAT BIN ABDUL MAJID", Matric = "B24080028" }
            };

            // Center panel that holds all content with slightly lighter background
            var centerPanel = new Panel
            {
                Width = 700,    // Optimal width for content
                Height = 700,   // Optimal height for content
                BackColor = Color.FromArgb(45, 45, 48),
                Location = new Point(
                    (aboutPanel.ClientSize.Width - 700) / 2,
                    (aboutPanel.ClientSize.Height - 700) / 2)
                };
            centerPanel.Anchor = AnchorStyles.None;

            // App logo
            var logoLabel = new Label
            {
                Text = "👥",
                Font = new Font("Segoe UI", 72),
                ForeColor = Color.FromArgb(0, 123, 255),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 120,
                Margin = new Padding(0, 30, 0, 0)
            };

            // App title
            var titleLabel = new Label
            {
                Text = "Employee Management",
                Font = new Font("Segoe UI", 29, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 60,
                Margin = new Padding(0, 10, 0, 10)
            };

            // Version number
            var versionLabel = new Label
            {
                Text = "Version 1.0",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(158, 158, 158),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 40,
                Margin = new Padding(0, 0, 0, 20)
            };

            // Top separator
            var separator = new Panel
            {
                Height = 2,
                BackColor = Color.FromArgb(60, 60, 60),
                Dock = DockStyle.Top,
                Margin = new Padding(150, 20, 150, 20)
            };

            // "Developed by" text
            var developerLabel = new Label
            {
                Text = "Developed by",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(158, 158, 158),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 30,
                Margin = new Padding(0, 20, 0, 10)
            };

            // Developer names panel
            var namesPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 250,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(40),
                BackColor = Color.FromArgb(45, 45, 48)
            };

            // Add developers with proper spacing
            foreach (var dev in developers)
            {
                var devPanel = new Panel { 
                    Height = 60,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0, 5, 0, 5),
                    BackColor = Color.FromArgb(45, 45, 48)
                };
                
                var nameLabel = new Label
                {
                    Text = dev.Name,
                    Font = new Font("Segoe UI", 13, FontStyle.Bold),
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Top,
                    Height = 35,
                    Padding = new Padding(0)
                };

                var matricLabel = new Label
                {
                    Text = dev.Matric,
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.FromArgb(158, 158, 158),
                    TextAlign = ContentAlignment.TopCenter,
                    Dock = DockStyle.Top,
                    Height = 25,
                    Padding = new Padding(0)
                };

                devPanel.Controls.Add(matricLabel);
                devPanel.Controls.Add(nameLabel);
                namesPanel.Controls.Add(devPanel);
            }

            // Bottom separator
            var bottomSeparator = new Panel
            {
                Height = 1,
                BackColor = Color.FromArgb(60, 60, 60),
                Dock = DockStyle.Top,
                Margin = new Padding(150, 15, 150, 15)
            };

            // Copyright text
            var copyrightLabel = new Label
            {
                Text = "© <DevTitans/> 2025 All rights reserved.",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(158, 158, 158),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 30
            };

            // Company Handbook button
            var handbookButton = new Button
            {
                Text = "📚 Company Handbook",
                Dock = DockStyle.Top,
                Height = 50,
                Margin = new Padding(150, 30, 150, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13),
                Cursor = Cursors.Hand
            };
            handbookButton.FlatAppearance.BorderSize = 0;

            // Button hover effects
            handbookButton.MouseEnter += (s, e) => {
                handbookButton.BackColor = Color.FromArgb(0, 105, 217);
            };
            handbookButton.MouseLeave += (s, e) => {
                handbookButton.BackColor = Color.FromArgb(0, 123, 255);
            };

            // Button click handler
            handbookButton.Click += (s, e) => {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "https://ems-company-policies.vercel.app/",
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening handbook: {ex.Message}", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // Add all controls in reverse order (bottom to top)
            centerPanel.Controls.AddRange(new Control[] {
                handbookButton,
                copyrightLabel,
                bottomSeparator,
                namesPanel,
                developerLabel,
                separator,
                versionLabel,
                titleLabel,
                logoLabel
            });

            // Add panels to form
            aboutPanel.Controls.Add(centerPanel);
            contentPanel.Controls.Add(aboutPanel);
            lblTitle.Text = "About";

            // Handle window resize
            aboutPanel.Resize += (s, e) => {
                centerPanel.Location = new Point(
                    (aboutPanel.ClientSize.Width - centerPanel.Width) / 2,
                    (aboutPanel.ClientSize.Height - centerPanel.Height) / 2);
            };
        }

        private void ShowSettings()
        {
            try
            {
                contentPanel.Controls.Clear();
                var settingsPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(37, 37, 38),
                    Padding = new Padding(20)
                };

                // Security Settings card
                var securityCard = new Panel
                {
                    Width = 400,
                    Height = 350,
                    BackColor = Color.FromArgb(45, 45, 48),
                    Location = new Point(20, 20)
                };

                var securityTitle = new Label
                {
                    Text = "Security Settings",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 123, 255),
                    Location = new Point(15, 15),
                    AutoSize = true
                };

                // Change Password section
                var passwordPanel = new Panel
                {
                    Location = new Point(15, 50),
                    Width = 370,
                    Height = 280
                };

                var currentPasswordLabel = new Label
                {
                    Text = "Current Password",
                    ForeColor = Color.White,
                    Location = new Point(0, 0),
                    AutoSize = true
                };

                var currentPasswordBox = new TextBox
                {
                    Location = new Point(0, 25),
                    Width = 350,
                    Height = 30,
                    PasswordChar = '•',
                    BackColor = Color.FromArgb(60, 60, 60),
                    ForeColor = Color.White
                };

                var newPasswordLabel = new Label
                {
                    Text = "New Password",
                    ForeColor = Color.White,
                    Location = new Point(0, 70),
                    AutoSize = true
                };

                var newPasswordBox = new TextBox
                {
                    Location = new Point(0, 95),
                    Width = 350,
                    Height = 30,
                    PasswordChar = '•',
                    BackColor = Color.FromArgb(60, 60, 60),
                    ForeColor = Color.White
                };

                var confirmPasswordLabel = new Label
                {
                    Text = "Confirm New Password",
                    ForeColor = Color.White,
                    Location = new Point(0, 140),
                    AutoSize = true
                };

                var confirmPasswordBox = new TextBox
                {
                    Location = new Point(0, 165),
                    Width = 350,
                    Height = 30,
                    PasswordChar = '•',
                    BackColor = Color.FromArgb(60, 60, 60),
                    ForeColor = Color.White
                };

                var changePasswordButton = new Button
                {
                    Text = "Change Password",
                    Location = new Point(0, 220),
                    Width = 350,
                    Height = 40,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(0, 123, 255),
                    ForeColor = Color.White,
                    Cursor = Cursors.Hand
                };
                changePasswordButton.FlatAppearance.BorderSize = 0;

                // Add controls to panels
                passwordPanel.Controls.AddRange(new Control[] {
                    changePasswordButton,
                    confirmPasswordBox,
                    confirmPasswordLabel,
                    newPasswordBox,
                    newPasswordLabel,
                    currentPasswordBox,
                    currentPasswordLabel
                });

                securityCard.Controls.AddRange(new Control[] {
                    passwordPanel,
                    securityTitle
                });

                // Only show database management for admin
                if (_currentUser.Role == UserRole.Administrator)
                {
                    // Database Management card
                    var databaseCard = new Panel
                    {
                        Width = 400,
                        Height = 350,
                        BackColor = Color.FromArgb(45, 45, 48),
                        Location = new Point(440, 20)
                    };

                    var databaseTitle = new Label
                    {
                        Text = "Database Management",
                        Font = new Font("Segoe UI", 12, FontStyle.Bold),
                        ForeColor = Color.FromArgb(0, 123, 255),
                        Location = new Point(15, 15),
                        AutoSize = true
                    };

                    // Database buttons panel
                    var databaseButtonsPanel = new Panel
                    {
                        Location = new Point(15, 50),
                        Width = 370,
                        Height = 280
                    };

                    var backupButton = new Button
                    {
                        Text = "Backup Database",
                        Location = new Point(0, 0),
                        Width = 350,
                        Height = 40,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.FromArgb(40, 167, 69),
                        ForeColor = Color.White,
                        Cursor = Cursors.Hand
                    };
                    backupButton.FlatAppearance.BorderSize = 0;

                    var backupLabel = new Label
                    {
                        Text = "Create a backup of your current database",
                        ForeColor = Color.LightGray,
                        Location = new Point(0, 45),
                        AutoSize = true
                    };

                    var restoreButton = new Button
                    {
                        Text = "Restore Database",
                        Location = new Point(0, 90),
                        Width = 350,
                        Height = 40,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.FromArgb(255, 193, 7),
                        ForeColor = Color.White,
                        Cursor = Cursors.Hand
                    };
                    restoreButton.FlatAppearance.BorderSize = 0;

                    var restoreLabel = new Label
                    {
                        Text = "Restore database from a backup file",
                        ForeColor = Color.LightGray,
                        Location = new Point(0, 135),
                        AutoSize = true
                    };

                    var resetButton = new Button
                    {
                        Text = "Reset Database",
                        Location = new Point(0, 180),
                        Width = 350,
                        Height = 40,
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.FromArgb(220, 53, 69),
                        ForeColor = Color.White,
                        Cursor = Cursors.Hand
                    };
                    resetButton.FlatAppearance.BorderSize = 0;

                    var resetLabel = new Label
                    {
                        Text = "Reset database to initial state (Warning: All data will be lost!)",
                        ForeColor = Color.LightGray,
                        Location = new Point(0, 225),
                        AutoSize = true
                    };

                    databaseButtonsPanel.Controls.AddRange(new Control[] {
                        resetLabel,
                        resetButton,
                        restoreLabel,
                        restoreButton,
                        backupLabel,
                        backupButton
                    });

                    databaseCard.Controls.AddRange(new Control[] {
                        databaseButtonsPanel,
                        databaseTitle
                    });

                    settingsPanel.Controls.Add(databaseCard);

                    // Add database management event handlers
                    backupButton.Click += (s, e) => BackupDatabase();
                    restoreButton.Click += (s, e) => RestoreDatabase();
                    resetButton.Click += (s, e) => ResetDatabase();
                }

                // Add security card to settings panel
                settingsPanel.Controls.Add(securityCard);

                // Add settings panel to content panel
                contentPanel.Controls.Add(settingsPanel);
                lblTitle.Text = "Settings";

                // Add event handlers
                changePasswordButton.Click += (s, e) => ChangePassword(currentPasswordBox, newPasswordBox, confirmPasswordBox);

                // Add hover effects for buttons
                changePasswordButton.MouseEnter += (s, e) => changePasswordButton.BackColor = ControlPaint.Dark(changePasswordButton.BackColor, 0.1f);
                changePasswordButton.MouseLeave += (s, e) => changePasswordButton.BackColor = Color.FromArgb(0, 123, 255);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ChangePassword(TextBox txtCurrentPassword, TextBox txtNewPassword, TextBox txtConfirmPassword)
        {
            try
            {
                if (txtCurrentPassword.Text != _currentUser.Password)
                {
                    MessageBox.Show("Current password is incorrect", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtNewPassword.Text))
                {
                    MessageBox.Show("New password cannot be empty", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (txtNewPassword.Text != txtConfirmPassword.Text)
                {
                    MessageBox.Show("New passwords do not match", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var user = _context.Users.Find(_currentUser.UserId);
                user.Password = txtNewPassword.Text;
                _context.SaveChanges();

                MessageBox.Show("Password changed successfully!\nPlease login again with your new password.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                Application.Restart();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error changing password: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BackupDatabase()
        {
            try
            {
                // Dispose current context to ensure all connections are closed
                _context.Dispose();

                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "SQLite Database (*.db)|*.db|All files (*.*)|*.*";
                    saveDialog.FileName = $"hrmanagement_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";
                    saveDialog.Title = "Select Backup Location";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        string sourceFile = Path.Combine(Application.StartupPath, "hrmanagement.db");
                        File.Copy(sourceFile, saveDialog.FileName, true);

                        MessageBox.Show(
                            $"Database backup created successfully!\nLocation: {saveDialog.FileName}",
                            "Backup Success",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }

                // Recreate context
                _context = new ApplicationDbContext();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error creating backup: {ex.Message}",
                    "Backup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void RestoreDatabase()
        {
            try
            {
                // Dispose current context
                _context.Dispose();

                using (var openDialog = new OpenFileDialog())
                {
                    openDialog.Filter = "SQLite Database (*.db)|*.db|All files (*.*)|*.*";
                    openDialog.Title = "Select Database Backup to Restore";

                    if (openDialog.ShowDialog() == DialogResult.OK)
                    {
                        if (MessageBox.Show(
                            "WARNING: This will replace your current database with the selected backup.\n" +
                            "All current data will be lost.\n\n" +
                            "Are you sure you want to continue?",
                            "Confirm Restore",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning) == DialogResult.Yes)
                        {
                            string destinationFile = Path.Combine(Application.StartupPath, "hrmanagement.db");
                            File.Copy(openDialog.FileName, destinationFile, true);

                            MessageBox.Show(
                                "Database restored successfully!\nThe application will now restart.",
                                "Restore Success",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                            Application.Restart();
                            Environment.Exit(0);
                        }
                    }
                }

                // Recreate context if not restarting
                _context = new ApplicationDbContext();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error restoring database: {ex.Message}",
                    "Restore Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                
                // Ensure context is recreated
                _context = new ApplicationDbContext();
            }
        }

        private void ResetDatabase()
        {
            try
            {
                if (MessageBox.Show(
                    "WARNING: This will delete ALL data from the database!\n" +
                    "This action cannot be undone.\n\n" +
                    "Are you sure you want to continue?",
                    "Confirm Database Reset",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    // Dispose current context
                    _context.Dispose();

                    // Delete the database file
                    string dbPath = Path.Combine(Application.StartupPath, "hrmanagement.db");
                    if (File.Exists(dbPath))
                    {
                        File.Delete(dbPath);
                    }

                    // Create new context and database
                    _context = new ApplicationDbContext();
                    _context.Database.EnsureCreated();

                    MessageBox.Show(
                        "Database has been reset successfully.\n" +
                        "The application will now restart.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    Application.Restart();
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error resetting database: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void SetupAccessControl()
        {
            // Restrict access based on user role
            if (_currentUser.Role != UserRole.Administrator)
            {
                // Hide management buttons for non-admin users
                btnEmployees.Visible = false;     // Hide employee management
                btnPayroll.Visible = false;       // Hide payroll management
                btnReports.Visible = false;       // Hide performance review
                
                // Keep settings visible for password change
                // btnSettings.Visible = true;  // Allow access to settings for password change
            }
        }

        private void LoadSecuritySettings(CheckBox chkComplex, CheckBox chk2FA, TextBox txtExpiry)
        {
            try
            {
                // Load settings from configuration or database
                // For now, just set default values
                chkComplex.Checked = true;
                chk2FA.Checked = false;
                txtExpiry.Text = "90";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading security settings: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                FlatAppearance = { BorderSize = 0 },
                Cursor = Cursors.Hand
            };
        }

        // Helper class for form dragging
        private class NativeMethods
        {
            public const int WM_NCLBUTTONDOWN = 0xA1;
            public const int HT_CAPTION = 0x2;

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool ReleaseCapture();
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

        private class RecentActivity
        {
            public string Activity { get; set; }
            public DateTime Date { get; set; }
        }
    }

    // Helper extension method to insert spaces before uppercase letters
    public static class StringExtensions
    {
        public static string InsertSpaceBeforeUpperCase(this string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            var result = text[0].ToString();
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    result += " " + text[i];
                else
                    result += text[i];
            }
            return result;
        }
    }
}
