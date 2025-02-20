using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagementSystem.Models
{
    public class Leave
    {
        [Key]
        public int LeaveId { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }
        [Required]
        public string LeaveType { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public string Reason { get; set; }
        [Required]
        public LeaveStatus Status { get; set; }
        [Required]
        public DateTime RequestDate { get; set; } = DateTime.Now;
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
    }

    public enum LeaveStatus
    {
        Pending,
        Approved,
        Rejected
    }
} 