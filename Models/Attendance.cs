using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagementSystem.Models
{
    public class Attendance
    {
        [Key]
        public int AttendanceId { get; set; }
        
        [Required]
        public int EmployeeId { get; set; }
        
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        public TimeSpan? CheckInTime { get; set; }
        
        public TimeSpan? CheckOutTime { get; set; }
        
        public string Status { get; set; }
        
        public string Notes { get; set; }
        
        public byte[] CheckInPhoto { get; set; }
        
        public byte[] CheckOutPhoto { get; set; }
        
        public string CheckInLocation { get; set; }
        
        public string CheckOutLocation { get; set; }
    }
} 