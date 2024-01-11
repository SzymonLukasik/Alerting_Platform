# publish task to queue
import uuid
from datetime import datetime, timedelta
import json
from google.cloud import pubsub_v1
import asyncio

# Google Cloud Pub/Sub settings
project_id = "irio-402819"
topic_id = "tasks"
publisher = pubsub_v1.PublisherClient()
topic_path = publisher.topic_path(project_id, topic_id)

def getTask(Url: str, FrequencyMs: int, TimeoutMs: int = 1000, ResourceCost: int = 1, timedeltaMinutes: int = 1):
    return {
        "TaskId": str(uuid.uuid4()),
        "MonitoredHttpServiceConfiguration": {
            "Url": Url,
            "TimeoutMs": TimeoutMs,
            "FrequencyMs": FrequencyMs,
        },
        "ResourceCost": ResourceCost,
        "MonitorFrom": datetime.now().strftime("%Y-%m-%dT%H:%M:%S.%fZ"),
        "MonitorTo": (datetime.now() + timedelta(minutes=timedeltaMinutes)).strftime("%Y-%m-%dT%H:%M:%S.%fZ"),
    }

# various popular services
tasks_com = [
    getTask("https://www.google.com", 1000),
    getTask("https://www.facebook.com", 1000),
    getTask("https://www.youtube.com", 1000),
    getTask("https://www.twitter.com", 1000),
    getTask("https://www.instagram.com", 1000),
    getTask("https://www.linkedin.com", 1000),
    getTask("https://www.amazon.com", 1000),
    getTask("https://www.reddit.com", 1000),
    getTask("https://www.netflix.com", 1000),
    getTask("https://www.ebay.com", 1000),
    getTask("https://www.wikipedia.org", 1000),
    getTask("https://www.apple.com", 1000),
    getTask("https://www.microsoft.com", 1000),
    getTask("https://www.wordpress.com", 1000),
    getTask("https://www.pinterest.com", 1000),
    getTask("https://www.paypal.com", 1000),
    getTask("https://www.tumblr.com", 1000),
    getTask("https://www.imgur.com", 1000),
    getTask("https://www.stackoverflow.com", 1000),
    getTask("https://www.imdb.com", 1000),
    getTask("https://www.fandom.com", 1000),
    getTask("https://www.quora.com", 1000),
    getTask("https://www.spotify.com", 1000),
    getTask("https://www.etsy.com", 1000),
    getTask("https://www.nytimes.com", 1000),
    getTask("https://www.blogger.com", 1000),
    getTask("https://www.wikia.com", 1000),
    getTask("https://www.imgur.com", 1000),
    getTask("https://www.stackexchange.com", 1000),
    getTask("https://www.github.com", 1000),
    getTask("https://www.weebly.com", 1000),
    getTask("https://www.live.com", 1000),
    getTask("https://www.msn.com", 1000),
]

tasks_pl = [
    getTask("https://www.wp.pl", 1000),
    getTask("https://www.onet.pl", 1000),
    getTask("https://www.interia.pl", 1000),
    getTask("https://www.o2.pl", 1000),
    getTask("https://www.pudelek.pl", 1000),
    getTask("https://www.wp.tv", 1000),
    getTask("https://www.gazeta.pl", 1000),
    getTask("https://www.money.pl", 1000),
    getTask("https://www.tvn24.pl", 1000),
    getTask("https://www.bankier.pl", 1000),
    getTask("https://www.polsatnews.pl", 1000),
    getTask("https://www.polsat.pl", 1000),
    getTask("https://www.tvn.pl", 1000),
    getTask("https://www.tvp.pl", 1000),
    getTask("https://www.tvp.info", 1000),
    getTask("https://www.tvn24.pl", 1000),
    getTask("https://www.tvnmeteo.pl", 1000),
    getTask("https://www.tvn24bis.pl", 1000),
    getTask("https://www.tvnstyle.pl", 1000),
    getTask("https://www.tvn7.pl", 1000),
    getTask("https://www.tvnfabula.pl", 1000),
    getTask("https://www.tvn24go.pl", 1000),
    getTask("https://www.tvn24.pl", 1000),
    getTask("https://www.tvn24bis.pl", 1000),
]

def publishTask(task):
    data = json.dumps(task).encode("utf-8")
    print(f"Publishing: {data}")
    return publisher.publish(topic_path, data=data)

PERCENTAGE = 1.0

def main():
    print(f"Publishing {len(tasks_com)} .com and {len(tasks_pl)} tasks to {topic_path}.")
    tasks_com_sample = tasks_com[:int(len(tasks_com) * PERCENTAGE)]
    tasks_pl_sample = tasks_pl[:int(len(tasks_pl) * PERCENTAGE)]
    tasks = tasks_com_sample + tasks_pl_sample
    for task in tasks:
        result = publishTask(task).result()
        print(result)

if __name__ == "__main__":
    main()