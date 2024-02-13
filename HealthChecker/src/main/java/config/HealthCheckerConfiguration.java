package config;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.annotation.JsonProperty;
import com.fasterxml.jackson.core.type.TypeReference;
import java.io.File;
import java.io.IOException;
import java.util.HashMap;
import java.util.Map;
import service.SecretManager;

public class HealthCheckerConfiguration {
    private String projectID;
    private String subscriptionID;
    private String connectionString;
    private String dbUser;
    private String dbPassword;
    private Integer maxResourceCost;
    private Integer dbWriteBatchSize;
    private Integer dbMinimumWriteDelayMs;

    private static final String CONFIGS_PATH = "config/health_checker.json";

    public HealthCheckerConfiguration(@JsonProperty("projectID") String projectID, @JsonProperty("subscriptionID") String subscriptionID, @JsonProperty("connectionString") String connectionString, @JsonProperty("dbUser") String dbUser, @JsonProperty("dbPassword") String dbPassword, @JsonProperty("maxResourceCost") Integer maxResourceCost, @JsonProperty("dbWriteBatchSize") Integer dbWriteBatchSize, @JsonProperty("dbMinimumWriteDelayMs") Integer dbMinimumWriteDelayMs) {
        this.projectID = projectID;
        this.subscriptionID = subscriptionID;
        this.connectionString = connectionString;
        this.dbUser = dbUser;
        this.dbPassword = dbPassword;
        this.maxResourceCost = maxResourceCost;
        this.dbWriteBatchSize = dbWriteBatchSize;
        this.dbMinimumWriteDelayMs = dbMinimumWriteDelayMs;
    }

    public String getProjectID() {
        return projectID;
    }

    public String getSubscriptionID() {
        return subscriptionID;
    }

    public String getConnectionString() {
        return connectionString;
    }

    public String getDbUser() {
        return dbUser;
    }

    public String getDbPassword() {
        return dbPassword;
    }

    public Integer getMaxResourceCost() {
        return maxResourceCost;
    }

    public Integer getDbWriteBatchSize() {
        return dbWriteBatchSize;
    }

    public Integer getDbMinimumWriteDelayMs() {
        return dbMinimumWriteDelayMs;
    }

    public String toJson() {
        try {
            ObjectMapper mapper = new ObjectMapper();
            return mapper.writeValueAsString(this);
        } catch (JsonProcessingException e) {
            throw new RuntimeException(e);
        }
    }

    public HealthCheckerConfiguration fromJson(String json) {
        try {
            ObjectMapper mapper = new ObjectMapper();
            return mapper.readValue(json, HealthCheckerConfiguration.class);
        } catch (JsonProcessingException e) {
            throw new RuntimeException(e);
        }
    }

    private void fetchSecrets(SecretManager secretManager) {
        // check if any of the key starts with the word "SECRET"
        String newConnectionString = fetchSecret(this.connectionString, secretManager);
        if (newConnectionString != null) {
            connectionString = newConnectionString;
        }
        String newDbUser = fetchSecret(this.dbUser, secretManager);
        if (newDbUser != null) {
            dbUser = newDbUser;
        }
        String newDbPassword = fetchSecret(this.dbPassword, secretManager);
        if (newDbPassword != null) {
            dbPassword = newDbPassword;
        }
    }

    private String fetchSecret(String fieldValue, SecretManager secretManager) {
        // check if the secret name starts
        if (fieldValue.startsWith("SECRET")) {
            // the secret is of form "SECRET:SECRET_NAME"
            // split
            String secretName = fieldValue.split(":")[1];
            String secretValue = secretManager.getSecret(secretName);
            return secretValue;
        }
        return null;
    }

    public static Map<String, HealthCheckerConfiguration> loadConfigs()
    {
        // CONFIGS_PATH contains the path to the configuration file, which contains a map of configuration names to configuration objects

        // Load the configuration file as map
        Map<String, HealthCheckerConfiguration> configs = new HashMap<>();
        try {
            ObjectMapper mapper = new ObjectMapper();
            configs = mapper.readValue(new File(CONFIGS_PATH), new TypeReference<Map<String, HealthCheckerConfiguration>>() {});
            // for each configuration, fetch secrets
            SecretManager secretManager = new SecretManager();
            for (HealthCheckerConfiguration config : configs.values()) {
                config.fetchSecrets(secretManager);
            }
            return configs;
        } catch (IOException e) {
            throw new RuntimeException(e);
        }
    }
}
