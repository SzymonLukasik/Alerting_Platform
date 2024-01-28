namespace Supervisor.Services;

using System.Data;
using Dapper;
using Models.DbModels;
using MySqlConnector;

public class MonitoredServicesRepository : IMonitoredServicesRepository
{
    private const string TableName = "alerting.monitored_services";
    private readonly IDbConnection _dbConnection;

    public MonitoredServicesRepository(
        ISecretManagerService secretManagerService)
    {
        var connectionString = secretManagerService.GetMySqlDbConnectionString();
        _dbConnection = new MySqlConnection(connectionString);
    }

    public async Task<IEnumerable<MonitoredService>> GetAllAsync()
    {
        const string sql = $"SELECT * FROM {TableName}";
        return await _dbConnection.QueryAsync<MonitoredService>(sql);
    }

    public async Task<MonitoredService> GetByIdAsync(int id)
    {
        const string sql = $"SELECT * FROM {TableName} WHERE id = @Id";
        return await _dbConnection.QuerySingleOrDefaultAsync<MonitoredService>(sql, new { Id = id });
    }

    public async Task AddAsync(MonitoredService monitoredService)
    {
        const string sql =
            $"INSERT INTO {TableName} (url, timeout_ms, frequency_ms, alerting_window_ms, expected_availability, first_admin_allowed_response_time_ms, first_admin_send_email, first_admin_send_sms, first_admin_name, first_admin_email, first_admin_phone_number, second_admin_allowed_response_time_ms, second_admin_send_email, second_admin_send_sms, second_admin_name, second_admin_email, second_admin_phone_number) VALUES (@Url, @TimeoutMs, @FrequencyMs, @AlertingWindowMs, @ExpectedAvailability, @FirstAdminAllowedResponseTimeMs, @FirstAdminSendEmail, @FirstAdminSendSms, @FirstAdminName, @FirstAdminEmail, @FirstAdminPhoneNumber, @SecondAdminAllowedResponseTimeMs, @SecondAdminSendEmail, @SecondAdminSendSms, @SecondAdminName, @SecondAdminEmail, @SecondAdminPhoneNumber)";
        await _dbConnection.ExecuteAsync(sql, monitoredService);
    }

    public async Task UpdateAsync(MonitoredService monitoredService)
    {
        const string sql =
            $"UPDATE {TableName} SET url = @Url, timeout_ms = @TimeoutMs, frequency_ms = @FrequencyMs, alerting_window_ms = @AlertingWindowMs, expected_availability = @ExpectedAvailability, first_admin_allowed_response_time_ms = @FirstAdminAllowedResponseTimeMs, first_admin_send_email = @FirstAdminSendEmail, first_admin_send_sms = @FirstAdminSendSms, first_admin_name = @FirstAdminName, first_admin_email = @FirstAdminEmail, first_admin_phone_number = @FirstAdminPhoneNumber, second_admin_allowed_response_time_ms = @SecondAdminAllowedResponseTimeMs, second_admin_send_email = @SecondAdminSendEmail, second_admin_send_sms = @SecondAdminSendSms, second_admin_name = @SecondAdminName, second_admin_email = @SecondAdminEmail, second_admin_phone_number = @SecondAdminPhoneNumber WHERE id = @Id";
        await _dbConnection.ExecuteAsync(sql, monitoredService);
    }

    public async Task DeleteAsync(int id)
    {
        const string sql = $"DELETE FROM {TableName} WHERE id = @Id";
        await _dbConnection.ExecuteAsync(sql, new { Id = id });
    }
}