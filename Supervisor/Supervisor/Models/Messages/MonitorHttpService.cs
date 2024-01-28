namespace Supervisor.Models.Messages;

using DbModels;

public record MonitorHttpService
{
    public string TaskId { get; init; }

    public MonitoredService MonitoredHttpServiceConfiguration { get; init; }

    public uint ResourceCost { get; init; }

    public DateTime MonitorFrom { get; init; }

    public DateTime MonitorTo { get; init; }
}