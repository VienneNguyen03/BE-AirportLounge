using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Leaves.Queries;

public record GetLeaveTypesQuery(int PageNumber = 1, int PageSize = 10) : IRequest<Result<PaginatedList<LeaveTypeDto>>>;

public record LeaveTypeDto(
    Guid Id,
    string Name,
    string? Description,
    int DefaultDaysPerYear,
    bool RequiresDocumentation,
    bool IsActive);

public class GetLeaveTypesQueryHandler : IRequestHandler<GetLeaveTypesQuery, Result<PaginatedList<LeaveTypeDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public GetLeaveTypesQueryHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<PaginatedList<LeaveTypeDto>>> Handle(GetLeaveTypesQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.LeaveTypesList;
        var cached = await _cache.GetAsync<List<LeaveTypeDto>>(cacheKey, cancellationToken);
        
        List<LeaveTypeDto> allTypes;
        if (cached is not null)
        {
            allTypes = cached;
        }
        else
        {
            allTypes = await _unitOfWork.LeaveTypes.Query()
                .Where(lt => !lt.IsDeleted)
                .OrderBy(lt => lt.Name)
                .Select(lt => new LeaveTypeDto(
                    lt.Id,
                    lt.Name,
                    lt.Description,
                    lt.DefaultDaysPerYear,
                    lt.RequiresDocumentation,
                    lt.IsActive))
                .ToListAsync(cancellationToken);
            await _cache.SetAsync(cacheKey, allTypes, TimeSpan.FromHours(24), cancellationToken);
        }

        var totalCount = allTypes.Count;
        var items = allTypes
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return Result<PaginatedList<LeaveTypeDto>>.Success(
            new PaginatedList<LeaveTypeDto>(items, totalCount, request.PageNumber, request.PageSize));
    }
}
