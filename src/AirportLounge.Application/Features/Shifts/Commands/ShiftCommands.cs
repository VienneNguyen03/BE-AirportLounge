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

// --- Assign Shift (single) ---
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
        var result = await AssignInternalAsync(req, ct);
        if (!result.IsSuccess) return result;

        return Result<Guid>.Success(result.Data, "Shift assigned");
    }

    internal async Task<Result<Guid>> AssignInternalAsync(AssignShiftCommand req, CancellationToken ct)
    {
        var shift = await _uow.Shifts.GetByIdAsync(req.ShiftId, ct);
        if (shift is null) return Result<Guid>.Failure("Shift not found");

        var hasOverlap = await _uow.ShiftAssignments.Query()
            .Where(sa => sa.EmployeeId == req.EmployeeId && sa.Date.Date == req.Date.Date)
            .Include(sa => sa.Shift)
            .AnyAsync(sa =>
                sa.Shift.StartTime < shift.EndTime && sa.Shift.EndTime > shift.StartTime, ct);

        if (hasOverlap)
            return Result<Guid>.Failure($"Employee already has an overlapping shift on {req.Date:yyyy-MM-dd}");

        var hasLeave = await _uow.LeaveRequests.Query()
            .AnyAsync(lr => lr.EmployeeId == req.EmployeeId
                         && lr.Status == LeaveRequestStatus.Approved
                         && lr.StartDate.Date <= req.Date.Date
                         && lr.EndDate.Date >= req.Date.Date, ct);

        if (hasLeave)
            return Result<Guid>.Failure($"Employee has approved leave on {req.Date:yyyy-MM-dd}. Please assign a different employee.");

        // Check for existing assignment (including soft-deleted)
        var existing = await _uow.ShiftAssignments.Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(sa => sa.EmployeeId == req.EmployeeId 
                                    && sa.Date.Date == req.Date.Date 
                                    && sa.ShiftId == req.ShiftId, ct);

        if (existing != null)
        {
            if (!existing.IsDeleted)
            {
                return Result<Guid>.Failure("This employee is already assigned to this shift on this date.");
            }
            
            // Reactivate and update
            existing.IsDeleted = false;
            existing.LoungeZoneId = req.LoungeZoneId;
            existing.UpdatedBy = _currentUser.Email;
            existing.UpdatedAt = DateTime.UtcNow;
            
            _uow.ShiftAssignments.Update(existing);
            await _uow.SaveChangesAsync(ct);
            return Result<Guid>.Success(existing.Id, "Shift assignment reactivated and updated");
        }

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

        return Result<Guid>.Success(assignment.Id);
    }
}

// --- Assign Shifts Batch ---
public record AssignShiftBatchItem(Guid ShiftId, Guid EmployeeId, DateTime Date, Guid? LoungeZoneId);

public record AssignShiftsBatchCommand(IReadOnlyList<AssignShiftBatchItem> Items) : IRequest<Result<List<Guid>>>;

public class AssignShiftsBatchCommandHandler : IRequestHandler<AssignShiftsBatchCommand, Result<List<Guid>>>
{
    private readonly IMediator _mediator;

    public AssignShiftsBatchCommandHandler(IMediator mediator) => _mediator = mediator;

    public async Task<Result<List<Guid>>> Handle(AssignShiftsBatchCommand req, CancellationToken ct)
    {
        if (req.Items is null || req.Items.Count == 0)
            return Result<List<Guid>>.Failure("No assignments provided");

        var createdIds = new List<Guid>();

        foreach (var item in req.Items)
        {
            var single = new AssignShiftCommand(item.ShiftId, item.EmployeeId, item.Date, item.LoungeZoneId);
            var result = await _mediator.Send(single, ct);
            if (!result.IsSuccess)
            {
                return Result<List<Guid>>.Failure(result.Message ?? "Failed to assign shift in batch");
            }

            createdIds.Add(result.Data);
        }

        return Result<List<Guid>>.Success(createdIds, "Shifts assigned");
    }
}

// --- Update Shift Assignment ---
public record UpdateShiftAssignmentCommand(Guid AssignmentId, Guid ShiftId, Guid EmployeeId, DateTime Date, Guid? LoungeZoneId) : IRequest<Result<bool>>;

public class UpdateShiftAssignmentCommandHandler : IRequestHandler<UpdateShiftAssignmentCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateShiftAssignmentCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(UpdateShiftAssignmentCommand req, CancellationToken ct)
    {
        var assignment = await _uow.ShiftAssignments.Query()
            .Include(sa => sa.Shift)
            .FirstOrDefaultAsync(sa => sa.Id == req.AssignmentId, ct);

        if (assignment is null)
            return Result<bool>.Failure("Assignment not found");

        var shift = await _uow.Shifts.GetByIdAsync(req.ShiftId, ct);
        if (shift is null)
            return Result<bool>.Failure("Shift not found");

        var hasOverlap = await _uow.ShiftAssignments.Query()
            .Where(sa => sa.EmployeeId == req.EmployeeId
                         && sa.Date.Date == req.Date.Date
                         && sa.Id != req.AssignmentId)
            .Include(sa => sa.Shift)
            .AnyAsync(sa =>
                sa.Shift.StartTime < shift.EndTime && sa.Shift.EndTime > shift.StartTime, ct);

        if (hasOverlap)
            return Result<bool>.Failure("Employee already has an overlapping shift on this date");

        var hasLeave = await _uow.LeaveRequests.Query()
            .AnyAsync(lr => lr.EmployeeId == req.EmployeeId
                         && lr.Status == LeaveRequestStatus.Approved
                         && lr.StartDate.Date <= req.Date.Date
                         && lr.EndDate.Date >= req.Date.Date, ct);

        if (hasLeave)
            return Result<bool>.Failure("Employee has approved leave on this date. Please assign a different employee.");

        assignment.ShiftId = req.ShiftId;
        assignment.EmployeeId = req.EmployeeId;
        assignment.Date = req.Date.Date;
        assignment.LoungeZoneId = req.LoungeZoneId;
        assignment.UpdatedBy = _currentUser.Email;

        _uow.ShiftAssignments.Update(assignment);
        await _uow.SaveChangesAsync(ct);

        return Result<bool>.Success(true, "Shift assignment updated");
    }
}

