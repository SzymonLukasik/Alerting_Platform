terraform {
  backend "gcs" {
    bucket = "0295e584b1b3204b-bucket-tfstate"
    prefix = "terraform/state"
  }
}