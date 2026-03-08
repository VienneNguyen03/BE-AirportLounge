using AirportLounge.Domain.Common;

namespace AirportLounge.Domain.Entities;

public class ShiftAssignment : AuditableEntity
{
    public Guid ShiftId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public Guid? LoungeZoneId { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Shift Shift { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
    public LoungeZone? LoungeZone { get; set; }
    public Attendance? Attendance { get; set; }
}
