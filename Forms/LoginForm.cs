using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Forms
{
    public partial class LoginForm : Form
    {
        private readonly ApplicationDbContext _context;
        private TextBox txtEmail;
        private TextBox txtPassword;

        public User LoggedInUser { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
            _context = new ApplicationDbContext();
            SetupLoginForm();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(400, 500);
            this.Name = "LoginForm";
            this.Text = "Login";
            this.ResumeLayout(false);
        }

        private void SetupLoginForm()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Text = "Login";
            this.BackColor = Color.FromArgb(37, 37, 38);

            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(40)
            };

            var logoLabel = new Label
            {
                Text = "👥",
                Font = new Font("Segoe UI", 48),
                ForeColor = Color.FromArgb(0, 123, 255),
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 80,
                Dock = DockStyle.Top
            };

            var lblTitle = new Label
            {
                Text = "HR Management System",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Height = 50,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 20)
            };

            var formContainer = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(20, 0, 20, 0)
            };

            var lblEmail = new Label
            {
                Text = "Email",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 5)
            };

            txtEmail = new TextBox
            {
                Size = new Size(300, 30),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 12),
                Margin = new Padding(0, 0, 0, 15)
            };

            var lblPassword = new Label
            {
                Text = "Password",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 5)
            };

            txtPassword = new TextBox
            {
                Size = new Size(300, 30),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                PasswordChar = '•',
                Font = new Font("Segoe UI", 12),
                Margin = new Padding(0, 0, 0, 25)
            };

            var btnLogin = new Button
            {
                Text = "Login",
                Size = new Size(300, 45),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = Color.FromArgb(0, 105, 217);
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = Color.FromArgb(0, 123, 255);

            formContainer.Controls.AddRange(new Control[] {
                lblEmail,
                txtEmail,
                lblPassword,
                txtPassword,
                btnLogin
            });

            mainPanel.Controls.Add(formContainer);
            mainPanel.Controls.Add(lblTitle);
            mainPanel.Controls.Add(logoLabel);

            this.Controls.Add(mainPanel);

            txtEmail.Focus();

            this.AcceptButton = btnLogin;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                var user = _context.Users
                    .FirstOrDefault(u => u.Email == txtEmail.Text && u.Password == txtPassword.Text);

                if (user != null)
                {
                    user.LastLogin = DateTime.Now;
                    _context.SaveChanges();

                    LoggedInUser = user;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid email or password", "Login Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 