using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.Leaves.Commands;

public record ConfigureLeaveBalanceCommand(
    Guid EmployeeId,
    Guid LeaveTypeId,
    int Year,
    decimal TotalDays) : IRequest<Result<Guid>>;
