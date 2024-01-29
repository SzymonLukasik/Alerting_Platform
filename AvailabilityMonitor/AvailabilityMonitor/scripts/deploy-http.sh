gcloud functions deploy availability-monitor \
--gen2 \
--entry-point=AvailabilityMonitor.Function \
--runtime=dotnet8 \
--region=us-west1 \
--source=. \
--trigger-http \
--allow-unauthenticated \
--env-vars-file scripts/env.yaml