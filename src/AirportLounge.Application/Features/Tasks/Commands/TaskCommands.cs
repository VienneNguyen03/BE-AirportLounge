using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Tasks.Commands;

// --- Create Task ---
public record CreateTaskCommand(
    string Title, string? Description, TaskPriority Priority,
    Guid? AssignedToId, Guid? LoungeZoneId, DateTime? DueDate
) : IRequest<Result<Guid>>;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;

    public CreateTaskCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, INotificationService notifications)
    { _uow = uow; _currentUser = currentUser; _notifications = notifications; }

    public async Task<Result<Guid>> Handle(CreateTaskCommand req, CancellationToken ct)
    {
        var task = new TaskItem
        {
            Title = req.Title,
            Description = req.Description,
            Priority = req.Priority,
            AssignedToId = req.AssignedToId,
            LoungeZoneId = req.LoungeZoneId,
            DueDate = req.DueDate,
            CreatedBy = _currentUser.Email
        };

        await _uow.TaskItems.AddAsync(task, ct);
        await _uow.SaveChangesAsync(ct);

        // Notify assigned employee
        if (req.AssignedToId.HasValue)
        {
            var emp = await _uow.Employees.Query().Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == req.AssignedToId.Value, ct);
            if (emp != null)
            {
                await _notifications.SendToUserAsync(emp.UserId,
                    "New Task Assigned", $"Task: {req.Title} (Priority: {req.Priority})", ct);
            }
        }

        return Result<Guid>.Success(task.Id, "Task created");
    }
}

// --- Update Task Status ---
public record UpdateTaskStatusCommand(Guid TaskId, TaskItemStatus NewStatus) : IRequest<Result<bool>>;

public class UpdateTaskStatusCommandHandler : IRequestHandler<UpdateTaskStatusCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    public UpdateTaskStatusCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<bool>> Handle(UpdateTaskStatusCommand req, CancellationToken ct)
    {
        var task = await _uow.TaskItems.GetByIdAsync(req.TaskId, ct);
        if (task is null) return Result<bool>.Failure("Task not found");

        task.Status = req.NewStatus;
        if (req.NewStatus == TaskItemStatus.Completed)
            task.CompletedAt = DateTime.UtcNow;

        _uow.TaskItems.Update(task);
        await _uow.SaveChangesAsync(ct);

        return Result<bool>.Success(true, $"Task status updated to {req.NewStatus}");
    }
}
