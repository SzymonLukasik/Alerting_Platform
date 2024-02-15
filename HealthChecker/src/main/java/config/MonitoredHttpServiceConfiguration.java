package config;

import com.fasterxml.jackson.annotation.JsonCreator;
import com.fasterxml.jackson.annotation.JsonProperty;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.PropertyNamingStrategies;
import com.fasterxml.jackson.core.JsonProcessingException;
// import IO exception
import java.io.IOException;

public class MonitoredHttpServiceConfiguration {
    private Integer Id;
    private String Url;
    private Integer TimeoutMs;
    private Integer FrequencyMs;
    private Integer AlertingWindowMs;
    private Double ExpectedAvailability;
    private Integer FirstAdminAllowedResponseTimeMs;
    private Boolean FirstAdminSendEmail;
    private Boolean FirstAdminSendSms;
    private String FirstAdminName;
    private String FirstAdminEmail;
    private String FirstAdminPhoneNumber;
    private Integer SecondAdminAllowedResponseTimeMs;
    private Boolean SecondAdminSendEmail;
    private Boolean SecondAdminSendSms;
    private String SecondAdminName;
    private String SecondAdminEmail;
    private String SecondAdminPhoneNumber;

    @JsonCreator
    public MonitoredHttpServiceConfiguration(@JsonProperty("Id") Integer id, @JsonProperty("Url") String Url, @JsonProperty("TimeoutMs") Integer TimeoutMs, @JsonProperty("FrequencyMs") Integer FrequencyMs, @JsonProperty("AlertingWindowMs") Integer AlertingWindowMs, @JsonProperty("ExpectedAvailability") Double ExpectedAvailability, @JsonProperty("FirstAdminAllowedResponseTimeMs") Integer FirstAdminAllowedResponseTimeMs, @JsonProperty("FirstAdminSendEmail") Boolean FirstAdminSendEmail, @JsonProperty("FirstAdminSendSms") Boolean FirstAdminSendSms, @JsonProperty("FirstAdminName") String FirstAdminName, @JsonProperty("FirstAdminEmail") String FirstAdminEmail, @JsonProperty("FirstAdminPhoneNumber") String FirstAdminPhoneNumber, @JsonProperty("SecondAdminAllowedResponseTimeMs") Integer SecondAdminAllowedResponseTimeMs, @JsonProperty("SecondAdminSendEmail") Boolean SecondAdminSendEmail, @JsonProperty("SecondAdminSendSms") Boolean SecondAdminSendSms, @JsonProperty("SecondAdminName") String SecondAdminName, @JsonProperty("SecondAdminEmail") String SecondAdminEmail, @JsonProperty("SecondAdminPhoneNumber") String SecondAdminPhoneNumber) {
        this.Id = id;
        this.Url = Url;
        this.TimeoutMs = TimeoutMs;
        this.FrequencyMs = FrequencyMs;
        this.AlertingWindowMs = AlertingWindowMs;
        this.ExpectedAvailability = ExpectedAvailability;
        this.FirstAdminAllowedResponseTimeMs = FirstAdminAllowedResponseTimeMs;
        this.FirstAdminSendEmail = FirstAdminSendEmail;
        this.FirstAdminSendSms = FirstAdminSendSms;
        this.FirstAdminName = FirstAdminName;
        this.FirstAdminEmail = FirstAdminEmail;
        this.FirstAdminPhoneNumber = FirstAdminPhoneNumber;
        this.SecondAdminAllowedResponseTimeMs = SecondAdminAllowedResponseTimeMs;
        this.SecondAdminSendEmail = SecondAdminSendEmail;
        this.SecondAdminSendSms = SecondAdminSendSms;
        this.SecondAdminName = SecondAdminName;
        this.SecondAdminEmail = SecondAdminEmail;
        this.SecondAdminPhoneNumber = SecondAdminPhoneNumber;
    }

    // Getters and setters for all the fields
    public Integer getId() {
        return Id;
    }

    public void setId(Integer id) {
        this.Id = id;
    }

    public String getUrl() {
        return Url;
    }

    public void setUrl(String Url) {
        this.Url = Url;
    }

    public Integer getTimeoutMs() {
        return TimeoutMs;
    }

    public void setTimeoutMs(Integer TimeoutMs) {
        this.TimeoutMs = TimeoutMs;
    }

    public Integer getFrequencyMs() {
        return FrequencyMs;
    }

    public void setFrequencyMs(Integer FrequencyMs) {
        this.FrequencyMs = FrequencyMs;
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