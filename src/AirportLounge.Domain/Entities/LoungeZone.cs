using AirportLounge.Domain.Common;
using AirportLounge.Domain.Enums;

namespace AirportLounge.Domain.Entities;

public class LoungeZone : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public int CurrentOccupancy { get; set; }
    public ZoneStatus Status { get; set; } = ZoneStatus.Available;

    // Navigation
    public ICollection<ZoneStatusLog> StatusLogs { get; set; } = new List<ZoneStatusLog>();
    public ICollection<ShiftAssignment> ShiftAssignments { get; set; } = new List<ShiftAssignment>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
