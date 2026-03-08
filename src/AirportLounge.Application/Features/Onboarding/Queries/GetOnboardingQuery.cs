using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Onboarding.Queries;

public record OnboardingTaskDto(
    Guid Id, string Title, string? Description, string? AssignedTo,
    bool IsCompleted, DateTime? CompletedAt, DateTime? DueDate, int SortOrder);

public record OnboardingDto(
    Guid Id, Guid EmployeeId, string EmployeeName, OnboardingStatus Status,
    DateTime StartDate, DateTime? CompletedDate, string? MentorName,
    List<OnboardingTaskDto> Tasks);

public record GetOnboardingQuery(Guid EmployeeId) : IRequest<Result<OnboardingDto>>;

public class GetOnboardingQueryHandler : IRequestHandler<GetOnboardingQuery, Result<OnboardingDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;

    public GetOnboardingQueryHandler(IUnitOfWork uow, ICacheService cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task<Result<OnboardingDto>> Handle(GetOnboardingQuery req, CancellationToken ct)
    {
        var cacheKey = CacheKeys.Onboarding(req.EmployeeId);
        var cached = await _cache.GetAsync<OnboardingDto>(cacheKey, ct);
        if (cached is not null)
            return Result<OnboardingDto>.Success(cached);

        var process = await _uow.OnboardingProcesses.Query()
            .Include(o => o.Employee).ThenInclude(e => e.User)
            .Include(o => o.AssignedMentor).ThenInclude(m => m!.User)
            .Include(o => o.Tasks).ThenInclude(t => t.AssignedTo)
            .Where(o => o.EmployeeId == req.EmployeeId)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (process is null)
            return Result<OnboardingDto>.Failure("No onboarding process found for this employee");

        var dto = new OnboardingDto(
            process.Id,
            process.EmployeeId,
            process.Employee.User.FullName,
            process.Status,
            process.StartDate,
            process.CompletedDate,
            process.AssignedMentor?.User.FullName,
            process.Tasks.OrderBy(t => t.SortOrder).Select(t => new OnboardingTaskDto(
                t.Id, t.Title, t.Description, t.AssignedTo?.FullName,
                t.IsCompleted, t.CompletedAt, t.DueDate, t.SortOrder
            )).ToList());

        await _cache.SetAsync(cacheKey, dto, CacheKeys.OnboardingTtl, ct);
        return Result<OnboardingDto>.Success(dto);
    }
}
