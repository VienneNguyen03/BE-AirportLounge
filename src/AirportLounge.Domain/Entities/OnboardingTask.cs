using AirportLounge.Domain.Common;

namespace AirportLounge.Domain.Entities;

public class OnboardingTask : AuditableEntity
{
    public Guid ProcessId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? TaskCategoryId { get; set; }
    public Guid? AssignedToId { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Notes { get; set; }
    public int SortOrder { get; set; }

    public OnboardingProcess Process { get; set; } = null!;
    public TaskCategory? TaskCategory { get; set; }
    public User? AssignedTo { get; set; }
}
