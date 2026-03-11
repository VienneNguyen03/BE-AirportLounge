using AirportLounge.Domain.Common;

namespace AirportLounge.Domain.Entities;

public class LeaveBalance : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public int Year { get; set; }
    public decimal TotalDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal ReservedDays { get; set; }
    public decimal RemainingDays => TotalDays - UsedDays - ReservedDays;

    public Employee Employee { get; set; } = null!;
    public LeaveType LeaveType { get; set; } = null!;
}
