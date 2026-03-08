using AirportLounge.Domain.Common;
using AirportLounge.Domain.Enums;

namespace AirportLounge.Domain.Entities;

public class Attendance : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Guid ShiftAssignmentId { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public double? WorkedHours { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Notes { get; set; }
    public bool IsManuallyAdjusted { get; set; }
    public bool IsConfirmed { get; set; }

    // Navigation
    public Employee Employee { get; set; } = null!;
    public ShiftAssignment ShiftAssignment { get; set; } = null!;
}
