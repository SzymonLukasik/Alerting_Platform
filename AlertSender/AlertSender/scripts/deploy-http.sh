gcloud functions deploy csharp-http-function \
--gen2 \
--entry-point=AlertSender.HttpMessageSender \
--runtime=dotnet8 \
--region=us-west1 \
--source=. \
--trigger-http \
--allow-unauthenticated \
--env-vars-file scripts/env.deployment.yaml