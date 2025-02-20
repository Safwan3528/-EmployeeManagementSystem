using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagementSystem.Models
{
    public class PerformanceReview
    {
        [Key]
        public int ReviewId { get; set; }
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }
        public DateTime ReviewDate { get; set; }
        public string ReviewPeriod { get; set; }
        public int ProductivityScore { get; set; }
        public int QualityScore { get; set; }
        public int InitiativeScore { get; set; }
        public int TeamworkScore { get; set; }
        public int CommunicationScore { get; set; }
        public string Achievements { get; set; }
        public string AreasOfImprovement { get; set; }
        public string ReviewerComments { get; set; }
        public string EmployeeComments { get; set; }
        public string ReviewedBy { get; set; }
        public ReviewStatus Status { get; set; }
    }

    public enum ReviewStatus
    {
        Draft,
        Submitted,
        Acknowledged,
        Completed
    }
} 