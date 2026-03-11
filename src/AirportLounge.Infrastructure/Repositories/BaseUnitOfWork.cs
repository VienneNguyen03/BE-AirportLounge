using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Interfaces;
using AirportLounge.Persistence;

namespace AirportLounge.Infrastructure.Repositories;

public class BaseUnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    private IRepository<User>? _users;
    private IRepository<Employee>? _employees;
    private IRepository<Department>? _departments;
    private IRepository<Position>? _positions;
    private IRepository<Skill>? _skills;
    private IRepository<Shift>? _shifts;
    private IRepository<ShiftAssignment>? _shiftAssignments;
    private IRepository<Attendance>? _attendances;
    private IRepository<LoungeZone>? _loungeZones;
    private IRepository<ZoneStatusLog>? _zoneStatusLogs;
    private IRepository<TaskItem>? _taskItems;
    private IRepository<Notification>? _notifications;
    private IRepository<AuditLog>? _auditLogs;
    private IRepository<LeaveType>? _leaveTypes;
    private IRepository<LeaveBalance>? _leaveBalances;
    private IRepository<LeaveRequest>? _leaveRequests;
    private IRepository<LeaveAttachment>? _leaveAttachments;
    private IRepository<SalaryStructure>? _salaryStructures;
    private IRepository<PayrollRecord>? _payrollRecords;
    private IRepository<PerformanceGoal>? _performanceGoals;
    private IRepository<PerformanceReview>? _performanceReviews;
    private IRepository<ReviewFeedback>? _reviewFeedbacks;
    private IRepository<TrainingCourse>? _trainingCourses;
    private IRepository<TrainingEnrollment>? _trainingEnrollments;
    private IRepository<OnboardingProcess>? _onboardingProcesses;
    private IRepository<OnboardingTask>? _onboardingTasks;
    private IRepository<OffboardingProcess>? _offboardingProcesses;
    private IRepository<OffboardingTask>? _offboardingTasks;
    private IRepository<EmployeeIdCard>? _employeeIdCards;
    private IRepository<IdCardEvent>? _idCardEvents;
    private IRepository<IdCardTemplate>? _idCardTemplates;
    private IRepository<EmployeeDocument>? _employeeDocuments;

    public BaseUnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepository<User> Users => _users ??= new BaseRepository<User>(_context);
    public IRepository<Employee> Employees => _employees ??= new BaseRepository<Employee>(_context);
    public IRepository<Department> Departments => _departments ??= new BaseRepository<Department>(_context);
    public IRepository<Position> Positions => _positions ??= new BaseRepository<Position>(_context);
    public IRepository<Skill> Skills => _skills ??= new BaseRepository<Skill>(_context);
    public IRepository<Shift> Shifts => _shifts ??= new BaseRepository<Shift>(_context);
    public IRepository<ShiftAssignment> ShiftAssignments => _shiftAssignments ??= new BaseRepository<ShiftAssignment>(_context);
    public IRepository<Attendance> Attendances => _attendances ??= new BaseRepository<Attendance>(_context);
    public IRepository<LoungeZone> LoungeZones => _loungeZones ??= new BaseRepository<LoungeZone>(_context);
    public IRepository<ZoneStatusLog> ZoneStatusLogs => _zoneStatusLogs ??= new BaseRepository<ZoneStatusLog>(_context);
    public IRepository<TaskItem> TaskItems => _taskItems ??= new BaseRepository<TaskItem>(_context);
    public IRepository<Notification> Notifications => _notifications ??= new BaseRepository<Notification>(_context);
    public IRepository<AuditLog> AuditLogs => _auditLogs ??= new BaseRepository<AuditLog>(_context);
    public IRepository<LeaveType> LeaveTypes => _leaveTypes ??= new BaseRepository<LeaveType>(_context);
    public IRepository<LeaveBalance> LeaveBalances => _leaveBalances ??= new BaseRepository<LeaveBalance>(_context);
    public IRepository<LeaveRequest> LeaveRequests => _leaveRequests ??= new BaseRepository<LeaveRequest>(_context);
    public IRepository<LeaveAttachment> LeaveAttachments => _leaveAttachments ??= new BaseRepository<LeaveAttachment>(_context);
    public IRepository<SalaryStructure> SalaryStructures => _salaryStructures ??= new BaseRepository<SalaryStructure>(_context);
    public IRepository<PayrollRecord> PayrollRecords => _payrollRecords ??= new BaseRepository<PayrollRecord>(_context);
    public IRepository<PerformanceGoal> PerformanceGoals => _performanceGoals ??= new BaseRepository<PerformanceGoal>(_context);
    public IRepository<PerformanceReview> PerformanceReviews => _performanceReviews ??= new BaseRepository<PerformanceReview>(_context);
    public IRepository<ReviewFeedback> ReviewFeedbacks => _reviewFeedbacks ??= new BaseRepository<ReviewFeedback>(_context);
    public IRepository<TrainingCourse> TrainingCourses => _trainingCourses ??= new BaseRepository<TrainingCourse>(_context);
    public IRepository<TrainingEnrollment> TrainingEnrollments => _trainingEnrollments ??= new BaseRepository<TrainingEnrollment>(_context);
    public IRepository<OnboardingProcess> OnboardingProcesses => _onboardingProcesses ??= new BaseRepository<OnboardingProcess>(_context);
    public IRepository<OnboardingTask> OnboardingTasks => _onboardingTasks ??= new BaseRepository<OnboardingTask>(_context);
    public IRepository<OffboardingProcess> OffboardingProcesses => _offboardingProcesses ??= new BaseRepository<OffboardingProcess>(_context);
    public IRepository<OffboardingTask> OffboardingTasks => _offboardingTasks ??= new BaseRepository<OffboardingTask>(_context);
    public IRepository<EmployeeIdCard> EmployeeIdCards => _employeeIdCards ??= new BaseRepository<EmployeeIdCard>(_context);
    public IRepository<IdCardEvent> IdCardEvents => _idCardEvents ??= new BaseRepository<IdCardEvent>(_context);
    public IRepository<IdCardTemplate> IdCardTemplates => _idCardTemplates ??= new BaseRepository<IdCardTemplate>(_context);
    public IRepository<EmployeeDocument> EmployeeDocuments => _employeeDocuments ??= new BaseRepository<EmployeeDocument>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
