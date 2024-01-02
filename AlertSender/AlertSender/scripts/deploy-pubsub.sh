gcloud functions deploy csharp-pubsub-function \
--gen2 \
--runtime=dotnet8 \
--region=us-west1 \
--source=. \
--entry-point=AlertSender.PubSubMessageSender \
--trigger-topic=tasks \
--env-vars-file scripts/env.deployment.yaml