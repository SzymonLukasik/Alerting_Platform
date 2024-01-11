import aiomysql
import asyncio


# Database settings
db_config = {
    'user': 'root',
    'password': 'FILL',
    'db': 'alerting',
    'host': '34.34.131.205',
    'port': 3306,
}

# connect to database
async def connect_to_db():
    pool = await aiomysql.create_pool(**db_config)
    async with pool.acquire() as conn:
        async with conn.cursor() as cursor:
            await cursor.execute("SHOW CREATE TABLE calls_copy")
            print(await cursor.fetchall())


if __name__ == "__main__":
    loop = asyncio.get_event_loop()
    loop.run_until_complete(connect_to_db())
    loop.close()