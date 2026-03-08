using AirportLounge.Domain.Common;
using AirportLounge.Domain.Enums;

namespace AirportLounge.Domain.Entities;

public class TaskItem : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; }
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
    public Guid? AssignedToId { get; set; }
    public Guid? LoungeZoneId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation
    public Employee? AssignedTo { get; set; }
    public LoungeZone? LoungeZone { get; set; }
}
