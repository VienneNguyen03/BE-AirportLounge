using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.Leaves.Queries;

public record GetLeaveTypesQuery(int PageNumber = 1, int PageSize = 10) : IRequest<Result<PaginatedList<LeaveTypeDto>>>;

public record LeaveTypeDto(
    Guid Id,
    string Name,
    string? Description,
    int DefaultDaysPerYear,
    bool RequiresDocumentation,
    bool IsActive);
