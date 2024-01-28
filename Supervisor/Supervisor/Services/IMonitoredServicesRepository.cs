namespace Supervisor.Services;

using Models.DbModels;

public interface IMonitoredServicesRepository
{
    Task<IEnumerable<MonitoredService>> GetAllAsync();

    Task<MonitoredService> GetByIdAsync(int id);

    Task AddAsync(MonitoredService monitoredService);

    Task UpdateAsync(MonitoredService monitoredService);

    Task DeleteAsync(int id);
}