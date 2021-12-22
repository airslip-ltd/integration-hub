output "resource_group_name" {
  value = module.api_management.resource_group_name
}

output "app_service_name" {
  value = module.api_management.app_service_name
}

output "function_app_names" {
  value = module.func_app_host.function_app_names
}