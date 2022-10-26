using MediatR;

namespace VRT.Backups.Application.Abstractions;
public interface INotificationService
{
    Task Publish<T>(T notification, bool shouldAwait = true) where T : INotification;
}
