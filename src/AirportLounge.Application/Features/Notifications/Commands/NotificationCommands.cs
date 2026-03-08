using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Notifications.Commands;

// --- Send Notification ---
public record SendNotificationCommand(
    string Title, string Content, NotificationType Type,
    List<Guid>? RecipientIds, // null = broadcast to all
    string? RelatedEntityType, Guid? RelatedEntityId
) : IRequest<Result<int>>;

public class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, Result<int>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _realtime;

    public SendNotificationCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, INotificationService realtime)
    { _uow = uow; _currentUser = currentUser; _realtime = realtime; }

    public async Task<Result<int>> Handle(SendNotificationCommand req, CancellationToken ct)
    {
        var recipients = req.RecipientIds;

        if (recipients is null || recipients.Count == 0)
        {
            // Broadcast to all employees
            recipients = await _uow.Employees.Query()
                .Select(e => e.Id).ToListAsync(ct);
        }

        var notifications = recipients.Select(recipientId => new Notification
        {
            Title = req.Title,
            Content = req.Content,
            Type = req.Type,
            RecipientId = recipientId,
            RelatedEntityType = req.RelatedEntityType,
            RelatedEntityId = req.RelatedEntityId,
            CreatedBy = _currentUser.Email
        }).ToList();

        foreach (var n in notifications)
            await _uow.Notifications.AddAsync(n, ct);

        await _uow.SaveChangesAsync(ct);

        // Push real-time
        var userIds = await _uow.Employees.Query()
            .Where(e => recipients.Contains(e.Id))
            .Select(e => e.UserId).ToListAsync(ct);

        await _realtime.SendToGroupAsync(userIds, req.Title, req.Content, ct);

        return Result<int>.Success(notifications.Count, $"Sent to {notifications.Count} recipients");
    }
}

// --- Mark as Read ---
public record MarkNotificationReadCommand(Guid NotificationId) : IRequest<Result<bool>>;

public class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    public MarkNotificationReadCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<bool>> Handle(MarkNotificationReadCommand req, CancellationToken ct)
    {
        var notification = await _uow.Notifications.GetByIdAsync(req.NotificationId, ct);
        if (notification is null) return Result<bool>.Failure("Notification not found");

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        _uow.Notifications.Update(notification);
        await _uow.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}
