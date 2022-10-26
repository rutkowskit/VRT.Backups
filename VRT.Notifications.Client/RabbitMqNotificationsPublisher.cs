using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using VRT.Notifications.Client.Options;

namespace VRT.Notifications.Client;
public sealed class RabbitMqNotificationsPublishService : INotificationsPublishService
{
    private const string ExchangeName = "VRT.Exchange";
    private const string QueueName = "VRT.Notifications";
    private const string RoutingKey = "Notifications";
    private readonly IOptions<RabbitMqOptions> _options;
    private IModel? _channel;
    private IConnection? _connection;

    public RabbitMqNotificationsPublishService(IOptions<RabbitMqOptions> options)
    {
        _options = options;
    }

    public Task Publish(string message)
    {
        var properties = Channel.CreateBasicProperties();
        properties.DeliveryMode = 1;
        properties.Persistent = false;        
        var body = Encoding.UTF8.GetBytes(message);
        Channel.BasicPublish(ExchangeName, RoutingKey, basicProperties: properties, body: body);
        return Task.CompletedTask;
    }

    private IModel Channel => _channel ??= CreateChannel();

    private IModel CreateChannel()
    {
        var options = _options.Value;
        var factory = new ConnectionFactory()
        {
            HostName = options.HostName,
            UserName = options.UserName,
            Password = options.Password,
            AutomaticRecoveryEnabled = options.AutomaticRecoveryEnabled
        };
        _connection = factory.CreateConnection();
        var channel = _connection.CreateModel();
        DeclareQueue(channel);
        return channel;
    }
    private static void DeclareQueue(IModel channel)
    {
        // ensure that exchange is declared
        channel.ExchangeDeclare(ExchangeName, "direct", true);
        // declare queue for this exchange
        var queue = channel.QueueDeclare(queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
        channel.QueueBind(queue: queue.QueueName, exchange: ExchangeName, routingKey: RoutingKey);
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        _channel = null;
        _connection = null;
    }
}
