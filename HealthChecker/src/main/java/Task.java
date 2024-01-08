import java.io.IOException;

import com.fasterxml.jackson.annotation.JsonCreator;
import com.fasterxml.jackson.annotation.JsonProperty;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.PropertyNamingStrategies;


public class Task {
    private String TaskId;
    private MonitoredHttpServiceConfiguration MonitoredHttpServiceConfiguration;
    private Integer ResourceCost;
    private String MonitorFrom;
    private String MonitorTo;


    @JsonCreator
    public Task(@JsonProperty("TaskId") String TaskId, @JsonProperty("MonitoredHttpServiceConfiguration") MonitoredHttpServiceConfiguration MonitoredHttpServiceConfiguration, @JsonProperty("ResourceCost") Integer ResourceCost, @JsonProperty("MonitorFrom") String MonitorFrom, @JsonProperty("MonitorTo") String MonitorTo) {
        this.TaskId = TaskId;
        this.MonitoredHttpServiceConfiguration = MonitoredHttpServiceConfiguration;
        this.ResourceCost = ResourceCost;
        this.MonitorFrom = MonitorFrom;
        this.MonitorTo = MonitorTo;
    }

    public String getTaskId() {
        return TaskId;
    }

    public MonitoredHttpServiceConfiguration getMonitoredHttpServiceConfiguration() {
        return MonitoredHttpServiceConfiguration;
    }

    public Integer getResourceCost() {
        return ResourceCost;
    }

    public String getMonitorFrom() {
        return MonitorFrom;
    }

    public String getMonitorTo() {
        return MonitorTo;
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