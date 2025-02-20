using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagementSystem.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public decimal Salary { get; set; }
        public DateTime JoinDate { get; set; }
        public string ContactNumber { get; set; }
        public string Address { get; set; }
        public virtual ICollection<Attendance> AttendanceRecords { get; set; }
        public virtual ICollection<Leave> Leaves { get; set; }
        public virtual ICollection<PerformanceReview> PerformanceReviews { get; set; }
    }
} 