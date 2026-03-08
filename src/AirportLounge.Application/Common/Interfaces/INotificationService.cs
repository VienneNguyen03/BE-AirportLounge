namespace AirportLounge.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendToUserAsync(Guid userId, string title, string message, CancellationToken cancellationToken = default);
    Task SendToGroupAsync(IEnumerable<Guid> userIds, string title, string message, CancellationToken cancellationToken = default);
    Task SendToAllAsync(string title, string message, CancellationToken cancellationToken = default);
}
