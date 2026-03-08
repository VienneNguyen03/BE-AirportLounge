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
    string? Reason,
    LeaveRequestStatus Status,
    string? ReviewerComment,
    DateTime? ReviewedAt,
    DateTime CreatedAt);

public class GetLeaveRequestsQueryHandler : IRequestHandler<GetLeaveRequestsQuery, Result<PaginatedList<LeaveRequestDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetLeaveRequestsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedList<LeaveRequestDto>>> Handle(
        GetLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
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
                lr.Reason,
                lr.Status,
                lr.ReviewerComment,
                lr.ReviewedAt,
                lr.CreatedAt))
            .ToListAsync(cancellationToken);

        var result = new PaginatedList<LeaveRequestDto>(items, totalCount, request.PageNumber, request.PageSize);
        return Result<PaginatedList<LeaveRequestDto>>.Success(result);
    }
}
