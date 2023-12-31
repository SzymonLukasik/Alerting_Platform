namespace Supervisor.IntegrationTests;

using System.Collections.Concurrent;
using Google.Cloud.PubSub.V1;
using Models.Consts;

public class MessageBusServiceTests
{
    [Fact]
    public async Task CanCreateTopic()
    {
        var messageBusService = CommonSetup.SetupMessageBusService();

        var topic = await messageBusService.CreateTopic(AppTopic.Tasks);

        Assert.NotNull(topic);
    }

    [Fact]
    public async Task CanDeleteTopic()
    {
        var messageBusService = CommonSetup.SetupMessageBusService();

        await messageBusService.DeleteTopic(AppTopic.Tasks);

        Assert.True(true);
    }

    [Fact]
    public async Task CanCreateSubscription()
    {
        var messageBusService = CommonSetup.SetupMessageBusService();

        var subscription = await messageBusService.CreateSubscription(AppTopic.Tasks);

        Assert.NotNull(subscription);
    }

    [Fact]
    public async Task CanDeleteSubscription()
    {
        var messageBusService = CommonSetup.SetupMessageBusService();

        await messageBusService.DeleteSubscription(AppTopic.Tasks);

        Assert.True(true);
    }

    [Fact]
    public async Task CanPublishMessage()
    {
        var messageBusService = CommonSetup.SetupMessageBusService();

        await messageBusService.PublishMessage(AppTopic.Tasks, "test");

        Assert.True(true);
    }

    [Fact]
    public async Task CanPublishRichMessage()
    {
        var messageBusService = CommonSetup.SetupMessageBusService();
        var richMessageInstance = new RichMessage("John", 35, false);

        await messageBusService.PublishMessage(AppTopic.Tasks, richMessageInstance);

        Assert.True(true);
    }

    [Fact]
    public async Task CanPublishAndPullRichMessage()
    {
        var messageBusService = CommonSetup.SetupMessageBusService();
        var richMessageInstance = new RichMessage("John", Random.Shared.Next(50, 90), false);
        await messageBusService.PublishMessage(AppTopic.Tasks, richMessageInstance);
        var message = await messageBusService.GetMessage<RichMessage>(AppTopic.Tasks);

        Assert.Equal(richMessageInstance, message);
    }

    [Fact]
    public async Task CanPublishAndConsumeSubscription()
    {
        var messageBusService = CommonSetup.SetupMessageBusService();
        var messagesToSend = new List<RichMessage>
        {
            new("John", 35, false), new("Fryderyk", 71, true), new("Marek", 25, false)
        };
        var receivedMessages = new ConcurrentBag<RichMessage>();

        await messageBusService.Subscribe(
            AppTopic.Tasks,
            (RichMessage message) =>
            {
                receivedMessages.Add(message);

                return Task.FromResult(SubscriberClient.Reply.Ack);
            });

        foreach (var message in messagesToSend)
        {
            await messageBusService.PublishMessage(AppTopic.Tasks, message);
        }

        await Task.Delay(2000);

        Assert.Equal(messagesToSend.Count, receivedMessages.Count);
        Assert.True(messagesToSend.All(x => receivedMessages.Contains(x)));
    }
}

public record RichMessage(string Name, int Age, bool Deleted);