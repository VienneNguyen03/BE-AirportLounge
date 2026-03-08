using AirportLounge.Domain.Common;
using AirportLounge.Domain.Enums;

namespace AirportLounge.Domain.Entities;

public class TrainingEnrollment : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Guid CourseId { get; set; }
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Enrolled;
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public decimal? Score { get; set; }
    public string? CertificateUrl { get; set; }

    public Employee Employee { get; set; } = null!;
    public TrainingCourse Course { get; set; } = null!;
}
