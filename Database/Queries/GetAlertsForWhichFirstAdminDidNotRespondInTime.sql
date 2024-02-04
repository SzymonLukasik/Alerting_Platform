select a.*
from alerting.alerts a
         inner join alerting.monitored_services s
                    on a.service_id = s.id
where a.response_status = 'waiting_for_first_admin'
  and a.first_alert_time >= date_sub(now(), interval (s.first_admin_allowed_response_time_ms * 1000) second)