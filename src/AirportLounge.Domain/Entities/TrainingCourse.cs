using AirportLounge.Domain.Common;

namespace AirportLounge.Domain.Entities;

public class TrainingCourse : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public int DurationInHours { get; set; }
    public string? ContentUrl { get; set; }
    public decimal PassingScore { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<TrainingEnrollment> Enrollments { get; set; } = new List<TrainingEnrollment>();
}
