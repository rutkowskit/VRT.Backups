namespace VRT.Notifications.Client;

public interface INotificationsPublishService
{
    Task Publish(string message);
}
