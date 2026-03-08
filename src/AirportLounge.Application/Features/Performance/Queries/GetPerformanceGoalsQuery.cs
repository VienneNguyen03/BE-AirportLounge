using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Performance.Queries;

public record GetPerformanceGoalsQuery(Guid EmployeeId) : IRequest<Result<List<PerformanceGoalDto>>>;

public record PerformanceGoalDto(
    Guid Id,
    Guid EmployeeId,
    string Title,
    string? Description,
    decimal TargetValue,
    decimal CurrentValue,
    string? Unit,
    DateTime DueDate,
    GoalStatus Status,
    DateTime CreatedAt);

public class GetPerformanceGoalsQueryHandler : IRequestHandler<GetPerformanceGoalsQuery, Result<List<PerformanceGoalDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;

    public GetPerformanceGoalsQueryHandler(IUnitOfWork uow, ICacheService cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task<Result<List<PerformanceGoalDto>>> Handle(GetPerformanceGoalsQuery request, CancellationToken ct)
    {
        var cacheKey = CacheKeys.PerformanceGoals(request.EmployeeId);
        var cached = await _cache.GetAsync<List<PerformanceGoalDto>>(cacheKey, ct);
        if (cached is not null)
            return Result<List<PerformanceGoalDto>>.Success(cached);

        var goals = await _uow.PerformanceGoals.Query()
            .Where(g => g.EmployeeId == request.EmployeeId)
            .OrderByDescending(g => g.CreatedAt)
            .Select(g => new PerformanceGoalDto(
                g.Id, g.EmployeeId, g.Title, g.Description,
                g.TargetValue, g.CurrentValue, g.Unit,
                g.DueDate, g.Status, g.CreatedAt))
            .ToListAsync(ct);

        await _cache.SetAsync(cacheKey, goals, CacheKeys.PerformanceTtl, ct);
        return Result<List<PerformanceGoalDto>>.Success(goals);
    }
}
