using CSharpFunctionalExtensions;
using Quartz;

namespace VRT.Backups.Cleanup;
public sealed record BackupCleanupJobOptions
{
    public string BackupsDirectoryPath { get; init; } = string.Empty;
    public string SearchPattern { get; init; } = "*.bak";
    public int RetentionDays { get; init; } = 30;

    internal static Result<BackupCleanupJobOptions> Load(IJobExecutionContext context)
    {
        if (context == null)
        {
            return Result.Failure<BackupCleanupJobOptions>("Job execution context cannot be null");
        }
        if (context.JobDetail == null)
        {
            return Result.Failure<BackupCleanupJobOptions>("Job detail cannot be null");
        }
        var options = new BackupCleanupJobOptions();
        var result = context
            .Get<string>(nameof(BackupsDirectoryPath))
            .Tap(backupsDir => options = options with { BackupsDirectoryPath = backupsDir })
            .Bind(_ => context.Get<int>(nameof(RetentionDays)).Compensate(_ => 30))
            .Tap(maxAge => options = options with { RetentionDays = maxAge })
            .Bind(_ => context.Get<string>(nameof(SearchPattern)).Compensate(_ => "*.bak"))
            .Tap(searchPattern => options = options with { SearchPattern = searchPattern })
            .Ensure(_ => options.IsValid, "Invalid job options")
            .Map(_ => options);

        return result;
    }
    public bool IsValid => string.IsNullOrEmpty(BackupsDirectoryPath) is false && RetentionDays > 0;
}
