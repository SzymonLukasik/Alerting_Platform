package service;
import io.vertx.core.AbstractVerticle;
import io.vertx.core.eventbus.Message;
import io.vertx.ext.jdbc.JDBCClient;
import io.vertx.core.json.JsonObject;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.CompletionException;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import config.HealthCheckerConfiguration;
import events.internal.Call;

public class HealthCheckerLogger extends AbstractVerticle {

    private static Logger logger = LoggerFactory.getLogger(HealthCheckerLogger.class);
    private HealthCheckerConfiguration config;
    private JDBCClient dbClient;
    private List<Call> batchedResults = new ArrayList<>();

    HealthCheckerLogger(HealthCheckerConfiguration config) {
        this.config = config;
    }

    @Override
    public void start() {
        dbClient = JDBCClient.createShared(vertx, new JsonObject()
            .put("driver_class", "com.mysql.cj.jdbc.Driver")
            .put("user", config.getDbUser())
            .put("password", config.getDbPassword())
            .put("url", config.getConnectionString()));
        doWarmup();

        vertx.eventBus().consumer("log.healthcheck", this::receiveHealthCheckMessage);

        // Schedule periodic database write for remaining batched results
        vertx.setPeriodic(config.getDbMinimumWriteDelayMs(), id -> writeBatchedResultsToDatabase()); // Every 10 seconds
    }

    private void doWarmup() {
    CompletableFuture<Void> future = new CompletableFuture<>();

    dbClient.getConnection(ar -> {
        if (ar.succeeded()) {
            logger.info("Successfully connected to database");
            ar.result().close();
            future.complete(null);
        } else {
            logger.error("Error connecting to database: " + ar.cause().getMessage());
            future.completeExceptionally(ar.cause());
        }
    });

    try {
        future.join();
    } catch (CompletionException e) {
        logger.error("Error while waiting for database connection", e);
    }
}

    private void receiveHealthCheckMessage(Message<String> message) {
        logger.info( "Received message: " + message.body());
        Call call = Call.fromJson(message.body());
        batchedResults.add(call);
        if (batchedResults.size() >= config.getDbWriteBatchSize()) { // Example batch size
            writeBatchedResultsToDatabase();
        }
    }

    private void writeBatchedResultsToDatabase() {
        if (!batchedResults.isEmpty()) {
            logger.info("Writing " + batchedResults.size() + " results to database");
            String query = getQuery();
            dbClient.query(query, ar -> {
                if (ar.succeeded()) {
                    logger.info("Successfully wrote all results to database");
                } else {
                    logger.error("Error writing results to database: " + ar.cause().getMessage());
                }
                batchedResults.clear();
            });
        }
    }

    private String getQuery()
    {
        StringBuilder values = new StringBuilder();
        for (Call call : batchedResults) {
            // Format timestamp to match myysql datetime format
            values.append("('%s', '%s', %d, '%s', %d),".formatted(call.getUrl(), call.getTimestamp(), call.getResponseTimeMs(), call.getCallResult(), call.getStatusCode()));
        }
        values.deleteCharAt(values.length() - 1);
        return "INSERT INTO calls_copy (url, timestamp, responseTimeMs, callResult, statusCode) VALUES " + values.toString();
    }
}
