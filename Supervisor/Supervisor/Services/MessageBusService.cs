namespace Supervisor.Services;

using System.Text.Json;
using Configuration;
using Google.Cloud.PubSub.V1;
using Microsoft.Extensions.Options;
using Models.Consts;

public class MessageBusService
{
    private readonly ILogger<MessageBusService> _logger;
    private readonly SubscriberServiceApiClient _subscriberServiceApiClient;
    private readonly PublisherClient _tasksPublisherClient;
    private readonly SubscriberClient _tasksSubscriberClient;
    private readonly SubscriptionName _tasksSubscriptionName;
    private readonly TopicName _tasksTopicName;

    public MessageBusService(
        ILogger<MessageBusService> logger,
        IOptions<SupervisorConfiguration> supervisorConfiguration)
    {
        _logger = logger;
        var supervisorConfiguration1 = supervisorConfiguration.Value;
        _tasksTopicName = new TopicName(supervisorConfiguration1.ProjectName, supervisorConfiguration1.TasksTopic);
        _tasksPublisherClient = PublisherClient.Create(_tasksTopicName);
        _tasksSubscriptionName = new SubscriptionName(
            supervisorConfiguration1.ProjectName,
            supervisorConfiguration1.TasksSubscription);
        _tasksSubscriberClient = SubscriberClient.Create(_tasksSubscriptionName);
        _subscriberServiceApiClient = SubscriberServiceApiClient.Create();
    }

    public async Task<Topic> CreateTopic(AppTopic appTopic)
    {
        var publisherService = await PublisherServiceApiClient.CreateAsync();
        var topicName = appTopic switch
        {
            AppTopic.Tasks => _tasksTopicName,
            _ => throw new ArgumentOutOfRangeException(nameof(appTopic), appTopic, null)
        };

        return await publisherService.CreateTopicAsync(topicName);
    }

    public async Task DeleteTopic(AppTopic appTopic)
    {
        var publisherService = await PublisherServiceApiClient.CreateAsync();
        var topicName = appTopic switch
        {
            AppTopic.Tasks => _tasksTopicName,
            _ => throw new ArgumentOutOfRangeException(nameof(appTopic), appTopic, null)
        };

        await publisherService.DeleteTopicAsync(topicName);
    }

    public async Task<Subscription> CreateSubscription(AppTopic appTopic)
    {
        var subscriberService = await SubscriberServiceApiClient.CreateAsync();
        var subscriptionName = appTopic switch
        {
            AppTopic.Tasks => _tasksSubscriptionName,
            _ => throw new ArgumentOutOfRangeException(nameof(appTopic), appTopic, null)
        };
        var topicName = appTopic switch
        {
            AppTopic.Tasks => _tasksTopicName,
            _ => throw new ArgumentOutOfRangeException(nameof(appTopic), appTopic, null)
        };

        return await subscriberService.CreateSubscriptionAsync(subscriptionName, topicName, null, 60);
    }

    public async Task DeleteSubscription(AppTopic appTopic)
    {
        var subscriberService = await SubscriberServiceApiClient.CreateAsync();
        var subscriptionName = appTopic switch
        {
            AppTopic.Tasks => _tasksSubscriptionName,
            _ => throw new ArgumentOutOfRangeException(nameof(appTopic), appTopic, null)
        };

        await subscriberService.DeleteSubscriptionAsync(subscriptionName);
    }

    public async Task PublishMessage<T>(AppTopic appTopic, T message)
    {
        var jsonMessage = JsonSerializer.Serialize(message);
        var client = appTopic switch
        {
            AppTopic.Tasks => _tasksPublisherClient,
            _ => throw new ArgumentOutOfRangeException(nameof(appTopic), appTopic, null)
        };

        await client.PublishAsync(jsonMessage);
    }

    public async Task<T> GetMessage<T>(AppTopic appTopic)
    {
        var messages = await _subscriberServiceApiClient.PullAsync(
            new PullRequest { MaxMessages = 1, SubscriptionAsSubscriptionName = _tasksSubscriptionName });

        if (messages.ReceivedMessages.Count == 0)
        {
            return default;
        }

        var message = messages.ReceivedMessages.First();
        var jsonMessage = message.Message.Data.ToStringUtf8();
        var messageObject = JsonSerializer.Deserialize<T>(jsonMessage);

        await _subscriberServiceApiClient.AcknowledgeAsync(
            new AcknowledgeRequest
            {
                SubscriptionAsSubscriptionName = _tasksSubscriptionName, AckIds = { message.AckId }
            });

        return messageObject;
    }

    public async Task Subscribe<T>(AppTopic appTopic, Func<T, Task<SubscriberClient.Reply>> messageHandler)
    {
        var client = appTopic switch
        {
            AppTopic.Tasks => _tasksSubscriberClient,
            _ => throw new ArgumentOutOfRangeException(nameof(appTopic), appTopic, null)
        };

        client.StartAsync(
            async (message, cancellationToken) =>
            {
                var jsonMessage = message.Data.ToStringUtf8();
                var messageObject = JsonSerializer.Deserialize<T>(jsonMessage);
                return await messageHandler(messageObject);
            });
    }
}