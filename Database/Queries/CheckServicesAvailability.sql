select service_id
from alerting.calls_copy2 c
         left join alerting.monitored_services s
                   on c.service_id = s.id
where c.timestamp <= now()
  and c.timestamp >= date_sub(now(), interval (s.alerting_window_ms * 1000) second)
group by c.service_id, s.expected_availability
having count(if(c.callResult = 'success', 1, null)) / count(*) < s.expected_availability