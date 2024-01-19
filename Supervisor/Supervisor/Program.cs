using Supervisor.Configuration;
using Supervisor.Infra;
using Supervisor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .Configure<MonitoredHttpServicesConfiguration>(
        builder.Configuration.GetSection("MonitoredHttpServicesConfiguration"))
    .Configure<SupervisorConfiguration>(
        builder.Configuration.GetSection("SupervisorConfiguration"));

builder.Services.AddSingleton<JobsCostCalculatorService>();
builder.Services.AddSingleton<MessageBusService>();
builder.Services.AddSingleton<ISecretManagerService, SecretManagerService>();

builder.SetupJobDistributionWorker();

var app = builder.Build();
app.MapGet("/", () => "app works!");

app.Run();