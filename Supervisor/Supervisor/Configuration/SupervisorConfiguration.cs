namespace Supervisor.Configuration;

public class SupervisorConfiguration
{
    public string ProjectName { get; set; }

    public string TasksTopic { get; set; }

    public string TasksSubscription { get; set; }

    public int JobDistributionWorkerIntervalMinutes { get; set; }

    public int CalculateServicesCostForWindowMinutes { get; set; }

    public double ResourceCostResponseTimeFactor { get; set; }

    public double ResourceCostFrequencyFactor { get; set; }

    public int MonitorDelaySeconds { get; set; }
}