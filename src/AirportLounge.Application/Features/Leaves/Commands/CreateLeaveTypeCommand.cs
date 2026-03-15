using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.Leaves.Commands;

public record CreateLeaveTypeCommand(
    string Name,
    string? Description,
    int DefaultDaysPerYear,
    bool RequiresDocumentation) : IRequest<Result<Guid>>;
