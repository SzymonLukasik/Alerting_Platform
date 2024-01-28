create table alerting.monitored_services
(
    id                                    int auto_increment primary key,
    url                                   varchar(4096),
    timeout_ms                            int,
    frequency_ms                          int,
    alerting_window_ms                    int,
    expected_availability                 double,
    first_admin_allowed_response_time_ms  int,
    first_admin_send_email                bool,
    first_admin_send_sms                  bool,
    first_admin_name                      varchar(255),
    first_admin_email                     varchar(255) null,
    first_admin_phone_number              varchar(255) null,
    second_admin_allowed_response_time_ms int,
    second_admin_send_email               bool,
    second_admin_send_sms                 bool,
    second_admin_name                     varchar(255),
    second_admin_email                    varchar(255) null,
    second_admin_phone_number             varchar(255) null
);

