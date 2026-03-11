using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Leaves.Queries;

public record GetPendingLeaveRequestsQuery : IRequest<Result<List<PendingLeaveRequestDto>>>;

public record PendingLeaveRequestDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    string? Department,
    Guid LeaveTypeId,
    string LeaveTypeName,
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalDays,
    bool IsHalfDay,
    string? Reason,
    LeaveRequestStatus Status,
    DateTime SubmittedAt);

public class GetPendingLeaveRequestsQueryHandler
    : IRequestHandler<GetPendingLeaveRequestsQuery, Result<List<PendingLeaveRequestDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public GetPendingLeaveRequestsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<List<PendingLeaveRequestDto>>> Handle(
        GetPendingLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role is not ("Admin" or "Manager"))
            return Result<List<PendingLeaveRequestDto>>.Failure("Only administrators and managers can view pending leave requests");

        var cached = await _cache.GetAsync<List<PendingLeaveRequestDto>>(
            CacheKeys.LeaveRequestsPending, cancellationToken);
        if (cached is not null)
            return Result<List<PendingLeaveRequestDto>>.Success(cached);

        // Show Submitted and UnderReview requests to managers
        var pendingStatuses = new[]
        {
            LeaveRequestStatus.Submitted,
            LeaveRequestStatus.UnderReview,
            LeaveRequestStatus.NeedsInfo
        };

        var pendingRequests = await _unitOfWork.LeaveRequests.Query()
            .Include(lr => lr.Employee).ThenInclude(e => e.User)
            .Include(lr => lr.Employee).ThenInclude(e => e.Department)
            .Include(lr => lr.LeaveType)
            .Where(lr => pendingStatuses.Contains(lr.Status) && !lr.IsDeleted)
            .OrderBy(lr => lr.StartDate)
            .Select(lr => new PendingLeaveRequestDto(
                lr.Id,
                lr.EmployeeId,
                lr.Employee.User.FullName,
                lr.Employee.Department != null ? lr.Employee.Department.Name : null,
                lr.LeaveTypeId,
                lr.LeaveType.Name,
                lr.StartDate,
                lr.EndDate,
                lr.TotalDays,
                lr.IsHalfDay,
                lr.Reason,
                lr.Status,
                lr.CreatedAt))
            .ToListAsync(cancellationToken);

        await _cache.SetAsync(CacheKeys.LeaveRequestsPending, pendingRequests, CacheKeys.LeaveRequestsTtl, cancellationToken);

        return Result<List<PendingLeaveRequestDto>>.Success(pendingRequests);
    }
}
