namespace Supervisor.IntegrationTests;

using Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Services;

public class CommonSetup
{
    public static MessageBusService SetupMessageBusService()
    {
        var configuration = SetupConfigurationFromAppsettings();
        var supervisorConfiguration =
            configuration.GetSection("SupervisorConfiguration").Get<SupervisorConfiguration>();
        var supervisorConfigurationOptions = Options.Create(supervisorConfiguration);
        var mockLogger = new Mock<ILogger<MessageBusService>>();
        var messageBusService = new MessageBusService(mockLogger.Object, supervisorConfigurationOptions);

        return messageBusService;
    }

    public static IConfiguration SetupConfigurationFromAppsettings()
    {
        Environment.SetEnvironmentVariable(
            "GOOGLE_APPLICATION_CREDENTIALS",
            "/Users/rotten/.config/gcloud/application_default_credentials.json");

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        return configuration;
    }
}