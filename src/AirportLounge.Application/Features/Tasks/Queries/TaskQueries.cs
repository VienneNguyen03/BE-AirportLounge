using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Tasks.Queries;

public record GetTasksQuery(
    TaskItemStatus? Status, Guid? AssignedToId, TaskPriority? Priority,
    int PageNumber = 1, int PageSize = 20
) : IRequest<Result<PaginatedList<TaskDto>>>;

public record TaskDto(
    Guid Id, string Title, string? Description, string Priority, string Status,
    string? AssignedTo, string? Zone, DateTime? DueDate, DateTime? CompletedAt, DateTime CreatedAt);

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, Result<PaginatedList<TaskDto>>>
{
    private readonly IUnitOfWork _uow;
    public GetTasksQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PaginatedList<TaskDto>>> Handle(GetTasksQuery req, CancellationToken ct)
    {
        var query = _uow.TaskItems.Query()
            .Include(t => t.AssignedTo).ThenInclude(e => e!.User)
            .Include(t => t.LoungeZone)
            .AsQueryable();

        if (req.Status.HasValue) query = query.Where(t => t.Status == req.Status.Value);
        if (req.AssignedToId.HasValue) query = query.Where(t => t.AssignedToId == req.AssignedToId.Value);
        if (req.Priority.HasValue) query = query.Where(t => t.Priority == req.Priority.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(t => t.Priority).ThenByDescending(t => t.CreatedAt)
            .Skip((req.PageNumber - 1) * req.PageSize).Take(req.PageSize)
            .Select(t => new TaskDto(
                t.Id, t.Title, t.Description, t.Priority.ToString(), t.Status.ToString(),
                t.AssignedTo != null ? t.AssignedTo.User.FullName : null,
                t.LoungeZone != null ? t.LoungeZone.Name : null,
                t.DueDate, t.CompletedAt, t.CreatedAt))
            .ToListAsync(ct);

        return Result<PaginatedList<TaskDto>>.Success(
            new PaginatedList<TaskDto>(items, total, req.PageNumber, req.PageSize));
    }
}

// --- Export (all records, no pagination) ---
public record ExportTasksQuery(
    TaskItemStatus? Status, Guid? AssignedToId, TaskPriority? Priority
) : IRequest<Result<List<TaskDto>>>;

public class ExportTasksQueryHandler : IRequestHandler<ExportTasksQuery, Result<List<TaskDto>>>
{
    private readonly IUnitOfWork _uow;
    public ExportTasksQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<TaskDto>>> Handle(ExportTasksQuery req, CancellationToken ct)
    {
        var query = _uow.TaskItems.Query()
            .Include(t => t.AssignedTo).ThenInclude(e => e!.User)
            .Include(t => t.LoungeZone)
            .AsQueryable();

        if (req.Status.HasValue) query = query.Where(t => t.Status == req.Status.Value);
        if (req.AssignedToId.HasValue) query = query.Where(t => t.AssignedToId == req.AssignedToId.Value);
        if (req.Priority.HasValue) query = query.Where(t => t.Priority == req.Priority.Value);

        var items = await query
            .OrderByDescending(t => t.Priority).ThenByDescending(t => t.CreatedAt)
            .Select(t => new TaskDto(
                t.Id, t.Title, t.Description, t.Priority.ToString(), t.Status.ToString(),
                t.AssignedTo != null ? t.AssignedTo.User.FullName : null,
                t.LoungeZone != null ? t.LoungeZone.Name : null,
                t.DueDate, t.CompletedAt, t.CreatedAt))
            .ToListAsync(ct);

        return Result<List<TaskDto>>.Success(items);
    }
}
