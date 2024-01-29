namespace AlertSender;

using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Google.Cloud.Functions.Framework;
using Google.Cloud.SecretManager.V1;
using Google.Events.Protobuf.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;
using RestSharp;
using RestSharp.Authenticators;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

public class PubSubMessageSender : ICloudEventFunction<MessagePublishedData>
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true
    };

    private readonly ILogger<PubSubMessageSender> _logger;
    private readonly string _mailgunApiKey;
    private readonly string _projectId;
    private readonly string _twilioAccountSid;
    private readonly string _twilioAccountToken;

    public PubSubMessageSender(ILogger<PubSubMessageSender> logger)
    {
        _logger = logger;
        _projectId = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT");
        _mailgunApiKey = GetMailgunApiKey();
        _twilioAccountSid = GetTwilioAccountSid();
        _twilioAccountToken = GetTwilioAccountToken();
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
            "Received SendMessageRequest: ChannelType: {ChannelType}, To: {To}, Subject: {Subject}, Body: {Body}",
            sendMessageRequest.ChannelType,
            sendMessageRequest.To,
            sendMessageRequest.Subject,
            sendMessageRequest.Body);

        switch (sendMessageRequest.ChannelType)
        {
            case MessageChannelType.Email:
                await SendEmail(sendMessageRequest);
                break;
            case MessageChannelType.Sms:
                await SendSms(sendMessageRequest);
                break;
            case MessageChannelType.Log:
                break;
            default:
                throw new ArgumentOutOfRangeException();
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

    private async Task SendSms(SendMessageRequest sendMessageRequest)
    {
        TwilioClient.Init(_twilioAccountSid, _twilioAccountToken);

        var message = await MessageResource.CreateAsync(
            body: sendMessageRequest.Body,
            from: new PhoneNumber("+12532168738"),
            to: new PhoneNumber(sendMessageRequest.To)
        );

        if (message.ErrorCode != null)
        {
            _logger.LogError(
                "Failed to send SMS to {To}: {ErrorCode} {ErrorMessage}",
                sendMessageRequest.To,
                message.ErrorCode,
                message.ErrorMessage);
        }

        _logger.LogInformation("Sent SMS to {To}", sendMessageRequest.To);
    }

    public string GetMailgunApiKey() => GetSecret("MAILGUN_API_KEY");

    public string GetTwilioAccountSid() => GetSecret("TWILIO_ACCOUNT_SID");

    public string GetTwilioAccountToken() => GetSecret("TWILIO_AUTH_TOKEN");

    private string GetSecret(string secretId, string versionId = "latest")
    {
        var client = SecretManagerServiceClient.Create();
        var secretVersionName = new SecretVersionName(_projectId, secretId, versionId);
        var result = client.AccessSecretVersion(secretVersionName);
        return result.Payload.Data.ToStringUtf8();
    }
}