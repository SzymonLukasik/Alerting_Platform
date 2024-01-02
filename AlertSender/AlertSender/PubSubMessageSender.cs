namespace AlertSender;

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Google.Cloud.Functions.Framework;
using Google.Events.Protobuf.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;
using RestSharp;
using RestSharp.Authenticators;

public class PubSubMessageSender : ICloudEventFunction<MessagePublishedData>
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true
    };

    private readonly ILogger<PubSubMessageSender> _logger;
    private readonly string _mailgunApiKey;

    public PubSubMessageSender(ILogger<PubSubMessageSender> logger)
    {
        _logger = logger;
        _mailgunApiKey = Environment.GetEnvironmentVariable("MAILGUN_API_KEY");
    }

    public async Task HandleAsync(CloudEvent cloudEvent, MessagePublishedData data, CancellationToken cancellationToken)
    {
        SendMessageRequest sendMessageRequest;

        try
        {
            var jsonMessage = data.Message.Data.ToStringUtf8();
            sendMessageRequest = JsonSerializer.Deserialize<SendMessageRequest>(jsonMessage, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize message body - expected a SendMessageRequest");

            throw;
        }

        if (sendMessageRequest == null)
        {
            _logger.LogError("Failed to deserialize message body - expected a SendMessageRequest");

            throw new Exception("Failed to deserialize message body - expected a SendMessageRequest");
        }

        _logger.LogInformation(
            "Received SendEmailReqeust: ChannelType: {ChannelType}, To: {To}, Subject: {Subject}, Body: {Body}",
            sendMessageRequest.ChannelType,
            sendMessageRequest.To,
            sendMessageRequest.Subject,
            sendMessageRequest.Body);

        if (sendMessageRequest.ChannelType == MessageChannelType.Email)
        {
            await SendEmail(sendMessageRequest);
        }
    }

    private async Task SendEmail(SendMessageRequest sendMessageRequest)
    {
        var client = new RestClient(
            "https://api.mailgun.net/v3",
            options =>
            {
                options.Authenticator =
                    new HttpBasicAuthenticator(
                        "api",
                        _mailgunApiKey);
            });

        var request = new RestRequest();
        request.AddParameter("domain", "sandboxddcea4df70ca41a1868461fab3c9920f.mailgun.org", ParameterType.UrlSegment);
        request.Resource = "{domain}/messages";
        request.AddParameter(
            "from",
            "Mailgun Sandbox <postmaster@sandboxddcea4df70ca41a1868461fab3c9920f.mailgun.org>");
        request.AddParameter("to", sendMessageRequest.To);
        request.AddParameter("subject", sendMessageRequest.Subject);
        request.AddParameter("text", sendMessageRequest.Body);
        request.Method = Method.Post;
        var result = await client.ExecuteAsync(request);

        if (!result.IsSuccessful)
        {
            _logger.LogError(
                "Failed to send email to {To}: {StatusCode} {StatusDescription} {Content}",
                sendMessageRequest.To,
                result.StatusCode,
                result.StatusDescription,
                result.Content);

            return;
        }

        _logger.LogInformation("Sent email to {To}", sendMessageRequest.To);
    }
}