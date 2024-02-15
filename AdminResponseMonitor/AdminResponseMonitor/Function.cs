namespace AdminResponseMonitor;

using System;
using System.Collections.Generic;
using System.Data;
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
    private const string AlertsTable = "alerting.alerts";
    private const string LinkTemplate = "https://test.com/alert?uuid={0}";
    private const string ExpectedQueryParameter = "run-admin-response-monitor";
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
        var alertsWithNoResponseFromFirstAdminDetails =
            (await GetAlertsDetailsForWhichFirstAdminDidNotRespondInTime(dbConnection)).ToList();

        _logger.LogInformation(
            "There are {Count} alerts with no response from the first admin",
            alertsWithNoResponseFromFirstAdminDetails.Count);

        await HandleAlertsWithNoResponseFromFirstAdmin(
            dbConnection,
            alertsWithNoResponseFromFirstAdminDetails,
            allServicesDictionary);

        var alertsWithNoResponseFromSecondAdminDetails =
            (await GetAlertDetailsForWhichSecondAdminDidNotRespondInTime(dbConnection)).ToList();

        _logger.LogInformation(
            "There are {Count} alerts with no response from the second admin",
            alertsWithNoResponseFromSecondAdminDetails.Count);

        await HandleAlertsWithNoResponseFromSecondAdmin(dbConnection, alertsWithNoResponseFromSecondAdminDetails);
    }

    private static async Task<IEnumerable<MonitoredService>> GetAllServices(MySqlConnection connection)
    {
        const string query = $"SELECT * FROM {ServicesTable}";
        return await connection.QueryAsync<MonitoredService>(query);
    }

    private static async Task<IEnumerable<Alert>> GetAlertsDetailsForWhichFirstAdminDidNotRespondInTime(
        IDbConnection dbConnection)
    {
        const string query = $"""
                                 select a.*
                                  from {AlertsTable} a
                                           inner join {ServicesTable} s
                                                      on a.service_id = s.id
                                  where a.response_status = 'waiting_for_first_admin'
                                    and a.first_alert_time < date_sub(now(), interval (s.first_admin_allowed_response_time_ms * 1000) second)
                              """;

        var alerts = await dbConnection.QueryAsync<Alert>(query);

        return alerts;
    }

    private static async Task<IEnumerable<Alert>> GetAlertDetailsForWhichSecondAdminDidNotRespondInTime(
        IDbConnection dbConnection)
    {
        const string query = $"""
                                 select a.*
                                  from {AlertsTable} a
                                           inner join {ServicesTable} s
                                                      on a.service_id = s.id
                                  where a.response_status = 'waiting_for_second_admin'
                                    and a.second_alert_time < date_sub(now(), interval (s.second_admin_allowed_response_time_ms * 1000) second)
                              """;

        var alerts = await dbConnection.QueryAsync<Alert>(query);

        return alerts;
    }

    private async Task HandleAlertsWithNoResponseFromFirstAdmin(
        MySqlConnection connection,
        List<Alert> alerts,
        IReadOnlyDictionary<int, MonitoredService> servicesDictionary)
    {
        // 1. Update alerts details
        var now = DateTime.UtcNow;

        foreach (var alert in alerts)
        {
            alert.ResponseStatus = ResponseStatus.waiting_for_second_admin;
            alert.SecondAlertTime = now;
            alert.SecondLinkUUID = Guid.NewGuid().ToByteArray();
        }

        await UpdateAlertsDetails(connection, alerts);

        // 2. Send messages to the Pub/Sub topic to notify the second admin
        await PushAlertMessagesOnMessageBus(alerts, servicesDictionary);
    }

    private static async Task HandleAlertsWithNoResponseFromSecondAdmin(
        MySqlConnection connection,
        List<Alert> alerts)
    {
        foreach (var alert in alerts)
        {
            alert.ResponseStatus = ResponseStatus.ignored;
        }

        await UpdateAlertsDetails(connection, alerts);
    }

    private static async Task UpdateAlertsDetails(
        MySqlConnection connection,
        IEnumerable<Alert> alerts)
    {
        await using var command = new MySqlCommand(
            $"update {AlertsTable} set response_status = @ResponseStatus, second_alert_time = @SecondAlertTime, second_link_uuid = @SecondLinkUUID where id = @Id",
            connection);

        command.Parameters.Add("@Id", MySqlDbType.Int32);
        command.Parameters.Add("@ResponseStatus", MySqlDbType.Int32);
        command.Parameters.Add("@SecondLinkUUID", MySqlDbType.Binary);
        command.Parameters.Add("@SecondAlertTime", MySqlDbType.DateTime);

        foreach (var alert in alerts)
        {
            command.Parameters["@Id"].Value = alert.Id;
            command.Parameters["@ResponseStatus"].Value = (int)alert.ResponseStatus;
            command.Parameters["SecondLinkUUID"].Value = alert.SecondLinkUUID;
            command.Parameters["@SecondAlertTime"].Value = alert.SecondAlertTime;

            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task PushAlertMessagesOnMessageBus(
        IEnumerable<Alert> alerts,
        IReadOnlyDictionary<int, MonitoredService> servicesDictionary)
    {
        var messages = alerts.Select(x => BuildMessagesForAlertForSecondAdmin(x, servicesDictionary[x.ServiceId]))
            .ToList();

        var tasks = messages.SelectMany(x => x).Select(
            message => Task.Run(
                async () =>
                {
                    await _messagesPublisherClient.PublishAsync(JsonSerializer.Serialize(message));
                }));

        await Task.WhenAll(tasks);
    }

    private static List<SendMessageRequest> BuildMessagesForAlertForSecondAdmin(Alert alert, MonitoredService service)
    {
        var messages = new List<SendMessageRequest>();

        if (service.SecondAdminSendEmail)
        {
            var message = new SendMessageRequest
            {
                Subject = $"[Alert] Service {service.Url} is down",
                Body =
                    $" (second admin) Attention! Service {service.Url} is down. Please check it. To resolve alert, click this link: {string.Format(LinkTemplate, new Guid(alert.SecondLinkUUID).ToString())}",
                To = service.SecondAdminEmail,
                ChannelType = MessageChannelType.Email
            };

            messages.Add(message);
        }

        if (service.SecondAdminSendSms)
        {
            var message = new SendMessageRequest
            {
                Subject = $"[Alert] Service {service.Url} is down",
                Body =
                    $" (second admin) Attention! Service {service.Url} is down. Please check it. To resolve alert, click this link: {string.Format(LinkTemplate, new Guid(alert.SecondLinkUUID).ToString())}",
                To = service.SecondAdminPhoneNumber,
                ChannelType = MessageChannelType.Sms
            };

            messages.Add(message);
        }

        var logMessage = new SendMessageRequest
        {
            Subject = $"[Alert] Service {service.Url} is down",
            Body =
                $" (second admin) Attention! Service {service.Url} is down. Please check it. To resolve alert, click this link: {string.Format(LinkTemplate, new Guid(alert.SecondLinkUUID).ToString())}",
            ChannelType = MessageChannelType.Log
        };

        messages.Add(logMessage);

        return messages;
    }

    private Task<MySqlConnection> GetDbConnection() =>
        Task.FromResult(new MySqlConnection(GetMySqlDbConnectionString()));

    private string GetMySqlDbConnectionString() => GetSecret("DB_CONNECTION_STRING");

    private string GetSecret(string secretId, string versionId = "latest")
    {
        var client = SecretManagerServiceClient.Create();
        var secretVersionName = new SecretVersionName(_projectId, secretId, versionId);
        var result = client.AccessSecretVersion(secretVersionName);
        return result.Payload.Data.ToStringUtf8();
    }
}