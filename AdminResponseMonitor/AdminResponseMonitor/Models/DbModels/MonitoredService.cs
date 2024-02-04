namespace AdminResponseMonitor.Models.DbModels;

public class MonitoredService
{
    public int Id { get; set; }
    public string Url { get; set; }
    public int TimeoutMs { get; set; }
    public int FrequencyMs { get; set; }
    public int AlertingWindowMs { get; set; }
    public double ExpectedAvailability { get; set; }
    public int FirstAdminAllowedResponseTimeMs { get; set; }
    public bool FirstAdminSendEmail { get; set; }
    public bool FirstAdminSendSms { get; set; }
    public string FirstAdminName { get; set; }
    public string FirstAdminEmail { get; set; }
    public string FirstAdminPhoneNumber { get; set; }
    public int SecondAdminAllowedResponseTimeMs { get; set; }
    public bool SecondAdminSendEmail { get; set; }
    public bool SecondAdminSendSms { get; set; }
    public string SecondAdminName { get; set; }
    public string SecondAdminEmail { get; set; }
    public string SecondAdminPhoneNumber { get; set; }
}