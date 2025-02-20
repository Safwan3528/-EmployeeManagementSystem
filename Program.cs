using System;
using System.Windows.Forms;
using System.Linq;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Forms;

namespace EmployeeManagementSystem;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        try
        {
            ApplicationConfiguration.Initialize();
            
            // Create default admin user if not exists
            using (var context = new ApplicationDbContext())
            {
                try
                {
                    // Ensure database exists and can be connected
                    context.Database.EnsureCreated();

                    if (!context.Users.Any(u => u.Role == UserRole.Administrator))
                    {
                        var adminUser = new User
                        {
                            Name = "Administrator",
                            Email = "admin@system.com",
                            Password = "admin123",
                            Role = UserRole.Administrator,
                            CreatedAt = DateTime.Now
                        };

                        context.Users.Add(adminUser);
                        context.SaveChanges();

                        MessageBox.Show(
                            "Default admin account created:\n" +
                            "Email: admin@system.com\n" +
                            "Password: admin123\n\n" +
                            "Please change the password after login.",
                            "Admin Account Created",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Database Error: {ex.Message}\n\n" +
                        "Please ensure the database file exists and is accessible.",
                        "Database Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
            }
            
            using (var loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    var mainForm = new Form1(loginForm.LoggedInUser);
                    Application.Run(mainForm);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Application Error: {ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }    
}