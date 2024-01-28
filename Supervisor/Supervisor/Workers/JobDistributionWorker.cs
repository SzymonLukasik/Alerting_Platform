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
    private readonly IMonitoredServicesRepository _monitoredServices;
    private readonly SupervisorConfiguration _supervisorConfiguration;

    public JobDistributionWorker(
        CronJobConfig<JobDistributionWorker> config,
        ILogger<JobDistributionWorker> logger,
        JobsCostCalculatorService jobsCostCalculatorService,
        MessageBusService bus,
        IOptions<SupervisorConfiguration> supervisorConfiguration,
        IMonitoredServicesRepository monitoredServices) : base(
        config.CronExpression,
        config.InstantJob,
        logger)
    {
        _logger = logger;
        _jobsCostCalculatorService = jobsCostCalculatorService;
        _bus = bus;
        _monitoredServices = monitoredServices;
        _supervisorConfiguration = supervisorConfiguration.Value;
    }

    protected override async Task DoWork(CancellationToken stoppingToken)
    {
        var monitoredServices = (await _monitoredServices.GetAllAsync()).ToList();

        var servicesResourceCosts =
            await _jobsCostCalculatorService.CalculateServicesResourceCosts(
                monitoredServices,
                DateTime.UtcNow.AddMinutes(-_supervisorConfiguration.CalculateServicesCostForWindowMinutes),
                DateTime.UtcNow);

        var monitorFrom = DateTime.UtcNow + TimeSpan.FromSeconds(_supervisorConfiguration.MonitorDelaySeconds);
        var monitorTo = DateTime.UtcNow +
                        TimeSpan.FromMinutes(_supervisorConfiguration.JobDistributionWorkerIntervalMinutes) +
                        TimeSpan.FromMinutes(_supervisorConfiguration.MonitorDelaySeconds);

        foreach (var monitoredService in monitoredServices)
        {
            _logger.LogInformation(
                "Send task for ID {ServiceId} ({ServiceUrl})...",
                monitoredService.Id,
                monitoredService.Url);

            var message = new MonitorHttpService
            {
                TaskId = Guid.NewGuid().ToString(),
                MonitoredHttpServiceConfiguration = monitoredService,
                ResourceCost = servicesResourceCosts[monitoredService.Url],
                MonitorFrom = monitorFrom,
                MonitorTo = monitorTo
            };

            await _bus.PublishMessage(AppTopic.Tasks, message);
        }
    }
}