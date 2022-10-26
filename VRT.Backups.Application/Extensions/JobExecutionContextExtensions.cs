using CSharpFunctionalExtensions;
using Quartz;

namespace VRT.Backups;
public static class JobExecutionContextExtensions
{
    public static Result<string> GetString(this IJobExecutionContext context, string key)
    {
        var result = context.MergedJobDataMap.GetString(key);
        return result ?? Result.Failure<string>($"{key} not found in data map");
    }
}
