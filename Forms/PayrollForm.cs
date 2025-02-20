using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;
using System.Collections.Generic;
using EmployeeManagementSystem.Helpers;

namespace EmployeeManagementSystem.Forms
{
    public partial class PayrollForm : Form
    {
        private readonly ApplicationDbContext _context;
        private readonly DataGridView dgvPayroll;
        private readonly Panel formPanel;
        private readonly Panel controlsContainer;
        private readonly ComboBox cmbEmployee;
        private readonly DateTimePicker dtpPayrollMonth;
        private readonly TextBox txtBaseSalary;
        private readonly TextBox txtOTHours;
        private readonly TextBox txtBonus;
        private readonly TextBox txtDeductions;
        private readonly TextBox txtTaxDeductions;
        private readonly TextBox txtPublicHolidayDays;
        private readonly TextBox txtOvertime;
        private readonly TextBox txtPublicHoliday;
        private readonly TextBox txtPHOTHours;
        private readonly TextBox txtPHOTAmount;
        private readonly Label lblNetSalary;
        private int? selectedPayrollId;
        private Dictionary<string, decimal> currentDeductions;
        private readonly User _currentUser;
        private readonly ComboBox cmbMaritalStatus;
        private readonly ComboBox cmbPCBStatus;
        private readonly TextBox txtDeductionDescription;

        public PayrollForm(User currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            _context = new ApplicationDbContext();

            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(37, 37, 38);

            // Initialize main controls
            dgvPayroll = new DataGridView
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
                Width = 350,
                BackColor = Color.FromArgb(45, 45, 48),
                AutoScroll = true,
                Padding = new Padding(15)
            };

            // Initialize controlsContainer
            controlsContainer = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            // Initialize form controls
            cmbEmployee = new ComboBox
            {
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            dtpPayrollMonth = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "MMMM yyyy",
                ShowUpDown = true,
                Dock = DockStyle.Top
            };
            
            dtpPayrollMonth.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            // add dropdown for maried status
            cmbMaritalStatus = new ComboBox
            {
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbMaritalStatus.Items.AddRange(new string[] {
                "Single",
                "Married (Non-Working Spouse)",
                "Married (Working Spouse)"
            });
            cmbMaritalStatus.SelectedIndex = 0;
            cmbMaritalStatus.SelectedIndexChanged += CalculateOvertimeAndNetSalary;

            // add dropdown for PCB status
            cmbPCBStatus = new ComboBox
            {
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbPCBStatus.Items.AddRange(new string[] {
                "Calculate PCB",
                "N/A"
            });
            cmbPCBStatus.SelectedIndex = 1;
            cmbPCBStatus.SelectedIndexChanged += CalculateOvertimeAndNetSalary;

            // Create input fields
            txtBaseSalary = CreateTextBox("Base Salary");
            txtOTHours = CreateTextBox("OT Hours");
            txtBonus = CreateTextBox("Bonus");
            txtDeductions = CreateTextBox("Other Deductions");
            txtTaxDeductions = CreateTextBox("Tax Deductions");
            txtPublicHolidayDays = CreateTextBox("Public Holiday Days");
            txtOvertime = CreateTextBox("Overtime Amount");
            txtPublicHoliday = CreateTextBox("Public Holiday Amount");
            txtPHOTHours = CreateTextBox("PH OT Hours");
            txtPHOTAmount = CreateTextBox("PH OT Amount");

            // add textbox for deduction description
            txtDeductionDescription = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 50,
                Multiline = true,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "Enter deduction details if any (optional)"
            };

            // Set readonly for auto-calculated field
            txtOvertime.ReadOnly = true;
            txtPublicHoliday.ReadOnly = true;
            txtPHOTAmount.ReadOnly = true;

            lblNetSalary = new Label
            {
                Text = "Net Salary: $0.00",
                Dock = DockStyle.Top,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft
            };

            SetupFormControls();
            SetupDataGridView();
            LoadEmployees();
            LoadPayroll();

            // Add event handlers for automatic calculation
            txtBaseSalary.TextChanged += CalculateOvertimeAndNetSalary;
            txtOTHours.TextChanged += CalculateOvertimeAndNetSalary;
            txtBonus.TextChanged += CalculateOvertimeAndNetSalary;
            txtDeductions.TextChanged += CalculateOvertimeAndNetSalary;
            txtTaxDeductions.TextChanged += CalculateOvertimeAndNetSalary;
            txtPublicHolidayDays.TextChanged += CalculateOvertimeAndNetSalary;
            txtPHOTHours.TextChanged += CalculateOvertimeAndNetSalary;

            CreateTooltips();
        }

