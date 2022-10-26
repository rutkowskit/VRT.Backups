// Based on https://github.com/quartznet/quartznet/blob/main/src/Quartz.Extensions.Hosting/QuartzHostedService.cs
using Quartz;

namespace VRT.Backups.Worker;

internal class QuartzHostedService : IHostedService
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ILogger<QuartzHostedService> _logger;
    private IScheduler? _scheduler;
    internal Task? _startupTask;
    private bool _schedulerWasStarted;

    public QuartzHostedService(
        IHostApplicationLifetime applicationLifetime,
        ISchedulerFactory schedulerFactory,
        ILogger<QuartzHostedService> logger)
    {
        _applicationLifetime = applicationLifetime;
        _schedulerFactory = schedulerFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _scheduler = await GetScheduler(cancellationToken);
        _startupTask = AwaitStartupCompletionAndStartSchedulerAsync(cancellationToken);

        if (_startupTask.IsCompleted)
        {
            await _startupTask.ConfigureAwait(false);
        }
    }
    private async Task<IScheduler?> GetScheduler(CancellationToken stoppingToken)
    {
        var retries = 0;
        while (stoppingToken.IsCancellationRequested is false)
        {
            try
            {
                var result = await _schedulerFactory.GetScheduler(stoppingToken);
                return result!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occured when trying to create scheduler");
                retries++;
                if (retries > 5)
                {
                    throw;
                }
            }
            await Task.Delay(10000, stoppingToken);
        }
        return null;
    }
    private async Task AwaitStartupCompletionAndStartSchedulerAsync(CancellationToken startupCancellationToken)
    {
        using var combinedCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(startupCancellationToken, _applicationLifetime.ApplicationStarted);

        await Task.Delay(Timeout.InfiniteTimeSpan, combinedCancellationSource.Token) // Wait "indefinitely", until startup completes or is aborted
            .ContinueWith(_ => { }, TaskContinuationOptions.OnlyOnCanceled) // Without an OperationCanceledException on cancellation
            .ConfigureAwait(false);

        if (!startupCancellationToken.IsCancellationRequested)
        {
            await StartSchedulerAsync(_applicationLifetime.ApplicationStopping).ConfigureAwait(false); // Startup has finished, but ApplicationStopping may still interrupt starting of the scheduler
        }
    }

    /// <summary>
    /// Starts the <see cref="IScheduler"/>, either immediately or after the delay configured in the <see cref="_options"/>.
    /// </summary>
    private async Task StartSchedulerAsync(CancellationToken cancellationToken)
    {
        if (_scheduler is null)
        {
            throw new InvalidOperationException("The scheduler should have been initialized first.");
        }

        _schedulerWasStarted = true;

        // Avoid potential race conditions between ourselves and StopAsync, in case it has already made its attempt to stop the scheduler
        if (_applicationLifetime.ApplicationStopping.IsCancellationRequested)
        {
            return;
        }
        await _scheduler.Start(cancellationToken).ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Stopped without having been started
        if (_scheduler is null || _startupTask is null)
        {
            return;
        }

        try
        {
            // Wait until any ongoing startup logic has finished or the graceful shutdown period is over
            await Task.WhenAny(_startupTask, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
        }
        finally
        {
            if (_schedulerWasStarted && !cancellationToken.IsCancellationRequested)
            {
                await _scheduler.Shutdown(true, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
