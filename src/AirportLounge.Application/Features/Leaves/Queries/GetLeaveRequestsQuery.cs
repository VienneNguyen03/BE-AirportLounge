using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Leaves.Queries;

public record GetLeaveRequestsQuery(
    Guid? EmployeeId,
    LeaveRequestStatus? Status,
    DateTime? StartDate,
    DateTime? EndDate,
    string? Search,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<Result<PaginatedList<LeaveRequestDto>>>;

public record LeaveRequestDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    Guid LeaveTypeId,
    string LeaveTypeName,
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalDays,
    bool IsHalfDay,
    string? Reason,
    LeaveRequestStatus Status,
    string? DecisionReason,
    string? ReviewerComment,
    DateTime? ReviewedAt,
    DateTime CreatedAt);

public class GetLeaveRequestsQueryHandler : IRequestHandler<GetLeaveRequestsQuery, Result<PaginatedList<LeaveRequestDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public GetLeaveRequestsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<PaginatedList<LeaveRequestDto>>> Handle(
        GetLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.LeaveRequests(
            request.EmployeeId, request.Status?.ToString(), request.StartDate, request.EndDate, request.Search, request.PageNumber);

        var cached = await _cache.GetAsync<PaginatedList<LeaveRequestDto>>(cacheKey, cancellationToken);
        if (cached is not null)
            return Result<PaginatedList<LeaveRequestDto>>.Success(cached);

        var query = _unitOfWork.LeaveRequests.Query()
            .Include(lr => lr.Employee).ThenInclude(e => e.User)
            .Include(lr => lr.LeaveType)
            .Where(lr => !lr.IsDeleted)
            .AsQueryable();

        if (_currentUser.Role == "Staff")
        {
            var employee = await _unitOfWork.Employees.Query()
                .FirstOrDefaultAsync(e => e.UserId == _currentUser.UserId!.Value && !e.IsDeleted, cancellationToken);

            if (employee is null)
                return Result<PaginatedList<LeaveRequestDto>>.Failure("Employee profile not found");

            query = query.Where(lr => lr.EmployeeId == employee.Id);
        }
        else if (request.EmployeeId.HasValue)
        {
            query = query.Where(lr => lr.EmployeeId == request.EmployeeId.Value);
        }

        if (request.Status.HasValue)
            query = query.Where(lr => lr.Status == request.Status.Value);

        if (request.StartDate.HasValue)
            query = query.Where(lr => lr.StartDate >= request.StartDate.Value.Date);

        if (request.EndDate.HasValue)
            query = query.Where(lr => lr.EndDate <= request.EndDate.Value.Date);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(lr => lr.Employee.User.FullName.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(lr => lr.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(lr => new LeaveRequestDto(
                lr.Id,
                lr.EmployeeId,
                lr.Employee.User.FullName,
                lr.LeaveTypeId,
                lr.LeaveType.Name,
                lr.StartDate,
                lr.EndDate,
                lr.TotalDays,
                lr.IsHalfDay,
                lr.Reason,
                lr.Status,
                lr.DecisionReason,
                lr.ReviewerComment,
                lr.ReviewedAt,
                lr.CreatedAt))
            .ToListAsync(cancellationToken);

        var result = new PaginatedList<LeaveRequestDto>(items, totalCount, request.PageNumber, request.PageSize);
        await _cache.SetAsync(cacheKey, result, CacheKeys.LeaveRequestsTtl, cancellationToken);

        return Result<PaginatedList<LeaveRequestDto>>.Success(result);
    }
}
