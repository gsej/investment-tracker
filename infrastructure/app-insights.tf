resource "azurerm_application_insights" "ai" {
  name                = "ai-${var.service_name}"
  location            = azurerm_resource_group.group.location
  resource_group_name = azurerm_resource_group.group.name
  application_type    = "web"
}