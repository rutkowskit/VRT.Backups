namespace VRT.Backups.Abstractions;

/// <summary>
/// Service returns current DateTime (UTC)
/// </summary>
public interface IDateTimeService
{
    DateTimeOffset UtcNow { get; }
}
