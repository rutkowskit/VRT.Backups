using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using VRT.Backups.Abstractions;
using VRT.Backups.Application.Abstractions;
using VRT.Backups.Infrastructure.Options;
using VRT.Backups.Infrastructure.Services;
using VRT.Notifications.Client;

namespace VRT.Backups.Infrastructure;
public static partial class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services
            .AddSerilogLogging()
            .AddRabbitMqNotificationsPublishService()
            .AddSingleton<IDateTimeService, DateTimeService>()
            .AddSingleton<INotificationService, NotificationService>()
            .AddQuartzInfrastructure()
            .AddMediatR(typeof(IMarker).Assembly);
        return services;
    }   

    private static IServiceCollection AddQuartzInfrastructure(this IServiceCollection services)
    {
        services.ConfigureOptions<QuartzOptionsSetup>();
        services.AddQuartz(q =>
        {
            q.UseMicrosoftDependencyInjectionJobFactory();
        });
        return services;
    }
}
