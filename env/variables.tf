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
variable "ui_hostname" {}

variable "certificate_path" {
  default = "./Certificates/airslip.com.pfx"
}
variable "certificate_password" {}
variable "certificate_name" {
  default = "airslip-com-certificate"
}
variable "admin_group_id" {}
variable "deployment_agent_group_id" {}
variable "hostname" {}

variable "shopify_api_key" {}
variable "shopify_api_secret" {}

variable "squarespace_api_key" {}
variable "squarespace_api_secret" {}

variable "ecwid_api_key" {}
variable "ecwid_api_secret" {}

variable "ebay_api_key" {}
variable "ebay_api_secret" {}
variable "ebay_app_name" {}

variable "bigcommerceapi_api_key" {}
variable "bigcommerceapi_api_secret" {}

variable "etsyapiv3_api_key" {}
variable "etsyapiv3_api_secret" {}

variable "threed_cart_api_key" {}
variable "threed_cart_api_secret" {}

variable "amazon_sp_api_key" {}
variable "amazon_sp_api_secret" {}
variable "amazon_sp_app_name" {}
variable "amazon_sp_environment" {}
variable "amazon_sp_location" {}
variable "amazon_sp_role" {}
variable "amazon_sp_user_id" {}
variable "amazon_sp_user_secret" {}
variable "log_level" {
  default = "Warning"
}
