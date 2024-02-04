namespace AvailabilityMonitor;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using Google.Cloud.Functions.Framework;
using Google.Cloud.PubSub.V1;
using Google.Cloud.SecretManager.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Models.DbModels;
using Models.Queue;
using MySqlConnector;
using TopicName = Google.Cloud.PubSub.V1.TopicName;

public class Function : IHttpFunction
{
    private const string ServicesTable = "alerting.monitored_services";
    private const string CallsTable = "alerting.calls_copy2";
    private const string AlertsTable = "alerting.alerts";
    private const string LinkTemplate = "https://test.com/alert?uuid={0}";
    private const string ExpectedQueryParameter = "run-availability-monitor";
    private readonly ILogger<Function> _logger;
    private readonly PublisherClient _messagesPublisherClient;
    private readonly string _projectId;

    public Function(ILogger<Function> logger)
    {
        _logger = logger;
        // for Dapper to map snake_case column names to PascalCase properties
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        _projectId = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT");
        var messagesTopicName = Environment.GetEnvironmentVariable("MESSAGES_TOPIC_NAME");
        var messagesTopic = new TopicName(_projectId, messagesTopicName);
        _messagesPublisherClient = PublisherClient.Create(messagesTopic);
    }

    public async Task HandleAsync(HttpContext context)
    {
        if (!context.Request.Query.ContainsKey(ExpectedQueryParameter))
        {
            context.Response.StatusCode = 404;

            return;
        }

        await using var dbConnection = await GetDbConnection();
        dbConnection.Open();
        var allServices = (await GetAllServices(dbConnection)).ToList();
        _logger.LogInformation("There are {Count} services in the database", allServices.Count);
        var allServicesDictionary = allServices.ToDictionary(x => x.Id);
        var unavailableServiceIds = (await GetUnavailableServiceIds(dbConnection)).ToList();
        _logger.LogInformation("There are {Count} unavailable services", unavailableServiceIds.Count);
        var unavailableServices = unavailableServiceIds
            .Select(id => allServicesDictionary.GetValueOrDefault(id))
            .Where(x => x != null);

        await SaveAndPushAlerts(dbConnection, unavailableServices.ToList());

        await context.Response.WriteAsync("Hello World!");
    }

    private async Task<MySqlConnection> GetDbConnection() => new(GetMySqlDbConnectionString());

    private static async Task<IEnumerable<int>> GetUnavailableServiceIds(MySqlConnection connection)
    {
        const string query = $"""
                                  select service_id
                                  from {CallsTable} c
                                           inner join {ServicesTable} s
                                                     on c.service_id = s.id
                                  where c.timestamp <= now()
                                    and c.timestamp >= date_sub(now(), interval (s.alerting_window_ms * 1000) second)
                                  group by c.service_id, s.expected_availability
                                  having count(if(c.callResult = 'success', 1, null)) / count(*) < s.expected_availability
                              """;

        return await connection.QueryAsync<int>(query);
    }

    private static async Task<IEnumerable<MonitoredService>> GetAllServices(MySqlConnection connection)
    {
        const string query = $"SELECT * FROM {ServicesTable}";
        return await connection.QueryAsync<MonitoredService>(query);
    }

    private async Task SaveAndPushAlerts(
        MySqlConnection connection,
        List<MonitoredService> unavailableServices)
    {
        var now = DateTime.UtcNow;
        var alerts = unavailableServices.Select(
            unavailableService => new Alert
            {
                ServiceId = unavailableService.Id,
                ResponseStatus = ResponseStatus.WaitingForFirstAdmin,
                FirstLinkUUID = Guid.NewGuid().ToByteArray(),
                FirstAlertTime = now
            }).ToList();

        await SaveAlertsInDb(connection, alerts);
        await PushAlertMessagesOnMessageBus(alerts, unavailableServices.ToDictionary(x => x.Id));
        _logger.LogInformation("Saved and pushed {Count} alerts ", alerts.Count);
    }

    private static async Task SaveAlertsInDb(
        MySqlConnection connection,
        IEnumerable<Alert> alerts)
    {
        await using var command = new MySqlCommand(
            $"insert into {AlertsTable} (service_id, response_status, first_link_uuid, first_alert_time) values (@ServiceId, @ResponseStatus, @FirstLinkUUID, @FirstAlertTime)",
            connection);
        
        foreach (var alert in alerts)
        {
            command.Parameters.Add("@ServiceId", MySqlDbType.Int32).Value = alert.ServiceId;
            command.Parameters.Add("@ResponseStatus", MySqlDbType.Int32).Value = (int)alert.ResponseStatus;
            command.Parameters.Add("@FirstLinkUUID", MySqlDbType.Binary).Value = alert.FirstLinkUUID;
            command.Parameters.Add("@FirstAlertTime", MySqlDbType.DateTime).Value = alert.FirstAlertTime;

            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task PushAlertMessagesOnMessageBus(
        IReadOnlyCollection<Alert> alerts,
        IReadOnlyDictionary<int, MonitoredService> servicesDictionary)
    {
        var messages = alerts.Select(x => BuildMessagesForAlert(x, servicesDictionary[x.ServiceId])).ToList();

        var tasks = messages.SelectMany(x => x).Select(
            message => Task.Run(
                async () =>
                {
                    await _messagesPublisherClient.PublishAsync(JsonSerializer.Serialize(message));
                }));

        await Task.WhenAll(tasks);
    }

    private static List<SendMessageRequest> BuildMessagesForAlert(Alert alert, MonitoredService service)
    {
        var messages = new List<SendMessageRequest>();

        if (service.FirstAdminSendEmail)
        {
            var message = new SendMessageRequest
            {
                Subject = $"[Alert] Service {service.Url} is down",
                Body =
                    $"Attention! Service {service.Url} is down. Please check it. To resolve alert, click this link: {string.Format(LinkTemplate, new Guid(alert.FirstLinkUUID).ToString())}",
                To = service.FirstAdminEmail,
                ChannelType = MessageChannelType.Email
            };

            messages.Add(message);
        }

        if (service.FirstAdminSendSms)
        {
            var message = new SendMessageRequest
            {
                Subject = $"[Alert] Service {service.Url} is down",
                Body =
                    $"Attention! Service {service.Url} is down. Please check it. To resolve alert, click this link: {string.Format(LinkTemplate, new Guid(alert.FirstLinkUUID).ToString())}",
                To = service.FirstAdminPhoneNumber,
                ChannelType = MessageChannelType.Sms
            };

            messages.Add(message);
        }

        var logMessage = new SendMessageRequest
        {
            Subject = $"[Alert] Service {service.Url} is down",
            Body =
                $"Attention! Service {service.Url} is down. Please check it. To resolve alert, click this link: {string.Format(LinkTemplate, new Guid(alert.FirstLinkUUID).ToString())}",
            ChannelType = MessageChannelType.Log
        };

        messages.Add(logMessage);

        return messages;
    }

    private string GetMySqlDbConnectionString() => GetSecret("DB_CONNECTION_STRING");

    private string GetSecret(string secretId, string versionId = "latest")
    {
        var client = SecretManagerServiceClient.Create();
        var secretVersionName = new SecretVersionName(_projectId, secretId, versionId);
        var result = client.AccessSecretVersion(secretVersionName);
        return result.Payload.Data.ToStringUtf8();
    }
}