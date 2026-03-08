using AirportLounge.Domain.Common;
using AirportLounge.Domain.Enums;

namespace AirportLounge.Domain.Entities;

public class Employee : AuditableEntity
{
    public Guid UserId { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string? Skills { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime HireDate { get; set; }

    public string? NationalId { get; set; }
    public string? Nationality { get; set; }
    public Gender? Gender { get; set; }
    public MaritalStatus? MaritalStatus { get; set; }
    public string? PermanentAddress { get; set; }
    public string? TemporaryAddress { get; set; }
    public string? TaxCode { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? BloodType { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelationship { get; set; }
    public string? ProfilePhotoUrl { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<ShiftAssignment> ShiftAssignments { get; set; } = new List<ShiftAssignment>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
    public ICollection<PerformanceGoal> PerformanceGoals { get; set; } = new List<PerformanceGoal>();
    public ICollection<PerformanceReview> PerformanceReviews { get; set; } = new List<PerformanceReview>();
    public ICollection<TrainingEnrollment> TrainingEnrollments { get; set; } = new List<TrainingEnrollment>();
    public ICollection<EmployeeDocument> Documents { get; set; } = new List<EmployeeDocument>();
    public ICollection<EmployeeIdCard> IdCards { get; set; } = new List<EmployeeIdCard>();
    public SalaryStructure? SalaryStructure { get; set; }
}
