package service;
import java.util.concurrent.atomic.AtomicBoolean;
import java.util.concurrent.atomic.AtomicInteger;

import com.google.api.client.util.DateTime;
import com.google.api.gax.rpc.ApiException;
import com.google.cloud.pubsub.v1.AckReplyConsumer;
import com.google.cloud.pubsub.v1.Subscriber;
import com.google.pubsub.v1.ProjectSubscriptionName;
import com.google.pubsub.v1.PubsubMessage;

import config.HealthCheckerConfiguration;
import events.external.Task;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import io.grpc.LoadBalancerRegistry;
import io.grpc.internal.PickFirstLoadBalancerProvider;
import io.vertx.core.Vertx;
import io.vertx.core.eventbus.Message;

import java.util.Map;

public class HealthCheckerService {
    private HealthCheckerConfiguration config;
    private AtomicInteger currentResourceUsage;
    private static Logger logger = LoggerFactory.getLogger(HealthCheckerService.class);
    private AtomicBoolean isShuttingDown = new AtomicBoolean(false);


    HealthCheckerService(HealthCheckerConfiguration config) {
        this.config = config;
        this.currentResourceUsage = new AtomicInteger(0);
        this.isShuttingDown = new AtomicBoolean(false);

        // Add shutdown hook
        Runtime.getRuntime().addShutdownHook(new Thread(() -> {
            logger.info("Received SIGTERM, stopping task fetching...");
            this.isShuttingDown.set(true);
            // Add code here to push existing tasks to the queue
        }));
    }

    private void subscribeToPubSub(Vertx vertx) {
        ProjectSubscriptionName subscriptionName = ProjectSubscriptionName.of(config.getProjectID(), config.getSubscriptionID());
        Subscriber subscriber = null;
        try {
            subscriber = Subscriber.newBuilder(subscriptionName, (PubsubMessage message, AckReplyConsumer consumer) -> {
                try {
                    Task task = parseTask(message.getData().toStringUtf8());
                    logger.info("Received task: " + task.toJson());
                    DateTime parsedMonitorTo = DateTime.parseRfc3339(task.getMonitorTo());
                    if (parsedMonitorTo.getValue() < System.currentTimeMillis()) {
                        logger.info("Task expired, sending ack");
                        consumer.ack();
                        return;
                    }
                    if (currentResourceUsage.get() + task.getResourceCost() <= config.getMaxResourceCost()) {
                        logger.info("Resource available, sending task to queue");
                        currentResourceUsage.addAndGet(task.getResourceCost());
                        logger.info("Current resource usage: " + currentResourceUsage.get());

                        String taskString = task.toJson();
                        vertx.eventBus().send("task.queue", taskString);
                        consumer.ack();
                    } else {
                        logger.info("Resource unavailable, sending task to dead letter queue");
                        consumer.nack();
                    }
                } catch (Exception e) {
                    logger.error("Error processing message: " + e.getMessage());
                    consumer.nack();
                }
            }).build();

            subscriber.startAsync().awaitRunning();
        } catch (ApiException e) {
            System.out.println("Error starting Pub/Sub subscriber: " + e.getStatusCode().getCode());
            subscriber.stopAsync();
        }
    }

    private Task parseTask(String messageData) {
        try {
            return Task.fromJson(messageData);
        } catch (Exception e) {
            throw new RuntimeException("Error parsing task {%s}: %s".formatted(messageData, e.getMessage()));
        }
    }

    private void receiveResourceFreeMessage(Message<Integer> message) {
        Integer freedResource = message.body();
        logger.info("Received resource free message: " + freedResource);
        currentResourceUsage.addAndGet(-freedResource);
        logger.info("Current resource usage: " + currentResourceUsage.get());
    }

    private static HealthCheckerConfiguration loadConfig()
    {
        Map<String, HealthCheckerConfiguration> configMap = HealthCheckerConfiguration.loadConfigs();
        String environment = System.getProperty("ENVIRONMENT");
        if (environment == null) {
            throw new RuntimeException("Environment variable ENVIRONMENT not set");
        }
        HealthCheckerConfiguration config = configMap.get(environment);
        if (config == null) {
            throw new RuntimeException("No configuration found for environment " + environment);
        }
        return config;
    }

    public void run()
    {
        LoadBalancerRegistry.getDefaultRegistry().register(new PickFirstLoadBalancerProvider());
        Vertx vertx = Vertx.vertx();

        vertx.eventBus().consumer("resource.free", this::receiveResourceFreeMessage);

        vertx.deployVerticle(new HealthCheckerExecutor());
        vertx.deployVerticle(new HealthCheckerLogger(config));
        
        subscribeToPubSub(vertx);
    }

    public static void start()
    {
        HealthCheckerConfiguration config = HealthCheckerService.loadConfig();
        logger.info("Starting HealthCheckerService with config: {}", config.toJson());
        HealthCheckerService healthChecker = new HealthCheckerService(config);
        healthChecker.run();
    }
}