// --- Update Shift ---
public record UpdateShiftCommand(Guid ShiftId, string Name, TimeSpan StartTime, TimeSpan EndTime, string? Description) : IRequest<Result<bool>>;

public class UpdateShiftCommandHandler : IRequestHandler<UpdateShiftCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public UpdateShiftCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    { _uow = uow; _currentUser = currentUser; }

    public async Task<Result<bool>> Handle(UpdateShiftCommand req, CancellationToken ct)
    {
        var shift = await _uow.Shifts.GetByIdAsync(req.ShiftId, ct);
        if (shift is null) return Result<bool>.Failure("Shift not found");

        shift.Name = req.Name;
        shift.StartTime = req.StartTime;
        shift.EndTime = req.EndTime;
        shift.Description = req.Description;
        shift.UpdatedBy = _currentUser.Email;

        _uow.Shifts.Update(shift);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true, "Shift updated");
    }
}

// --- Delete Shift ---
public record DeleteShiftCommand(Guid ShiftId) : IRequest<Result<bool>>;

public class DeleteShiftCommandHandler : IRequestHandler<DeleteShiftCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;

    public DeleteShiftCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<bool>> Handle(DeleteShiftCommand req, CancellationToken ct)
    {
        var shift = await _uow.Shifts.GetByIdAsync(req.ShiftId, ct);
        if (shift is null) return Result<bool>.Failure("Shift not found");

        var hasAssignments = await _uow.ShiftAssignments.Query()
            .AnyAsync(sa => sa.ShiftId == req.ShiftId && sa.Date >= DateTime.UtcNow.Date, ct);
        if (hasAssignments)
            return Result<bool>.Failure("Cannot delete shift with future assignments");

        shift.IsDeleted = true;
        _uow.Shifts.Update(shift);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true, "Shift deleted");
    }
}

// --- Unassign Shift ---
public record UnassignShiftCommand(Guid AssignmentId) : IRequest<Result<bool>>;

public class UnassignShiftCommandHandler : IRequestHandler<UnassignShiftCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;

    public UnassignShiftCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<bool>> Handle(UnassignShiftCommand req, CancellationToken ct)
    {
        var assignment = await _uow.ShiftAssignments.GetByIdAsync(req.AssignmentId, ct);
        if (assignment is null) return Result<bool>.Failure("Assignment not found");

        assignment.IsDeleted = true;
        _uow.ShiftAssignments.Update(assignment);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true, "Shift unassigned");
    }
}

// --- Delete Shifts Batch ---
public record DeleteShiftsBatchCommand(IReadOnlyList<Guid> ShiftIds) : IRequest<Result<int>>;

public class DeleteShiftsBatchCommandHandler : IRequestHandler<DeleteShiftsBatchCommand, Result<int>>
{
    private readonly IMediator _mediator;

    public DeleteShiftsBatchCommandHandler(IMediator mediator) => _mediator = mediator;

    public async Task<Result<int>> Handle(DeleteShiftsBatchCommand req, CancellationToken ct)
    {
        if (req.ShiftIds is null || req.ShiftIds.Count == 0)
            return Result<int>.Failure("No shift IDs provided");

        var processed = 0;
        foreach (var id in req.ShiftIds.Distinct())
        {
            var result = await _mediator.Send(new DeleteShiftCommand(id), ct);
            if (!result.IsSuccess)
                return Result<int>.Failure(result.Message ?? $"Failed to delete shift {id}");

            processed++;
        }

        return Result<int>.Success(processed, $"Deleted {processed} shifts");
    }
}

// --- Unassign Shifts Batch ---
public record UnassignShiftsBatchCommand(IReadOnlyList<Guid> AssignmentIds) : IRequest<Result<int>>;

public class UnassignShiftsBatchCommandHandler : IRequestHandler<UnassignShiftsBatchCommand, Result<int>>
{
    private readonly IMediator _mediator;

    public UnassignShiftsBatchCommandHandler(IMediator mediator) => _mediator = mediator;

    public async Task<Result<int>> Handle(UnassignShiftsBatchCommand req, CancellationToken ct)
    {
        if (req.AssignmentIds is null || req.AssignmentIds.Count == 0)
            return Result<int>.Failure("No assignment IDs provided");

        var processed = 0;
        foreach (var id in req.AssignmentIds.Distinct())
        {
            var result = await _mediator.Send(new UnassignShiftCommand(id), ct);
            if (!result.IsSuccess)
                return Result<int>.Failure(result.Message ?? $"Failed to unassign shift assignment {id}");

            processed++;
        }

        return Result<int>.Success(processed, $"Unassigned {processed} shift assignments");
    }
}
