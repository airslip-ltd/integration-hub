terraform {
  required_providers {
    azurerm = {
      source = "hashicorp/azurerm"
      version = "2.90.0"
    }
  }

  backend "azurerm" {
  }
}

provider "azurerm" {
  features {}
}

locals {
  tags = {
    Environment = var.environment
  }
  app_id = "integration-hub"
  short_app_id = "int-hub"
  short_environment = var.short_environment
  location = var.location
  api_key = var.api_key
  integrations_hostname = var.integrations_hostname
  ui_hostname = var.ui_hostname
  admin_group_id            = var.admin_group_id
  certificate_name          = var.certificate_name
  certificate_path          = var.certificate_path
  certificate_password      = var.certificate_password
  deployment_agent_group_id = var.deployment_agent_group_id
  hostname                  = var.hostname
  log_level                 = var.log_level

  shopify_api_key = var.shopify_api_key
  shopify_api_secret = var.shopify_api_secret

  squarespace_api_key = var.squarespace_api_key
  squarespace_api_secret = var.squarespace_api_secret

  ecwid_api_key = var.ecwid_api_key
  ecwid_api_secret = var.ecwid_api_secret

  ebay_api_key = var.ebay_api_key
  ebay_api_secret = var.ebay_api_secret
  ebay_app_name = var.ebay_app_name

  bigcommerceapi_api_key = var.bigcommerceapi_api_key
  bigcommerceapi_api_secret = var.bigcommerceapi_api_secret

  etsyapiv3_api_key = var.etsyapiv3_api_key
  etsyapiv3_api_secret = var.etsyapiv3_api_secret

  threed_cart_api_key = var.threed_cart_api_key
  threed_cart_api_secret = var.threed_cart_api_secret

  amazon_sp_api_key = var.amazon_sp_api_key
  amazon_sp_api_secret = var.amazon_sp_api_secret
  amazon_sp_app_name = var.amazon_sp_app_name
  amazon_sp_environment = var.amazon_sp_environment
  amazon_sp_location = var.amazon_sp_location
  amazon_sp_role = var.amazon_sp_role
  amazon_sp_user_id = var.amazon_sp_user_id
  amazon_sp_user_secret = var.amazon_sp_user_secret

  xero_api_key = var.xero_api_key
  xero_api_secret = var.xero_api_secret
}

module "ingredient_bowl" {
  source              = "./tf_modules/Airslip.Terraform.Modules/modules/core/resource_group"

  tags                = local.tags
  app_id              = local.app_id
  short_environment   = local.short_environment
  location            = local.location
}

module "func_app_host" {
  source = "./tf_modules/Airslip.Terraform.Modules/recipes/function_app_multiple_apps"

  app_configuration = {
    app_id = local.app_id,
    short_app_id = local.short_app_id,
    short_environment = local.short_environment,
    location = local.location,
    tags = local.tags,
    account_tier = "Standard",
    account_replication_type = "LRS"
  }

  resource_group = {
    use_existing = true,
    resource_group_name = module.ingredient_bowl.name,
    resource_group_location = module.ingredient_bowl.location
  }

  function_apps = [
    {
      function_app_name = "proc",
      app_settings = {
        "EnvironmentSettings:EnvironmentName": var.environment,
        "Serilog:MinimumLevel:Default": local.log_level,
        "PublicApiSettings:Settings:Base:BaseUri": "https://${local.hostname}",
        "PublicApiSettings:Settings:Base:UriSuffix": "",
        "PublicApiSettings:Settings:UI:BaseUri": local.ui_hostname,
        "PublicApiSettings:Settings:Api2Cart:BaseUri": local.integrations_hostname,
        "PublicApiSettings:Settings:Api2Cart:UriSuffix": "api2cart",
        "PublicApiSettings:Settings:Api2Cart:ApiKey": local.api_key,
        "PublicApiSettings:Settings:Vend:BaseUri": local.integrations_hostname,
        "PublicApiSettings:Settings:Vend:ApiKey": local.api_key,
        "IntegrationSettings:Settings:Shopify:ApiKey": local.shopify_api_key,
        "IntegrationSettings:Settings:Shopify:ApiSecret": local.shopify_api_secret,
        "IntegrationSettings:Settings:Squarespace:ApiKey": local.squarespace_api_key,
        "IntegrationSettings:Settings:Squarespace:ApiSecret": local.squarespace_api_secret,
        "IntegrationSettings:Settings:Ecwid:ApiKey": local.ecwid_api_key,
        "IntegrationSettings:Settings:Ecwid:ApiSecret": local.ecwid_api_secret,
        "IntegrationSettings:Settings:Ebay:ApiKey": local.ebay_api_key,
        "IntegrationSettings:Settings:Ebay:ApiSecret": local.ebay_api_secret,
        "IntegrationSettings:Settings:Ebay:AppName": local.ebay_app_name,
        "IntegrationSettings:Settings:BigcommerceApi:ApiKey": local.bigcommerceapi_api_key,
        "IntegrationSettings:Settings:BigcommerceApi:ApiSecret": local.bigcommerceapi_api_secret,
        "IntegrationSettings:Settings:EtsyAPIv3:ApiKey": local.etsyapiv3_api_key,
        "IntegrationSettings:Settings:EtsyAPIv3:ApiSecret": local.etsyapiv3_api_secret,
        "IntegrationSettings:Settings:_3DCart:ApiKey": local.threed_cart_api_key,
        "IntegrationSettings:Settings:_3DCart:ApiSecret": local.threed_cart_api_secret,
        "IntegrationSettings:Settings:AmazonSP:ApiKey": local.amazon_sp_api_key,
        "IntegrationSettings:Settings:AmazonSP:ApiSecret": local.amazon_sp_api_secret,
        "IntegrationSettings:Settings:AmazonSP:AppName": local.amazon_sp_app_name,
        "IntegrationSettings:Settings:AmazonSP:Environment": local.amazon_sp_environment,
        "IntegrationSettings:Settings:AmazonSP:Location": local.amazon_sp_location,
        "IntegrationSettings:Settings:AmazonSP:AdditionalFieldOne": local.amazon_sp_role,
        "IntegrationSettings:Settings:AmazonSP:AdditionalFieldTwo": local.amazon_sp_user_id,
        "IntegrationSettings:Settings:AmazonSP:AdditionalFieldThree": local.amazon_sp_user_secret
      }
    }
  ]
}

data "azurerm_client_config" "current" {}

module "frontdoor" {
  source = "./tf_modules/Airslip.Terraform.Modules/recipes/app_service_front_door"

  app_configuration = {
    app_id               = local.app_id,
    hostname             = local.hostname,
    backend_hostname     = module.func_app_host.hostname.0,
    app_id_short         = local.short_app_id,
    short_environment    = local.short_environment,
    location             = module.ingredient_bowl.location,
    tags                 = local.tags,
    certificate_name     = local.certificate_name,
    certificate_path     = local.certificate_path,
    certificate_password = local.certificate_password,
    tenant_id            = data.azurerm_client_config.current.tenant_id,
    admin_group_id       = local.admin_group_id,
    deployer_id          = local.deployment_agent_group_id
  }

  resource_group = {
    use_existing = true,
    resource_group_name = module.ingredient_bowl.name,
    resource_group_location = module.ingredient_bowl.location
  }
}
