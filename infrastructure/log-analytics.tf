resource "azurerm_log_analytics_workspace" "log_analytics_workspace" {
    name                = "la-${var.service_name}"
    location            = var.location
	resource_group_name = azurerm_resource_group.group.name
    retention_in_days   = 30
}
