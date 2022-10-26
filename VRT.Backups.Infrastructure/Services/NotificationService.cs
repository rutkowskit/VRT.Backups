using MediatR;
using VRT.Backups.Application.Abstractions;

namespace VRT.Backups.Infrastructure.Services;
public sealed class NotificationService : INotificationService
{
    private readonly IPublisher _publisher;

    public NotificationService(IPublisher publisher)
    {
        _publisher = publisher;
    }
    public async Task Publish<T>(T notification, bool shouldAwait = true) where T : INotification
    {
        var task = _publisher.Publish(notification);
        if(shouldAwait)
        {
            await task;
        }
    }
}
