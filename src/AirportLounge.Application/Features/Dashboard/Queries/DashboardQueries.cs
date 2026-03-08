using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Dashboard.Queries;

// --- Staff Dashboard ---
public record GetStaffDashboardQuery(Guid EmployeeId) : IRequest<Result<StaffDashboardDto>>;

public record StaffDashboardDto(
    TodayShiftDto? TodayShift,
    List<ActiveTaskDto> ActiveTasks,
    int UnreadNotifications,
    decimal RemainingLeaveDays,
    int ActiveTrainingCourses,
    decimal? LatestReviewScore);

public record TodayShiftDto(string ShiftName, TimeSpan StartTime, TimeSpan EndTime,
    string? ZoneName, bool IsCheckedIn);

public record ActiveTaskDto(Guid Id, string Title, string Priority, string Status);

public class GetStaffDashboardQueryHandler : IRequestHandler<GetStaffDashboardQuery, Result<StaffDashboardDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;

    public GetStaffDashboardQueryHandler(IUnitOfWork uow, ICacheService cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task<Result<StaffDashboardDto>> Handle(GetStaffDashboardQuery req, CancellationToken ct)
    {
        var cacheKey = CacheKeys.StaffDashboard(req.EmployeeId);
        var cached = await _cache.GetAsync<StaffDashboardDto>(cacheKey, ct);
        if (cached is not null)
            return Result<StaffDashboardDto>.Success(cached);

        var today = DateTime.UtcNow.Date;
        var year = today.Year;

        var todayAssignment = await _uow.ShiftAssignments.Query()
            .Include(sa => sa.Shift).Include(sa => sa.LoungeZone)
            .Where(sa => sa.EmployeeId == req.EmployeeId && sa.Date.Date == today)
            .FirstOrDefaultAsync(ct);

        TodayShiftDto? todayShift = null;
        if (todayAssignment != null)
        {
            var isCheckedIn = await _uow.Attendances.Query()
                .AnyAsync(a => a.ShiftAssignmentId == todayAssignment.Id && a.CheckInTime != null, ct);

            todayShift = new TodayShiftDto(
                todayAssignment.Shift.Name,
                todayAssignment.Shift.StartTime,
                todayAssignment.Shift.EndTime,
                todayAssignment.LoungeZone?.Name,
                isCheckedIn);
        }

        var tasks = await _uow.TaskItems.Query()
            .Where(t => t.AssignedToId == req.EmployeeId &&
                         (t.Status == TaskItemStatus.Pending || t.Status == TaskItemStatus.InProgress))
            .OrderByDescending(t => t.Priority)
            .Take(5)
            .Select(t => new ActiveTaskDto(t.Id, t.Title, t.Priority.ToString(), t.Status.ToString()))
            .ToListAsync(ct);

        var unread = await _uow.Notifications.Query()
            .CountAsync(n => n.RecipientId == req.EmployeeId && !n.IsRead, ct);

        var balances = await _uow.LeaveBalances.Query()
            .Where(lb => lb.EmployeeId == req.EmployeeId && lb.Year == year)
            .ToListAsync(ct);
        var remainingLeave = balances.Sum(b => b.TotalDays - b.UsedDays);

        var activeTraining = await _uow.TrainingEnrollments.CountAsync(
            te => te.EmployeeId == req.EmployeeId &&
                  (te.Status == EnrollmentStatus.Enrolled || te.Status == EnrollmentStatus.InProgress), ct);

        var latestReview = await _uow.PerformanceReviews.Query()
            .Where(r => r.EmployeeId == req.EmployeeId && r.Status == ReviewStatus.Completed)
            .OrderByDescending(r => r.ReviewDate)
            .Select(r => r.OverallScore)
            .FirstOrDefaultAsync(ct);

        var dto = new StaffDashboardDto(todayShift, tasks, unread, remainingLeave, activeTraining, latestReview);
        await _cache.SetAsync(cacheKey, dto, CacheKeys.StaffDashboardTtl, ct);
        return Result<StaffDashboardDto>.Success(dto);
    }
}

// --- Manager Dashboard ---
public record GetManagerDashboardQuery() : IRequest<Result<ManagerDashboardDto>>;

public record ManagerDashboardDto(
    int ActiveStaffToday, int TotalEmployees,
    List<UnderstaffedShiftDto> UnderstaffedShifts,
    int PendingTasks, int InProgressTasks,
    int PendingLeaveRequests, int ActiveOnboardings);

public record UnderstaffedShiftDto(string ShiftName, DateTime Date, int AssignedCount);

