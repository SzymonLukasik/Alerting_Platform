namespace Supervisor.Models.Messages;

using Configuration;

public record MonitorHttpService
{
    public string TaskId { get; init; }

    public MonitoredHttpServiceConfiguration MonitoredHttpServiceConfiguration { get; init; }

    public uint ResourceCost { get; init; }

    public DateTime MonitorFrom { get; init; }

    public DateTime MonitorTo { get; init; }
}