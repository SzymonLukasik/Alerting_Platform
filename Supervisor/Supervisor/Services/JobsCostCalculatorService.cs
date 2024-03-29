namespace Supervisor.Services;

using Configuration;
using Dapper;
using Microsoft.Extensions.Options;
using Models.Calls;
using Models.DbModels;
using MySqlConnector;

public class JobsCostCalculatorService
{
    private readonly string _connectionString;
    private readonly SupervisorConfiguration _supervisorConfiguration;

    public JobsCostCalculatorService(
        ISecretManagerService secretManagerService,
        IOptions<SupervisorConfiguration> supervisorConfiguration)
    {
        _supervisorConfiguration = supervisorConfiguration.Value;
        _connectionString = secretManagerService.GetMySqlDbConnectionString();
    }

    public async Task<Dictionary<string, uint>> CalculateServicesResourceCosts(
        IEnumerable<MonitoredService> monitoredServices,
        DateTime from,
        DateTime to)
    {
        var averageResponseTimes =
            (await GetAverageResponseTimesOfServicesInTimeRange(from, to)).ToDictionary(
                x => x.Url,
                x => x.AverageResponseTime);
        var resourceCosts = new Dictionary<string, uint>();

        foreach (var monitoredHttpService in monitoredServices)
        {
            var averageResponseTime = averageResponseTimes.GetValueOrDefault(monitoredHttpService.Url);
            var resourceCost = CalculateServiceResourceCost(monitoredHttpService, averageResponseTime);

            resourceCosts[monitoredHttpService.Url] = resourceCost;
        }

        return resourceCosts;
    }

    public uint CalculateServiceResourceCost(
        MonitoredService service,
        double averageResponseTime)
    {
        var averageResponseTimeFactor = averageResponseTime * _supervisorConfiguration.ResourceCostResponseTimeFactor;

        if (service.FrequencyMs <= 0)
        {
            throw new Exception("FrequencyMs must be greater than 0");
        }

        var frequencyFactor = 1 / (double)service.FrequencyMs * _supervisorConfiguration.ResourceCostFrequencyFactor;

        return (uint)(averageResponseTimeFactor + frequencyFactor);
    }

    private async Task<IEnumerable<CallsAverageResponseTimes>> GetAverageResponseTimesOfServicesInTimeRange(
        DateTime from,
        DateTime to)
    {
        await using var connection = new MySqlConnection(_connectionString);
        connection.Open();
        var query =
            $"""
                 select url, avg(responseTimeMs) as averageResponseTime
                 from alerting.calls
                 where timestamp >= '{from:yyyy-MM-dd HH:mm:ss}'
                   and timestamp <= '{to:yyyy-MM-dd HH:mm:ss}'
                 group by url;
             """;

        var averageResponseTimes = await connection.QueryAsync<CallsAverageResponseTimes>(query);

        return averageResponseTimes;
    }

    // private async Task InsertRandomData()
    // {
    //     await using var connection = new MySqlConnection(_connectionString);
    //     connection.Open();
    //
    //     for (var i = 1; i <= 500; i++)
    //     {
    //         var url = new[] { "https://www.google.com/", "https://www.wp.pl/" }[new Random().Next(2)];
    //         var timestamp = GenerateRandomDate(DateTime.UtcNow.AddMinutes(-100), DateTime.UtcNow);
    //         var responseTime = new Random().Next(100, 500);
    //         var callResult = new[] { "success", "error", "timeout" }[new Random().Next(3)];
    //
    //         await using var cmd = new MySqlCommand(
    //             "INSERT INTO alerting.calls (url, timestamp, responseTimeMs, callResult) VALUES (@url, @timestamp, @responseTime, @callResult)",
    //             connection);
    //         cmd.Parameters.AddWithValue("@url", url);
    //         cmd.Parameters.AddWithValue(
    //             "@timestamp",
    //             timestamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
    //         cmd.Parameters.AddWithValue("@responseTime", responseTime);
    //         cmd.Parameters.AddWithValue("@callResult", callResult);
    //
    //         cmd.ExecuteNonQuery();
    //     }
    // }
    //
    // private DateTime GenerateRandomDate(
    //     DateTime from,
    //     DateTime to)
    // {
    //     var fromTimestamp = ToUnixTimestamp(from);
    //     var toTimestamp = ToUnixTimestamp(to);
    //
    //     if (fromTimestamp >= toTimestamp)
    //     {
    //         fromTimestamp = toTimestamp;
    //     }
    //
    //     var randomDateTimestamp = Random.Shared.Next(fromTimestamp, toTimestamp);
    //
    //     return UnixTimestampToDateTime(randomDateTimestamp);
    // }
    //
    // private int ToUnixTimestamp(DateTime dateTime)
    // {
    //     var dateTime1 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    //     return (int)(dateTime.ToUniversalTime() - dateTime1).TotalSeconds;
    // }
    //
    // private static DateTime UnixTimestampToDateTime(int unixTimestamp) =>
    //     new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTimestamp).ToUniversalTime();
}