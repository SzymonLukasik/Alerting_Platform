namespace AvailabilityMonitor.Models;

public class MonitoredHttpServiceConfiguration
{
    public string Url { get; set; }

    public int TimeoutMs { get; set; }

    public int FrequencyMs { get; set; }

    public int AlertingWindow { get; set; }

    public double ExpectedAvailability { get; set; }

    public int FirstAdminAllowedResponseTimeMs { get; set; }

    public AdminConfiguration FirstAdmin { get; set; }

    public AdminConfiguration SecondAdmin { get; set; }
}