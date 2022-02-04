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

  shopify_api_key = var.shopify_api_key
  shopify_api_secret = var.shopify_api_secret

  squarespace_api_key = var.squarespace_api_key
  squarespace_api_secret = var.squarespace_api_secret

  ecwid_api_key = var.ecwid_api_key
  ecwid_api_secret = var.ecwid_api_secret

  ebay_api_key = var.ebay_api_key
  ebay_api_secret = var.ebay_api_secret

  bigcommerceapi_api_key = var.bigcommerceapi_api_key
  bigcommerceapi_api_secret = var.bigcommerceapi_api_secret

  etsyapiv3_api_key = var.etsyapiv3_api_key
  etsyapiv3_api_secret = var.etsyapiv3_api_secret
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
        "PublicApiSettings:Settings:Base:BaseUri": local.integrations_hostname,
        "PublicApiSettings:Settings:Base:UriSuffix": "oauth",
        "PublicApiSettings:Settings:Api2Cart:BaseUri": local.integrations_hostname,
        "PublicApiSettings:Settings:Api2Cart:UriSuffix": "api2cart",
        "PublicApiSettings:Settings:Api2Cart:ApiKey": local.api_key,
        "PublicApiSettings:Settings:Vend:BaseUri": local.integrations_hostname,
        "PublicApiSettings:Settings:Vend:ApiKey": local.api_key,
        "ProviderSettings:Settings:Shopify:ApiKey": local.shopify_api_key,
        "ProviderSettings:Settings:Shopify:ApiSecret": local.shopify_api_secret,
        "ProviderSettings:Settings:Squarespace:ApiKey": local.squarespace_api_key,
        "ProviderSettings:Settings:Squarespace:ApiSecret": local.squarespace_api_secret,
        "ProviderSettings:Settings:Ecwid:ApiKey": local.ecwid_api_key,
        "ProviderSettings:Settings:Ecwid:ApiSecret": local.ecwid_api_secret,
        "ProviderSettings:Settings:Ebay:ApiKey": local.ebay_api_key,
        "ProviderSettings:Settings:Ebay:ApiSecret": local.ebay_api_secret,
        "ProviderSettings:Settings:BigcommerceApi:ApiKey": local.bigcommerceapi_api_key,
        "ProviderSettings:Settings:BigcommerceApi:ApiSecret": local.bigcommerceapi_api_secret,
        "ProviderSettings:Settings:EtsyAPIv3:ApiKey": local.etsyapiv3_api_key,
        "ProviderSettings:Settings:EtsyAPIv3:ApiSecret": local.etsyapiv3_api_secret,
        "ProviderSettings:Settings:_3DCart:ApiKey": local.threed_cart_api_key,
        "ProviderSettings:Settings:_3DCart:ApiSecret": local.threed_cart_api_secret,
      }
    }
  ]
}