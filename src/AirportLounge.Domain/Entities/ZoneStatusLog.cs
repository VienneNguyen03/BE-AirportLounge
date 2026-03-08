using AirportLounge.Domain.Common;
using AirportLounge.Domain.Enums;

namespace AirportLounge.Domain.Entities;

public class ZoneStatusLog : BaseEntity
{
    public Guid LoungeZoneId { get; set; }
    public ZoneStatus PreviousStatus { get; set; }
    public ZoneStatus NewStatus { get; set; }
    public string? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public LoungeZone LoungeZone { get; set; } = null!;
}
