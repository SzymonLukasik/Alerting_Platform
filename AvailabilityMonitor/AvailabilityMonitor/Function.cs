namespace AvailabilityMonitor;

using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using Models;
using MySqlConnector;

public class Function : IHttpFunction
{
    public async Task HandleAsync(HttpContext context)
    {
        await context.Response.WriteAsync("Hello World!");
    }

    private async Task<List<MonitoredHttpServiceConfiguration>> LoadConfig()
    {
        return new List<MonitoredHttpServiceConfiguration>();
    }

    private async Task<MySqlConnection> GetDbConnection()
    {
        return new MySqlConnection();
    }

    private async Task BuildAlertConfigsTemporaryTable(MySqlConnection connection, List<MonitoredHttpServiceConfiguration> monitoredHttpServices)
    {
        
    }

    private async Task CheckAvailability(MySqlConnection connection)
    {
        
    }

    private async Task SaveAndPushAlerts(MySqlConnection connection)
    {
        
    }
}