import os
import mysql.connector
from mysql.connector import Error
from flask import escape

def alert_ack_service(request):
    request_args = request.args

    if request_args and 'uuid' in request_args:
        uuid = escape(request_args['uuid'])  # Escaping the user-provided UUID
    else:
        return 'Missing UUID', 400

    try:
        # Connect to the database
        connection = mysql.connector.connect(
            host='34.118.111.98',
            database='alerting',
            user='root',
            password='FILL'
        )

        if connection.is_connected():
            cursor = connection.cursor()
            # Update the alert record
            update_query = """
            UPDATE alerts
            SET response_status = 'first_admin_responded',
                first_alert_response_time = NOW()
            WHERE first_link_uuid = UNHEX(REPLACE(%s, '-', ''))
            """
            cursor.execute(update_query, (str(uuid)))
            connection.commit()
            update_query = """
            UPDATE alerts
            SET response_status = 'second_admin_responded',
                second_alert_response_time = NOW()
            WHERE second_link_uuid = UNHEX(REPLACE(%s, '-', ''))
            """
            cursor.execute(update_query, (str(uuid)))
            connection.commit()
            cursor.close()
            connection.close()

            # Return a static page or a simple message
            return 'Alert acknowledged. Thank you!', 200
    except Error as e:
        return f"Database error: {e}", 500

    return 'Error processing request', 500
