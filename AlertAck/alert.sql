CREATE TABLE alerts (
    id                      INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    url                     VARCHAR(4096),
    alertStatus             ENUM(
	'completed',
	'timed-out',
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
