using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Leaves.Queries;

public record GetLeaveTypesQuery : IRequest<Result<List<LeaveTypeDto>>>;

public record LeaveTypeDto(
    Guid Id,
    string Name,
    string? Description,
    int DefaultDaysPerYear,
    bool RequiresDocumentation,
    bool IsActive);

public class GetLeaveTypesQueryHandler : IRequestHandler<GetLeaveTypesQuery, Result<List<LeaveTypeDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public GetLeaveTypesQueryHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<List<LeaveTypeDto>>> Handle(GetLeaveTypesQuery request, CancellationToken cancellationToken)
    {
        var cached = await _cache.GetAsync<List<LeaveTypeDto>>(CacheKeys.LeaveTypesList, cancellationToken);
        if (cached is not null)
            return Result<List<LeaveTypeDto>>.Success(cached);

        var leaveTypes = await _unitOfWork.LeaveTypes.Query()
            .Where(lt => lt.IsActive && !lt.IsDeleted)
            .OrderBy(lt => lt.Name)
            .Select(lt => new LeaveTypeDto(
                lt.Id,
                lt.Name,
                lt.Description,
                lt.DefaultDaysPerYear,
                lt.RequiresDocumentation,
                lt.IsActive))
            .ToListAsync(cancellationToken);

        await _cache.SetAsync(CacheKeys.LeaveTypesList, leaveTypes, CacheKeys.LeaveTypesTtl, cancellationToken);

        return Result<List<LeaveTypeDto>>.Success(leaveTypes);
    }
}
