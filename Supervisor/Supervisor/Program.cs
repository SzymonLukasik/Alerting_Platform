using Supervisor.Configuration;
using Supervisor.Infra.CronJobService;
using Supervisor.Services;
using Supervisor.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .Configure<MonitoredHttpServicesConfiguration>(
        builder.Configuration.GetSection("MonitoredHttpServicesConfiguration"))
    .Configure<SupervisorConfiguration>(
        builder.Configuration.GetSection("SupervisorConfiguration"));

builder.Services.AddSingleton<JobsCostCalculatorService>();
builder.Services.AddSingleton<MessageBusService>();

builder.Host
    .AddCronJob<JobDistributionWorker>(
        options =>
        {
            options.CronExpression = "*/5 * * * *"; // every 5 minutes
            options.InstantJob = true; // run instantly on startup
        });

builder.Services.AddHealthChecks();

var app = builder.Build();
app.MapGet("/", () => "hey hi hello!");
app.MapHealthChecks("/health");

app.Run();