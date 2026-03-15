using AirportLounge.Domain.Common;
using AirportLounge.Domain.Enums;

namespace AirportLounge.Domain.Entities;

public class OffboardingProcess : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public OffboardingStatus Status { get; set; } = OffboardingStatus.Initiated;
    public DateTime ResignationDate { get; set; }
    public DateTime LastWorkingDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public bool ExitSurveyCompleted { get; set; }
    public bool AssetReturned { get; set; }
    public bool AccessRevoked { get; set; }
    public Guid? IdCardIdToRevoke { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Employee Employee { get; set; } = null!;
    public ICollection<OffboardingTask> Tasks { get; set; } = new List<OffboardingTask>();
}
