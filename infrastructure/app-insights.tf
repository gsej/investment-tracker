resource "azurerm_application_insights" "ai" {
  name                = "${var.service_name}-application-insights"
  location            = azurerm_resource_group.group.location
  resource_group_name = azurerm_resource_group.group.name
  application_type    = "web"
}