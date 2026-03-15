using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using MediatR;

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
