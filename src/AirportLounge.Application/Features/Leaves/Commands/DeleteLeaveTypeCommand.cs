using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.Leaves.Commands;

public record DeleteLeaveTypeCommand(Guid LeaveTypeId) : IRequest<Result<bool>>;
