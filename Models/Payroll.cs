using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagementSystem.Models
{
    public class Payroll
    {
        [Key]
        public int PayrollId { get; set; }
        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }
        [Required]
        public DateTime PayPeriodStart { get; set; }
        [Required]
        public DateTime PayPeriodEnd { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseSalary { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Overtime { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Bonus { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PublicHoliday { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Deductions { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxDeductions { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetSalary { get; set; }
        [Required]
        public DateTime PaymentDate { get; set; }
        [Required]
        public string PaymentStatus { get; set; }
        [Required]
        public string PaymentReference { get; set; }
        public string DeductionDescription { get; set; }
    }
} 