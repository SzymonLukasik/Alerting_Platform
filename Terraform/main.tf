terraform {
  required_providers {
    google = {
      source  = "hashicorp/google"
      version = "4.51.0"
    }
  }
}

provider "google" {
  credentials = file(var.credentials_file)
  project     = var.project
  region      = var.region
  zone        = var.zone
}

resource "google_sql_database_instance" "mysql_instance" {
  name             = "terraform-sql-instance"
  region           = "us-central1"
  database_version = "MYSQL_5_7"

  settings {
    tier = "db-f1-micro"

    ip_configuration {
      authorized_networks {
        name  = "all"
        value = "0.0.0.0/0"
      }
    }
  }
}

resource "google_pubsub_topic" "tasks_topic" {
  name = "tasks"
}

resource "google_pubsub_topic" "messages_topic" {
  name = "messages"
}