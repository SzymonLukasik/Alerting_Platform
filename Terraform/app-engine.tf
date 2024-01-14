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