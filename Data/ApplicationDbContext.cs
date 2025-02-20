using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Models;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace EmployeeManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<PerformanceReview> PerformanceReviews { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = "hrmanagement.db";
            
            // Check if running from Visual Studio
            if (Debugger.IsAttached)
            {
                // Get the executable path
                string exePath = Assembly.GetExecutingAssembly().Location;
                string binPath = Path.GetDirectoryName(exePath);
                dbPath = Path.Combine(binPath, "hrmanagement.db");
            }

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
            
            // Optional: Print path for debugging
            Console.WriteLine($"Database Path: {Path.GetFullPath(dbPath)}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<Employee>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasOne(a => a.Employee)
                    .WithMany(e => e.AttendanceRecords)
                    .HasForeignKey(a => a.EmployeeId);

                entity.Property(e => e.CheckInLocation)
                    .HasColumnType("TEXT")
                    .IsRequired(false);

                entity.Property(e => e.CheckOutLocation)
                    .HasColumnType("TEXT")
                    .IsRequired(false);

                entity.Property(e => e.CheckInPhoto)
                    .HasColumnType("BLOB")
                    .IsRequired(false);

                entity.Property(e => e.CheckOutPhoto)
                    .HasColumnType("BLOB")
                    .IsRequired(false);
            });

            modelBuilder.Entity<Leave>()
                .HasOne(l => l.Employee)
                .WithMany(e => e.Leaves)
                .HasForeignKey(l => l.EmployeeId);
        }
    }
} 