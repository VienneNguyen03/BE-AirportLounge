using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Attendance.Commands;

// --- Check In ---
public record CheckInCommand(Guid EmployeeId) : IRequest<Result<CheckInResponse>>;

public record CheckInResponse(Guid AttendanceId, DateTime CheckInTime, string Status, string ShiftName);

public class CheckInCommandHandler : IRequestHandler<CheckInCommand, Result<CheckInResponse>>
{
    private readonly IUnitOfWork _uow;
    private static readonly int EarlyCheckinMinutes = 15;

    public CheckInCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<CheckInResponse>> Handle(CheckInCommand req, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var currentTime = now.TimeOfDay;

        var assignment = await _uow.ShiftAssignments.Query()
            .Include(sa => sa.Shift)
            .Where(sa => sa.EmployeeId == req.EmployeeId && sa.Date.Date == today)
            .Where(sa => sa.Shift.StartTime.Add(TimeSpan.FromMinutes(-EarlyCheckinMinutes)) <= currentTime
                         && sa.Shift.EndTime >= currentTime)
            .FirstOrDefaultAsync(ct);

        if (assignment is null)
            return Result<CheckInResponse>.Failure("No active shift found for check-in at this time");

        var existing = await _uow.Attendances.Query()
            .AnyAsync(a => a.ShiftAssignmentId == assignment.Id && a.CheckInTime != null, ct);

        if (existing)
            return Result<CheckInResponse>.Failure("Already checked in for this shift");

        var status = currentTime > assignment.Shift.StartTime
            ? AttendanceStatus.Late
            : AttendanceStatus.OnTime;

        var attendance = new Domain.Entities.Attendance
        {
            EmployeeId = req.EmployeeId,
            ShiftAssignmentId = assignment.Id,
            CheckInTime = now,
            Status = status
        };

        await _uow.Attendances.AddAsync(attendance, ct);
        await _uow.SaveChangesAsync(ct);

        var response = new CheckInResponse(attendance.Id, now, status.ToString(), assignment.Shift.Name);
        return Result<CheckInResponse>.Success(response, $"Checked in successfully ({status})");
    }
}

// --- Check Out ---
public record CheckOutCommand(Guid EmployeeId) : IRequest<Result<CheckOutResponse>>;

public record CheckOutResponse(Guid AttendanceId, DateTime CheckInTime, DateTime CheckOutTime, double WorkedHours, string Status);

public class CheckOutCommandHandler : IRequestHandler<CheckOutCommand, Result<CheckOutResponse>>
{
    private readonly IUnitOfWork _uow;
    public CheckOutCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<CheckOutResponse>> Handle(CheckOutCommand req, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var attendance = await _uow.Attendances.Query()
            .Include(a => a.ShiftAssignment).ThenInclude(sa => sa.Shift)
            .Where(a => a.EmployeeId == req.EmployeeId && a.CheckInTime != null && a.CheckOutTime == null)
            .OrderByDescending(a => a.CheckInTime)
            .FirstOrDefaultAsync(ct);

        if (attendance is null)
            return Result<CheckOutResponse>.Failure("No active check-in found");

        attendance.CheckOutTime = now;
        attendance.WorkedHours = (now - attendance.CheckInTime!.Value).TotalHours;

        var shiftEnd = attendance.ShiftAssignment.Shift.EndTime;
        var currentTime = now.TimeOfDay;

        if (currentTime < shiftEnd.Add(TimeSpan.FromMinutes(-15)))
            attendance.Status = AttendanceStatus.EarlyLeave;
        else if (currentTime > shiftEnd.Add(TimeSpan.FromMinutes(15)))
            attendance.Status = AttendanceStatus.Overtime;

        _uow.Attendances.Update(attendance);
        await _uow.SaveChangesAsync(ct);

        var response = new CheckOutResponse(
            attendance.Id,
            attendance.CheckInTime!.Value,
            now,
            Math.Round(attendance.WorkedHours!.Value, 2),
            attendance.Status.ToString());

        return Result<CheckOutResponse>.Success(response,
            $"Checked out — {attendance.WorkedHours:F2} hours worked ({attendance.Status})");
    }
}

// --- Update (Manager edit) ---
public record UpdateAttendanceCommand(
    Guid AttendanceId,
    DateTime? CheckInTime,
    DateTime? CheckOutTime,
    AttendanceStatus? Status,
    string? Notes
) : IRequest<Result<bool>>;

public class UpdateAttendanceCommandHandler : IRequestHandler<UpdateAttendanceCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;

    public UpdateAttendanceCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<bool>> Handle(UpdateAttendanceCommand req, CancellationToken ct)
    {
        var attendance = await _uow.Attendances.Query()
            .Include(a => a.ShiftAssignment).ThenInclude(sa => sa.Shift)
            .FirstOrDefaultAsync(a => a.Id == req.AttendanceId, ct);

        if (attendance is null)
            return Result<bool>.Failure("Attendance record not found");

        if (req.CheckInTime.HasValue)
        {
            attendance.CheckInTime = req.CheckInTime.Value;
            if (attendance.CheckOutTime.HasValue)
                attendance.WorkedHours = (attendance.CheckOutTime.Value - attendance.CheckInTime.Value).TotalHours;
        }
        if (req.CheckOutTime.HasValue)
        {
            attendance.CheckOutTime = req.CheckOutTime.Value;
            if (attendance.CheckInTime.HasValue)
                attendance.WorkedHours = (attendance.CheckOutTime.Value - attendance.CheckInTime.Value).TotalHours;
        }
        if (req.Status.HasValue) attendance.Status = req.Status.Value;
        if (req.Notes is not null) attendance.Notes = req.Notes;

        attendance.IsManuallyAdjusted = true;
        _uow.Attendances.Update(attendance);
        await _uow.SaveChangesAsync(ct);

        return Result<bool>.Success(true, "Attendance record updated");
    }
}

// --- Confirm (Manager confirm) ---
public record ConfirmAttendanceCommand(Guid AttendanceId) : IRequest<Result<bool>>;

public class ConfirmAttendanceCommandHandler : IRequestHandler<ConfirmAttendanceCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;

    public ConfirmAttendanceCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<bool>> Handle(ConfirmAttendanceCommand req, CancellationToken ct)
    {
        var attendance = await _uow.Attendances.GetByIdAsync(req.AttendanceId, ct);
        if (attendance is null)
            return Result<bool>.Failure("Attendance record not found");

        attendance.IsConfirmed = true;
        _uow.Attendances.Update(attendance);
        await _uow.SaveChangesAsync(ct);

        return Result<bool>.Success(true, "Attendance confirmed");
    }
}
