gcloud functions deploy admin-response-monitor \
--gen2 \
--entry-point=AdminResponseMonitor.Function \
--runtime=dotnet8 \
--region=us-west1 \
--source=. \
--trigger-http \
--allow-unauthenticated \
--env-vars-file scripts/env.yaml