using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Forms
{
    public partial class LeaveForm : Form
    {
        private readonly ApplicationDbContext _context;
        private readonly DataGridView dgvLeaves;
        private readonly Panel formPanel;
        private readonly ComboBox cmbEmployee;
        private readonly ComboBox cmbLeaveType;
        private readonly DateTimePicker dtpStartDate;
        private readonly DateTimePicker dtpEndDate;
        private readonly TextBox txtReason;
        private int? selectedLeaveId;
        private readonly User _currentUser;

        public LeaveForm(User currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            _context = new ApplicationDbContext();

            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(37, 37, 38);

            // Initialize main controls
            dgvLeaves = new DataGridView
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

            // Initialize form controls
            cmbEmployee = new ComboBox
            {
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbLeaveType = new ComboBox
            {
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            dtpStartDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Dock = DockStyle.Top
            };

            dtpEndDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Dock = DockStyle.Top
            };

            txtReason = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 100,
                Multiline = true,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            SetupFormControls();
            SetupDataGridView();
            LoadLeaveTypes();
            LoadEmployees();
            LoadLeaves();
        }

        private void SetupFormControls()
        {
            var lblTitle = new Label
            {
                Text = "Leave Request",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Height = 40
            };

            // Create buttons
            var btnSubmit = CreateButton("Submit Request", Color.FromArgb(0, 123, 255));
            var btnApprove = CreateButton("Approve", Color.FromArgb(40, 167, 69));
            var btnReject = CreateButton("Reject", Color.FromArgb(220, 53, 69));
            var btnClear = CreateButton("Clear Form", Color.FromArgb(108, 117, 125));

            // Hide approval buttons for non-admin users
            btnApprove.Visible = _currentUser.Role == UserRole.Administrator;
            btnReject.Visible = _currentUser.Role == UserRole.Administrator;

            // For non-admin users, only show their own leave requests
            if (_currentUser.Role != UserRole.Administrator)
            {
                var employee = _context.Employees.FirstOrDefault(e => e.UserId == _currentUser.UserId);
                if (employee != null)
                {
                    cmbEmployee.Enabled = false;
                    cmbEmployee.SelectedValue = employee.EmployeeId;
                }
            }

            formPanel.Controls.AddRange(new Control[] {
                btnClear,
                btnReject,
                btnApprove,
                btnSubmit,
                CreateLabel("Reason"),
                txtReason,
                CreateLabel("End Date"),
                dtpEndDate,
                CreateLabel("Start Date"),
                dtpStartDate,
                CreateLabel("Leave Type"),
                cmbLeaveType,
                CreateLabel("Employee"),
                cmbEmployee,
                lblTitle
            });

            // Add event handlers
            btnSubmit.Click += BtnSubmit_Click;
            btnApprove.Click += BtnApprove_Click;
            btnReject.Click += BtnReject_Click;
            btnClear.Click += BtnClear_Click;
        }

        private void LoadLeaveTypes()
        {
            cmbLeaveType.Items.AddRange(new string[] {
                "Annual Leave",
                "Sick Leave",
                "Emergency Leave",
                "Unpaid Leave",
                "Maternity Leave",
                "Paternity Leave"
            });
            cmbLeaveType.SelectedIndex = 0;
        }

        private void SetupDataGridView()
        {
            // Update DataGridView styling
            dgvLeaves.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };

            dgvLeaves.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f),
                Padding = new Padding(10, 0, 0, 0),
                SelectionBackColor = Color.FromArgb(100, 100, 100),
                SelectionForeColor = Color.White
            };

            dgvLeaves.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(55, 55, 58),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f),
                SelectionBackColor = Color.FromArgb(100, 100, 100),
                SelectionForeColor = Color.White
            };

            dgvLeaves.EnableHeadersVisualStyles = false;
            dgvLeaves.RowHeadersVisible = false;
            dgvLeaves.ColumnHeadersHeight = 40;
            dgvLeaves.RowTemplate.Height = 35;

            // Add columns with styling
            dgvLeaves.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { 
                    Name = "Id", 
                    HeaderText = "ID", 
                    Width = 50,
                    DefaultCellStyle = new DataGridViewCellStyle {
                        Alignment = DataGridViewContentAlignment.MiddleCenter
                    }
                },
                new DataGridViewTextBoxColumn { 
                    Name = "EmployeeName", 
                    HeaderText = "Employee", 
                    Width = 150 
                },
                new DataGridViewTextBoxColumn { 
                    Name = "LeaveType", 
                    HeaderText = "Leave Type", 
                    Width = 120 
                },
                new DataGridViewTextBoxColumn { 
                    Name = "StartDate", 
                    HeaderText = "Start Date", 
                    Width = 100,
                    DefaultCellStyle = new DataGridViewCellStyle {
                        Alignment = DataGridViewContentAlignment.MiddleCenter
                    }
                },
                new DataGridViewTextBoxColumn { 
                    Name = "EndDate", 
                    HeaderText = "End Date", 
                    Width = 100,
                    DefaultCellStyle = new DataGridViewCellStyle {
                        Alignment = DataGridViewContentAlignment.MiddleCenter
                    }
                },
                new DataGridViewTextBoxColumn { 
                    Name = "Status", 
                    HeaderText = "Status", 
                    Width = 100,
                    DefaultCellStyle = new DataGridViewCellStyle {
                        Alignment = DataGridViewContentAlignment.MiddleCenter
                    }
                },
                new DataGridViewTextBoxColumn { 
                    Name = "Reason", 
                    HeaderText = "Reason", 
                    Width = 200 
                }
            });

            dgvLeaves.CellClick += DgvLeaves_CellClick;

            // Add to form
            var mainContainer = new Panel { Dock = DockStyle.Fill };
            mainContainer.Controls.Add(dgvLeaves);
            mainContainer.Controls.Add(formPanel);
            this.Controls.Add(mainContainer);
        }

        private void LoadEmployees()
        {
            try 
            {
                // Log current user
                Console.WriteLine($"Current User ID: {_currentUser.UserId}, Role: {_currentUser.Role}");
                
                var employees = _context.Employees
                    .Include(e => e.User)
                    .OrderBy(e => e.User.Name)
                    .ToList();
                    
                // For non-admin users, only show themselves
                if (_currentUser.Role != UserRole.Administrator)
                {
                    employees = employees.Where(e => e.UserId == _currentUser.UserId).ToList();
                }

                // Log employees found
                foreach (var emp in employees)
                {
                    Console.WriteLine($"Employee ID: {emp.EmployeeId}, User ID: {emp.UserId}, Name: {emp.User.Name}");
                }

                cmbEmployee.DisplayMember = "Name";
                cmbEmployee.ValueMember = "Id";
                cmbEmployee.DataSource = employees.Select(e => new { 
                    Id = e.EmployeeId, 
                    Name = e.User.Name 
                }).ToList();

                // Verify selected employee for non-admin users
                if (_currentUser.Role != UserRole.Administrator)
                {
                    var currentEmployee = employees.FirstOrDefault(e => e.UserId == _currentUser.UserId);
                    if (currentEmployee != null)
                    {
                        cmbEmployee.SelectedValue = currentEmployee.EmployeeId;
                        cmbEmployee.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading employees: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadLeaves()
        {
            dgvLeaves.Rows.Clear();
            var query = _context.Leaves
                .Include(l => l.Employee)
                .ThenInclude(e => e.User)
                .AsQueryable();

            // If not admin, only show own leaves
            if (_currentUser.Role != UserRole.Administrator)
            {
                var employeeId = _context.Employees
                    .FirstOrDefault(e => e.UserId == _currentUser.UserId)?.EmployeeId;
                if (employeeId.HasValue)
                {
                    query = query.Where(l => l.EmployeeId == employeeId);
                }
            }

            var leaves = query.OrderByDescending(l => l.RequestDate).ToList();

            foreach (var leave in leaves)
            {
                var rowIndex = dgvLeaves.Rows.Add(
                    leave.LeaveId,
                    leave.Employee.User.Name,
                    leave.LeaveType,
                    leave.StartDate.ToShortDateString(),
                    leave.EndDate.ToShortDateString(),
                    leave.Status,
                    leave.Reason
                );

                // Color code the status cell
                var statusCell = dgvLeaves.Rows[rowIndex].Cells["Status"];
                switch (leave.Status)
                {
                    case LeaveStatus.Approved:
                        statusCell.Style.ForeColor = Color.FromArgb(40, 167, 69); // Green
                        statusCell.Style.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                        break;
                    case LeaveStatus.Rejected:
                        statusCell.Style.ForeColor = Color.FromArgb(220, 53, 69); // Red
                        statusCell.Style.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                        break;
                    case LeaveStatus.Pending:
                        statusCell.Style.ForeColor = Color.FromArgb(255, 193, 7); // Yellow
                        statusCell.Style.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                        break;
                }
            }
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                // Verify selected employee matches current user for non-admin
                if (_currentUser.Role != UserRole.Administrator)
                {
                    var currentEmployee = _context.Employees
                        .FirstOrDefault(e => e.UserId == _currentUser.UserId);
                        
                    if (currentEmployee == null || currentEmployee.EmployeeId != (int)cmbEmployee.SelectedValue)
                    {
                        MessageBox.Show("You can only submit leave requests for yourself", 
                            "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                if (cmbEmployee.SelectedValue == null)
                {
                    MessageBox.Show("Please select an employee", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtReason.Text))
                {
                    MessageBox.Show("Please provide a reason for leave", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (dtpEndDate.Value < dtpStartDate.Value)
                {
                    MessageBox.Show("End date cannot be earlier than start date", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var leave = new Leave
                {
                    EmployeeId = (int)cmbEmployee.SelectedValue,
                    LeaveType = cmbLeaveType.Text,
                    StartDate = dtpStartDate.Value,
                    EndDate = dtpEndDate.Value,
                    Reason = txtReason.Text,
                    Status = LeaveStatus.Pending,
                    RequestDate = DateTime.Now
                };

                // Verify employee ID one more time
                Console.WriteLine($"Submitting leave for Employee ID: {leave.EmployeeId}");

                _context.Leaves.Add(leave);
                _context.SaveChanges();
                LoadLeaves();
                ClearForm();

                MessageBox.Show("Leave request submitted successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error submitting leave request: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnApprove_Click(object sender, EventArgs e)
        {
            if (_currentUser.Role != UserRole.Administrator)
            {
                MessageBox.Show("Only administrators can approve leave requests.", 
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (!selectedLeaveId.HasValue)
                {
                    MessageBox.Show("Please select a leave request to approve", 
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var leave = _context.Leaves.Find(selectedLeaveId.Value);
                if (leave == null) return;

                leave.Status = LeaveStatus.Approved;
                leave.ApprovedBy = _currentUser.Name;
                _context.SaveChanges();
                LoadLeaves();

                MessageBox.Show("Leave request approved successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error approving leave request: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnReject_Click(object sender, EventArgs e)
        {
            if (_currentUser.Role != UserRole.Administrator)
            {
                MessageBox.Show("Only administrators can reject leave requests.", 
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (!selectedLeaveId.HasValue)
                {
                    MessageBox.Show("Please select a leave request to reject", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var leave = _context.Leaves.Find(selectedLeaveId.Value);
                if (leave == null) return;

                leave.Status = LeaveStatus.Rejected;
                leave.ApprovedBy = "Current User"; // Should be replaced with actual logged-in user
                _context.SaveChanges();
                LoadLeaves();

                MessageBox.Show("Leave request rejected successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error rejecting leave request: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void DgvLeaves_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            selectedLeaveId = (int)dgvLeaves.Rows[e.RowIndex].Cells["Id"].Value;
            var leave = _context.Leaves
                .Include(l => l.Employee)
                .FirstOrDefault(l => l.LeaveId == selectedLeaveId);

            if (leave == null) return;

            // Fill form with leave data
            cmbEmployee.SelectedValue = leave.EmployeeId;
            cmbLeaveType.Text = leave.LeaveType;
            dtpStartDate.Value = leave.StartDate;
            dtpEndDate.Value = leave.EndDate;
            txtReason.Text = leave.Reason;
        }

        private void ClearForm()
        {
            selectedLeaveId = null;
            if (cmbEmployee.Items.Count > 0) cmbEmployee.SelectedIndex = 0;
            if (cmbLeaveType.Items.Count > 0) cmbLeaveType.SelectedIndex = 0;
            dtpStartDate.Value = DateTime.Today;
            dtpEndDate.Value = DateTime.Today;
            txtReason.Text = string.Empty;
        }

        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Top,
                ForeColor = Color.White,
                Height = 20
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
    }
} 