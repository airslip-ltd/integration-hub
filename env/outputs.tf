output "resource_group_name" {
  value = module.ingredient_bowl.name
}

output "function_app_names" {
  value = module.func_app_host.function_app_names
}