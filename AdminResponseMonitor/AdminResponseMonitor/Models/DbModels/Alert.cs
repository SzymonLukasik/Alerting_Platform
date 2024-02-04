namespace AdminResponseMonitor.Models.DbModels;

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
    waiting_for_first_admin = 1,
    first_admin_responded,
    waiting_for_second_admin,
    second_admin_responded,
    ignored
}