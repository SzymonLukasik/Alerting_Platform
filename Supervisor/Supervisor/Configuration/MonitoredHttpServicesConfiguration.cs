namespace Supervisor.Configuration;

public class MonitoredHttpServicesConfiguration
{
    public List<MonitoredHttpServiceConfiguration> MonitoredHttpServices { get; set; }

    public List<MonitoredHttpServiceConfiguration> UniqueMonitoredHttpServices =>
        MonitoredHttpServices
            .GroupBy(x => x.Url)
            .Select(x => x.First())
            .ToList();
}