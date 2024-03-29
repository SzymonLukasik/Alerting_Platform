from flask import Flask, request
import mysql.connector
from mysql.connector import Error

app = Flask(__name__)

@app.route('/alert_ack_service', methods=['GET'])
def alert_ack_service():
    request_args = request.args

    if request_args and 'uuid' in request_args:
        uuid = request_args['uuid']
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
            SET responseStatus = 'responded',
                firstAlertResponseTime = NOW()
            WHERE firstLinkUUID = UNHEX(REPLACE(%s, '-', ''))
            OR secondLinkUUID = UNHEX(REPLACE(%s, '-', ''))
            """
            cursor.execute(update_query, (uuid, uuid))
            connection.commit()
            cursor.close()
            connection.close()

            # Return a static page or a simple message
            return 'Alert acknowledged. Thank you!', 200
    except Error as e:
        return f"Database error: {e}", 500

if __name__ == '__main__':
    app.run(debug=True)