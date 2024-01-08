import com.fasterxml.jackson.annotation.JsonCreator;
import com.fasterxml.jackson.annotation.JsonProperty;
import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import java.io.IOException;

public class Call {
    private String url;
    private String timestamp;
    private Integer responseTimeMs;
    private CallResult callResult;
    private Integer statusCode;

    @JsonCreator
    public Call(@JsonProperty("url") String url, @JsonProperty("timestamp") String timestamp, @JsonProperty("responseTimeMs") Integer responseTimeMs, @JsonProperty("status") CallResult callResult, @JsonProperty("statusCode") Integer statusCode) {
        this.url = url;
        this.timestamp = timestamp;
        this.responseTimeMs = responseTimeMs;
        this.callResult = callResult;
        this.statusCode = statusCode;
    }

    public String getUrl() {
        return url;
    }

    public String getTimestamp() {
        return timestamp;
    }

    public Integer getResponseTimeMs() {
        return responseTimeMs;
    }

    public CallResult getCallResult() {
        return callResult;
    }

    public Integer getStatusCode() {
        return statusCode;
    }

    public String toJson() {
        try {
            ObjectMapper mapper = new ObjectMapper();
            return mapper.writeValueAsString(this);
        } catch (JsonProcessingException e) {
            throw new RuntimeException(e);
        }
    }

    public static Call fromJson(String json) {
        try {
            ObjectMapper mapper = new ObjectMapper();        
            return mapper.readValue(json, Call.class);
        } catch (IOException e) {
            throw new RuntimeException(e);
        }
    }
}
