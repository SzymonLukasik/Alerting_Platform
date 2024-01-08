public class HealthCheckerConfiguration {
    public String projectID = "irio-402819";
    public String subscriptionID = "tasks-sub";
    public String connectionString = "jdbc:mysql://34.34.131.205:3306/alerting";
    public String dbUser = "root";
    public String dbPassword = "FILL";
    public Integer maxResourceCost = 100;
    public Integer dbWriteBatchSize = 100;
    public Integer dbMinimumWriteDelayMs = 5000;
}
