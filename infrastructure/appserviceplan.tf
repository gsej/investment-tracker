resource "azurerm_service_plan" "plan" {
  name                = "asp-${var.service_name}"
  resource_group_name = azurerm_resource_group.group.name
  location            = azurerm_resource_group.group.location
  os_type             = "Linux"
  sku_name            = "S1"
}
