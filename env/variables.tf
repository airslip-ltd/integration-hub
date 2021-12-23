variable "short_environment" {
  description = "The prefix used for all resources"
}
variable "location" {
  description = "The Azure location where all resources should be created"
}
variable "environment" {
  description = "The environment name being deployed to"
}
variable "api_key" {}
variable "integrations_hostname" {}
variable "shopify_app_id" {}
variable "shopify_app_secret" {}