using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.LoungeZones.Queries;

public record GetZonesQuery(
    string? Search,
    ZoneStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<Result<PaginatedList<ZoneDto>>>;

public record ZoneDto(Guid Id, string Name, string? Description, int Capacity,
    int CurrentOccupancy, string Status);

public class GetZonesQueryHandler : IRequestHandler<GetZonesQuery, Result<PaginatedList<ZoneDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;

    public GetZonesQueryHandler(IUnitOfWork uow, ICacheService cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task<Result<PaginatedList<ZoneDto>>> Handle(GetZonesQuery req, CancellationToken ct)
    {
        var query = _uow.LoungeZones.Query().AsQueryable();

        if (req.Status.HasValue)
            query = query.Where(z => z.Status == req.Status.Value);
        if (!string.IsNullOrWhiteSpace(req.Search))
        {
            var search = req.Search.ToLower();
            query = query.Where(z =>
                z.Name.ToLower().Contains(search) ||
                (z.Description != null && z.Description.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(z => z.Name)
            .Skip((req.PageNumber - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(z => new ZoneDto(z.Id, z.Name, z.Description, z.Capacity,
                z.CurrentOccupancy, z.Status.ToString()))
            .ToListAsync(ct);

        return Result<PaginatedList<ZoneDto>>.Success(
            new PaginatedList<ZoneDto>(items, totalCount, req.PageNumber, req.PageSize));
    }
}

// --- Zone Alerts (capacity nearly reached or support needed) ---
public record GetZoneAlertsQuery() : IRequest<Result<List<ZoneAlertDto>>>;

public record ZoneAlertDto(Guid ZoneId, string ZoneName, string AlertType, string Message);

public class GetZoneAlertsQueryHandler : IRequestHandler<GetZoneAlertsQuery, Result<List<ZoneAlertDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;
    private const double CapacityThreshold = 0.9;

    public GetZoneAlertsQueryHandler(IUnitOfWork uow, ICacheService cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task<Result<List<ZoneAlertDto>>> Handle(GetZoneAlertsQuery req, CancellationToken ct)
    {
        var cached = await _cache.GetAsync<List<ZoneAlertDto>>(CacheKeys.ZoneAlerts, ct);
        if (cached is not null)
            return Result<List<ZoneAlertDto>>.Success(cached);

        var zones = await _uow.LoungeZones.Query()
            .Where(z => z.Capacity > 0 || z.Status == ZoneStatus.NeedsCleaning
                || z.Status == ZoneStatus.NeedsSupport || z.Status == ZoneStatus.Full)
            .ToListAsync(ct);

        var alerts = new List<ZoneAlertDto>();
        foreach (var z in zones)
        {
            if (z.Capacity > 0 && (double)z.CurrentOccupancy / z.Capacity >= CapacityThreshold)
                alerts.Add(new ZoneAlertDto(z.Id, z.Name, "Capacity",
                    $"Zone '{z.Name}' is {z.CurrentOccupancy}/{z.Capacity} ({(100.0 * z.CurrentOccupancy / z.Capacity):F0}%) - nearly at capacity"));
            if (z.Status == ZoneStatus.NeedsSupport)
                alerts.Add(new ZoneAlertDto(z.Id, z.Name, "Support",
                    $"Zone '{z.Name}' needs urgent support"));
            if (z.Status == ZoneStatus.NeedsCleaning)
                alerts.Add(new ZoneAlertDto(z.Id, z.Name, "Cleaning",
                    $"Zone '{z.Name}' needs cleaning"));
            if (z.Status == ZoneStatus.Full)
                alerts.Add(new ZoneAlertDto(z.Id, z.Name, "Full",
                    $"Zone '{z.Name}' is at full capacity"));
        }

        await _cache.SetAsync(CacheKeys.ZoneAlerts, alerts, CacheKeys.ZoneAlertsTtl, ct);
        return Result<List<ZoneAlertDto>>.Success(alerts);
    }
}
