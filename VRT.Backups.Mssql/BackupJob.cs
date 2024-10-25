using CSharpFunctionalExtensions;
using Microsoft.Data.SqlClient;
using Quartz;
using System.Text;
using VRT.Backups.Abstractions;
using VRT.Backups.Application.Abstractions;
using VRT.Backups.Application.Events;

namespace VRT.Backups.Mssql;
public sealed class BackupJob : IJob
{
    private readonly IDateTimeService _dateTimeService;
    private readonly INotificationService _notificationService;

    public BackupJob(
        IDateTimeService dateTimeService,
        INotificationService notificationService)
    {
        _dateTimeService = dateTimeService;
        _notificationService = notificationService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var result = await CreateDbConnection(context)
            .Bind(conn => CreateBackupCommand(context, conn))
            .MapTry(cmd => cmd.ExecuteNonQuery())
            .TapError(error => throw new JobExecutionException(error))
            .Tap(_ => NotifyAboutBackupCompletion(context));
    }

    private async Task NotifyAboutBackupCompletion(IJobExecutionContext context)
    {
        await Result.Try(async () =>
        {
            await context
                .GetString(Constants.DatabaseName)
                .Map(dbName => new BackupCompleted($"{dbName} database backup completed"))
                .Tap(n => _notificationService.Publish(n, false));
        });
    }

    private static Result<SqlConnection> CreateDbConnection(IJobExecutionContext context)
    {
        var result = context
            .GetString(Constants.ConnectionString)
            .Map(cs => new SqlConnection(cs))
            .MapTry(cs => { cs.Open(); return cs; });

        return result;
    }

    private Result<SqlCommand> CreateBackupCommand(IJobExecutionContext context,
        SqlConnection connection)
    {
        var cmd = new StringBuilder();
        var result = context
            .GetString(Constants.DatabaseName)
            .BindJoin(_ => context.GetString(Constants.BackupsDirectory))
            .Tap(t => CreateBackupDirectoryIfNotExists(t.Item2))
            .Tap(db => cmd.Append($"BACKUP DATABASE {db.Item1} "))
            .Tap(db => cmd.AppendLine($"TO DISK = '{GetBackupFileName(db)}' "))
            .Tap(db => cmd.AppendLine($"WITH FORMAT,MEDIANAME='{db.Item1}Backups', "))
            .Tap(db => cmd.AppendLine($"NAME='Full backup of {db.Item1}'"))
            .Map(_ => new SqlCommand(cmd.ToString(), connection));
        return result;
    }
    private string GetBackupFileName((string DbName, string BackupsDirectory) @params)
    {
        var fileName = $"{@params.DbName}_full_{_dateTimeService.UtcNow:yyyyMMdd_HHmmss}.bak";
        return Path.Combine(@params.BackupsDirectory, fileName);
    }
    private static void CreateBackupDirectoryIfNotExists(string backupsDirectory)
    {
        if (Directory.Exists(backupsDirectory))
        {
            return;
        }
        Directory.CreateDirectory(backupsDirectory);
    }
}
