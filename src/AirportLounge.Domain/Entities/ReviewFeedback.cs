using AirportLounge.Domain.Common;

namespace AirportLounge.Domain.Entities;

public class ReviewFeedback : AuditableEntity
{
    public Guid ReviewId { get; set; }
    public Guid FromStaffId { get; set; }
    public string? Comment { get; set; }
    public decimal? Score { get; set; }
    public bool IsAnonymous { get; set; }

    // Navigation
    public PerformanceReview Review { get; set; } = null!;
}
