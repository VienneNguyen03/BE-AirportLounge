using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Offboarding.Queries;

public record OffboardingDto(
    Guid Id, Guid EmployeeId, string EmployeeName, OffboardingStatus Status,
    DateTime ResignationDate, DateTime LastWorkingDate, string? Reason,
    bool ExitSurveyCompleted, bool AssetReturned, bool AccessRevoked);

public record GetOffboardingQuery(Guid EmployeeId) : IRequest<Result<OffboardingDto>>;

public class GetOffboardingQueryHandler : IRequestHandler<GetOffboardingQuery, Result<OffboardingDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;

    public GetOffboardingQueryHandler(IUnitOfWork uow, ICacheService cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task<Result<OffboardingDto>> Handle(GetOffboardingQuery req, CancellationToken ct)
    {
        var cacheKey = CacheKeys.Offboarding(req.EmployeeId);
        var cached = await _cache.GetAsync<OffboardingDto>(cacheKey, ct);
        if (cached is not null)
            return Result<OffboardingDto>.Success(cached);

        var process = await _uow.OffboardingProcesses.Query()
            .Include(o => o.Employee).ThenInclude(e => e.User)
            .Where(o => o.EmployeeId == req.EmployeeId)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (process is null)
            return Result<OffboardingDto>.Failure("No offboarding process found for this employee");

        var dto = new OffboardingDto(
            process.Id,
            process.EmployeeId,
            process.Employee.User.FullName,
            process.Status,
            process.ResignationDate,
            process.LastWorkingDate,
            process.Reason,
            process.ExitSurveyCompleted,
            process.AssetReturned,
            process.AccessRevoked);

        await _cache.SetAsync(cacheKey, dto, CacheKeys.OnboardingTtl, ct);
        return Result<OffboardingDto>.Success(dto);
    }
}
