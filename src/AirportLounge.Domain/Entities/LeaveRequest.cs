using AirportLounge.Domain.Common;
using AirportLounge.Domain.Enums;

namespace AirportLounge.Domain.Entities;

public class LeaveRequest : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalDays { get; set; }
    public bool IsHalfDay { get; set; }
    public string? Reason { get; set; }
    public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Draft;
    public Guid? ManagerId { get; set; }
    public string? DecisionReason { get; set; }
    public Guid? ReviewedById { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewerComment { get; set; }

    /// <summary>
    /// Concurrency token for optimistic concurrency control.
    /// </summary>
    public uint RowVersion { get; set; }

    public Employee Employee { get; set; } = null!;
    public LeaveType LeaveType { get; set; } = null!;
    public User? ReviewedBy { get; set; }
    public ICollection<LeaveAttachment> Attachments { get; set; } = new List<LeaveAttachment>();
}
