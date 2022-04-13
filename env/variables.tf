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

variable "log_level" {
  default = "Warning"
}

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

variable "xero_api_key" {}
variable "xero_api_secret" {}

variable "quickbooksonline_api_key" {}
variable "quickbooksonline_api_secret" {}

variable "square_api_key" {}
variable "square_api_secret" {}
variable "square_authorisation_base_uri" {}
variable "square_authorise_path_uri" {}

variable "stripe_api_key" {}
variable "stripe_api_secret" {}

variable "zettle_api_key" {}
variable "zettle_api_secret" {}

variable "clover_api_key" {}
variable "clover_api_secret" {}
variable "clover_authorisation_base_uri" {}
variable "clover_authorise_path_uri" {}

variable "lsrseries_api_key" {}
variable "lsrseries_api_secret" {}

variable "sumup_api_key" {}
variable "sumup_api_secret" {}
