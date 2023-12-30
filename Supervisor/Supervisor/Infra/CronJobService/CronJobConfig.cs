namespace Supervisor.Infra.CronJobService;

public class CronJobConfig<T> where T : CronJobService
{
    public string CronExpression { get; set; }

    public bool InstantJob { get; set; }
}