using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Attendance.Queries;

public record GetAttendanceReportQuery(
    DateTime StartDate, DateTime EndDate,
    Guid? EmployeeId, AttendanceStatus? Status,
    string? Search,
    int PageNumber = 1, int PageSize = 20
) : IRequest<Result<PaginatedList<AttendanceReportDto>>>;

public record AttendanceReportDto(
    Guid Id, string EmployeeName, string EmployeeCode,
    DateTime Date, string ShiftName, DateTime? CheckIn, DateTime? CheckOut,
    double? WorkedHours, string Status, bool IsManuallyAdjusted, bool IsConfirmed);

public class GetAttendanceReportQueryHandler
    : IRequestHandler<GetAttendanceReportQuery, Result<PaginatedList<AttendanceReportDto>>>
{
    private readonly IUnitOfWork _uow;
    public GetAttendanceReportQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PaginatedList<AttendanceReportDto>>> Handle(
        GetAttendanceReportQuery req, CancellationToken ct)
    {
        var query = _uow.Attendances.Query()
            .Include(a => a.Employee).ThenInclude(e => e.User)
            .Include(a => a.ShiftAssignment).ThenInclude(sa => sa.Shift)
            .Where(a => a.ShiftAssignment.Date >= req.StartDate.Date
                        && a.ShiftAssignment.Date <= req.EndDate.Date);

        if (req.EmployeeId.HasValue)
            query = query.Where(a => a.EmployeeId == req.EmployeeId.Value);
        if (req.Status.HasValue)
            query = query.Where(a => a.Status == req.Status.Value);
        if (!string.IsNullOrWhiteSpace(req.Search))
        {
            var search = req.Search.ToLower();
            query = query.Where(a =>
                a.Employee.User.FullName.ToLower().Contains(search) ||
                a.Employee.EmployeeCode.ToLower().Contains(search));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.ShiftAssignment.Date)
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(a => new AttendanceReportDto(
                a.Id, a.Employee.User.FullName, a.Employee.EmployeeCode,
                a.ShiftAssignment.Date, a.ShiftAssignment.Shift.Name,
                a.CheckInTime, a.CheckOutTime, a.WorkedHours,
                a.Status.ToString(), a.IsManuallyAdjusted, a.IsConfirmed))
            .ToListAsync(ct);

        return Result<PaginatedList<AttendanceReportDto>>.Success(
            new PaginatedList<AttendanceReportDto>(items, total, req.PageNumber, req.PageSize));
    }
}

// --- Export (all records, no pagination) ---
public record ExportAttendanceQuery(
    DateTime StartDate, DateTime EndDate,
    Guid? EmployeeId, AttendanceStatus? Status
) : IRequest<Result<List<AttendanceReportDto>>>;

public class ExportAttendanceQueryHandler : IRequestHandler<ExportAttendanceQuery, Result<List<AttendanceReportDto>>>
{
    private readonly IUnitOfWork _uow;
    public ExportAttendanceQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<AttendanceReportDto>>> Handle(ExportAttendanceQuery req, CancellationToken ct)
    {
        var query = _uow.Attendances.Query()
            .Include(a => a.Employee).ThenInclude(e => e.User)
            .Include(a => a.ShiftAssignment).ThenInclude(sa => sa.Shift)
            .Where(a => a.ShiftAssignment.Date >= req.StartDate.Date
                        && a.ShiftAssignment.Date <= req.EndDate.Date);

        if (req.EmployeeId.HasValue)
            query = query.Where(a => a.EmployeeId == req.EmployeeId.Value);
        if (req.Status.HasValue)
            query = query.Where(a => a.Status == req.Status.Value);

        var items = await query
            .OrderByDescending(a => a.ShiftAssignment.Date)
            .Select(a => new AttendanceReportDto(
                a.Id, a.Employee.User.FullName, a.Employee.EmployeeCode,
                a.ShiftAssignment.Date, a.ShiftAssignment.Shift.Name,
                a.CheckInTime, a.CheckOutTime, a.WorkedHours,
                a.Status.ToString(), a.IsManuallyAdjusted, a.IsConfirmed))
            .ToListAsync(ct);

        return Result<List<AttendanceReportDto>>.Success(items);
    }
}
