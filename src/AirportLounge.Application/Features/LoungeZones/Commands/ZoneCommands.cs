using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.LoungeZones.Commands;

// --- Create Zone ---
public record CreateZoneCommand(string Name, string? Description, int Capacity) : IRequest<Result<Guid>>;

public class CreateZoneCommandHandler : IRequestHandler<CreateZoneCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;

    public CreateZoneCommandHandler(IUnitOfWork uow, ICacheService cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task<Result<Guid>> Handle(CreateZoneCommand req, CancellationToken ct)
    {
        var zone = new LoungeZone { Name = req.Name, Description = req.Description, Capacity = req.Capacity };
        await _uow.LoungeZones.AddAsync(zone, ct);
        await _uow.SaveChangesAsync(ct);

        // Invalidate all zone list caches and dashboard
        foreach (var key in CacheKeys.AllZoneListKeys)
            await _cache.RemoveAsync(key, ct);
        await _cache.RemoveAsync(CacheKeys.ZoneAlerts, ct);
        await _cache.RemoveAsync(CacheKeys.AdminDashboard, ct);

        return Result<Guid>.Success(zone.Id, "Zone created");
    }
}

// --- Update Zone Status ---
public record UpdateZoneStatusCommand(Guid ZoneId, ZoneStatus NewStatus, string? Notes) : IRequest<Result<bool>>;

public class UpdateZoneStatusCommandHandler : IRequestHandler<UpdateZoneStatusCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;
    private readonly ICacheService _cache;

    public UpdateZoneStatusCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser,
        INotificationService notifications, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _notifications = notifications;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(UpdateZoneStatusCommand req, CancellationToken ct)
    {
        var zone = await _uow.LoungeZones.GetByIdAsync(req.ZoneId, ct);
        if (zone is null) return Result<bool>.Failure("Zone not found");

        var previousStatus = zone.Status;
        zone.Status = req.NewStatus;

        await _uow.ZoneStatusLogs.AddAsync(new ZoneStatusLog
        {
            LoungeZoneId = zone.Id,
            PreviousStatus = previousStatus,
            NewStatus = req.NewStatus,
            ChangedBy = _currentUser.Email,
            ChangedAt = DateTime.UtcNow,
            Notes = req.Notes
        }, ct);

        _uow.LoungeZones.Update(zone);
        await _uow.SaveChangesAsync(ct);

        // Invalidate zone caches immediately – status is time-sensitive
        foreach (var key in CacheKeys.AllZoneListKeys)
            await _cache.RemoveAsync(key, ct);
        await _cache.RemoveAsync(CacheKeys.ZoneAlerts, ct);
        await _cache.RemoveAsync(CacheKeys.ManagerDashboard, ct);

        if (req.NewStatus == ZoneStatus.Full || req.NewStatus == ZoneStatus.NeedsSupport)
        {
            await _notifications.SendToAllAsync(
                $"Zone Alert: {zone.Name}",
                $"Zone '{zone.Name}' status changed to {req.NewStatus}", ct);
        }

        return Result<bool>.Success(true, "Zone status updated");
    }
}

// --- Update Zone ---
public record UpdateZoneCommand(Guid ZoneId, string Name, string? Description, int Capacity) : IRequest<Result<bool>>;

public class UpdateZoneCommandHandler : IRequestHandler<UpdateZoneCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;

    public UpdateZoneCommandHandler(IUnitOfWork uow, ICacheService cache)
    { _uow = uow; _cache = cache; }

    public async Task<Result<bool>> Handle(UpdateZoneCommand req, CancellationToken ct)
    {
        var zone = await _uow.LoungeZones.GetByIdAsync(req.ZoneId, ct);
        if (zone is null) return Result<bool>.Failure("Zone not found");

        zone.Name = req.Name;
        zone.Description = req.Description;
        zone.Capacity = req.Capacity;

        _uow.LoungeZones.Update(zone);
        await _uow.SaveChangesAsync(ct);

        foreach (var key in CacheKeys.AllZoneListKeys)
            await _cache.RemoveAsync(key, ct);

        return Result<bool>.Success(true, "Zone updated");
    }
}

// --- Delete Zone ---
public record DeleteZoneCommand(Guid ZoneId) : IRequest<Result<bool>>;

public class DeleteZoneCommandHandler : IRequestHandler<DeleteZoneCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;

    public DeleteZoneCommandHandler(IUnitOfWork uow, ICacheService cache)
    { _uow = uow; _cache = cache; }

    public async Task<Result<bool>> Handle(DeleteZoneCommand req, CancellationToken ct)
    {
        var zone = await _uow.LoungeZones.GetByIdAsync(req.ZoneId, ct);
        if (zone is null) return Result<bool>.Failure("Zone not found");

        zone.IsDeleted = true;
        _uow.LoungeZones.Update(zone);
        await _uow.SaveChangesAsync(ct);

        foreach (var key in CacheKeys.AllZoneListKeys)
            await _cache.RemoveAsync(key, ct);

        return Result<bool>.Success(true, "Zone deleted");
    }
}

// --- Delete Zones Batch ---
public record DeleteZonesBatchCommand(IReadOnlyList<Guid> ZoneIds) : IRequest<Result<int>>;

public class DeleteZonesBatchCommandHandler : IRequestHandler<DeleteZonesBatchCommand, Result<int>>
{
    private readonly IMediator _mediator;

    public DeleteZonesBatchCommandHandler(IMediator mediator) => _mediator = mediator;

    public async Task<Result<int>> Handle(DeleteZonesBatchCommand req, CancellationToken ct)
    {
        if (req.ZoneIds is null || req.ZoneIds.Count == 0)
            return Result<int>.Failure("No zone IDs provided");

        var processed = 0;
        foreach (var id in req.ZoneIds.Distinct())
        {
            var result = await _mediator.Send(new DeleteZoneCommand(id), ct);
            if (!result.IsSuccess)
                return Result<int>.Failure(result.Message ?? $"Failed to delete zone {id}");

            processed++;
        }

        return Result<int>.Success(processed, $"Deleted {processed} zones");
    }
}
