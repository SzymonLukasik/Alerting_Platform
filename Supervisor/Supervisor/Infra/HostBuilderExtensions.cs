namespace Supervisor.Infra;

using Configuration;
using CronJobService;
using Workers;

public static class HostBuilderExtensions
{
    public static WebApplicationBuilder SetupJobDistributionWorker(this WebApplicationBuilder builder)
    {
        var intervalMinutes = GetWorkerIntervalMinutes(builder);

        if (60 % intervalMinutes != 0)
        {
            throw new ArgumentException(
                $"Interval minutes must be a divisor of 60. {intervalMinutes} is not a divisor of 60.");
        }

        builder.Host.AddCronJob<JobDistributionWorker>(
            options =>
            {
                options.CronExpression = $"*/{intervalMinutes} * * * *";
                options.InstantJob = true; // run instantly on startup
            });

        return builder;
    }

    private static int GetWorkerIntervalMinutes(WebApplicationBuilder builder)
    {
        var config = builder.Configuration.GetSection("SupervisorConfiguration").Get<SupervisorConfiguration>();
        return config.JobDistributionWorkerIntervalMinutes;
    }
}