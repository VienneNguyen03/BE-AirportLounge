using AirportLounge.Domain.Common;
using AirportLounge.Domain.Enums;

namespace AirportLounge.Domain.Entities;

public class EmployeeIdCard : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public string? TemplateType { get; set; }
    public IdCardStatus Status { get; set; } = IdCardStatus.Active;
    public DateTime IssuedAt { get; set; }
    public Guid IssuedById { get; set; }
    public DateTime? RevokedAt { get; set; }
    public Guid? RevokedById { get; set; }
    public string? RevokeReason { get; set; }
    public string? QrCodeData { get; set; }

    public Employee Employee { get; set; } = null!;
    public User IssuedBy { get; set; } = null!;
}
