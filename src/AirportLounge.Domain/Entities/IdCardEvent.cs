using AirportLounge.Domain.Common;
using AirportLounge.Domain.Enums;

namespace AirportLounge.Domain.Entities;

public class IdCardEvent : AuditableEntity
{
    public Guid CardId { get; set; }
    public IdCardStatus FromStatus { get; set; }
    public IdCardStatus ToStatus { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public Guid PerformedById { get; set; }
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

    public EmployeeIdCard Card { get; set; } = null!;
    public User PerformedBy { get; set; } = null!;
}
