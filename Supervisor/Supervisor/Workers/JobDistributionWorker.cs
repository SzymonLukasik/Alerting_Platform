namespace Supervisor.Workers;

using Configuration;
using Infra.CronJobService;
using Microsoft.Extensions.Options;
using Models.Consts;
using Models.Messages;
using Services;

public class JobDistributionWorker : CronJobService
{
    private readonly MessageBusService _bus;
    private readonly JobsCostCalculatorService _jobsCostCalculatorService;
    private readonly ILogger<JobDistributionWorker> _logger;
    private readonly List<MonitoredHttpServiceConfiguration> _monitoredHttpServices;

    public JobDistributionWorker(
        CronJobConfig<JobDistributionWorker> config,
        ILogger<JobDistributionWorker> logger,
        IOptions<MonitoredHttpServicesConfiguration> monitoredHttpServicesConfiguration,
        JobsCostCalculatorService jobsCostCalculatorService,
        MessageBusService bus) : base(
        config.CronExpression,
        config.InstantJob,
        logger)
    {
        _logger = logger;
        _jobsCostCalculatorService = jobsCostCalculatorService;
        _bus = bus;
        _monitoredHttpServices = monitoredHttpServicesConfiguration.Value.UniqueMonitoredHttpServices;
    }

    protected override async Task DoWork(CancellationToken stoppingToken)
    {
        var servicesResourceCosts =
            await _jobsCostCalculatorService.CalculateServicesResourceCosts(
                DateTime.UtcNow.AddMinutes(-100),
                DateTime.UtcNow);

        foreach (var monitoredHttpService in _monitoredHttpServices)
        {
            _logger.LogInformation("Start processing {ServiceUrl}...", monitoredHttpService.Url);

            var message = new MonitorHttpService
            {
                TaskId = Guid.NewGuid().ToString(),
                MonitoredHttpServiceConfiguration = monitoredHttpService,
                ResourceCost = servicesResourceCosts[monitoredHttpService.Url],
                MonitorFrom = DateTime.UtcNow,
                MonitorTo = DateTime.UtcNow.AddMinutes(15)
            };

            await _bus.PublishMessage(AppTopic.Tasks, message);
        }
    }
}