using AirportLounge.Application.Common;
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
    private readonly ICacheService _cache;

    public CreateTaskCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, INotificationService notifications, ICacheService cache)
    { _uow = uow; _currentUser = currentUser; _notifications = notifications; _cache = cache; }

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

        await _cache.RemoveByPrefixAsync(CacheKeys.TasksPrefix, ct);

        return Result<Guid>.Success(task.Id, "Task created");
    }
}

// --- Update Task Status ---
public record UpdateTaskStatusCommand(Guid TaskId, TaskItemStatus NewStatus) : IRequest<Result<bool>>;

public class UpdateTaskStatusCommandHandler : IRequestHandler<UpdateTaskStatusCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;

    public UpdateTaskStatusCommandHandler(IUnitOfWork uow, ICacheService cache)
    { _uow = uow; _cache = cache; }

    public async Task<Result<bool>> Handle(UpdateTaskStatusCommand req, CancellationToken ct)
    {
        var task = await _uow.TaskItems.GetByIdAsync(req.TaskId, ct);
        if (task is null) return Result<bool>.Failure("Task not found");

        task.Status = req.NewStatus;
        if (req.NewStatus == TaskItemStatus.Completed)
            task.CompletedAt = DateTime.UtcNow;

        _uow.TaskItems.Update(task);
        await _uow.SaveChangesAsync(ct);

        await _cache.RemoveByPrefixAsync(CacheKeys.TasksPrefix, ct);

        return Result<bool>.Success(true, $"Task status updated to {req.NewStatus}");
    }
}

// --- Update Task ---
public record UpdateTaskCommand(
    Guid TaskId, string Title, string? Description, TaskPriority Priority,
    Guid? AssignedToId, Guid? LoungeZoneId, DateTime? DueDate
) : IRequest<Result<bool>>;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public UpdateTaskCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    { _uow = uow; _currentUser = currentUser; _cache = cache; }

    public async Task<Result<bool>> Handle(UpdateTaskCommand req, CancellationToken ct)
    {
        var task = await _uow.TaskItems.GetByIdAsync(req.TaskId, ct);
        if (task is null) return Result<bool>.Failure("Task not found");

        task.Title = req.Title;
        task.Description = req.Description;
        task.Priority = req.Priority;
        task.AssignedToId = req.AssignedToId;
        task.LoungeZoneId = req.LoungeZoneId;
        task.DueDate = req.DueDate;
        task.UpdatedBy = _currentUser.Email;

        _uow.TaskItems.Update(task);
        await _uow.SaveChangesAsync(ct);

        await _cache.RemoveByPrefixAsync(CacheKeys.TasksPrefix, ct);

        return Result<bool>.Success(true, "Task updated");
    }
}

// --- Delete Task ---
public record DeleteTaskCommand(Guid TaskId) : IRequest<Result<bool>>;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;

    public DeleteTaskCommandHandler(IUnitOfWork uow, ICacheService cache)
    { _uow = uow; _cache = cache; }

    public async Task<Result<bool>> Handle(DeleteTaskCommand req, CancellationToken ct)
    {
        var task = await _uow.TaskItems.GetByIdAsync(req.TaskId, ct);
        if (task is null) return Result<bool>.Failure("Task not found");

        task.IsDeleted = true;
        _uow.TaskItems.Update(task);
        await _uow.SaveChangesAsync(ct);

        await _cache.RemoveByPrefixAsync(CacheKeys.TasksPrefix, ct);

        return Result<bool>.Success(true, "Task deleted");
    }
}

// --- Delete Tasks Batch ---
public record DeleteTasksBatchCommand(IReadOnlyList<Guid> TaskIds) : IRequest<Result<int>>;

public class DeleteTasksBatchCommandHandler : IRequestHandler<DeleteTasksBatchCommand, Result<int>>
{
    private readonly IMediator _mediator;

    public DeleteTasksBatchCommandHandler(IMediator mediator) => _mediator = mediator;

    public async Task<Result<int>> Handle(DeleteTasksBatchCommand req, CancellationToken ct)
    {
        if (req.TaskIds is null || req.TaskIds.Count == 0)
            return Result<int>.Failure("No task IDs provided");

        var processed = 0;
        foreach (var id in req.TaskIds.Distinct())
        {
            var result = await _mediator.Send(new DeleteTaskCommand(id), ct);
            if (!result.IsSuccess)
                return Result<int>.Failure(result.Message ?? $"Failed to delete task {id}");

            processed++;
        }

        return Result<int>.Success(processed, $"Deleted {processed} tasks");
    }
}
