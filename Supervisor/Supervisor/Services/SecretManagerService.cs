using Google.Cloud.SecretManager.V1;
using Microsoft.Extensions.Options;
using Supervisor.Configuration;
using Supervisor.Services;

public class SecretManagerService : ISecretManagerService
{
    private readonly SecretManagerServiceClient _client;
    private readonly SupervisorConfiguration _supervisorConfiguration;

    public SecretManagerService(IOptions<SupervisorConfiguration> supervisorConfiguration)
    {
        _supervisorConfiguration = supervisorConfiguration.Value;
        _client = SecretManagerServiceClient.Create();
    }

    public string GetMySqlDbConnectionString() => GetSecret("DB_CONNECTION_STRING");

    private string GetSecret(string secretId, string versionId = "latest")
    {
        var secretVersionName = new SecretVersionName(_supervisorConfiguration.ProjectName, secretId, versionId);
        var result = _client.AccessSecretVersion(secretVersionName);
        return result.Payload.Data.ToStringUtf8();
    }
}