using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Forms
{
    public partial class PerformanceReviewForm : Form
    {
        private readonly ApplicationDbContext _context;
        private readonly DataGridView dgvReviews;
        private readonly Panel formPanel;
        private readonly ComboBox cmbEmployee;
        private readonly TextBox txtReviewPeriod;
        private readonly NumericUpDown nudProductivity;
        private readonly NumericUpDown nudQuality;
        private readonly NumericUpDown nudInitiative;
        private readonly NumericUpDown nudTeamwork;
        private readonly NumericUpDown nudCommunication;
        private readonly TextBox txtAchievements;
        private readonly TextBox txtAreasOfImprovement;
        private readonly TextBox txtReviewerComments;
        private readonly TextBox txtEmployeeComments;
        private readonly Label lblOverallScore;
        private int? selectedReviewId;
        private readonly User _currentUser;

        public PerformanceReviewForm(User currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            _context = new ApplicationDbContext();

            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(37, 37, 38);

            // Initialize main controls
            dgvReviews = new DataGridView
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
                Width = 400,
                BackColor = Color.FromArgb(45, 45, 48),
                Padding = new Padding(10),
                AutoScroll = true
            };

            // Initialize form controls
            cmbEmployee = new ComboBox
            {
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            txtReviewPeriod = CreateTextBox("Review Period");

            // Create rating controls
            nudProductivity = CreateRatingControl();
            nudQuality = CreateRatingControl();
            nudInitiative = CreateRatingControl();
            nudTeamwork = CreateRatingControl();
            nudCommunication = CreateRatingControl();

            // Create text areas
            txtAchievements = CreateTextArea("Key Achievements");
            txtAreasOfImprovement = CreateTextArea("Areas for Improvement");
            txtReviewerComments = CreateTextArea("Reviewer Comments");
            txtEmployeeComments = CreateTextArea("Employee Comments");

            lblOverallScore = new Label
            {
                Text = "Overall Score: 0",
                Dock = DockStyle.Top,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft
            };

            SetupFormControls();
            SetupDataGridView();
            LoadEmployees();
            LoadReviews();

            // Add event handlers for automatic calculation
            nudProductivity.ValueChanged += CalculateOverallScore;
            nudQuality.ValueChanged += CalculateOverallScore;
            nudInitiative.ValueChanged += CalculateOverallScore;
            nudTeamwork.ValueChanged += CalculateOverallScore;
            nudCommunication.ValueChanged += CalculateOverallScore;
        }

        private NumericUpDown CreateRatingControl()
        {
            return new NumericUpDown
            {
                Dock = DockStyle.Top,
                Minimum = 1,
                Maximum = 5,
                Value = 3,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                Height = 30,
                Margin = new Padding(0, 5, 0, 10)
            };
        }

        private TextBox CreateTextArea(string placeholder)
        {
            return new TextBox
            {
                Dock = DockStyle.Top,
                Height = 100,
                Multiline = true,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 5, 0, 10)
            };
        }

        private void SetupFormControls()
        {
            var lblTitle = new Label
            {
                Text = "Performance Review",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Height = 40
            };

            // Create buttons
            var btnSubmit = CreateButton("Submit Review", Color.FromArgb(0, 123, 255));
            var btnAcknowledge = CreateButton("Acknowledge Review", Color.FromArgb(40, 167, 69));
            var btnClear = CreateButton("Clear Form", Color.FromArgb(108, 117, 125));

            // Add controls to form panel
            formPanel.Controls.AddRange(new Control[] {
                btnClear,
                btnAcknowledge,
                btnSubmit,
                CreateLabel("Employee Comments"),
                txtEmployeeComments,
                CreateLabel("Reviewer Comments"),
                txtReviewerComments,
                CreateLabel("Areas for Improvement"),
                txtAreasOfImprovement,
                CreateLabel("Key Achievements"),
                txtAchievements,
                lblOverallScore,
                CreateLabel("Communication (1-5)"),
                nudCommunication,
                CreateLabel("Teamwork (1-5)"),
                nudTeamwork,
                CreateLabel("Initiative (1-5)"),
                nudInitiative,
                CreateLabel("Quality of Work (1-5)"),
                nudQuality,
                CreateLabel("Productivity (1-5)"),
                nudProductivity,
                CreateLabel("Review Period"),
                txtReviewPeriod,
                CreateLabel("Employee"),
                cmbEmployee,
                lblTitle
            });

            // Add event handlers
            btnSubmit.Click += BtnSubmit_Click;
            btnAcknowledge.Click += BtnAcknowledge_Click;
            btnClear.Click += BtnClear_Click;
        }

        private void SetupDataGridView()
        {
            dgvReviews.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 50 },
                new DataGridViewTextBoxColumn { Name = "EmployeeName", HeaderText = "Employee", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "ReviewPeriod", HeaderText = "Review Period", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "OverallScore", HeaderText = "Overall Score", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "ReviewDate", HeaderText = "Review Date", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 100 }
            });

            dgvReviews.CellClick += DgvReviews_CellClick;

            // Add to form
            var mainContainer = new Panel { Dock = DockStyle.Fill };
            mainContainer.Controls.Add(dgvReviews);
            mainContainer.Controls.Add(formPanel);
            this.Controls.Add(mainContainer);
        }

        private void LoadEmployees()
        {
            var employees = _context.Employees
                .Include(e => e.User)
                .OrderBy(e => e.User.Name)
                .ToList();

            cmbEmployee.DisplayMember = "Name";
            cmbEmployee.ValueMember = "Id";
            cmbEmployee.DataSource = employees.Select(e => new { 
                Id = e.EmployeeId, 
                Name = e.User.Name 
            }).ToList();
        }

        private void LoadReviews()
        {
            dgvReviews.Rows.Clear();
            var reviews = _context.PerformanceReviews
                .Include(r => r.Employee)
                .ThenInclude(e => e.User)
                .OrderByDescending(r => r.ReviewDate)
                .ToList();

            foreach (var review in reviews)
            {
                var overallScore = (review.ProductivityScore + review.QualityScore + 
                    review.InitiativeScore + review.TeamworkScore + review.CommunicationScore) / 5.0;

                dgvReviews.Rows.Add(
                    review.ReviewId,
                    review.Employee.User.Name,
                    review.ReviewPeriod,
                    overallScore.ToString("F1"),
                    review.ReviewDate.ToShortDateString(),
                    review.Status
                );
            }
        }

        private void CalculateOverallScore(object sender, EventArgs e)
        {
            var overallScore = (nudProductivity.Value + nudQuality.Value + 
                nudInitiative.Value + nudTeamwork.Value + nudCommunication.Value) / 5.0m;
            lblOverallScore.Text = $"Overall Score: {overallScore:F1}";
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbEmployee.SelectedValue == null)
                {
                    MessageBox.Show("Please select an employee", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtReviewPeriod.Text))
                {
                    MessageBox.Show("Please enter the review period", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var review = new PerformanceReview
                {
                    EmployeeId = (int)cmbEmployee.SelectedValue,
                    ReviewDate = DateTime.Now,
                    ReviewPeriod = txtReviewPeriod.Text,
                    ProductivityScore = (int)nudProductivity.Value,
                    QualityScore = (int)nudQuality.Value,
                    InitiativeScore = (int)nudInitiative.Value,
                    TeamworkScore = (int)nudTeamwork.Value,
                    CommunicationScore = (int)nudCommunication.Value,
                    Achievements = txtAchievements.Text,
                    AreasOfImprovement = txtAreasOfImprovement.Text,
                    ReviewerComments = txtReviewerComments.Text,
                    Status = ReviewStatus.Submitted,
                    ReviewedBy = "Current User" // Should be replaced with actual logged-in user
                };

                _context.PerformanceReviews.Add(review);
                _context.SaveChanges();
                LoadReviews();
                ClearForm();

                MessageBox.Show("Performance review submitted successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error submitting review: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAcknowledge_Click(object sender, EventArgs e)
        {
            try
            {
                if (!selectedReviewId.HasValue)
                {
                    MessageBox.Show("Please select a review to acknowledge", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var review = _context.PerformanceReviews.Find(selectedReviewId.Value);
                if (review == null) return;

                review.Status = ReviewStatus.Acknowledged;
                review.EmployeeComments = txtEmployeeComments.Text;
                _context.SaveChanges();
                LoadReviews();

                MessageBox.Show("Review acknowledged successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error acknowledging review: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void DgvReviews_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            selectedReviewId = (int)dgvReviews.Rows[e.RowIndex].Cells["Id"].Value;
            var review = _context.PerformanceReviews
                .Include(r => r.Employee)
                .FirstOrDefault(r => r.ReviewId == selectedReviewId);

            if (review == null) return;

            // Fill form with review data
            cmbEmployee.SelectedValue = review.EmployeeId;
            txtReviewPeriod.Text = review.ReviewPeriod;
            nudProductivity.Value = review.ProductivityScore;
            nudQuality.Value = review.QualityScore;
            nudInitiative.Value = review.InitiativeScore;
            nudTeamwork.Value = review.TeamworkScore;
            nudCommunication.Value = review.CommunicationScore;
            txtAchievements.Text = review.Achievements;
            txtAreasOfImprovement.Text = review.AreasOfImprovement;
            txtReviewerComments.Text = review.ReviewerComments;
            txtEmployeeComments.Text = review.EmployeeComments;
        }

        private void ClearForm()
        {
            selectedReviewId = null;
            if (cmbEmployee.Items.Count > 0) cmbEmployee.SelectedIndex = 0;
            txtReviewPeriod.Text = $"{DateTime.Now:MMMM yyyy}";
            nudProductivity.Value = 3;
            nudQuality.Value = 3;
            nudInitiative.Value = 3;
            nudTeamwork.Value = 3;
            nudCommunication.Value = 3;
            txtAchievements.Text = string.Empty;
            txtAreasOfImprovement.Text = string.Empty;
            txtReviewerComments.Text = string.Empty;
            txtEmployeeComments.Text = string.Empty;
        }

        private TextBox CreateTextBox(string placeholder)
        {
            return new TextBox
            {
                Dock = DockStyle.Top,
                Height = 30,
                Margin = new Padding(0, 5, 0, 10),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
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