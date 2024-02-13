package config;
import java.io.IOException;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.PropertyNamingStrategies;

import events.external.Admin;

import com.fasterxml.jackson.annotation.JsonCreator;
import com.fasterxml.jackson.annotation.JsonProperty;

public class MonitoredHttpServiceConfiguration {
    private Integer Id;
    private String Url;
    private Integer TimeoutMs;
    private Integer FrequencyMs;
    private Integer AlertingWindowMs;
    private Double ExpectedAvailability;
    private Integer FirstAdminAllowedResponseTimeMs;
    private Admin FirstAdmin;
    private Admin SecondAdmin;

    @JsonCreator
    public MonitoredHttpServiceConfiguration(@JsonProperty("Id") Integer id, @JsonProperty("Url") String Url, @JsonProperty("TimeoutMs") Integer TimeoutMs, @JsonProperty("FrequencyMs") Integer FrequencyMs, @JsonProperty("AlertingWindowMs") Integer AlertingWindowMs, @JsonProperty("ExpectedAvailability") Double ExpectedAvailability, @JsonProperty("FirstAdminAllowedResponseTimeMs") Integer FirstAdminAllowedResponseTimeMs, @JsonProperty("FirstAdmin") Admin FirstAdmin, @JsonProperty("SecondAdmin") Admin SecondAdmin) {
        this.Id = id;
        this.Url = Url;
        this.TimeoutMs = TimeoutMs;
        this.FrequencyMs = FrequencyMs;
        this.AlertingWindowMs = AlertingWindowMs;
        this.ExpectedAvailability = ExpectedAvailability;
        this.FirstAdminAllowedResponseTimeMs = FirstAdminAllowedResponseTimeMs;
        this.FirstAdmin = FirstAdmin;
        this.SecondAdmin = SecondAdmin;
    }

    public Integer getId() {
        return Id;
    }

    public String getUrl() {
        return Url;
    }

    public Integer getTimeoutMs() {
        return TimeoutMs;
    }

    public Integer getFrequencyMs() {
        return FrequencyMs;
    }

    public Integer getAlertingWindowMs() {
        return AlertingWindowMs;
    }

    public Double getExpectedAvailability() {
        return ExpectedAvailability;
    }

    public Integer getFirstAdminAllowedResponseTimeMs() {
        return FirstAdminAllowedResponseTimeMs;
    }

    public Admin getFirstAdmin() {
        return FirstAdmin;
    }

    public Admin getSecondAdmin() {
        return SecondAdmin;
    }

    public String toJson() {
        try {
            ObjectMapper mapper = new ObjectMapper();
            mapper.setPropertyNamingStrategy(PropertyNamingStrategies.UPPER_CAMEL_CASE);
            return mapper.writeValueAsString(this);
        } catch (JsonProcessingException e) {
            throw new RuntimeException(e);
        }
    }

    public static MonitoredHttpServiceConfiguration fromJson(String json) {
        try {
            ObjectMapper mapper = new ObjectMapper();
            return mapper.readValue(json, MonitoredHttpServiceConfiguration.class);
        } catch (IOException e) {
            throw new RuntimeException(e);
        }
    }
}