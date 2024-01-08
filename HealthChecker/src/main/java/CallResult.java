import java.util.Arrays;
import java.util.List;

public enum CallResult {
    SUCCESS,
    TIMEOUT,
    ERROR;

    private static List<Integer> SUCCESS_CODES = Arrays.asList(200, 201, 202, 203, 204, 205, 206);

    public static CallResult fromStatusCode(Integer statusCode, boolean timedOut) {
        if (timedOut) {
            return TIMEOUT;
        } else if (statusCode == null) {
            throw new RuntimeException("Null status code for non-timed out call");
        } 
        else if (SUCCESS_CODES.contains(statusCode)) {
            return SUCCESS;
        } else {
            return ERROR;
        }
    }

    public static CallResult fromString(String status) {
        if (status.equals("SUCCESS")) {
            return SUCCESS;
        } else if (status.equals("TIMEOUT")) {
            return TIMEOUT;
        } else if (status.equals("ERROR")) {
            return ERROR;
        } else {
            throw new RuntimeException("Invalid status: " + status);
        }
    }

}
