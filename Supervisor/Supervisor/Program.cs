using Dapper;
using Supervisor.Configuration;
using Supervisor.Infra;
using Supervisor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .Configure<SupervisorConfiguration>(
        builder.Configuration.GetSection("SupervisorConfiguration"));

builder.Services.AddSingleton<JobsCostCalculatorService>();
builder.Services.AddSingleton<MessageBusService>();
builder.Services.AddSingleton<ISecretManagerService, SecretManagerService>();
builder.Services.AddSingleton<IMonitoredServicesRepository, MonitoredServicesRepository>();

builder.SetupJobDistributionWorker();
DefaultTypeMap.MatchNamesWithUnderscores = true;

var app = builder.Build();
app.MapGet("/", () => "app works!");
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();

app.Run();