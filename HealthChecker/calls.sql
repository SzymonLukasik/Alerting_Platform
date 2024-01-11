CREATE TABLE `calls_copy` (
  `id` int NOT NULL AUTO_INCREMENT,
  `url` varchar(255) DEFAULT NULL,
  `timestamp` datetime DEFAULT NULL,
  `responseTimeMs` int unsigned DEFAULT NULL,
  `callResult` enum('success','error','timeout') DEFAULT NULL,
  `statusCode` int DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_url` (`url`),
  KEY `idx_timestamp` (`timestamp`)
)