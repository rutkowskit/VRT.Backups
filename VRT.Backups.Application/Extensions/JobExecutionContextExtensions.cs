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

    public static Result<T> Get<T>(this IJobExecutionContext context, string key)
    {
        return context.MergedJobDataMap.TryGetValue(key, out var value) && value is T t
            ? Result.Success(t)
            : Result.Failure<T>($"{key} not found in data map");
    }
}
