import com.google.cloud.pubsub.v1.Subscriber;
import com.google.cloud.pubsub.v1.AckReplyConsumer;
import com.google.pubsub.v1.ProjectSubscriptionName;
import com.google.pubsub.v1.PubsubMessage;
import com.google.api.gax.rpc.ApiException;
import com.google.api.gax.rpc.StatusCode.Code;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.PreparedStatement;
import java.sql.SQLException;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.Executors;
import java.util.concurrent.ExecutorService;

public class HealthChecker {
    private static final String PROJECT_ID = "irio-402819";
    private static final String SUBSCRIPTION_ID = "tasks-sub";
    private static final String DB_URL = "jdbc:mysql://34.34.131.205:3306/alerting";
    private static final String DB_USER = "root";
    private static final String DB_PASSWORD = "FILL";

    private static final BlockingQueue<PubsubMessage> taskQueue = new LinkedBlockingQueue<>();

    public static void main(String[] args) throws Exception {
        // Set up database connection
        Connection connection = DriverManager.getConnection(DB_URL, DB_USER, DB_PASSWORD);

        // Set up subscriber
        ProjectSubscriptionName subscriptionName = ProjectSubscriptionName.of(PROJECT_ID, SUBSCRIPTION_ID);
        Subscriber subscriber = Subscriber.newBuilder(subscriptionName, HealthChecker::receiveMessage).build();
        subscriber.startAsync().awaitRunning();

        // Set up worker thread
        ExecutorService executor = Executors.newSingleThreadExecutor();
        executor.submit(() -> {
            try {
                processTasks(connection);
            } catch (SQLException e) {
                e.printStackTrace();
            }
        });

        // Keep the main thread alive
        try {
            Thread.sleep(Long.MAX_VALUE);
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
        }
    }

    public static void receiveMessage(PubsubMessage message, AckReplyConsumer consumer) {
        try {
            taskQueue.put(message);
            consumer.ack();
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
        }
    }

    private static void processTasks(Connection connection) throws SQLException {
        while (!Thread.currentThread().isInterrupted()) {
            try {
                PubsubMessage message = taskQueue.take();
                // Process the message
                // For example, parse the message and insert data into the database
                // String data = message.getData().toStringUtf8();
                // Insert data into the database

                System.out.println("Message received: " + message.getData().toStringUtf8());
            } catch (InterruptedException e) {
                Thread.currentThread().interrupt();
            }
        }
    }

    // Main
    public static void main(String[] args) throws Exception {
        // Set up database connection
        Connection connection = DriverManager.getConnection(DB_URL, DB_USER, DB_PASSWORD);

        // Set up subscriber
        ProjectSubscriptionName subscriptionName = ProjectSubscriptionName.of(PROJECT_ID, SUBSCRIPTION_ID);
        Subscriber subscriber = Subscriber.newBuilder(subscriptionName, HealthChecker::receiveMessage).build();
        subscriber.startAsync().awaitRunning();

        // Set up worker thread
        ExecutorService executor = Executors.newSingleThreadExecutor();
        executor.submit(() -> {
            try {
                processTasks(connection);
            } catch (SQLException e) {
                e.printStackTrace();
            }
        });

        // Keep the main thread alive
        try {
            Thread.sleep(Long.MAX_VALUE);
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
        }
    }
}
