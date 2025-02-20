using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using EmployeeManagementSystem.Helpers;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using System.Drawing.Printing;  // Print Document
using ZXing.Rendering;
using ZXing.Windows.Compatibility;

namespace EmployeeManagementSystem.Forms
{
    public partial class EmployeeForm : Form
    {
        private readonly ApplicationDbContext _context;
        private readonly DataGridView dgvEmployees;
        private readonly Panel formPanel;
        private readonly User _currentUser;
        private readonly TableLayoutPanel mainContainer;
        private readonly Panel gridContainer;

        public EmployeeForm(User currentUser)
        {
            try
            {
                Console.WriteLine("Initializing EmployeeForm...");
                
                _currentUser = currentUser;
                InitializeComponent();
                _context = new ApplicationDbContext();
                
                // Initialize DataGridView
                dgvEmployees = new DataGridView
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

                // Initialize form panel
                formPanel = new Panel
                {
                    Dock = DockStyle.Right,
                    Width = 300,
                    BackColor = Color.FromArgb(45, 45, 48),
                    Padding = new Padding(10),
                    AutoScroll = true
                };

                // Initialize grid container
                gridContainer = new Panel
                {
                    Dock = DockStyle.Fill
                };

                // Create main container
                mainContainer = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1,
                    BackColor = Color.FromArgb(37, 37, 38)
                };

                // Configure TableLayoutPanel
                mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));  // DataGridView
                mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));  // Form Panel

                // Add controls to containers
                gridContainer.Controls.Add(dgvEmployees);
                mainContainer.Controls.Add(gridContainer, 0, 0);
                mainContainer.Controls.Add(formPanel, 1, 0);

                // Add TableLayoutPanel to form
                this.Controls.Add(mainContainer);

                // Setup other components
                SetupFormControls();
                SetupDataGridView();
                LoadEmployees();

                // Make sure form is visible
                this.Visible = true;
                this.BringToFront();

                Console.WriteLine("EmployeeForm initialized successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing EmployeeForm: {ex.Message}\n\n{ex.StackTrace}", 
                    "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupFormControls()
        {
            // add internal panel for all controls
            var innerPanel = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,  // responsive panel
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            // add PictureBox for empolyee picture
            var pictureBox = new PictureBox
            {
                Name = "picEmployee",
                Size = new Size(150, 150),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 5, 0, 10),
                BackColor = Color.FromArgb(60, 60, 60),
                Image = CreateSimpleAvatar()
            };

            // add upload button for employee picture
            var btnUploadImage = new Button
            {
                Text = "Upload Image",
                Dock = DockStyle.Top,
                Height = 30,
                Margin = new Padding(0, 5, 0, 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatAppearance = { BorderSize = 0 },
                TabIndex = 0
            };

            btnUploadImage.Click += (s, e) => UploadImage(pictureBox);

            var lblTitle = new Label
            {
                Text = "Employee Details",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Height = 40
            };

            // Create input fields with specific names and TabIndex
            var txtName = CreateTextBox("txtName", "Name", 1);
            var txtEmail = CreateTextBox("txtEmail", "Email", 2);
            var txtPassword = CreateTextBox("txtPassword", "Password", 3);
            txtPassword.PasswordChar = '•';
            var txtPosition = CreateTextBox("txtPosition", "Position", 4);
            var txtDepartment = CreateTextBox("txtDepartment", "Department", 5);
            var txtSalary = CreateTextBox("txtSalary", "Salary", 6);
            var dtpJoinDate = new DateTimePicker
            {
                Name = "dtpJoinDate",
                Format = DateTimePickerFormat.Short,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 5, 0, 10),
                TabIndex = 7
            };

            // Create buttons with TabIndex
            var btnAdd = CreateButton("Add Employee", Color.FromArgb(0, 123, 255), 8);
            var btnUpdate = CreateButton("Update", Color.FromArgb(40, 167, 69), 9);
            var btnDelete = CreateButton("Delete", Color.FromArgb(220, 53, 69), 10);
            var btnPrintId = CreateButton("Print ID Card", Color.FromArgb(108, 117, 125), 11);

            // add all control to innerPanel instead of formPanel
            innerPanel.Controls.AddRange(new Control[] {
                btnDelete,
                btnUpdate,
                btnAdd,
                btnPrintId,
                dtpJoinDate,
                CreateLabel("Join Date"),
                txtSalary,
                CreateLabel("Salary"),
                txtDepartment,
                CreateLabel("Department"),
                txtPosition,
                CreateLabel("Position"),
                txtPassword,
                CreateLabel("Password"),
                txtEmail,
                CreateLabel("Email"),
                txtName,
                CreateLabel("Name"),
                lblTitle,
                btnUploadImage,
                pictureBox
            });

            // add innerPanel to formPanel
            formPanel.Controls.Add(innerPanel);

            // Add event handlers
            btnAdd.Click += (s, e) => AddEmployee(txtName, txtEmail, txtPassword, txtPosition, txtDepartment, txtSalary, dtpJoinDate, pictureBox);
            btnUpdate.Click += (s, e) => UpdateEmployee(txtName, txtEmail, txtPassword, txtPosition, txtDepartment, txtSalary, dtpJoinDate, pictureBox);
            btnDelete.Click += (s, e) => DeleteEmployee();
            btnPrintId.Click += (s, e) => GenerateAndPrintIdCard();
        }

        private void SetupDataGridView()
        {
            dgvEmployees.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { 
                    Name = "Id", 
                    HeaderText = "ID", 
                    Width = 80,
                    DefaultCellStyle = new DataGridViewCellStyle {
                        Format = "000", // Format ID to show as 001, 002, etc.
                        BackColor = Color.FromArgb(60, 60, 60),
                        SelectionBackColor = Color.FromArgb(100, 100, 100)
                    }
                },
                new DataGridViewTextBoxColumn { 
                    Name = "Name", 
                    HeaderText = "Name", 
                    Width = 150,
                    DefaultCellStyle = new DataGridViewCellStyle {
                        BackColor = Color.FromArgb(60, 60, 60),
                        SelectionBackColor = Color.FromArgb(100, 100, 100)
                    }
                },
                new DataGridViewTextBoxColumn { 
                    Name = "Email", 
                    HeaderText = "Email", 
                    Width = 150,
                    DefaultCellStyle = new DataGridViewCellStyle {
                        BackColor = Color.FromArgb(60, 60, 60),
                        SelectionBackColor = Color.FromArgb(100, 100, 100)
                    }
                },
                new DataGridViewTextBoxColumn { 
                    Name = "Position", 
                    HeaderText = "Position", 
                    Width = 120,
                    DefaultCellStyle = new DataGridViewCellStyle {
                        BackColor = Color.FromArgb(60, 60, 60),
                        SelectionBackColor = Color.FromArgb(100, 100, 100)
                    }
                },
                new DataGridViewTextBoxColumn { 
                    Name = "Department", 
                    HeaderText = "Department", 
                    Width = 120,
                    DefaultCellStyle = new DataGridViewCellStyle {
                        BackColor = Color.FromArgb(60, 60, 60),
                        SelectionBackColor = Color.FromArgb(100, 100, 100)
                    }
                },
                new DataGridViewTextBoxColumn { 
                    Name = "Salary", 
                    HeaderText = "Salary", 
                    Width = 100,
                    DefaultCellStyle = new DataGridViewCellStyle {
                        Format = "C2",
                        BackColor = Color.FromArgb(60, 60, 60),
                        SelectionBackColor = Color.FromArgb(100, 100, 100)
                    }
                },
                new DataGridViewTextBoxColumn { 
                    Name = "JoinDate", 
                    HeaderText = "Join Date", 
                    Width = 100,
                    DefaultCellStyle = new DataGridViewCellStyle {
                        BackColor = Color.FromArgb(60, 60, 60),
                        SelectionBackColor = Color.FromArgb(100, 100, 100)
                    }
                }
            });

            // add style to header
            dgvEmployees.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                SelectionBackColor = Color.FromArgb(45, 45, 48),
                Font = new Font("Segoe UI", 9.75F, FontStyle.Bold)
            };

            // add style for alternate rows
            dgvEmployees.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(50, 50, 53),
                ForeColor = Color.White,
                SelectionBackColor = Color.FromArgb(100, 100, 100)
            };

            // add style for rows
            dgvEmployees.RowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                SelectionBackColor = Color.FromArgb(100, 100, 100)
            };

            dgvEmployees.EnableHeadersVisualStyles = false;
            dgvEmployees.CellClick += DgvEmployees_CellClick;
        }

        private void LoadEmployees()
        {
            dgvEmployees.Rows.Clear();
            var employees = _context.Employees
                .Include(e => e.User)
                .OrderBy(e => e.EmployeeId)
                .ToList();

            foreach (var employee in employees)
            {
                dgvEmployees.Rows.Add(
                    employee.EmployeeId,
                    employee.User.Name,
                    employee.User.Email,
                    employee.Position,
                    employee.Department,
                    employee.Salary,
                    employee.JoinDate.ToShortDateString()
                );
            }
        }

        private TextBox CreateTextBox(string name, string placeholder, int tabIndex)
        {
            return new TextBox
            {
                Name = name,
                Dock = DockStyle.Top,
                Height = 30,
                Margin = new Padding(0, 5, 0, 10),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                TabIndex = tabIndex,
                Tag = placeholder  // for text placeholder if needed
            };
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

        private Button CreateButton(string text, Color backColor, int tabIndex)
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
                TabIndex = tabIndex
            };
        }

        private void UploadImage(PictureBox pictureBox)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
                openFileDialog.Title = "Select Employee Image";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        pictureBox.Image = Image.FromFile(openFileDialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading image: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void AddEmployee(TextBox txtName, TextBox txtEmail, TextBox txtPassword, TextBox txtPosition, 
            TextBox txtDepartment, TextBox txtSalary, DateTimePicker dtpJoinDate, PictureBox pictureBox)
        {
            try
            {
                if (string.IsNullOrEmpty(txtName.Text) || string.IsNullOrEmpty(txtEmail.Text) || 
                    string.IsNullOrEmpty(txtPassword.Text))
                {
                    MessageBox.Show("Name, Email and Password are required!", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Convert image to byte array
                byte[] imageData = null;
                if (pictureBox.Image != null && !IsDefaultAvatar(pictureBox.Image))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        pictureBox.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        imageData = ms.ToArray();
                    }
                }

                var user = new User
                {
                    Name = txtName.Text,
                    Email = txtEmail.Text,
                    Password = txtPassword.Text,
                    Role = UserRole.Employee,
                    CreatedAt = DateTime.Now,
                    ProfileImage = imageData
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                // Create new employee
                var employee = new Employee
                {
                    UserId = user.UserId,
                    Position = txtPosition.Text,
                    Department = txtDepartment.Text,
                    Salary = decimal.Parse(txtSalary.Text),
                    JoinDate = dtpJoinDate.Value,
                };

                _context.Employees.Add(employee);
                _context.SaveChanges();

                LoadEmployees();
                ClearForm(txtName, txtEmail, txtPassword, txtPosition, txtDepartment, txtSalary, dtpJoinDate, pictureBox);
                
                MessageBox.Show(
                    $"Employee added successfully!\n\n" +
                    $"Login Credentials:\n" +
                    $"Email: {user.Email}\n" +
                    $"Password: {txtPassword.Text}",
                    "Success",
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding employee: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateEmployee(TextBox txtName, TextBox txtEmail, TextBox txtPassword, TextBox txtPosition,
            TextBox txtDepartment, TextBox txtSalary, DateTimePicker dtpJoinDate, PictureBox pictureBox)
        {
            try
            {
                if (dgvEmployees.SelectedRows.Count == 0) return;

                var employeeId = (int)dgvEmployees.SelectedRows[0].Cells["Id"].Value;
                var employee = _context.Employees
                    .Include(e => e.User)
                    .FirstOrDefault(e => e.EmployeeId == employeeId);

                if (employee == null) return;

                // Update user details
                employee.User.Name = txtName.Text;
                employee.User.Email = txtEmail.Text;
                employee.User.Password = txtPassword.Text;

                // Update employee details
                employee.Position = txtPosition.Text;
                employee.Department = txtDepartment.Text;
                employee.Salary = decimal.Parse(txtSalary.Text);
                employee.JoinDate = dtpJoinDate.Value;

                _context.SaveChanges();
                LoadEmployees();
                MessageBox.Show("Employee updated successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating employee: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteEmployee()
        {
            try
            {
                if (dgvEmployees.SelectedRows.Count == 0) return;

                if (MessageBox.Show("Are you sure you want to delete this employee?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;

                var employeeId = (int)dgvEmployees.SelectedRows[0].Cells["Id"].Value;
                var employee = _context.Employees.Find(employeeId);

                if (employee != null)
                {
                    _context.Employees.Remove(employee);
                    _context.SaveChanges();
                    LoadEmployees();
                    MessageBox.Show("Employee deleted successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting employee: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvEmployees_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var employeeId = (int)dgvEmployees.Rows[e.RowIndex].Cells["Id"].Value;
            var employee = _context.Employees
                .Include(e => e.User)
                .FirstOrDefault(e => e.EmployeeId == employeeId);

            if (employee == null) return;

            // Fill form with employee data
            var innerPanel = formPanel.Controls[0] as Panel;  // Get innerPanel
            foreach (Control ctrl in innerPanel.Controls)
            {
                if (ctrl is PictureBox pic && pic.Name == "picEmployee")
                {
                    if (employee.User.ProfileImage != null && employee.User.ProfileImage.Length > 0)
                    {
                        using (MemoryStream ms = new MemoryStream(employee.User.ProfileImage))
                        {
                            pic.Image = Image.FromStream(ms);
                        }
                    }
                    else
                    {
                        pic.Image = CreateSimpleAvatar();
                    }
                }
                else if (ctrl is TextBox txt)
                {
                    switch (txt.Name)
                    {
                        case "txtName":
                            txt.Text = employee.User.Name;
                            break;
                        case "txtEmail":
                            txt.Text = employee.User.Email;
                            break;
                        case "txtPosition":
                            txt.Text = employee.Position;
                            break;
                        case "txtDepartment":
                            txt.Text = employee.Department;
                            break;
                        case "txtSalary":
                            txt.Text = employee.Salary.ToString();
                            break;
                    }
                }
                else if (ctrl is DateTimePicker dtp)
                {
                    dtp.Value = employee.JoinDate;
                }
            }
        }

        private void ClearForm(TextBox txtName, TextBox txtEmail, TextBox txtPassword, TextBox txtPosition,
            TextBox txtDepartment, TextBox txtSalary, DateTimePicker dtpJoinDate, PictureBox pictureBox)
        {
            var innerPanel = formPanel.Controls[0] as Panel;
            foreach (Control ctrl in innerPanel.Controls)
            {
                if (ctrl is TextBox txt)
                {
                    txt.Text = string.Empty;
                }
                else if (ctrl is DateTimePicker dtp)
                {
                    dtp.Value = DateTime.Now;
                }
                else if (ctrl is PictureBox pic)
                {
                    pic.Image = CreateSimpleAvatar();
                }
            }
        }

        private Image CreateSimpleAvatar()
        {
            var bitmap = new Bitmap(150, 150);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.FromArgb(74, 74, 74));
                using (var brush = new SolidBrush(Color.FromArgb(102, 102, 102)))
                {
                    g.FillEllipse(brush, 45, 30, 60, 60); // head
                    g.FillEllipse(brush, 30, 95, 90, 75); // body
                }
            }
            return bitmap;
        }

        private bool IsDefaultAvatar(Image image)
        {
            // compare with bitmap created with CreateSimpleAvatar
            using (var defaultAvatar = CreateSimpleAvatar())
            using (var ms1 = new MemoryStream())
            using (var ms2 = new MemoryStream())
            {
                image.Save(ms1, System.Drawing.Imaging.ImageFormat.Png);
                defaultAvatar.Save(ms2, System.Drawing.Imaging.ImageFormat.Png);

                var bytes1 = ms1.ToArray();
                var bytes2 = ms2.ToArray();

                return bytes1.SequenceEqual(bytes2);
            }
        }

        private void GenerateAndPrintIdCard()
        {
            try
            {
                if (dgvEmployees.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select an employee first!", "Warning",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var employeeId = (int)dgvEmployees.SelectedRows[0].Cells["Id"].Value;
                var employee = _context.Employees
                    .Include(e => e.User)
                    .FirstOrDefault(e => e.EmployeeId == employeeId);

                if (employee == null)
                {
                    MessageBox.Show("Employee data not found!", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // create bitnap for ID Card (Potrait: 250x400)
                using (Bitmap idCard = new Bitmap(250, 400))
                using (Graphics g = Graphics.FromImage(idCard))
                {
                    // Set white background
                    g.Clear(Color.White);

                    // Draw border
                    using (Pen borderPen = new Pen(Color.DarkBlue, 2))
                    {
                        g.DrawRectangle(borderPen, 1, 1, 247, 397);
                    }

                    // Header colour
                    using (SolidBrush headerBrush = new SolidBrush(Color.DarkBlue))
                    {
                        g.FillRectangle(headerBrush, 2, 2, 246, 50);
                    }

                    // Company Name
                    using (Font headerFont = new Font("Arial", 16, FontStyle.Bold))
                    using (SolidBrush textBrush = new SolidBrush(Color.White))
                    {
                        string companyName = "COMPANY NAME";
                        SizeF textSize = g.MeasureString(companyName, headerFont);
                        float x = (250 - textSize.Width) / 2; // Center text
                        g.DrawString(companyName, headerFont, textBrush, x, 12);
                    }

                    // Add employee picture (in potrait format)
                    Rectangle photoRect = new Rectangle(50, 60, 150, 180);
                    if (employee.User.ProfileImage != null && employee.User.ProfileImage.Length > 0)
                    {
                        using (MemoryStream ms = new MemoryStream(employee.User.ProfileImage))
                        using (Image photo = Image.FromStream(ms))
                        {
                            g.DrawImage(photo, photoRect);
                        }
                    }
                    else
                    {
                        using (Image defaultAvatar = CreateSimpleAvatar())
                        {
                            g.DrawImage(defaultAvatar, photoRect);
                        }
                    }

                    // add employee detail
                    using (Font detailFont = new Font("Arial", 10))
                    using (SolidBrush textBrush = new SolidBrush(Color.Black))
                    {
                        int y = 250; // Start below photo
                        int leftMargin = 20;
                        
                        // Draw labels and values with proper formatting
                        string[] details = {
                            $"ID: {employee.EmployeeId:D4}",
                            $"Name: {employee.User.Name}",
                            $"Position: {employee.Position}",
                            $"Department: {employee.Department}"
                        };

                        foreach (string detail in details)
                        {
                            g.DrawString(detail, detailFont, textBrush, leftMargin, y);
                            y += 25; // Spacing between lines
                        }
                    }

                    // Add QR code in buttom
                    var writer = new BarcodeWriter
                    {
                        Format = BarcodeFormat.QR_CODE,
                        Options = new EncodingOptions
                        {
                            Height = 30,
                            Width = 30,
                            Margin = 0
                        }
                    };

                    using (var qrImage = writer.Write(employee.EmployeeId.ToString("D4")))
                    {
                        // Center the QR code at the very bottom
                        int qrX = (250 - qrImage.Width) / 2;
                        int qrY = 400 - qrImage.Height - 10; // 10 pixels dari bawah
                        g.DrawImage(qrImage, new Point(qrX, qrY));
                    }

                    // Preview dan Print
                    using (PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog())
                    using (PrintDocument pd = new PrintDocument())
                    {
                        pd.DefaultPageSettings.Landscape = false; // Set to portrait
                        pd.PrintPage += (sender, args) =>
                        {
                            // Calculate scaling to fit on the page while maintaining aspect ratio
                            float ratio = Math.Min(
                                args.MarginBounds.Width / (float)idCard.Width,
                                args.MarginBounds.Height / (float)idCard.Height);

                            int scaledWidth = (int)(idCard.Width * ratio);
                            int scaledHeight = (int)(idCard.Height * ratio);

                            // Center on page
                            int x = args.MarginBounds.X + (args.MarginBounds.Width - scaledWidth) / 2;
                            int y = args.MarginBounds.Y + (args.MarginBounds.Height - scaledHeight) / 2;

                            args.Graphics.DrawImage(idCard, x, y, scaledWidth, scaledHeight);
                        };

                        printPreviewDialog.Document = pd;
                        if (printPreviewDialog.ShowDialog() == DialogResult.OK)
                        {
                            pd.Print();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating ID card: {ex.Message}\n\nStack Trace: {ex.StackTrace}", 
                    "Error",
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                Console.WriteLine($"Error in GenerateAndPrintIdCard: {ex}");
            }
        }
    }
} 