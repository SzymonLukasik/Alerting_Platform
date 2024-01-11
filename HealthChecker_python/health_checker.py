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
    loop = asyncio.get_running_loop()
    print(loop.is_running())
    subscription_path = subscriber.subscription_path(project_id, subscription_id)
    
    def callback(message):
        print(f"Received message: {message}")
        task = parse_message_to_task(message)  # Implement this function to convert Pub/Sub message to task format
        # loop.create_task(monitor_service(task, session, pool))  # Implement this function to monitor the service
        asyncio.run_coroutine_threadsafe(monitor_service(task, session, pool), loop)
        # asyncio.create_task(monitor_service(task, session, pool))
        message.ack()

    streaming_pull_future = subscriber.subscribe(subscription_path, callback=callback)
    print(f"Listening for messages on {subscription_path}...")

    # Wrap subscriber in a 'with' block to automatically call close() to close the underlying StreamingPullFuture.
    with subscriber:
        try:
            # Calling result() on StreamingPullFuture keeps the main thread from exiting while messages get processed in the callbacks.
            streaming_pull_future.result()
        except:  # Add specific exceptions here
            streaming_pull_future.cancel()

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
        # select all calls
        async with pool.acquire() as conn:
            async with conn.cursor() as cursor:
                # create a copy of calls table
                # await cursor.execute("CREATE TABLE calls_copy LIKE calls")
                await cursor.execute("SELECT COUNT(*) FROM calls_copy")
                tasks = await cursor.fetchall()
                print(tasks)
        # await consume_tasks(session, pool)


# async def main():
#     subscription_path = subscriber.subscription_path(project_id, subscription_id)
#     streaming_pull_future = subscriber.subscribe(subscription_path, callback=callback)
#     print(f"Listening for messages on {subscription_path}...")

#     async with aiohttp.ClientSession() as session:
#         task_processor = asyncio.create_task(process_tasks(session))
#         await streaming_pull_future
#         await task_processor


if __name__ == "__main__":
    asyncio.run(main())

# locate a windows executable pid by command
# wmic process where "commandline like '%python%'" get processid,commandline
