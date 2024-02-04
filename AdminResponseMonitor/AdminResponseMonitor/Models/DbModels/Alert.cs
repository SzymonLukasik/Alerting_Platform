namespace AvailabilityMonitor.Models.DbModels;

using System;

public class Alert
{
    public int Id { get; set; }

    public int ServiceId { get; set; }

    public ResponseStatus ResponseStatus { get; set; }

    public byte[] FirstLinkUUID { get; set; }

    public byte[] SecondLinkUUID { get; set; }

    public DateTime FirstAlertTime { get; set; }

    public DateTime? SecondAlertTime { get; set; }

    public DateTime? FirstAlertResponseTime { get; set; }

    public DateTime? SecondAlertResponseTime { get; set; }
}

public enum ResponseStatus
{
    WaitingForFirstAdmin = 1,
    FirstAdminResponded,
    WaitingForSecondAdmin,
    SecondAdminResponded,
    Ignored
}