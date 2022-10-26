namespace VRT.Notifications.Client.Options;
public sealed class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public bool AutomaticRecoveryEnabled { get; set; } = true;
}
