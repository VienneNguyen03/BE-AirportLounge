using AirportLounge.Domain.Common;
using AirportLounge.Domain.Enums;

namespace AirportLounge.Domain.Entities;

public class PerformanceReview : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Guid ReviewerId { get; set; }
    public string Period { get; set; } = string.Empty;
    public ReviewType ReviewType { get; set; }
    public string? SelfAssessment { get; set; }
    public string? ManagerAssessment { get; set; }
    public decimal? OverallScore { get; set; }
    public ReviewStatus Status { get; set; } = ReviewStatus.Draft;
    public DateTime? ReviewDate { get; set; }
    public string? Comments { get; set; }
    public string? ImprovementPlan { get; set; }

    public Employee Employee { get; set; } = null!;
    public User Reviewer { get; set; } = null!;
}
