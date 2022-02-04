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

variable "shopify_api_key" {}
variable "shopify_api_secret" {}

variable "squarespace_api_key" {}
variable "squarespace_api_secret" {}

variable "ecwid_api_key" {}
variable "ecwid_api_secret" {}

variable "ebay_api_key" {}
variable "ebay_api_secret" {}

variable "bigcommerceapi_api_key" {}
variable "bigcommerceapi_api_secret" {}

variable "etsyapiv3_api_key" {}
variable "etsyapiv3_api_secret" {}

variable "threed_api_key" {}
variable "threed_api_secret" {}