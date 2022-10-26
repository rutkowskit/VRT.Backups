using VRT.Backups.Infrastructure;
using VRT.Backups.Infrastructure.Helpers;
using VRT.Backups.Worker;

Directory.SetCurrentDirectory(DirectoryHelpers.GetExecutingAssemblyDirectory());

IHost host = Host
    .CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureServices(services =>
    {        
        services.AddInfrastructure();
        services.AddHostedService<QuartzHostedService>();
    })
    .Build();
await host.RunAsync();
