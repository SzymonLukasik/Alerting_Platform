provider "kubernetes" {
  # Configure the Kubernetes provider
}

resource "kubernetes_deployment" "example" {
  metadata {
    name = "healthchecker"
    labels = {
      App = "healthchecker"
    }
  }

  spec {
    replicas = 1

    selector {
      match_labels = {
        App = "healthchecker"
      }
    }

    template {
      metadata {
        labels = {
          App = "healthchecker"
        }
      }

      spec {
        container {
          image = "us-central1-docker.pkg.dev/irio-402819/mimuw-labs/healthchecker:v1"
          name  = "healthchecker"

          port {
            container_port = 8080
          }

          volume_mount {
            name       = "google-cloud-keys"
            mount_path = "/var/secrets/google"
            read_only  = true
          }

          env {
            name  = "GOOGLE_APPLICATION_CREDENTIALS"
            value = "/var/secrets/google/application_default_credentials.json"
          }
        }

        volume {
          name = "google-cloud-keys"

          secret {
            secret_name = "google-cloud-keys"
          }
        }
      }
    }
  }
}

resource "kubernetes_horizontal_pod_autoscaler" "example" {
  metadata {
    name = "healthchecker"
  }

  spec {
    scale_target_ref {
      api_version = "apps/v1"
      kind        = "Deployment"
      name        = kubernetes_deployment.example.metadata[0].name
    }

    min_replicas = 1
    max_replicas = 5

    metric {
      type = "External"
      external_metric {
        metric_name     = "pubsub.googleapis.com/subscription/num_undelivered_messages"
        metric_selector {
          match_labels = {
            "resource.labels.subscription_id" = "tasks-sub"
          }
        }
        target {
          type  = "AverageValue"
          value = "2"
        }
      }
    }
  }
}