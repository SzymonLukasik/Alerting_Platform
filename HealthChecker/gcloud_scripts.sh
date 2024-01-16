# I think it resolved the issue with HPA not being authenticated to fetch Pub/Sub metrics

gcloud projects add-iam-policy-binding "$PROJECT_ID" \
    --member "serviceAccount:autoscaling-pubsub-sa@$PROJECT_ID.iam.gserviceaccount.com" \
    --role "roles/monitoring.editor"

gcloud iam service-accounts add-iam-policy-binding \
    --role roles/iam.workloadIdentityUser \
    --member "serviceAccount:$PROJECT_ID.svc.id.goog[custom-metrics/custom-metrics-stackdriver-adapter]" \
    autoscaling-pubsub-sa@$PROJECT_ID.iam.gserviceaccount.com

kubectl annotate serviceaccount \
    --namespace custom-metrics \
    custom-metrics-stackdriver-adapter \
    iam.gke.io/gcp-service-account=autoscaling-pubsub-sa@$PROJECT_ID.iam.gserviceaccount.com