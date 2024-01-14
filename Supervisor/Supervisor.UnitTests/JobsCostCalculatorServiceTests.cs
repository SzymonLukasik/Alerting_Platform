namespace Supervisor.UnitTests;

using Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Services;

public class JobsCostCalculatorServiceTests
{
    private JobsCostCalculatorService GetJobsCostCalculatorServiceInstance()
    {
        var secretManagerServiceMock = new Mock<ISecretManagerService>();
        secretManagerServiceMock.Setup(x => x.GetMySqlDbConnectionString()).Returns("");

        var monitoredHttpServicesConfiguration = new MonitoredHttpServicesConfiguration
        {
            MonitoredHttpServices = new List<MonitoredHttpServiceConfiguration>()
        };

        var monitoredHttpServicesConfigurationOptions =
            Options.Create(monitoredHttpServicesConfiguration);

        return new JobsCostCalculatorService(
            monitoredHttpServicesConfigurationOptions,
            secretManagerServiceMock.Object);
    }

    [Fact]
    public void CalculateServiceResourceCost_WhenFrequencyMsIsZero_ThrowsException()
    {
        var jobsCalculatorService = GetJobsCostCalculatorServiceInstance();
        var monitoredHttpServicesConfiguration = new MonitoredHttpServiceConfiguration
        {
            Url = "wp.pl", FrequencyMs = 0
        };

        Assert.Throws<Exception>(
            () => jobsCalculatorService.CalculateServiceResourceCost(monitoredHttpServicesConfiguration, 0));
    }

    [Fact]
    public void CalculateServiceResourceCost_WhenFrequencyMsIsGreaterThanZero_ReturnsCorrectValue1()
    {
        var jobsCalculatorService = GetJobsCostCalculatorServiceInstance();
        var monitoredHttpServicesConfiguration = new MonitoredHttpServiceConfiguration
        {
            Url = "wp.pl", FrequencyMs = 1000
        };

        var result = jobsCalculatorService.CalculateServiceResourceCost(monitoredHttpServicesConfiguration, 0);

        Assert.Equal((uint)1, result);
    }

    [Fact]
    public void CalculateServiceResourceCost_WhenFrequencyMsIsGreaterThanZero_ReturnsCorrectValue2()
    {
        var jobsCalculatorService = GetJobsCostCalculatorServiceInstance();
        var monitoredHttpServicesConfiguration = new MonitoredHttpServiceConfiguration
        {
            Url = "wp.pl", FrequencyMs = 500
        };

        var result = jobsCalculatorService.CalculateServiceResourceCost(monitoredHttpServicesConfiguration, 0);

        Assert.Equal((uint)2, result);
    }
}