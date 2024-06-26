resource "azurerm_log_analytics_workspace" "log_analytics_workspace" {
    name                = "${var.service_name}-log-analytics"
    location            = var.location
	resource_group_name = azurerm_resource_group.group.name
    retention_in_days   = 30
}
