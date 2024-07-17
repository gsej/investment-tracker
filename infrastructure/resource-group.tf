resource "azurerm_resource_group" "group" {
  name     = "rg-${var.service_name}"
  location = var.location
}