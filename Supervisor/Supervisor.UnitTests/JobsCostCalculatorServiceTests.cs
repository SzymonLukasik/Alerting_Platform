namespace Supervisor.UnitTests;

using Configuration;
using Microsoft.Extensions.Options;
using Models.DbModels;
using Moq;
using Services;

public class JobsCostCalculatorServiceTests
{
    private JobsCostCalculatorService GetJobsCostCalculatorServiceInstance(
        double resourceCostFrequencyFactor,
        double resourceCostResponseTimeFactor)
    {
        var secretManagerServiceMock = new Mock<ISecretManagerService>();
        secretManagerServiceMock.Setup(x => x.GetMySqlDbConnectionString()).Returns("");

        var supervisorConfiguration = new SupervisorConfiguration
        {
            ResourceCostFrequencyFactor = resourceCostFrequencyFactor,
            ResourceCostResponseTimeFactor = resourceCostResponseTimeFactor
        };

        var supervisorConfigurationOptions = Options.Create(supervisorConfiguration);

        return new JobsCostCalculatorService(
            secretManagerServiceMock.Object,
            supervisorConfigurationOptions);
    }

    [Fact]
    public void CalculateServiceResourceCost_WhenFrequencyMsIsZero_ThrowsException()
    {
        var jobsCalculatorService = GetJobsCostCalculatorServiceInstance(1, 1);
        var monitoredService = new MonitoredService { FrequencyMs = 0 };

        Assert.Throws<Exception>(
            () => jobsCalculatorService.CalculateServiceResourceCost(monitoredService, 0));
    }

    [Fact]
    public void CalculateServiceResourceCost_WhenFrequencyMsIsGreaterThanZero_ReturnsCorrectValue1()
    {
        var jobsCalculatorService = GetJobsCostCalculatorServiceInstance(1, 1);
        var monitoredService = new MonitoredService { FrequencyMs = 1000 };

        var result = jobsCalculatorService.CalculateServiceResourceCost(monitoredService, 1);

        Assert.Equal((uint)1, result);
    }

    [Fact]
    public void CalculateServiceResourceCost_WhenFrequencyMsIsGreaterThanZero_ReturnsCorrectValue2()
    {
        var jobsCalculatorService = GetJobsCostCalculatorServiceInstance(1000, 0);
        var monitoredService = new MonitoredService { FrequencyMs = 500 };

        var result = jobsCalculatorService.CalculateServiceResourceCost(monitoredService, 0);

        Assert.Equal((uint)2, result);
    }
}