using AirportLounge.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;
using AirportLounge.Infrastructure.Hubs;

namespace AirportLounge.Infrastructure.Services;

public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToUserAsync(Guid userId, string title, string message, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group($"user_{userId}")
            .SendAsync("ReceiveNotification", new { title, message, timestamp = DateTime.UtcNow }, cancellationToken);
    }

    public async Task SendToGroupAsync(IEnumerable<Guid> userIds, string title, string message, CancellationToken cancellationToken = default)
    {
        var tasks = userIds.Select(id => SendToUserAsync(id, title, message, cancellationToken));
        await Task.WhenAll(tasks);
    }

    public async Task SendToAllAsync(string title, string message, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All
            .SendAsync("ReceiveNotification", new { title, message, timestamp = DateTime.UtcNow }, cancellationToken);
    }
}
