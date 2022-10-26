using VRT.Notifications.Client;
using VRT.Notifications.Client.Options;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyInjection
{
    public static IServiceCollection AddRabbitMqNotificationsPublishService(this IServiceCollection services,
        Action<RabbitMqOptions>? configureAction = null)
    {
        services.ConfigureOptions<RabbitMqOptionsSetup>();
        if(configureAction is not null)
        {
            services.Configure(configureAction);
        }        
        services.AddSingleton<INotificationsPublishService, RabbitMqNotificationsPublishService>();
        return services;
    }
}
