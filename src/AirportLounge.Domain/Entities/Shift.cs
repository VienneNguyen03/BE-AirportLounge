using AirportLounge.Domain.Common;

namespace AirportLounge.Domain.Entities;

public class Shift : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Description { get; set; }

    // Navigation
    public ICollection<ShiftAssignment> ShiftAssignments { get; set; } = new List<ShiftAssignment>();
}