        private void SetupFormControls()
        {
            controlsContainer.Controls.Clear();

            var lblTitle = new Label
            {
                Text = "Payroll Management",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Height = 50,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Create single panel for all controls
            var mainPanel = CreateGroupPanel("Payroll Details");

            // Style the employee combobox and month picker
            StyleEmployeeComboBox();
            StylePayrollMonthPicker();

            // Add controls in correct order (from top to bottom)
            mainPanel.Controls.AddRange(new Control[] {
                // 1. Employee Name (paling atas)
                CreateInputGroup("Employee Name", cmbEmployee, 0),
                
                // 2. Payroll Month
                CreateInputGroup("Payroll Month", dtpPayrollMonth, 1),
                
                // 3. Base Salary
                CreateInputGroup("Base Salary", txtBaseSalary, 2),
                
                // 4. Overtime Hours
                CreateInputGroup("Overtime Hours", txtOTHours, 3),
                
                // 5. Public Holiday Days
                CreateInputGroup("Public Holiday Days", txtPublicHolidayDays, 4),
                
                // 6. PH OT Hours
                CreateInputGroup("PH OT Hours", txtPHOTHours, 5),
                
                // 7. Overtime Amount
                CreateInputGroup("Overtime Amount", txtOvertime, 6),
                
                // 8. Public Holiday Amount
                CreateInputGroup("Public Holiday Amount", txtPublicHoliday, 7),
                
                // 9. PH OT Amount
                CreateInputGroup("PH OT Amount", txtPHOTAmount, 8),
                
                // 10. Other Deductions
                CreateInputGroup("Other Deductions", txtDeductions, 9),
                
                // 11. Deduction Description
                CreateInputGroup("Deduction Description", txtDeductionDescription, 10),
                
                // 12. Tax Deductions
                CreateInputGroup("Tax Deductions", txtTaxDeductions, 11),
                
                // 13. PCB Status
                CreateInputGroup("PCB Status", cmbPCBStatus, 12),
                
                // 14. Marital Status (paling bawah)
                CreateInputGroup("Marital Status", cmbMaritalStatus, 13)
            });

            // Actions Panel
            var actionsPanel = CreateGroupPanel("Actions");
            var btnGenerate = ButtonHelper.CreateStyledButton("Generate Payroll", Color.FromArgb(0, 123, 255), DockStyle.Top, 14);
            var btnPrint = ButtonHelper.CreateStyledButton("Print Payslip", Color.FromArgb(40, 167, 69), DockStyle.Top, 15);
            var btnDelete = ButtonHelper.CreateStyledButton("Delete", Color.FromArgb(220, 53, 69), DockStyle.Top, 16);
            var btnClear = ButtonHelper.CreateStyledButton("Clear", Color.FromArgb(108, 117, 125), DockStyle.Top, 17);

            actionsPanel.Controls.AddRange(new Control[] {
                lblNetSalary,
                btnGenerate,
                btnPrint,
                btnDelete,
                btnClear
            });

            // Add event handlers
            btnGenerate.Click += BtnGenerate_Click;
            btnPrint.Click += BtnPrint_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += BtnClear_Click;

            // Add panels to container
            controlsContainer.Controls.Add(actionsPanel);
            controlsContainer.Controls.Add(mainPanel);
            controlsContainer.Controls.Add(lblTitle);

            formPanel.Controls.Clear();
            formPanel.Controls.Add(controlsContainer);

            // Add event handler for employee selection change
            cmbEmployee.SelectedIndexChanged += CmbEmployee_SelectedIndexChanged;
        }

        private Panel CreateGroupPanel(string title)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(10),
                Margin = new Padding(0, 0, 0, 15),
                BackColor = Color.FromArgb(50, 50, 53)
            };