public class GetManagerDashboardQueryHandler : IRequestHandler<GetManagerDashboardQuery, Result<ManagerDashboardDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;

    public GetManagerDashboardQueryHandler(IUnitOfWork uow, ICacheService cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task<Result<ManagerDashboardDto>> Handle(GetManagerDashboardQuery req, CancellationToken ct)
    {
        var cached = await _cache.GetAsync<ManagerDashboardDto>(CacheKeys.ManagerDashboard, ct);
        if (cached is not null)
            return Result<ManagerDashboardDto>.Success(cached);

        var today = DateTime.UtcNow.Date;

        var activeStaff = await _uow.Attendances.Query()
            .Where(a => a.ShiftAssignment.Date.Date == today && a.CheckInTime != null && a.CheckOutTime == null)
            .Select(a => a.EmployeeId).Distinct().CountAsync(ct);

        var totalEmployees = await _uow.Employees.CountAsync(cancellationToken: ct);
        var pendingTasks = await _uow.TaskItems.CountAsync(t => t.Status == TaskItemStatus.Pending, ct);
        var inProgressTasks = await _uow.TaskItems.CountAsync(t => t.Status == TaskItemStatus.InProgress, ct);

        var pendingLeave = await _uow.LeaveRequests.CountAsync(
            lr => lr.Status == LeaveRequestStatus.Pending, ct);
        var activeOnboardings = await _uow.OnboardingProcesses.CountAsync(
            o => o.Status == OnboardingStatus.InProgress, ct);

        var weekEnd = today.AddDays(7);
        var understaffed = await _uow.ShiftAssignments.Query()
            .Include(sa => sa.Shift)
            .Where(sa => sa.Date >= today && sa.Date <= weekEnd)
            .GroupBy(sa => new { sa.ShiftId, sa.Date, Name = sa.Shift!.Name })
            .Where(g => g.Count() < 2)
            .Select(g => new UnderstaffedShiftDto(g.Key.Name, g.Key.Date, g.Count()))
            .ToListAsync(ct);

        var dto = new ManagerDashboardDto(activeStaff, totalEmployees, understaffed,
            pendingTasks, inProgressTasks, pendingLeave, activeOnboardings);
        await _cache.SetAsync(CacheKeys.ManagerDashboard, dto, CacheKeys.DashboardTtl, ct);
        return Result<ManagerDashboardDto>.Success(dto);
    }
}

// --- Admin Dashboard ---
public record GetAdminDashboardQuery() : IRequest<Result<AdminDashboardDto>>;

public record AdminDashboardDto(int TotalUsers, int ActiveUsers, int TotalZones,
    Dictionary<string, int> UsersByRole,
    int PendingLeaveRequests, int ActiveOnboardings, int ActiveOffboardings,
    int TotalTrainingCourses);

public class GetAdminDashboardQueryHandler : IRequestHandler<GetAdminDashboardQuery, Result<AdminDashboardDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;

    public GetAdminDashboardQueryHandler(IUnitOfWork uow, ICacheService cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task<Result<AdminDashboardDto>> Handle(GetAdminDashboardQuery req, CancellationToken ct)
    {
        var cached = await _cache.GetAsync<AdminDashboardDto>(CacheKeys.AdminDashboard, ct);
        if (cached is not null)
            return Result<AdminDashboardDto>.Success(cached);

        var totalUsers = await _uow.Users.CountAsync(cancellationToken: ct);
        var activeUsers = await _uow.Users.CountAsync(u => u.IsActive, ct);
        var totalZones = await _uow.LoungeZones.CountAsync(cancellationToken: ct);

        var usersByRole = await _uow.Users.Query()
            .GroupBy(u => u.Role)
            .Select(g => new { Role = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Role, x => x.Count, ct);

        var pendingLeave = await _uow.LeaveRequests.CountAsync(
            lr => lr.Status == LeaveRequestStatus.Pending, ct);
        var activeOnboardings = await _uow.OnboardingProcesses.CountAsync(
            o => o.Status == OnboardingStatus.InProgress, ct);
        var activeOffboardings = await _uow.OffboardingProcesses.CountAsync(
            o => o.Status != OffboardingStatus.Completed, ct);
        var totalCourses = await _uow.TrainingCourses.CountAsync(
            c => c.IsActive, ct);

        var dto = new AdminDashboardDto(totalUsers, activeUsers, totalZones, usersByRole,
            pendingLeave, activeOnboardings, activeOffboardings, totalCourses);
        await _cache.SetAsync(CacheKeys.AdminDashboard, dto, CacheKeys.DashboardTtl, ct);
        return Result<AdminDashboardDto>.Success(dto);
    }
}
