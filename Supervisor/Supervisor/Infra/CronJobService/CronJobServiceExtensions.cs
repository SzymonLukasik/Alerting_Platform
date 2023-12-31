namespace Supervisor.Infra.CronJobService;

public static class CronJobServiceExtensions
{
    public static IHostBuilder AddCronJob<T>(this IHostBuilder builder, Action<CronJobConfig<T>> options)
        where T : CronJobService
    {
        var config = new CronJobConfig<T>();
        options(config);

        if (string.IsNullOrWhiteSpace(config.CronExpression))
        {
            throw new ArgumentNullException(
                nameof(CronJobConfig<T>.CronExpression),
                @"Empty Cron Expression is not allowed.");
        }

        builder.ConfigureServices(
            (context, services) =>
            {
                services.AddSingleton(config);
                services.AddHostedService<T>();
            });

        return builder;
    }
}