resource "google_app_engine_application" "default" {
    location_id = "us-central"
}

resource "google_app_engine_application_url_dispatch_rules" "terraform-supervisor-dispatch-rules" {
    dispatch_rules {
        domain = "*"
        path = "/*"
        service = "default"
    }
}

resource "google_storage_bucket" "supervisor" {
    name          = "supervisor-${random_id.app.hex}"
    location      = "US"
    force_destroy = true
    versioning {
        enabled = true
    }
}

resource "random_id" "app" {
    byte_length = 8
}

data "archive_file" "function_dist" {
    type        = "zip"
    source_dir  = "../Supervisor/Supervisor"
    output_path = "../Supervisor/supervisor.zip"
}

resource "google_storage_bucket_object" "supervisor" {
    name   = "supervisor.zip"
    source = data.archive_file.function_dist.output_path
    bucket = google_storage_bucket.supervisor.name
}

resource "google_app_engine_flexible_app_version" "latest_version" {
    version_id = "v1"
    service    = "default"
    runtime    = "aspnetcore"

    deployment {
        zip {
            source_url = "https://storage.googleapis.com/${google_storage_bucket.supervisor.name}/${google_storage_bucket_object.supervisor.name}"
        }
    }
    
    liveness_check {
        path = "/"
    }

    readiness_check {
        path = "/"
    }
    
    automatic_scaling {
        cool_down_period = "120s"
        cpu_utilization {
            target_utilization = 0.5
        }
    }
    
    runtime_config {
        operating_system  = "ubuntu22"
        runtime_version =  "8" 
    }

    instance_class = "F1"
    
    noop_on_destroy = true
    delete_service_on_destroy = true
}