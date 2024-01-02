namespace AlertSender;

public class SendMessageRequest
{
    public MessageChannelType ChannelType { get; set; }

    public string To { get; set; }

    public string Subject { get; set; }

    public string Body { get; set; }
}