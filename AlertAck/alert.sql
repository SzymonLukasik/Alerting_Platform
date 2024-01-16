CREATE TABLE alerts (
    id                      INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    url                     VARCHAR(4096),
    alertStatus             ENUM(
	'success',
	'timeout',
	'error'
	  ),
    responseStatus          ENUM('responded', 'ignored'),
    firstLinkUUID           BINARY(16),
    secondLinkUUID          BINARY(16),
    firstAlertTime          DATETIME,
    secondAlertTime         DATETIME,
    firstAlertResponseTime  DATETIME,
    secondAlertResponseTime DATETIME
);

CREATE INDEX firstLinkUUID ON alerts (firstLinkUUID);
CREATE INDEX secondLinkUUID ON alerts (secondLinkUUID);

-- Insert some dummy data
INSERT INTO alerts (url, alertStatus, responseStatus, firstLinkUUID, secondLinkUUID, firstAlertTime, secondAlertTime, firstAlertResponseTime, secondAlertResponseTime) VALUES (
    'http://www.google.com',
    'timeout',
    NULL,
    UNHEX(REPLACE('75314b56-67ab-4f51-bb4a-03471f9e8efe', '-', '')),
    NULL,
    '2015-01-01 00:00:00',
    NULL,
    NULL,
    NULL
);