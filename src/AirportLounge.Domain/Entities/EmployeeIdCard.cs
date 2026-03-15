using AirportLounge.Domain.Common;
using AirportLounge.Domain.Enums;

namespace AirportLounge.Domain.Entities;

public class EmployeeIdCard : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public string? TemplateType { get; set; }
    public IdCardStatus Status { get; set; } = IdCardStatus.Issued;
    public DateTime? IssuedAt { get; set; }
    public Guid? IssuedById { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public Guid? RevokedById { get; set; }
    public string? RevokeReason { get; set; }
    public string? QrCodeData { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public Guid? ReplacedByCardId { get; set; }
    public Guid? TemplateId { get; set; }
    public uint RowVersion { get; set; }

    public Employee Employee { get; set; } = null!;
    public IdCardTemplate? Template { get; set; }
    public User? IssuedBy { get; set; }
    public EmployeeIdCard? ReplacedByCard { get; set; }
    public ICollection<IdCardEvent> Events { get; set; } = new List<IdCardEvent>();
}
