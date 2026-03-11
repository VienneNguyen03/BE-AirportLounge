using AirportLounge.Domain.Common;
using AirportLounge.Domain.Enums;

namespace AirportLounge.Domain.Entities;

public class PerformanceReview : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Guid ReviewerId { get; set; }
    public string Period { get; set; } = string.Empty;
    public ReviewType ReviewType { get; set; }
    public ReviewStatus Status { get; set; } = ReviewStatus.NotStarted;

    // Self assessment
    public string? SelfAssessment { get; set; }
    public decimal? SelfScore { get; set; }

    // Manager assessment
    public string? ManagerAssessment { get; set; }
    public decimal? ManagerScore { get; set; }

    // Final
    public decimal? OverallScore { get; set; }
    public string? Comments { get; set; }
    public string? ImprovementPlan { get; set; }
    public DateTime? ReviewDate { get; set; }
    public DateTime? DueDate { get; set; }

    // Concurrency
    public uint RowVersion { get; set; }

    // Navigation
    public Employee Employee { get; set; } = null!;
    public User Reviewer { get; set; } = null!;
    public ICollection<ReviewFeedback> Feedbacks { get; set; } = new List<ReviewFeedback>();
}
