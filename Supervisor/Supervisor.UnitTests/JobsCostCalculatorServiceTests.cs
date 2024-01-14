namespace Supervisor.UnitTests;

using Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Services;

public class JobsCostCalculatorServiceTests
{
    private JobsCostCalculatorService GetJobsCostCalculatorServiceInstance()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .Build();
        var monitoredHttpServicesConfiguration = new MonitoredHttpServicesConfiguration
        {
            MonitoredHttpServices = new List<MonitoredHttpServiceConfiguration>()
        };

        var monitoredHttpServicesConfigurationOptions =
            Options.Create(monitoredHttpServicesConfiguration);

        return new JobsCostCalculatorService(configuration, monitoredHttpServicesConfigurationOptions);
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