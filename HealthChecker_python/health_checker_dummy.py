import asyncio
import aiohttp
import uuid
from datetime import datetime, timedelta
import json

# Simulated task queue
task_queue = [
    {
        "TaskId": str(uuid.uuid4()),
        "MonitoredHttpServiceConfiguration": {
            "url": "https://www.google1.com/",
            "timeoutMs": 5000,
            "frequencyMs": 10000  # 10 seconds for example
        },
        "ResourceCost": 10,  # Example resource cost
        "MonitorFrom": datetime.utcnow().timestamp(),
        "MonitorTo": (datetime.utcnow() + timedelta(minutes=15)).timestamp()
    },
    # Add more tasks as needed
]
print (json.dumps(task_queue[0], indent=4))
exit()
async def monitor_service(task, session):
    config = task["MonitoredHttpServiceConfiguration"]
    end_time = task["MonitorTo"]

    while datetime.utcnow() < end_time:
        start_time = datetime.utcnow()
        try:
            async with session.get(config["url"], timeout=config["timeoutMs"] / 1000) as response:
                response_time_ms = int((datetime.utcnow() - start_time).total_seconds() * 1000)
                print(f"Task {task['TaskId']}: {config['url']} responded in {response_time_ms} ms")
                # Here, you would save the result to a database
        except asyncio.TimeoutError:
            print(f"Task {task['TaskId']}: {config['url']} timed out")
            # Handle timeout case, possibly saving this result to the database

        await asyncio.sleep(config["frequencyMs"] / 1000)

async def main():
    async with aiohttp.ClientSession() as session:
        tasks = [monitor_service(task, session) for task in task_queue]
        await asyncio.gather(*tasks)

if __name__ == "__main__":
    asyncio.run(main())
{
    "TaskId": "3f0ee84a-8d05-4fa3-a004-de32e4926afe",
    "MonitoredHttpServiceConfiguration": {
        "url": "https://www.google1.com/",
        "timeoutMs": 5000,
        "frequencyMs": 10000
    },
    "ResourceCost": 10,
    "MonitorFrom": 1704042766.818947,
    "MonitorTo": 1704043666.818947
}