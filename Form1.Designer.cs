namespace EmployeeManagementSystem;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        
        // Initialize all buttons
        btnDashboard = new Button();
        btnAttendance = new Button();
        btnEmployees = new Button();
        btnLeave = new Button();
        btnPayroll = new Button();
        btnReports = new Button();
        btnSettings = new Button();
        btnAbout = new Button();
        btnClose = new Button();
        btnMaximize = new Button();
        btnMinimize = new Button();
        btnExit = new Button();

        // Initialize panels
        headerPanel = new Panel();
        sidebarPanel = new Panel();
        contentPanel = new Panel();
        lblTitle = new Label();

        // Configure header panel
        headerPanel.BackColor = Color.FromArgb(45, 45, 48);
        headerPanel.Dock = DockStyle.Top;
        headerPanel.Height = 40;

        // Configure window control buttons
        btnClose.Size = new Size(46, 40);
        btnClose.Location = new Point(1154, 0);
        btnClose.Text = "×";
        btnClose.Font = new Font("Segoe UI", 14);
        btnClose.FlatStyle = FlatStyle.Flat;
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.BackColor = Color.FromArgb(45, 45, 48);
        btnClose.ForeColor = Color.White;
        btnClose.Cursor = Cursors.Hand;
        btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        btnMaximize.Size = new Size(46, 40);
        btnMaximize.Location = new Point(1108, 0);
        btnMaximize.Text = "□";
        btnMaximize.Font = new Font("Segoe UI", 14);
        btnMaximize.FlatStyle = FlatStyle.Flat;
        btnMaximize.FlatAppearance.BorderSize = 0;
        btnMaximize.BackColor = Color.FromArgb(45, 45, 48);
        btnMaximize.ForeColor = Color.White;
        btnMaximize.Cursor = Cursors.Hand;
        btnMaximize.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        btnMinimize.Size = new Size(46, 40);
        btnMinimize.Location = new Point(1062, 0);
        btnMinimize.Text = "−";
        btnMinimize.Font = new Font("Segoe UI", 14);
        btnMinimize.FlatStyle = FlatStyle.Flat;
        btnMinimize.FlatAppearance.BorderSize = 0;
        btnMinimize.BackColor = Color.FromArgb(45, 45, 48);
        btnMinimize.ForeColor = Color.White;
        btnMinimize.Cursor = Cursors.Hand;
        btnMinimize.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        // Configure title label
        lblTitle.Text = "HR Management System";
        lblTitle.ForeColor = Color.White;
        lblTitle.Font = new Font("Segoe UI", 12);
        lblTitle.Location = new Point(10, 8);
        lblTitle.AutoSize = true;

        // Add controls to header panel
        headerPanel.Controls.Add(btnClose);
        headerPanel.Controls.Add(btnMaximize);
        headerPanel.Controls.Add(btnMinimize);
        headerPanel.Controls.Add(lblTitle);

        // Configure sidebar panel
        sidebarPanel.BackColor = Color.FromArgb(45, 45, 48);
        sidebarPanel.Dock = DockStyle.Left;
        sidebarPanel.Width = 250;

        // Configure content panel
        contentPanel.BackColor = Color.FromArgb(37, 37, 38);
        contentPanel.Dock = DockStyle.Fill;

        // Configure navigation buttons
        ConfigureNavigationButton(btnDashboard, "Dashboard", 0);
        ConfigureNavigationButton(btnAttendance, "Attendance", 1);
        ConfigureNavigationButton(btnEmployees, "Employee", 2);
        ConfigureNavigationButton(btnLeave, "Leave Management", 3);
        ConfigureNavigationButton(btnPayroll, "Payroll", 4);
        ConfigureNavigationButton(btnReports, "Reports", 5);
        ConfigureNavigationButton(btnSettings, "Settings", 6);
        ConfigureNavigationButton(btnAbout, "About", 7);

        // Configure exit button
        btnExit.Text = "🚪 Exit";
        btnExit.Height = 45;
        btnExit.Dock = DockStyle.Bottom;
        btnExit.FlatStyle = FlatStyle.Flat;
        btnExit.FlatAppearance.BorderSize = 0;
        btnExit.Font = new Font("Segoe UI", 10);
        btnExit.ForeColor = Color.White;
        btnExit.BackColor = Color.FromArgb(220, 53, 69);
        btnExit.TextAlign = ContentAlignment.MiddleLeft;
        btnExit.Padding = new Padding(15, 0, 0, 0);
        btnExit.Cursor = Cursors.Hand;

        // Add buttons to sidebar
        sidebarPanel.Controls.AddRange(new Control[] {
            btnExit,
            btnAbout,
            btnSettings,
            btnReports,
            btnPayroll,
            btnLeave,
            btnEmployees,
            btnAttendance,
            btnDashboard
        });

        // Add panels to form
        Controls.Add(contentPanel);
        Controls.Add(sidebarPanel);
        Controls.Add(headerPanel);

        // Form properties
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1200, 700);
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(37, 37, 38);
    }

    private void ConfigureNavigationButton(Button btn, string text, int position)
    {
        string icon = "";
        switch (text)
        {
            case "Dashboard":
                icon = "📊 ";  // Dashboard icon
                break;
            case "Attendance":
                icon = "⏲ ";  // Clock icon
                break;
            case "Employee":
                icon = "👥 ";  // People icon
                break;
            case "Leave Management":
                icon = "📅 ";  // Calendar icon
                break;
            case "Payroll":
                icon = "💰 ";  // Money bag icon
                break;
            case "Reports":
                icon = "📈 ";  // Chart icon
                break;
            case "Settings":
                icon = "⚙️ ";  // Gear icon
                break;
            case "About":
                icon = "ℹ️ ";  // Info icon
                break;
        }

        btn.Text = icon + text;
        btn.Height = 45;
        btn.Dock = DockStyle.Top;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Font = new Font("Segoe UI", 10);
        btn.ForeColor = Color.White;
        btn.TextAlign = ContentAlignment.MiddleLeft;
        btn.Padding = new Padding(15, 0, 0, 0);
        btn.ImageAlign = ContentAlignment.MiddleLeft;
        btn.TextImageRelation = TextImageRelation.ImageBeforeText;
    }

    #endregion

    private Button btnDashboard;
    private Button btnAttendance;
    private Button btnEmployees;
    private Button btnLeave;
    private Button btnPayroll;
    private Button btnReports;
    private Button btnSettings;
    private Button btnAbout;
    private Button btnClose;
    private Button btnMaximize;
    private Button btnMinimize;
    private Button btnExit;
    private Panel headerPanel;
    private Panel sidebarPanel;
    private Panel contentPanel;
    private Label lblTitle;
}
