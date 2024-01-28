namespace Supervisor.Mock;

using Models.DbModels;
using Services;

public static class MockServices
{
    public static async Task GenerateAndAddServices(IMonitoredServicesRepository repository)
    {
        var random = new Random();

        for (var i = 0; i < 2000; i++)
        {
            var url = random.Next(0, 2) == 0 ? $"http://wp.pl/test{i}" : $"http://fakeservice{i}.com";
            var service = new MonitoredService
            {
                Id = i,
                Url = url,
                TimeoutMs = random.Next(1000, 5000),
                FrequencyMs = random.Next(1000, 5000),
                AlertingWindowMs = random.Next(1000, 5000),
                ExpectedAvailability = random.NextDouble(),
                FirstAdminAllowedResponseTimeMs = random.Next(1000, 5000),
                FirstAdminSendEmail = random.Next(0, 2) == 0,
                FirstAdminSendSms = random.Next(0, 2) == 0,
                FirstAdminName = $"Admin{i}",
                FirstAdminEmail = $"admin{i}@service.com",
                FirstAdminPhoneNumber = $"123-456-789{i}",
                SecondAdminAllowedResponseTimeMs = random.Next(1000, 5000),
                SecondAdminSendEmail = random.Next(0, 2) == 0,
                SecondAdminSendSms = random.Next(0, 2) == 0,
                SecondAdminName = $"Admin{i + 1}",
                SecondAdminEmail = $"admin{i + 1}@service.com",
                SecondAdminPhoneNumber = $"123-456-789{i + 1}"
            };

            await repository.AddAsync(service);
        }
    }
}