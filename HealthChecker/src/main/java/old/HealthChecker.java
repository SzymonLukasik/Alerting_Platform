package old;
import com.google.cloud.pubsub.v1.Subscriber;
import com.google.cloud.pubsub.v1.AckReplyConsumer;
import com.google.pubsub.v1.ProjectSubscriptionName;
import com.google.pubsub.v1.PubsubMessage;
import java.sql.Connection;
import java.sql.DriverManager;

public class HealthChecker {
    private static final String PROJECT_ID = "irio-402819";
    private static final String SUBSCRIPTION_ID = "tasks-sub";
    private static final String CONNECTION_STRING = "jdbc:mysql://34.34.131.205:3306/alerting";
    private static final String DB_USER = "root";
    private static final String DB_PASSWORD = "FILL";

    public static void main(String[] args) throws Exception {
        // Setup database connection
        Connection connection = DriverManager.getConnection(CONNECTION_STRING, DB_USER, DB_PASSWORD);

        // Setup Pub/Sub subscription
        ProjectSubscriptionName subscriptionName = ProjectSubscriptionName.of(PROJECT_ID, SUBSCRIPTION_ID);
        Subscriber subscriber = null;
        try {
            // Create a subscriber bound to the asynchronous message receiver
            subscriber = Subscriber.newBuilder(subscriptionName, (PubsubMessage message, AckReplyConsumer consumer) -> {
                System.out.println("Received message: " + message.getData().toStringUtf8());

                // Process the message
                processMessage(message, connection);

                // Acknowledge the message
                consumer.ack();
            }).build();

            // Start the subscriber
            subscriber.startAsync().awaitRunning();

            System.out.println("Subscriber started. Listening for messages on " + subscriptionName.toString());

            // Allow the subscriber to run indefinitely unless an unrecoverable error occurs
            subscriber.awaitTerminated();
        } finally {
            if (subscriber != null) {
                subscriber.stopAsync();
            }
        }
    }

    private static void processMessage(PubsubMessage message, Connection connection) {
        // Implement your message processing logic here
        // For example, parse the message and insert data into the database
        String messageData = message.getData().toStringUtf8();

        // Print message
        System.out.println("Message: " + messageData);
    }

    // example task
    // {
    //     "TaskId": "2a06b02f-6d57-4048-951e-631ad94e1385",
    //     "MonitoredHttpServiceConfiguration": {
    //         "url": "https://www.google1.com/",
    //         "timeoutMs": 5000,
    //         "frequencyMs": 10000
    //     },
    //     "ResourceCost": 10,
    //     "MonitorFrom": 1704567369.656074,
    //     "MonitorTo": 1704568269.656074
    // }
}
