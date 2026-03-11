using AirportLounge.Domain.Entities;

namespace AirportLounge.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Employee> Employees { get; }
    IRepository<Department> Departments { get; }
    IRepository<Position> Positions { get; }
    IRepository<Skill> Skills { get; }
    IRepository<Shift> Shifts { get; }
    IRepository<ShiftAssignment> ShiftAssignments { get; }
    IRepository<Attendance> Attendances { get; }
    IRepository<LoungeZone> LoungeZones { get; }
    IRepository<ZoneStatusLog> ZoneStatusLogs { get; }
    IRepository<TaskItem> TaskItems { get; }
    IRepository<Notification> Notifications { get; }
    IRepository<AuditLog> AuditLogs { get; }
    IRepository<LeaveType> LeaveTypes { get; }
    IRepository<LeaveBalance> LeaveBalances { get; }
    IRepository<LeaveRequest> LeaveRequests { get; }
    IRepository<LeaveAttachment> LeaveAttachments { get; }
    IRepository<SalaryStructure> SalaryStructures { get; }
    IRepository<PayrollRecord> PayrollRecords { get; }
    IRepository<PerformanceGoal> PerformanceGoals { get; }
    IRepository<PerformanceReview> PerformanceReviews { get; }
    IRepository<ReviewFeedback> ReviewFeedbacks { get; }
    IRepository<TrainingCourse> TrainingCourses { get; }
    IRepository<TrainingEnrollment> TrainingEnrollments { get; }
    IRepository<OnboardingProcess> OnboardingProcesses { get; }
    IRepository<OnboardingTask> OnboardingTasks { get; }
    IRepository<OffboardingProcess> OffboardingProcesses { get; }
    IRepository<OffboardingTask> OffboardingTasks { get; }
    IRepository<EmployeeIdCard> EmployeeIdCards { get; }
    IRepository<IdCardEvent> IdCardEvents { get; }
    IRepository<IdCardTemplate> IdCardTemplates { get; }
    IRepository<EmployeeDocument> EmployeeDocuments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
