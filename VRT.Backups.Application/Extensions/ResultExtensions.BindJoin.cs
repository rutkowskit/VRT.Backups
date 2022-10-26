using CSharpFunctionalExtensions;

namespace VRT.Backups;

public static partial class ResultExtensions
{
    public static async Task<Result<(T, K)>> BindJoin<T, K>(this Result<T> result, Func<T, Task<Result<K>>> toJoin)
    {        
        return await result.Bind(r1 => toJoin(r1).Map(r2 => (r1, r2)));
    }
    public static Result<(T, K)> BindJoin<T, K>(this Result<T> result, Func<T, Result<K>> toJoin)
    {
        return result.Bind(r1 => toJoin(r1).Map(r2 => (r1, r2)));
    }
    public static Result<(T, K)> BindJoin<T, K>(this Result<T> result, Func<T, K> toJoin)
    {
        return result.BindJoin((t) => Result.Success(toJoin(t)));
    }
}
