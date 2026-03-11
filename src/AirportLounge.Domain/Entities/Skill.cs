using AirportLounge.Domain.Common;

namespace AirportLounge.Domain.Entities;

public class Skill : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
