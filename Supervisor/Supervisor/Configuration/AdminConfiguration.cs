namespace Supervisor.Configuration;

using AlertSender;

public class AdminConfiguration
{
    public List<MessageChannelType> MessageChannelTypes { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    public string PhoneNumber { get; set; }
}