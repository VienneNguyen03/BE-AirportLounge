using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using MediatR;

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
