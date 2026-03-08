using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Shifts.Commands;

// --- Create Shift ---
public record CreateShiftCommand(string Name, TimeSpan StartTime, TimeSpan EndTime, string? Description) : IRequest<Result<Guid>>;

public class CreateShiftCommandHandler : IRequestHandler<CreateShiftCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateShiftCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<Result<Guid>> Handle(CreateShiftCommand req, CancellationToken ct)
    {
        var shift = new Shift
        {
            Name = req.Name,
            StartTime = req.StartTime,
            EndTime = req.EndTime,
            Description = req.Description,
            CreatedBy = _currentUser.Email
        };
        await _uow.Shifts.AddAsync(shift, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(shift.Id, "Shift created");
    }
}

// --- Assign Shift ---
public record AssignShiftCommand(Guid ShiftId, Guid EmployeeId, DateTime Date, Guid? LoungeZoneId) : IRequest<Result<Guid>>;

public class AssignShiftCommandHandler : IRequestHandler<AssignShiftCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;

    public AssignShiftCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, INotificationService notifications)
    { _uow = uow; _currentUser = currentUser; _notifications = notifications; }

    public async Task<Result<Guid>> Handle(AssignShiftCommand req, CancellationToken ct)
    {
        var shift = await _uow.Shifts.GetByIdAsync(req.ShiftId, ct);
        if (shift is null) return Result<Guid>.Failure("Shift not found");

        var hasOverlap = await _uow.ShiftAssignments.Query()
            .Where(sa => sa.EmployeeId == req.EmployeeId && sa.Date.Date == req.Date.Date)
            .Include(sa => sa.Shift)
            .AnyAsync(sa =>
                sa.Shift.StartTime < shift.EndTime && sa.Shift.EndTime > shift.StartTime, ct);

        if (hasOverlap)
            return Result<Guid>.Failure("Employee already has an overlapping shift on this date");

        var hasLeave = await _uow.LeaveRequests.Query()
            .AnyAsync(lr => lr.EmployeeId == req.EmployeeId
                         && lr.Status == LeaveRequestStatus.Approved
                         && lr.StartDate.Date <= req.Date.Date
                         && lr.EndDate.Date >= req.Date.Date, ct);

        if (hasLeave)
            return Result<Guid>.Failure("Employee has approved leave on this date. Please assign a different employee.");

        var assignment = new ShiftAssignment
        {
            ShiftId = req.ShiftId,
            EmployeeId = req.EmployeeId,
            Date = req.Date.Date,
            LoungeZoneId = req.LoungeZoneId,
            CreatedBy = _currentUser.Email
        };

        await _uow.ShiftAssignments.AddAsync(assignment, ct);
        await _uow.SaveChangesAsync(ct);

        var employee = await _uow.Employees.Query().Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == req.EmployeeId, ct);
        if (employee != null)
        {
            await _notifications.SendToUserAsync(employee.UserId,
                "New Shift Assignment",
                $"You have been assigned to shift '{shift.Name}' on {req.Date:yyyy-MM-dd}", ct);
        }

        return Result<Guid>.Success(assignment.Id, "Shift assigned");
    }
}
