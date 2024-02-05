terraform {
  required_providers {
    google = {
      source  = "hashicorp/google"
      version = "4.51.0"
    }
  }
}

provider "google" {
  project     = var.project
  region      = var.region
  zone        = var.zone
}

resource "random_id" "bucket_prefix" {
  byte_length = 8
}

resource "google_storage_bucket" "default" {
  name          = "${random_id.bucket_prefix.hex}-bucket-tfstate"
  force_destroy = false
  location      = "US"
  storage_class = "STANDARD"
  versioning {
    enabled = true
  }
}

resource "google_sql_database_instance" "mysql_instance" {
  name             = "alerting-sql2"
  region           = "us-central1"
  database_version = "MYSQL_8_0"

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