import asyncio
import aiohttp
import aiomysql
import json
from google.cloud import pubsub_v1
from datetime import datetime, timedelta

# Google Cloud Pub/Sub settings
project_id = "irio-402819"
subscription_id = "tasks-sub"
subscriber = pubsub_v1.SubscriberClient()

# Database settings
db_config = {
    'user': 'root',
    'password': 'FILL',
    'db': 'alerting',
    'host': '34.34.131.205',
    'port': 3306,
}

async def monitor_service(task, session, pool):
    print("executing monitor_service")
    config = task["MonitoredHttpServiceConfiguration"]
    end_time = datetime.fromtimestamp(task["MonitorTo"])

    async with pool.acquire() as conn:
        async with conn.cursor() as cursor:
            while datetime.utcnow() < end_time:
                start_time = datetime.utcnow()
                try:
                    print(f"Executing request to {config['url']}")
                    async with session.get(config["url"], timeout=config["timeoutMs"] / 1000) as response:
                        response_time_ms = int((datetime.utcnow() - start_time).total_seconds() * 1000)
                        print(f"Task {task['TaskId']}: {config['url']} responded in {response_time_ms} ms")
                        # Insert result into database
                        await cursor.execute("INSERT INTO calls (url, timestamp, responseTimeMs, callResult) VALUES (%s, %s, %s, %s)",
                                             (config["url"], datetime.now(), response_time_ms, response.status))
                        await conn.commit()
                except asyncio.TimeoutError:
                    print(f"Task {task['TaskId']}: {config['url']} timed out")
                    # Handle timeout case, possibly saving this result to the database

                await asyncio.sleep(config["frequencyMs"] / 1000)

async def consume_tasks(session, pool):
    subscription_path = subscriber.subscription_path(project_id, subscription_id)
    while True:
        response = subscriber.pull(subscription=subscription_path, max_messages=1, return_immediately=True)
        for msg in response.received_messages:
            print(f"Received message: {msg.message}")
            task = parse_message_to_task(msg.message)  # Implement this function based on your message format
            asyncio.ensure_future(monitor_service(task, session, pool))
            subscriber.acknowledge(subscription=subscription_path, ack_ids=[msg.ack_id])

def parse_message_to_task(message):
    """
    Parses a Pub/Sub message into the task format.

    Args:
        message (google.cloud.pubsub_v1.subscriber.message.Message): The Pub/Sub message.

    Returns:
        dict: The parsed task.
    """
    try:
        # Decode the message data from bytes to a string
        message_data = message.data.decode('utf-8')
        print(message_data)
        # Convert the JSON string to a Python dictionary
        task = json.loads(message_data)
        
        # Ensure the task has the expected structure and types
        # You might need to convert strings to datetime objects, etc.
        # Example: task["MonitorFrom"] = datetime.fromisoformat(task["MonitorFrom"])

        return task
    except json.JSONDecodeError as e:
        print(f"Failed to decode message: {e}")
        return None
    except KeyError as e:
        print(f"Missing key in message: {e}")
        return None
    except Exception as e:
        print(f"Error parsing message: {e}")
        return None

async def main():
    async with aiohttp.ClientSession() as session:
        pool = await aiomysql.create_pool(**db_config)
        await consume_tasks(session, pool)


if __name__ == "__main__":
    asyncio.run(main())

# locate a windows executable pid by command
# wmic process where "commandline like '%python%'" get processid,commandline
