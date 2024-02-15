# Retrieve an access token as the Terraform runner
data "google_client_config" "provider" {}

data "google_container_cluster" "my_cluster" {
  name     = "health-checker-cluster"
  location = "us-central1-a"
}

provider "kubernetes" {
  host  = "https://${data.google_container_cluster.my_cluster.endpoint}"
  token = data.google_client_config.provider.access_token
  cluster_ca_certificate = base64decode(
    data.google_container_cluster.my_cluster.master_auth[0].cluster_ca_certificate,
  )
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
          image = "us-central1-docker.pkg.dev/irio-402819/mimuw-labs/healthchecker:v2"
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