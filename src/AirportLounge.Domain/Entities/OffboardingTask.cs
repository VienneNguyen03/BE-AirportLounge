using AirportLounge.Domain.Common;

namespace AirportLounge.Domain.Entities;

public class OffboardingTask : AuditableEntity
{
    public Guid ProcessId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int SortOrder { get; set; }

    public OffboardingProcess Process { get; set; } = null!;
}
