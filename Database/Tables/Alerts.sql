CREATE TABLE alerts
(
    id                         int auto_increment primary key,
    service_id                 int,
    response_status            ENUM (
        'waiting_for_first_admin',
        'first_admin_responded',
        'waiting_for_second_admin',
        'second_admin_responded',
        'ignored'),
    first_link_uuid            BINARY(16),
    second_link_uuid           BINARY(16) null,
    first_alert_time           DATETIME,
    second_alert_time          DATETIME   null,
    first_alert_response_time  DATE       null,
    second_alert_response_time DATE       null,
    foreign key (service_id) references alerting.monitored_services (id)
);

drop table alerting.alerts;