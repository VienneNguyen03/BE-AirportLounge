using AirportLounge.Domain.Common;
using AirportLounge.Domain.Enums;

namespace AirportLounge.Domain.Entities;

public class OnboardingProcess : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public OnboardingStatus Status { get; set; } = OnboardingStatus.InProgress;
    public DateTime StartDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public Guid? AssignedMentorId { get; set; }

    public Employee Employee { get; set; } = null!;
    public Employee? AssignedMentor { get; set; }
    public ICollection<OnboardingTask> Tasks { get; set; } = new List<OnboardingTask>();
}
