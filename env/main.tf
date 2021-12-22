terraform {
  required_providers {
    azurerm = {
      source = "hashicorp/azurerm"
      version = "2.68.0"
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
      function_app_name = "hub",
      app_settings = {
        "EnvironmentSettings:EnvironmentName": var.environment,
        "PublicApiSettings:Settings:Base:BaseUri": "https://something",
        "PublicApiSettings:Settings:Api2Cart:BaseUri": local.integrations_hostname,
        "PublicApiSettings:Settings:Api2Cart:ApiKey": local.api_key,
        "PublicApiSettings:Settings:Vend:BaseUri": local.integrations_hostname,
        "PublicApiSettings:Settings:Vend:ApiKey": local.api_key
      }
    }
  ]
}