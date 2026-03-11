using AirportLounge.Domain.Common;

namespace AirportLounge.Domain.Entities;

public class LeaveAttachment : AuditableEntity
{
    public Guid LeaveRequestId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;

    public LeaveRequest LeaveRequest { get; set; } = null!;
}
