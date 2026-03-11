using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.MasterData.Queries;

public class GetMasterDataQueryHandlers 
    : IRequestHandler<GetDepartmentsQuery, Result<PaginatedList<MasterDataDto>>>,
      IRequestHandler<GetPositionsQuery, Result<PaginatedList<MasterDataDto>>>,
      IRequestHandler<GetSkillsQuery, Result<PaginatedList<MasterDataDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public GetMasterDataQueryHandlers(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<PaginatedList<MasterDataDto>>> Handle(GetDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.DepartmentsList;
        var cached = await _cache.GetAsync<List<MasterDataDto>>(cacheKey, cancellationToken);
        
        List<MasterDataDto> allDepts;
        if (cached is not null)
        {
            allDepts = cached;
        }
        else
        {
            allDepts = await _unitOfWork.Departments.Query()
                .OrderBy(x => x.Name)
                .Select(x => new MasterDataDto(x.Id, x.Name, x.Description, !x.IsDeleted))
                .ToListAsync(cancellationToken);
            await _cache.SetAsync(cacheKey, allDepts, TimeSpan.FromHours(24), cancellationToken);
        }

        var totalCount = allDepts.Count;
        var items = allDepts
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return Result<PaginatedList<MasterDataDto>>.Success(
            new PaginatedList<MasterDataDto>(items, totalCount, request.PageNumber, request.PageSize));
    }

    public async Task<Result<PaginatedList<MasterDataDto>>> Handle(GetPositionsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.PositionsList;
        var cached = await _cache.GetAsync<List<MasterDataDto>>(cacheKey, cancellationToken);
        
        List<MasterDataDto> allPositions;
        if (cached is not null)
        {
            allPositions = cached;
        }
        else
        {
            allPositions = await _unitOfWork.Positions.Query()
                .OrderBy(x => x.Name)
                .Select(x => new MasterDataDto(x.Id, x.Name, x.Description, !x.IsDeleted))
                .ToListAsync(cancellationToken);
            await _cache.SetAsync(cacheKey, allPositions, TimeSpan.FromHours(24), cancellationToken);
        }

        var totalCount = allPositions.Count;
        var items = allPositions
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return Result<PaginatedList<MasterDataDto>>.Success(
            new PaginatedList<MasterDataDto>(items, totalCount, request.PageNumber, request.PageSize));
    }

    public async Task<Result<PaginatedList<MasterDataDto>>> Handle(GetSkillsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.SkillsList;
        var cached = await _cache.GetAsync<List<MasterDataDto>>(cacheKey, cancellationToken);
        
        List<MasterDataDto> allSkills;
        if (cached is not null)
        {
            allSkills = cached;
        }
        else
        {
            allSkills = await _unitOfWork.Skills.Query()
                .OrderBy(x => x.Name)
                .Select(x => new MasterDataDto(x.Id, x.Name, x.Description, !x.IsDeleted))
                .ToListAsync(cancellationToken);
            await _cache.SetAsync(cacheKey, allSkills, TimeSpan.FromHours(24), cancellationToken);
        }

        var totalCount = allSkills.Count;
        var items = allSkills
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return Result<PaginatedList<MasterDataDto>>.Success(
            new PaginatedList<MasterDataDto>(items, totalCount, request.PageNumber, request.PageSize));
    }
}
