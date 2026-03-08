using AirportLounge.Domain.Common;
using AirportLounge.Domain.Enums;

namespace AirportLounge.Domain.Entities;

public class EmployeeDocument : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DocumentCategory Category { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? ContentType { get; set; }
    public Guid UploadedById { get; set; }
    public bool IsConfidential { get; set; }

    public Employee Employee { get; set; } = null!;
    public User UploadedBy { get; set; } = null!;
}
