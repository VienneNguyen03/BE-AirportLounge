using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Shifts.Queries;

// --- Get All Shifts ---
public record GetShiftsQuery : IRequest<Result<List<ShiftDto>>>;

public record ShiftDto(Guid Id, string Name, TimeSpan StartTime, TimeSpan EndTime, string? Description, DateTime CreatedAt);

public class GetShiftsQueryHandler : IRequestHandler<GetShiftsQuery, Result<List<ShiftDto>>>
{
    private readonly IUnitOfWork _uow;
    public GetShiftsQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<ShiftDto>>> Handle(GetShiftsQuery req, CancellationToken ct)
    {
        var shifts = await _uow.Shifts.Query()
            .OrderBy(s => s.StartTime)
            .Select(s => new ShiftDto(s.Id, s.Name, s.StartTime, s.EndTime, s.Description, s.CreatedAt))
            .ToListAsync(ct);

        return Result<List<ShiftDto>>.Success(shifts);
    }
}

// --- Get Schedule by Date Range ---
public record GetScheduleQuery(DateTime StartDate, DateTime EndDate, Guid? EmployeeId)
    : IRequest<Result<List<ScheduleItemDto>>>;

public record ScheduleItemDto(
    Guid AssignmentId, DateTime Date, string ShiftName,
    TimeSpan StartTime, TimeSpan EndTime,
    string EmployeeName, string? ZoneName);

public class GetScheduleQueryHandler : IRequestHandler<GetScheduleQuery, Result<List<ScheduleItemDto>>>
{
    private readonly IUnitOfWork _uow;
    public GetScheduleQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<ScheduleItemDto>>> Handle(GetScheduleQuery req, CancellationToken ct)
    {
        var query = _uow.ShiftAssignments.Query()
            .Include(sa => sa.Shift)
            .Include(sa => sa.Employee).ThenInclude(e => e.User)
            .Include(sa => sa.LoungeZone)
            .Where(sa => sa.Date >= req.StartDate.Date && sa.Date <= req.EndDate.Date);

        if (req.EmployeeId.HasValue)
            query = query.Where(sa => sa.EmployeeId == req.EmployeeId.Value);

        var items = await query
            .OrderBy(sa => sa.Date).ThenBy(sa => sa.Shift.StartTime)
            .Select(sa => new ScheduleItemDto(
                sa.Id, sa.Date, sa.Shift.Name,
                sa.Shift.StartTime, sa.Shift.EndTime,
                sa.Employee.User.FullName, sa.LoungeZone != null ? sa.LoungeZone.Name : null))
            .ToListAsync(ct);

        return Result<List<ScheduleItemDto>>.Success(items);
    }
}
