using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Shifts.Queries;

// --- Get All Shifts (paginated, search) ---
public record GetShiftsQuery(
    string? Search,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<Result<PaginatedList<ShiftDto>>>;

public record ShiftDto(Guid Id, string Name, TimeSpan StartTime, TimeSpan EndTime, string? Description, DateTime CreatedAt);

public class GetShiftsQueryHandler : IRequestHandler<GetShiftsQuery, Result<PaginatedList<ShiftDto>>>
{
    private readonly IUnitOfWork _uow;
    public GetShiftsQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PaginatedList<ShiftDto>>> Handle(GetShiftsQuery req, CancellationToken ct)
    {
        var query = _uow.Shifts.Query().AsQueryable();

        if (!string.IsNullOrWhiteSpace(req.Search))
        {
            var search = req.Search.ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(search) ||
                (s.Description != null && s.Description.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(s => s.StartTime)
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(s => new ShiftDto(s.Id, s.Name, s.StartTime, s.EndTime, s.Description, s.CreatedAt))
            .ToListAsync(ct);

        return Result<PaginatedList<ShiftDto>>.Success(
            new PaginatedList<ShiftDto>(items, totalCount, req.PageNumber, req.PageSize));
    }
}

// --- Get Schedule by Date Range (paginated, search) ---
public record GetScheduleQuery(
    DateTime StartDate, DateTime EndDate, Guid? EmployeeId,
    string? Search,
    int PageNumber = 1, int PageSize = 10
) : IRequest<Result<PaginatedList<ScheduleItemDto>>>;

public record ScheduleItemDto(
    Guid AssignmentId,
    Guid ShiftId,
    Guid EmployeeId,
    Guid? LoungeZoneId,
    DateTime Date,
    string ShiftName,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string EmployeeName,
    string EmployeeCode,
    string? ZoneName);

public class GetScheduleQueryHandler : IRequestHandler<GetScheduleQuery, Result<PaginatedList<ScheduleItemDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;

    public GetScheduleQueryHandler(IUnitOfWork uow, ICacheService cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task<Result<PaginatedList<ScheduleItemDto>>> Handle(GetScheduleQuery req, CancellationToken ct)
    {
        var cacheKey = CacheKeys.ShiftSchedule(req.StartDate, req.EndDate, req.EmployeeId, req.Search, req.PageNumber);
        
        var cached = await _cache.GetAsync<PaginatedList<ScheduleItemDto>>(cacheKey, ct);
        if (cached is null)
        {
            var query = _uow.ShiftAssignments.Query()
                .Include(sa => sa.Shift)
                .Include(sa => sa.Employee).ThenInclude(e => e.User)
                .Include(sa => sa.LoungeZone)
                .Where(sa => sa.Date >= req.StartDate.Date && sa.Date <= req.EndDate.Date);

            if (req.EmployeeId.HasValue)
                query = query.Where(sa => sa.EmployeeId == req.EmployeeId.Value);

            if (!string.IsNullOrWhiteSpace(req.Search))
            {
                var search = req.Search.ToLower();
                query = query.Where(sa =>
                    sa.Employee.User.FullName.ToLower().Contains(search) ||
                    sa.Employee.EmployeeCode.ToLower().Contains(search) ||
                    (sa.LoungeZone != null && sa.LoungeZone.Name.ToLower().Contains(search)) ||
                    sa.Shift.Name.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderBy(sa => sa.Date).ThenBy(sa => sa.Shift.StartTime)
                .Skip((req.PageNumber - 1) * req.PageSize)
                .Take(req.PageSize)
                .Select(sa => new ScheduleItemDto(
                    sa.Id,
                    sa.ShiftId,
                    sa.EmployeeId,
                    sa.LoungeZoneId,
                    sa.Date,
                    sa.Shift.Name,
                    sa.Shift.StartTime,
                    sa.Shift.EndTime,
                    sa.Employee.User.FullName,
                    sa.Employee.EmployeeCode,
                    sa.LoungeZone != null ? sa.LoungeZone.Name : null))
                .ToListAsync(ct);

            cached = new PaginatedList<ScheduleItemDto>(items, totalCount, req.PageNumber, req.PageSize);
            await _cache.SetAsync(cacheKey, cached, CacheKeys.ShiftsTtl, ct);
        }

        return Result<PaginatedList<ScheduleItemDto>>.Success(cached);
    }
}
