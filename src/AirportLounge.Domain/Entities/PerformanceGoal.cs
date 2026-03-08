using AirportLounge.Domain.Common;
using AirportLounge.Domain.Enums;

namespace AirportLounge.Domain.Entities;

public class PerformanceGoal : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TargetValue { get; set; }
    public decimal CurrentValue { get; set; }
    public string? Unit { get; set; }
    public DateTime DueDate { get; set; }
    public GoalStatus Status { get; set; } = GoalStatus.NotStarted;

    public Employee Employee { get; set; } = null!;
}