            var titleLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0)
            };

            panel.Controls.Add(titleLabel);
            return panel;
        }

        private Panel CreateInputGroup(string labelText, Control input, int tabIndex)
        {
            var group = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 65,
                Margin = new Padding(0, 0, 0, 10)
            };

            var label = new Label
            {
                Text = labelText,
                Dock = DockStyle.Top,
                ForeColor = Color.White,
                Height = 25,
                Padding = new Padding(5, 5, 0, 0)
            };

            input.Dock = DockStyle.Top;
            input.Height = 35;
            input.Margin = new Padding(5, 0, 5, 0);
            input.TabIndex = tabIndex;
            
            if (input is TextBox txt)
            {
                txt.BorderStyle = BorderStyle.FixedSingle;
                txt.BackColor = Color.FromArgb(60, 60, 60);
                txt.ForeColor = Color.White;
                txt.Font = new Font("Segoe UI", 10f);
            }
            else if (input is ComboBox cmb)
            {
                cmb.FlatStyle = FlatStyle.Flat;
                cmb.BackColor = Color.FromArgb(60, 60, 60);
                cmb.ForeColor = Color.White;
                cmb.Font = new Font("Segoe UI", 10f);
            }
            else if (input is DateTimePicker dtp)
            {
                dtp.BackColor = Color.FromArgb(60, 60, 60);
                dtp.ForeColor = Color.White;
                dtp.Font = new Font("Segoe UI", 10f);
            }

            group.Controls.Add(input);
            group.Controls.Add(label);

            return group;
        }

        private void SetupDataGridView()
        {
            dgvPayroll.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { 
                    Name = "Id", 
                    HeaderText = "ID", 
                    Width = 50,
                    DefaultCellStyle = new DataGridViewCellStyle {
                        Format = "000",  // ID format eg : 001, 002, etc
                        Alignment = DataGridViewContentAlignment.MiddleCenter  // centered text
                    }
                },
                new DataGridViewTextBoxColumn { 
                    Name = "EmployeeName", 
                    HeaderText = "Employee", 
                    Width = 150 
                },
                new DataGridViewTextBoxColumn { 
                    Name = "PayPeriod", 
                    HeaderText = "Pay Period", 
                    Width = 120 
                },
                new DataGridViewTextBoxColumn { 
                    Name = "BaseSalary", 
                    HeaderText = "Base Salary", 
                    Width = 100,
                    DefaultCellStyle = new DataGridViewCellStyle {
                        Format = "C2",  // Format currency
                        Alignment = DataGridViewContentAlignment.MiddleRight  // Align right for numbers
                    }
                },
                new DataGridViewTextBoxColumn { 
                    Name = "NetSalary", 
                    HeaderText = "Net Salary", 
                    Width = 100,
                    DefaultCellStyle = new DataGridViewCellStyle {
                        Format = "C2",  // Format currency
                        Alignment = DataGridViewContentAlignment.MiddleRight  // Align right for numbers
                    }
                },
                new DataGridViewTextBoxColumn { 
                    Name = "Status", 
                    HeaderText = "Status", 
                    Width = 80 
                }
            });

            dgvPayroll.CellClick += DgvPayroll_CellClick;

            // Add to form
            var mainContainer = new Panel { Dock = DockStyle.Fill };
            mainContainer.Controls.Add(dgvPayroll);
            mainContainer.Controls.Add(formPanel);
            this.Controls.Add(mainContainer);

            StyleDataGridView();
        }

        private void StyleDataGridView()
        {
            dgvPayroll.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };

            dgvPayroll.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f),
                Padding = new Padding(10, 0, 0, 0)
            };

            dgvPayroll.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(55, 55, 58),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f)
            };

            dgvPayroll.EnableHeadersVisualStyles = false;
            dgvPayroll.RowHeadersVisible = false;
            dgvPayroll.ColumnHeadersHeight = 40;
            dgvPayroll.RowTemplate.Height = 35;
        }

        private void LoadEmployees()
        {
            try
            {
                var employees = _context.Employees
                    .Include(e => e.User)
                    .AsEnumerable()  // Evaluate query first
                    .Select(e => new
                    {
                        EmployeeId = e.EmployeeId,
                        DisplayName = e.User.Name + " (" + e.Position + ")",  // Avoid string.Format
                        Salary = e.Salary
                    })
                    .OrderBy(e => e.DisplayName)
                    .ToList();

                cmbEmployee.DataSource = null;  // Clear existing datasource
                cmbEmployee.DataSource = employees;
                cmbEmployee.DisplayMember = "DisplayName";
                cmbEmployee.ValueMember = "EmployeeId";
                cmbEmployee.SelectedIndex = -1;  // Reset selection
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading employees: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void LoadPayroll()
        {
            dgvPayroll.Rows.Clear();
            var payrolls = _context.Payrolls
                .Include(p => p.Employee)
                .ThenInclude(e => e.User)
                .OrderByDescending(p => p.PayPeriodStart)
                .ToList();

            foreach (var payroll in payrolls)
            {
                dgvPayroll.Rows.Add(
                    payroll.PayrollId,
                    payroll.Employee.User.Name,
                    payroll.PayPeriodStart.ToString("MMMM yyyy"),
                    payroll.BaseSalary.ToString("C"),
                    payroll.NetSalary.ToString("C"),
                    payroll.PaymentStatus
                );
            }
        }

        private void CmbEmployee_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbEmployee.SelectedItem != null)
                {
                    // Get the anonymous type properties using reflection
                    var selectedItem = cmbEmployee.SelectedItem;
                    var salaryProperty = selectedItem.GetType().GetProperty("Salary");
                    if (salaryProperty != null)
                    {
                        var salary = salaryProperty.GetValue(selectedItem);
                        txtBaseSalary.Text = salary?.ToString();
                        
                        // Trigger calculations
                        CalculateDeductions();
                        CalculateOvertimeAndNetSalary(sender, e);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading employee salary: {ex.Message}", 
                    "Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }

        private void CalculateOvertimeAndNetSalary(object sender, EventArgs e)
        {
            try
            {
                decimal baseSalary = 0, otHours = 0, publicHolidayDays = 0, phOTHours = 0, bonus = 0, deductions = 0, taxDeductions = 0;

                // Parse input values
                decimal.TryParse(txtBaseSalary.Text, out baseSalary);
                decimal.TryParse(txtOTHours.Text, out otHours);
                decimal.TryParse(txtPublicHolidayDays.Text, out publicHolidayDays);
                decimal.TryParse(txtPHOTHours.Text, out phOTHours);  // New field for PH OT Hours
                decimal.TryParse(txtBonus.Text, out bonus);
                decimal.TryParse(txtDeductions.Text, out deductions);
                decimal.TryParse(txtTaxDeductions.Text, out taxDeductions);

                // Calculate normal overtime (1.5x)
                decimal hourlyRate = baseSalary / 26m / 8m;
                decimal overtimeAmount = otHours * (hourlyRate * 1.5m);

                // Calculate Public Holiday Pay (base/26 * 3)
                decimal phDailyRate = baseSalary / 26m;
                decimal publicHolidayAmount = publicHolidayDays * (phDailyRate * 3m);

                // Calculate Public Holiday OT (base/26/8 * 3)
                decimal phOTRate = hourlyRate * 3m;
                decimal phOTAmount = phOTHours * phOTRate;

                // Update textboxes
                txtOvertime.Text = overtimeAmount.ToString("F2");
                txtPublicHoliday.Text = publicHolidayAmount.ToString("F2");
                txtPHOTAmount.Text = phOTAmount.ToString("F2");

                // Calculate net salary
                decimal netSalary = baseSalary + overtimeAmount + publicHolidayAmount + phOTAmount + bonus - deductions - taxDeductions;

                // Create tooltips for each calculation
                var toolTip = new ToolTip { InitialDelay = 0, ShowAlways = true };

                // Public Holiday Tooltip
                var phDetails = 
                    $"Public Holiday Pay Calculation:\n" +
                    $"Days Worked: {publicHolidayDays} days\n" +
                    $"Daily Rate: RM {phDailyRate:N2} (Base ÷ 26)\n" +
                    $"PH Rate (3x): RM {phDailyRate * 3m:N2}\n" +
                    $"Formula: Days × (Daily Rate × 3)\n" +
                    $"Total: {publicHolidayDays} × RM {phDailyRate * 3m:N2} = RM {publicHolidayAmount:N2}";
                toolTip.SetToolTip(txtPublicHoliday, phDetails);

                // Public Holiday OT Tooltip
                var phOTDetails = 
                    $"Public Holiday Overtime Calculation:\n" +
                    $"Hours Worked: {phOTHours} hours\n" +
                    $"Base Hourly Rate: RM {hourlyRate:N2} (Base ÷ 26 ÷ 8)\n" +
                    $"PH OT Rate (3x): RM {phOTRate:N2}\n" +
                    $"Formula: Hours × (Hourly Rate × 3)\n" +
                    $"Total: {phOTHours} × RM {phOTRate:N2} = RM {phOTAmount:N2}";
                toolTip.SetToolTip(txtPHOTAmount, phOTDetails);

                // Base Salary Tooltip
                var baseSalaryDetails = 
                    $"Base Salary Breakdown:\n" +
                    $"Monthly Salary: RM {baseSalary:N2}\n" +
                    $"Daily Rate: RM {(baseSalary / 26m):N2} (Base ÷ 26 days)\n" +
                    $"Hourly Rate: RM {hourlyRate:N2} (Daily ÷ 8 hours)";
                toolTip.SetToolTip(txtBaseSalary, baseSalaryDetails);

                // Overtime Tooltip
                var overtimeDetails = 
                    $"Overtime Calculation:\n" +
                    $"Hours Worked: {otHours} hours\n" +
                    $"Base Hourly Rate: RM {hourlyRate:N2}\n" +
                    $"OT Rate (1.5x): RM {hourlyRate * 1.5m:N2}\n" +
                    $"Formula: Hours × OT Rate\n" +
                    $"Total: {otHours} × RM {hourlyRate * 1.5m:N2} = RM {overtimeAmount:N2}";
                toolTip.SetToolTip(txtOvertime, overtimeDetails);

                // Deductions Tooltip
                var deductionsDetails = 
                    $"Total Deductions:\n" +
                    $"Other Deductions: RM {deductions:N2}\n" +
                    $"Tax (PCB): RM {taxDeductions:N2}\n" +
                    $"Total: RM {(deductions + taxDeductions):N2}";
                toolTip.SetToolTip(txtDeductions, deductionsDetails);

                // Net Salary Tooltip with detailed breakdown
                var netSalaryDetails = 
                    $"Net Salary Calculation:\n\n" +
                    $"Earnings:\n" +
                    $"Base Salary: RM {baseSalary:N2}\n" +
                    $"Normal Overtime: RM {overtimeAmount:N2}\n" +
                    $"Public Holiday Pay: RM {publicHolidayAmount:N2}\n" +
                    $"Public Holiday OT: RM {phOTAmount:N2}\n" +
                    $"Bonus: RM {bonus:N2}\n" +
                    $"Total Earnings: RM {(baseSalary + overtimeAmount + publicHolidayAmount + phOTAmount + bonus):N2}\n\n" +
                    $"Deductions:\n" +
                    $"Other Deductions: RM {deductions:N2}\n" +
                    $"Tax (PCB): RM {taxDeductions:N2}\n" +
                    $"Total Deductions: RM {(deductions + taxDeductions):N2}\n\n" +
                    $"Net Salary: RM {netSalary:N2}";
                toolTip.SetToolTip(lblNetSalary, netSalaryDetails);

                // Update net salary label
                lblNetSalary.Text = $"Net Salary: RM {netSalary:N2}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating salary: {ex.Message}", "Calculation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbEmployee.SelectedValue == null)
                {
                    MessageBox.Show("Please select an employee", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get base values first
                decimal baseSalary = 0, otHours = 0, publicHolidayDays = 0, bonus = 0, deductions = 0, taxDeductions = 0;

                // Parse each field individually
                if (!decimal.TryParse(txtBaseSalary.Text.Trim(), out baseSalary))
                {
                    MessageBox.Show("Invalid base salary amount", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Parse OT hours
                if (!string.IsNullOrEmpty(txtOTHours.Text))
                    decimal.TryParse(txtOTHours.Text.Trim(), out otHours);

                if (!string.IsNullOrEmpty(txtPublicHolidayDays.Text))
                    decimal.TryParse(txtPublicHolidayDays.Text.Trim(), out publicHolidayDays);

                if (!string.IsNullOrEmpty(txtBonus.Text))
                    decimal.TryParse(txtBonus.Text.Trim(), out bonus);

                if (!string.IsNullOrEmpty(txtDeductions.Text))
                    decimal.TryParse(txtDeductions.Text.Trim(), out deductions);

                if (!string.IsNullOrEmpty(txtTaxDeductions.Text))
                    decimal.TryParse(txtTaxDeductions.Text.Trim(), out taxDeductions);

                // Calculate deductions before generating payroll
                CalculateDeductions();

                // Calculate rates
                decimal dailyRate = baseSalary / 26m;
                decimal hourlyRate = dailyRate / 8m;
                decimal otRate = hourlyRate * 1.5m;
                decimal publicHolidayRate = dailyRate * 3m;

                // Calculate totals
                decimal overtime = otRate * otHours;
                decimal publicHoliday = publicHolidayRate * publicHolidayDays;
                decimal netSalary = baseSalary + overtime + publicHoliday + bonus - deductions - taxDeductions;

                // Get month start and end dates
                DateTime payrollMonth = dtpPayrollMonth.Value;
                DateTime monthStart = new DateTime(payrollMonth.Year, payrollMonth.Month, 1);
                DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);

                // Check if payroll already exists for this month
                var existingPayroll = _context.Payrolls
                    .FirstOrDefault(p => p.EmployeeId == (int)cmbEmployee.SelectedValue 
                                     && p.PayPeriodStart.Year == monthStart.Year 
                                     && p.PayPeriodStart.Month == monthStart.Month);

                if (existingPayroll != null)
                {
                    MessageBox.Show(
                        $"Payroll for {payrollMonth:MMMM yyyy} already exists for this employee.",
                        "Duplicate Payroll",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Create new payroll record
                var payroll = new Payroll
                {
                    EmployeeId = (int)cmbEmployee.SelectedValue,
                    PayPeriodStart = monthStart,
                    PayPeriodEnd = monthEnd,
                    BaseSalary = baseSalary,
                    Overtime = overtime,
                    PublicHoliday = publicHoliday,
                    Bonus = bonus,
                    Deductions = deductions,
                    DeductionDescription = txtDeductionDescription.Text.Trim(),
                    TaxDeductions = taxDeductions,
                    NetSalary = netSalary,
                    PaymentDate = DateTime.Now,
                    PaymentStatus = "Generated",
                    PaymentReference = GeneratePaymentReference()
                };

                _context.Payrolls.Add(payroll);
                _context.SaveChanges();
                LoadPayroll();
                ClearForm();

                MessageBox.Show("Payroll generated successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating payroll: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                if (!selectedPayrollId.HasValue)
                {
                    MessageBox.Show("Please select a payroll record to print", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var payroll = _context.Payrolls
                    .Include(p => p.Employee)
                    .ThenInclude(e => e.User)
                    .FirstOrDefault(p => p.PayrollId == selectedPayrollId.Value);

                if (payroll == null) return;

                // Calculate deductions before printing
                txtBaseSalary.Text = payroll.BaseSalary.ToString();
                CalculateDeductions();

                // Create print document with landscape orientation
                var printDocument = new PrintDocument();
                printDocument.DefaultPageSettings.Landscape = true; // Set to landscape
                printDocument.PrintPage += (s, ev) => PrintPayslip(ev, payroll);

                // Show print preview
                var preview = new PrintPreviewDialog
                {
                    Document = printDocument,
                    WindowState = FormWindowState.Maximized,
                    StartPosition = FormStartPosition.CenterScreen,
                    PrintPreviewControl = { Zoom = 1.0 }
                };

                preview.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing payslip: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CalculateDeductions()
        {
            try
            {
                if (!decimal.TryParse(txtBaseSalary.Text, out decimal baseSalary))
                    return;

                // KWSP (EPF) Calculation
                decimal employeeEPF = Math.Round(baseSalary * 0.11m, 2); // 11% for employee
                decimal employerEPF = Math.Round(baseSalary * 0.13m, 2); // 13% for employer

                // SOCSO Calculation (simplified version - actual calculation has brackets)
                decimal employeeSOCSO = Math.Round(baseSalary * 0.007833m, 2); // 0.78% for employee
                decimal employerSOCSO = Math.Round(baseSalary * 0.027433m, 2); // 2.74% for employer

                // EIS/SIP Calculation
                decimal employeeEIS = Math.Round(baseSalary * 0.003133m, 2); // 0.31% for employee
                decimal employerEIS = Math.Round(baseSalary * 0.003133m, 2); // 0.31% for employer

                // Store values for printing
                currentDeductions = new Dictionary<string, decimal>
                {
                    { "EmployeeEPF", employeeEPF },
                    { "EmployerEPF", employerEPF },
                    { "EmployeeSOCSO", employeeSOCSO },
                    { "EmployerSOCSO", employerSOCSO },
                    { "EmployeeEIS", employeeEIS },
                    { "EmployerEIS", employerEIS }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating deductions: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintPayslip(PrintPageEventArgs e, Payroll payroll)
        {
            // Set up fonts and colors
            var titleFont = new Font("Segoe UI", 20, FontStyle.Bold);
            var headerFont = new Font("Segoe UI", 12, FontStyle.Bold);
            var normalFont = new Font("Segoe UI", 10);
            var smallFont = new Font("Segoe UI", 8);
            var lightGray = Color.FromArgb(240, 240, 240);
            
            float yPos = 50;
            float leftMargin = 50;
            float rightMargin = e.PageBounds.Width - 100;
            float centerPos = e.PageBounds.Width / 2;
            float columnWidth = (rightMargin - leftMargin) / 2;
            float rightColumnStart = leftMargin + columnWidth + 50;

            // Draw header background
            var headerRect = new RectangleF(leftMargin - 20, 30, rightMargin - leftMargin + 40, 100);
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 123, 255)), headerRect);

            // Company header (centered, white text)
            e.Graphics.DrawString("EMPLOYEE MANAGEMENT SYSTEM", titleFont, Brushes.White, 
                new PointF(centerPos - 200, yPos));
            yPos += 30;

            e.Graphics.DrawString("PAYSLIP", headerFont, Brushes.White,
                new PointF(centerPos - 40, yPos));
            yPos += 50;

            // Draw pay period in a box
            var periodRect = new RectangleF(leftMargin, yPos, rightMargin - leftMargin, 30);
            e.Graphics.FillRectangle(new SolidBrush(lightGray), periodRect);
            e.Graphics.DrawString($"Pay Period: {payroll.PayPeriodStart:dd/MM/yyyy} - {payroll.PayPeriodEnd:dd/MM/yyyy}", 
                headerFont, Brushes.Black, new PointF(leftMargin + 10, yPos + 5));
            yPos += 50;

            // Employee details section
            var employee = payroll.Employee;
            var detailsRect = new RectangleF(leftMargin, yPos, columnWidth - 20, 120);
            e.Graphics.FillRectangle(new SolidBrush(lightGray), detailsRect);
            
            var employeeDetails = new Dictionary<string, string>
            {
                { "Employee Name", employee.User.Name },
                { "Employee ID", employee.EmployeeId.ToString() },
                { "Position", employee.Position },
                { "Department", employee.Department }
            };

            float detailsY = yPos + 10;
            foreach (var detail in employeeDetails)
            {
                e.Graphics.DrawString($"{detail.Key}:", normalFont, Brushes.Black, 
                    leftMargin + 10, detailsY);
                e.Graphics.DrawString(detail.Value, normalFont, Brushes.Black, 
                    leftMargin + 150, detailsY);
                detailsY += 25;
            }
            yPos += 140;

            // Earnings section
            DrawSectionHeader(e, "EARNINGS", leftMargin, yPos, columnWidth - 20);
            yPos += 30;

            var earnings = new Dictionary<string, decimal>
            {
                { "Basic Salary", payroll.BaseSalary },
                { "Overtime", payroll.Overtime },
                { "Public Holiday", payroll.PublicHoliday },
                { "Bonus", payroll.Bonus }
            };

            foreach (var earning in earnings)
            {
                DrawPaymentLine(e, earning.Key, earning.Value, leftMargin, leftMargin + columnWidth - 20, 
                    ref yPos, normalFont, false);
            }

            decimal totalEarnings = earnings.Values.Sum();
            yPos += 10;
            DrawPaymentLine(e, "Total Earnings", totalEarnings, leftMargin, leftMargin + columnWidth - 20, 
                ref yPos, headerFont, true);

            // Reset Y position for deductions
            yPos = 270;

            // Deductions section
            DrawSectionHeader(e, "DEDUCTIONS", rightColumnStart, yPos, columnWidth - 20);
            yPos += 30;

            var deductions = new Dictionary<string, decimal>
            {
                { "KWSP (Employee 11%)", currentDeductions["EmployeeEPF"] },
                { "SOCSO (Employee)", currentDeductions["EmployeeSOCSO"] },
                { "EIS/SIP (Employee)", currentDeductions["EmployeeEIS"] },
                { "PCB (Tax)", payroll.TaxDeductions },
                { "Other Deductions", payroll.Deductions }
            };

            foreach (var deduction in deductions)
            {
                DrawPaymentLine(e, deduction.Key, deduction.Value, rightColumnStart, rightMargin, 
                    ref yPos, normalFont, false);
            }

            decimal totalDeductions = deductions.Values.Sum();
            yPos += 10;
            DrawPaymentLine(e, "Total Deductions", totalDeductions, rightColumnStart, rightMargin, 
                ref yPos, headerFont, true);

            // Net Salary section with highlight
            yPos += 20;
            var netSalaryRect = new RectangleF(rightColumnStart, yPos, columnWidth - 20, 40);
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 123, 255)), netSalaryRect);
            e.Graphics.DrawString("NET SALARY:", headerFont, Brushes.White, 
                rightColumnStart + 10, yPos + 10);
            e.Graphics.DrawString($"RM {payroll.NetSalary:N2}", headerFont, Brushes.White, 
                rightMargin - 120, yPos + 10);

            // Employer Contributions section
            yPos += 60;
            DrawSectionHeader(e, "EMPLOYER CONTRIBUTIONS", rightColumnStart, yPos, columnWidth - 20);
            yPos += 30;

            var employerContributions = new Dictionary<string, decimal>
            {
                { "KWSP (Employer 13%)", currentDeductions["EmployerEPF"] },
                { "SOCSO (Employer)", currentDeductions["EmployerSOCSO"] },
                { "EIS/SIP (Employer)", currentDeductions["EmployerEIS"] }
            };

            foreach (var contribution in employerContributions)
            {
                DrawPaymentLine(e, contribution.Key, contribution.Value, rightColumnStart, rightMargin, 
                    ref yPos, normalFont, false);
            }

            // Footer with payment details
            var footerRect = new RectangleF(leftMargin, e.PageBounds.Height - 100, 
                rightMargin - leftMargin, 80);
            e.Graphics.FillRectangle(new SolidBrush(lightGray), footerRect);
            
            float footerY = e.PageBounds.Height - 90;
            e.Graphics.DrawString($"Payment Date: {payroll.PaymentDate:dd/MM/yyyy}", 
                normalFont, Brushes.Black, leftMargin + 10, footerY);
            e.Graphics.DrawString($"Payment Reference: {payroll.PaymentReference}", 
                normalFont, Brushes.Black, leftMargin + 10, footerY + 20);
            e.Graphics.DrawString("This is a computer generated payslip and does not require signature", 
                smallFont, Brushes.Gray, leftMargin + 10, footerY + 40);
        }

        private void DrawSectionHeader(PrintPageEventArgs e, string title, float x, float y, float width)
        {
            var headerRect = new RectangleF(x, y, width, 25);
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(0, 123, 255)), headerRect);
            e.Graphics.DrawString(title, new Font("Segoe UI", 10, FontStyle.Bold), 
                Brushes.White, x + 10, y + 4);
        }

        private void DrawPaymentLine(PrintPageEventArgs e, string label, decimal amount, 
            float left, float right, ref float yPos, Font font, bool highlight)
        {
            var lineRect = new RectangleF(left, yPos, right - left, 25);
            if (highlight)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(240, 240, 240)), lineRect);
            }
            
            e.Graphics.DrawString(label, font, Brushes.Black, left + 10, yPos + 4);
            e.Graphics.DrawString($"RM {amount:N2}", font, Brushes.Black, 
                right - 120, yPos + 4);
            yPos += 25;
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (!selectedPayrollId.HasValue)
                {
                    MessageBox.Show("Please select a payroll record to delete", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (MessageBox.Show(
                    "Are you sure you want to delete this payroll record?\n" +
                    "This action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    var payroll = _context.Payrolls.Find(selectedPayrollId.Value);
                    if (payroll != null)
                    {
                        _context.Payrolls.Remove(payroll);
                        _context.SaveChanges();
                        LoadPayroll();
                        ClearForm();

                        MessageBox.Show("Payroll record deleted successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting payroll record: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void DgvPayroll_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            selectedPayrollId = (int)dgvPayroll.Rows[e.RowIndex].Cells["Id"].Value;
            var payroll = _context.Payrolls
                .Include(p => p.Employee)
                .FirstOrDefault(p => p.PayrollId == selectedPayrollId);

            if (payroll == null) return;

            // Fill form with payroll data
            cmbEmployee.SelectedValue = payroll.EmployeeId;
            dtpPayrollMonth.Value = payroll.PayPeriodStart;
            txtBaseSalary.Text = payroll.BaseSalary.ToString();
            txtOTHours.Text = payroll.Overtime.ToString();
            txtBonus.Text = payroll.Bonus.ToString();
            txtDeductions.Text = payroll.Deductions.ToString();
            txtTaxDeductions.Text = payroll.TaxDeductions.ToString();
            txtPublicHolidayDays.Text = payroll.PublicHoliday.ToString();

            // Set PCB status based on tax deductions
            cmbPCBStatus.SelectedIndex = payroll.TaxDeductions > 0 ? 0 : 1;
        }

        private void ClearForm()
        {
            selectedPayrollId = null;
            if (cmbEmployee.Items.Count > 0) cmbEmployee.SelectedIndex = 0;
            dtpPayrollMonth.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            txtBaseSalary.Text = "0";
            txtOTHours.Text = "0";
            txtBonus.Text = "0";
            txtDeductions.Text = "0";
            txtTaxDeductions.Text = "0";
            txtPublicHolidayDays.Text = "0";
            txtOvertime.Text = "0";
            txtPublicHoliday.Text = "0";
            txtPHOTHours.Text = "0";
            txtPHOTAmount.Text = "0";
            txtDeductionDescription.Text = "";
            cmbPCBStatus.SelectedIndex = 1;
        }

        private string GeneratePaymentReference()
        {
            return $"PAY-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        private TextBox CreateTextBox(string placeholder)
        {
            var textBox = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 30,
                Margin = new Padding(0, 0, 0, 10), // Add bottom margin
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "0"  // Default value is already "0"
            };

            // Add validation for numeric input only
            textBox.KeyPress += (s, e) => {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                {
                    e.Handled = true;
                }

                // Allow only one decimal point
                if (e.KeyChar == '.' && (s as TextBox).Text.IndexOf('.') > -1)
                {
                    e.Handled = true;
                }
            };

            return textBox;
        }

        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Top,
                ForeColor = Color.White,
                Height = 20,
                Padding = new Padding(0, 10, 0, 0) // Add padding to create space between controls
            };
        }

        private decimal CalculatePCB(decimal monthlyIncome, string category)
        {
            // 1. calculate yearly income
            decimal yearlyIncome = monthlyIncome * 12;
            
            // 2. deduct contribution
            decimal epf = Math.Min(yearlyIncome * 0.11m, 4000m);  // EPF cap at RM4000 setahun
            decimal socso = Math.Min(yearlyIncome * 0.005m, 592.80m); // SOCSO cap
            decimal eis = Math.Min(yearlyIncome * 0.002m, 238.80m);   // EIS cap

            // 3. individual relief (2023)
            decimal individualRelief = 9000m;  // Relief asas
            decimal epdRelief = Math.Min(epf, 4000m); // EPF relief
            decimal socsoRelief = socso + eis; // SOCSO & EIS relief
            
            // 4. devide by relief status
            decimal marriageRelief = category switch
            {
                "2" => 4000m,  // Married relief (spouse no income)
                "3" => 0m,     // Married (working spouse)
                _ => 0m        // Single
            };

            // 5. Total relief
            decimal totalRelief = individualRelief + epdRelief + socsoRelief + marriageRelief;

            // 6. calculate income tax
            decimal taxableIncome = yearlyIncome - totalRelief;
            if (taxableIncome <= 0) return 0;

            // 7. calculated income tax table (2023)
            decimal tax = 0;
            if (taxableIncome <= 5000)
                tax = 0;
            else if (taxableIncome <= 20000)
                tax = (taxableIncome - 5000) * 0.01m;
            else if (taxableIncome <= 35000)
                tax = 150 + (taxableIncome - 20000) * 0.03m;
            else if (taxableIncome <= 50000)
                tax = 600 + (taxableIncome - 35000) * 0.08m;
            else if (taxableIncome <= 70000)
                tax = 1800 + (taxableIncome - 50000) * 0.13m;
            else if (taxableIncome <= 100000)
                tax = 4400 + (taxableIncome - 70000) * 0.21m;
            else if (taxableIncome <= 250000)
                tax = 10700 + (taxableIncome - 100000) * 0.24m;
            else if (taxableIncome <= 400000)
                tax = 46700 + (taxableIncome - 250000) * 0.245m;
            else if (taxableIncome <= 600000)
                tax = 83450 + (taxableIncome - 400000) * 0.25m;
            else if (taxableIncome <= 1000000)
                tax = 133450 + (taxableIncome - 600000) * 0.26m;
            else
                tax = 237450 + (taxableIncome - 1000000) * 0.28m;

            // 8. Calculated monthly PCB
            decimal monthlyPCB = Math.Round(tax / 12, 2);

            // 9. Show calculation details in tooltip
            var details = 
                $"PCB Calculation Details:\n\n" +
                $"Monthly Income: RM{monthlyIncome:N2}\n" +
                $"Yearly Income: RM{yearlyIncome:N2}\n\n" +
                $"Deductions:\n" +
                $"EPF (11%): RM{epf:N2}\n" +
                $"SOCSO: RM{socso:N2}\n" +
                $"EIS: RM{eis:N2}\n\n" +
                $"Relief:\n" +
                $"Individual: RM{individualRelief:N2}\n" +
                $"EPF Relief: RM{epdRelief:N2}\n" +
                $"SOCSO Relief: RM{socsoRelief:N2}\n" +
                $"Marriage Relief: RM{marriageRelief:N2}\n" +
                $"Total Relief: RM{totalRelief:N2}\n\n" +
                $"Taxable Income: RM{taxableIncome:N2}\n" +
                $"Yearly Tax: RM{tax:N2}\n" +
                $"Monthly PCB: RM{monthlyPCB:N2}";

            var toolTip = new ToolTip();
            toolTip.SetToolTip(txtTaxDeductions, details);

            return monthlyPCB;
        }

        // Add styling for ComboBox Employee
        private void StyleEmployeeComboBox()
        {
            cmbEmployee.Height = 35;
            cmbEmployee.Font = new Font("Segoe UI", 11f);
            cmbEmployee.FlatStyle = FlatStyle.Flat;
            cmbEmployee.BackColor = Color.FromArgb(60, 60, 60);
            cmbEmployee.ForeColor = Color.White;
            cmbEmployee.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEmployee.Margin = new Padding(0, 0, 0, 15);
        }

        // Add styling for DateTimePicker
        private void StylePayrollMonthPicker()
        {
            dtpPayrollMonth.Height = 35;
            dtpPayrollMonth.Font = new Font("Segoe UI", 11f);
            dtpPayrollMonth.Format = DateTimePickerFormat.Custom;
            dtpPayrollMonth.CustomFormat = "MMMM yyyy";
            dtpPayrollMonth.ShowUpDown = true;
            dtpPayrollMonth.BackColor = Color.FromArgb(60, 60, 60);
            dtpPayrollMonth.ForeColor = Color.White;
            dtpPayrollMonth.Margin = new Padding(0, 0, 0, 15);
        }

        private void CreateTooltips()
        {
            var toolTip = new ToolTip { 
                InitialDelay = 0, 
                ShowAlways = true,
                AutoPopDelay = 10000  // Tooltips will stay for 10 seconds
            };

            // Base Salary Tooltip
            toolTip.SetToolTip(txtBaseSalary, 
                "Base Salary is the monthly basic salary\n" +
                "Used to calculate:\n" +
                "- Daily rate (÷ 26 days)\n" +
                "- Hourly rate (÷ 26 days ÷ 8 hours)");

            // Normal OT Tooltip
            toolTip.SetToolTip(txtOTHours, 
                "Normal Overtime (1.5x normal rate)\n" +
                "Formula: (Base Salary ÷ 26 ÷ 8) × 1.5 × OT hours");

            // Public Holiday Tooltip
            toolTip.SetToolTip(txtPublicHolidayDays, 
                "Public Holiday (3x daily rate)\n" +
                "Formula: (Base Salary ÷ 26) × 3 × number of days");

            // Public Holiday OT Tooltip
            toolTip.SetToolTip(txtPHOTHours, 
                "Public Holiday Overtime (3x hourly rate)\n" +
                "Formula: (Base Salary ÷ 26 ÷ 8) × 3 × OT hours");

            // Overtime Amount Tooltip
            toolTip.SetToolTip(txtOvertime, 
                "Normal overtime payment amount\n" +
                "Automatically calculated based on OT Hours");

            // Public Holiday Amount Tooltip
            toolTip.SetToolTip(txtPublicHoliday, 
                "Public Holiday payment amount\n" +
                "Automatically calculated based on PH Days");

            // PH OT Amount Tooltip
            toolTip.SetToolTip(txtPHOTAmount, 
                "Public Holiday overtime payment amount\n" +
                "Automatically calculated based on PH OT Hours");

            // Bonus Tooltip
            toolTip.SetToolTip(txtBonus, 
                "Bonus or additional allowances\n" +
                "Enter manually if applicable");

            // Deductions Tooltip
            toolTip.SetToolTip(txtDeductions, 
                "Other deductions (if any)\n" +
                "Example: Advance salary, loans, etc.\n" +
                "Enter manually");

            // Tax Deductions Tooltip
            toolTip.SetToolTip(txtTaxDeductions, 
                "Tax deductions (PCB)\n" +
                "Automatically calculated if PCB Status = 'Calculate PCB'");

            // PCB Status Tooltip
            toolTip.SetToolTip(cmbPCBStatus, 
                "Calculate PCB: Automatically calculate PCB\n" +
                "N/A: No PCB calculation");

            // Marital Status Tooltip
            toolTip.SetToolTip(cmbMaritalStatus, 
                "Marital status for PCB calculation:\n" +
                "Single: Not married\n" +
                "Married (Non-Working Spouse): Married with non-working spouse\n" +
                "Married (Working Spouse): Married with working spouse");

            // Net Salary Tooltip
            toolTip.SetToolTip(lblNetSalary, 
                "Net Salary = Base Salary + OT + PH + PH OT + Bonus - Deductions - Tax");
        }
    }
} 