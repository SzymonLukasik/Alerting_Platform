namespace Supervisor.Infra.CronJobService;

using System.Timers;
using Cronos;

// Based on https://github.com/dotnet-labs/ServiceWorkerCronJob/blob/master/ServiceWorkerCronJobDemo/Services/CronJobService.cs
public abstract class CronJobService : IHostedService, IDisposable
{
    private readonly CronExpression _expression;
    private readonly bool _isInstant;
    private readonly ILogger<CronJobService> _logger;
    private Timer _timer;

    protected CronJobService(
        string expression,
        bool isInstant,
        ILogger<CronJobService> logger)
    {
        _expression = CronExpression.Parse(expression);
        _isInstant = isInstant;
        _logger = logger;
    }

    public virtual void Dispose() => _timer?.Dispose();

    public virtual Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(() => ScheduleJob(_isInstant, cancellationToken), cancellationToken);

        return Task.CompletedTask;
    }

    public virtual Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Stop();

        return Task.CompletedTask;
    }

    private async Task ScheduleJob(
        bool startInstant,
        CancellationToken cancellationToken)
    {
        if (startInstant)
        {
            await StartJob(cancellationToken);
        }

        var next = _expression.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Utc);

        if (next.HasValue)
        {
            var delay = next.Value - DateTimeOffset.Now;

            if (delay.TotalMilliseconds <= 0)
            {
                await ScheduleJob(false, cancellationToken);
            }

            _timer = new Timer(delay.TotalMilliseconds);
            _timer.Elapsed += async (_, _) =>
            {
                _timer.Dispose();
                _timer = null;

                if (!cancellationToken.IsCancellationRequested)
                {
                    await StartJob(cancellationToken);
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    await ScheduleJob(false, cancellationToken); // reschedule next
                }
            };
            _timer.Start();
        }

        await Task.CompletedTask;
    }

    protected virtual async Task DoWork(CancellationToken cancellationToken) =>
        await Task.Delay(5000, cancellationToken); // do the work

    private async Task StartJob(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting {Job}...", GetType().Name);
        try
        {
            await DoWork(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError("Job {Job} FAILED, exception: {Exception}", GetType().Name, ex);

            return;
        }

        _logger.LogInformation("Finished {Job}", GetType().Name);
    }
}