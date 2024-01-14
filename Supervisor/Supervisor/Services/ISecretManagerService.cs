namespace Supervisor.Services;

public interface ISecretManagerService
{
    public string GetMySqlDbConnectionString();
}