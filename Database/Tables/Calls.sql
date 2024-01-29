create table alerting.calls_copy2
(
    id             int auto_increment primary key,
    service_id     int,
    url            varchar(4096)                        null,
    timestamp      datetime                             null,
    responseTimeMs int unsigned                         null,
    callResult     enum ('success', 'error', 'timeout') null,
    statusCode     int                                  null,
    foreign key (service_id) references alerting.monitored_services (id)
);

create index idx_timestamp
    on alerting.calls_copy2 (timestamp);

create index idx_url
    on alerting.calls_copy2 (url);

