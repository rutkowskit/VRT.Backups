using Microsoft.Extensions.DependencyInjection;
using Quartz;
using VRT.Backups.Abstractions;
using VRT.Backups.Application.Abstractions;
using VRT.Backups.Infrastructure.Options;
using VRT.Backups.Infrastructure.Services;

namespace VRT.Backups.Infrastructure;
public static partial class DependencyInjection
{
    private record Marker;
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services
            .AddSerilogLogging()
            .AddRabbitMqNotificationsPublishService()
            .AddSingleton<IDateTimeService, DateTimeService>()
            .AddSingleton<INotificationService, NotificationService>()
            .AddQuartzInfrastructure()
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Marker>());
        return services;
    }

    private static IServiceCollection AddQuartzInfrastructure(this IServiceCollection services)
    {
        services.ConfigureOptions<QuartzOptionsSetup>();
        services.AddQuartz();
        return services;
    }
}
