import io.vertx.core.AbstractVerticle;
import io.vertx.ext.web.client.WebClient;
import io.vertx.ext.web.client.HttpResponse;
import io.vertx.core.buffer.Buffer;
import java.util.stream.Stream;
import java.time.Instant;
import java.util.stream.IntStream;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import io.vertx.core.eventbus.Message;

public class HealthCheckerExecutor extends AbstractVerticle {
    private WebClient webClient;
    private static Logger logger = LoggerFactory.getLogger(HealthCheckerExecutor.class);

    @Override
    public void start() {
        webClient = WebClient.create(vertx);
        vertx.eventBus().consumer("task.queue", this::receiveTaskMessage);
    }

    private void receiveTaskMessage(Message<String> message) {
        logger.info("Received message: " + message.body().toString());
        Task task = Task.fromJson(message.body());
        scheduleHealthCheck(task);
    }

    private void scheduleHealthCheck(Task task) {
        final long taskFrom = Instant.parse(task.getMonitorFrom()).toEpochMilli();
        final long from = taskFrom > System.currentTimeMillis() ? taskFrom : System.currentTimeMillis();
        final long to = Instant.parse(task.getMonitorTo()).toEpochMilli();
        final long duration = to - from;
        long frequency = task.getMonitoredHttpServiceConfiguration().getFrequencyMs();
        final int count = (int)(duration / frequency);

        logger.info("Scheduling  {} health checks for {} from {} to {} with frequency {} ms", count, task.getMonitoredHttpServiceConfiguration().getUrl(), task.getMonitorFrom(), task.getMonitorTo(), frequency);
        Stream<Integer> stream = IntStream.range(1, count + 1).boxed();
        stream.forEach(i -> {
            vertx.setTimer((long)(i * frequency), id -> {
                try {
                    performHealthCheck(task, i, count);
                } catch (Exception e) {
                    logger.error("Error performing health check: " + e.getMessage());
                }
            });
        });
    }

    private void performHealthCheck(Task task, int i, int count) {
        logger.info("Performing health check " + i + " for " + task.getMonitoredHttpServiceConfiguration().getUrl());

        long start = System.currentTimeMillis();
        webClient.getAbs(task.getMonitoredHttpServiceConfiguration().getUrl())
            .timeout(task.getMonitoredHttpServiceConfiguration().getTimeoutMs())
            .send(ar -> {
                try {
                    long end = System.currentTimeMillis();
                    int responseTimeMs = (int)(end - start);
                    Integer statusCode = null;
                    boolean timedOut = false;
                    if (ar.succeeded()) {
                        HttpResponse<Buffer> response = ar.result();
                        statusCode = response.statusCode();
                        if (response.statusCode() == 200) {
                            logger.info("Health check succeeded for: " + task.getMonitoredHttpServiceConfiguration().getUrl());
                        } else {
                            logger.info("Health check failed for: " + task.getMonitoredHttpServiceConfiguration().getUrl() + " with status code " + response.statusCode());
                        }
                    } else {
                        logger.error("Health check failed for: " + task.getMonitoredHttpServiceConfiguration().getUrl() + " with exception " + ar.cause());
                        if (ar.cause() instanceof java.util.concurrent.TimeoutException) {
                            timedOut = true;
                        }
                    }
                    CallResult status = CallResult.fromStatusCode(statusCode, timedOut);
                    String timestampString = Instant.ofEpochMilli(start).toString().replace("T", " ").replace("Z", "");
                    Call call = new Call(task.getMonitoredHttpServiceConfiguration().getUrl(), timestampString, responseTimeMs, status, statusCode);
                    vertx.eventBus().send("log.healthcheck", call.toJson());
                } catch (Exception e) {
                    logger.error("Error performing health check: " + e.getMessage());
                }
            });
        if (i == count) {
            logger.info("Health check complete for {}, freeing resources", task.getMonitoredHttpServiceConfiguration().getUrl());
            vertx.eventBus().send("resource.free", task.getResourceCost());
        }
    }
}