using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Quartz;
using VRT.Backups.Abstractions;
using VRT.Backups.Application.Abstractions;
using VRT.Backups.Application.Events;

namespace VRT.Backups.Cleanup;
public sealed class BackupCleanupJob : IJob
{
    private readonly IDateTimeService _dateTimeService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<BackupCleanupJob> _logger;

    public BackupCleanupJob(
        IDateTimeService dateTimeService,
        INotificationService notificationService,
        ILogger<BackupCleanupJob> logger)
    {
        _dateTimeService = dateTimeService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var result = await BackupCleanupJobOptions
            .Load(context)
            .Map(PerformCleanup)
            .TapError(error => throw new JobExecutionException(error))
            .Tap(NotifyAboutBackupCompletion);
    }

    private async Task NotifyAboutBackupCompletion(int deletedFilesCount)
    {
        if (deletedFilesCount == 0)
        {
            return;
        }
        await Result
            .Success(new BackupCompleted($"{deletedFilesCount} backup files deleted"))
            .TapTry(n => _notificationService.Publish(n, false));
    }

    private int PerformCleanup(BackupCleanupJobOptions options)
    {
        var minBackupCreationTime = _dateTimeService.UtcNow.AddDays(-options.RetentionDays);
        var toDelete = Directory
            .EnumerateFiles(options.BackupsDirectoryPath, options.SearchPattern, SearchOption.TopDirectoryOnly)
            .Where(f => File.GetLastWriteTimeUtc(f) < minBackupCreationTime);

        var deletedCount = 0;
        foreach (var f in toDelete)
        {
            try
            {
                File.Delete(f);
                _logger?.LogInformation("Deleted file {File}", f);
                deletedCount++;
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "Failed to delete file {File}", f);
            }
        }
        return deletedCount;
    }
}
