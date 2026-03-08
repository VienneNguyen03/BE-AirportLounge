using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Notifications.Queries;

public record GetMyNotificationsQuery(Guid EmployeeId, bool? UnreadOnly = false, int PageNumber = 1, int PageSize = 20)
    : IRequest<Result<PaginatedList<NotificationDto>>>;

public record NotificationDto(Guid Id, string Title, string Content, string Type,
    bool IsRead, DateTime? ReadAt, DateTime CreatedAt);

public class GetMyNotificationsQueryHandler
    : IRequestHandler<GetMyNotificationsQuery, Result<PaginatedList<NotificationDto>>>
{
    private readonly IUnitOfWork _uow;
    public GetMyNotificationsQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PaginatedList<NotificationDto>>> Handle(GetMyNotificationsQuery req, CancellationToken ct)
    {
        var query = _uow.Notifications.Query()
            .Where(n => n.RecipientId == req.EmployeeId);

        if (req.UnreadOnly == true)
            query = query.Where(n => !n.IsRead);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((req.PageNumber - 1) * req.PageSize).Take(req.PageSize)
            .Select(n => new NotificationDto(n.Id, n.Title, n.Content,
                n.Type.ToString(), n.IsRead, n.ReadAt, n.CreatedAt))
            .ToListAsync(ct);

        return Result<PaginatedList<NotificationDto>>.Success(
            new PaginatedList<NotificationDto>(items, total, req.PageNumber, req.PageSize));
    }
}
