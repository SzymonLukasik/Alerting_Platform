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
    private readonly SupervisorConfiguration _supervisorConfiguration;

    public JobDistributionWorker(
        CronJobConfig<JobDistributionWorker> config,
        ILogger<JobDistributionWorker> logger,
        IOptions<MonitoredHttpServicesConfiguration> monitoredHttpServicesConfiguration,
        JobsCostCalculatorService jobsCostCalculatorService,
        MessageBusService bus,
        IOptions<SupervisorConfiguration> supervisorConfiguration) : base(
        config.CronExpression,
        config.InstantJob,
        logger)
    {
        _logger = logger;
        _jobsCostCalculatorService = jobsCostCalculatorService;
        _bus = bus;
        _supervisorConfiguration = supervisorConfiguration.Value;
        _monitoredHttpServices = monitoredHttpServicesConfiguration.Value.UniqueMonitoredHttpServices;
    }

    protected override async Task DoWork(CancellationToken stoppingToken)
    {
        var servicesResourceCosts =
            await _jobsCostCalculatorService.CalculateServicesResourceCosts(
                DateTime.UtcNow.AddMinutes(-_supervisorConfiguration.CalculateServicesCostForWindowMinutes),
                DateTime.UtcNow);

        var monitorFrom = DateTime.UtcNow + TimeSpan.FromSeconds(_supervisorConfiguration.MonitorDelaySeconds);
        var monitorTo = DateTime.UtcNow +
                        TimeSpan.FromMinutes(_supervisorConfiguration.JobDistributionWorkerIntervalMinutes) +
                        TimeSpan.FromMinutes(_supervisorConfiguration.MonitorDelaySeconds);

        foreach (var monitoredHttpService in _monitoredHttpServices)
        {
            _logger.LogInformation("Start processing {ServiceUrl}...", monitoredHttpService.Url);

            var message = new MonitorHttpService
            {
                TaskId = Guid.NewGuid().ToString(),
                MonitoredHttpServiceConfiguration = monitoredHttpService,
                ResourceCost = servicesResourceCosts[monitoredHttpService.Url],
                MonitorFrom = monitorFrom,
                MonitorTo = monitorTo
            };

            await _bus.PublishMessage(AppTopic.Tasks, message);
        }
    }
}