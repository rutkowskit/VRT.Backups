using MediatR;
using Microsoft.Extensions.Logging;
using VRT.Backups.Application.Events;
using VRT.Notifications.Client;

namespace VRT.Backups.Infrastructure.Events;
public sealed class BackupCompletedEventHandler : INotificationHandler<BackupCompleted>
{
    private readonly INotificationsPublishService _publishService;
    private readonly ILogger<BackupCompletedEventHandler> _logger;

    public BackupCompletedEventHandler(INotificationsPublishService publishService,
        ILogger<BackupCompletedEventHandler> logger)
    {
        _publishService = publishService;
        _logger = logger;
    }

    public async Task Handle(BackupCompleted notification, CancellationToken cancellationToken)
    {
        try
        {
            await _publishService.Publish(notification.Message);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, $"Exception when publishing BackupCompleted event");
        }
    }
}