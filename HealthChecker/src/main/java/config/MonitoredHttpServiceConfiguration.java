package config;
import java.io.IOException;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.PropertyNamingStrategies;

import events.external.Task;

import com.fasterxml.jackson.annotation.JsonCreator;
import com.fasterxml.jackson.annotation.JsonProperty;

public class MonitoredHttpServiceConfiguration {
    private String Url;
    private Integer TimeoutMs;
    private Integer FrequencyMs;

    @JsonCreator
    public MonitoredHttpServiceConfiguration(@JsonProperty("Url") String Url, @JsonProperty("TimeoutMs") Integer TimeoutMs, @JsonProperty("FrequencyMs") Integer FrequencyMs) {
        this.Url = Url;
        this.TimeoutMs = TimeoutMs;
        this.FrequencyMs = FrequencyMs;
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

     public String toJson() {
        try {
            ObjectMapper mapper = new ObjectMapper();
            mapper.setPropertyNamingStrategy(PropertyNamingStrategies.UPPER_CAMEL_CASE);
            return mapper.writeValueAsString(this);
        } catch (JsonProcessingException e) {
            throw new RuntimeException(e);
        }
    }

    public static Task fromJson(String json) {
        try {
            ObjectMapper mapper = new ObjectMapper();
            return mapper.readValue(json, Task.class);
        } catch (IOException e) {
            throw new RuntimeException(e);
        }
    }
}