using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.Leaves.Queries;

public record GetLeaveBalanceQuery(Guid EmployeeId, int Year) : IRequest<Result<List<LeaveBalanceDto>>>;

public record LeaveBalanceDto(
    Guid Id,
    Guid LeaveTypeId,
    string LeaveTypeName,
    int Year,
    decimal TotalDays,
    decimal UsedDays,
    decimal RemainingDays);
