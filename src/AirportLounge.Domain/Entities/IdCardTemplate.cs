using AirportLounge.Domain.Common;

namespace AirportLounge.Domain.Entities;

public class IdCardTemplate : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string LayoutJson { get; set; } = "{}";
    public bool IsActive { get; set; } = true;

    public ICollection<EmployeeIdCard> Cards { get; set; } = new List<EmployeeIdCard>();
}
