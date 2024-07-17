resource "azurerm_mssql_server" "sqlserver" {
  name                         = "sqlserver-${var.service_name}"
  resource_group_name          = azurerm_resource_group.group.name
  location                     = azurerm_resource_group.group.location
  version                      = "12.0"
  administrator_login          = "jack"
  administrator_login_password = "password123!"
  minimum_tls_version          = "1.2"

  identity {
    type = "SystemAssigned"
  }
}


resource "azurerm_mssql_database" "database" {
  name           = "database-${var.service_name}"
  server_id      = azurerm_mssql_server.sqlserver.id
  license_type   = "LicenseIncluded"
  max_size_gb    = 1
  sku_name       = "Basic"
  zone_redundant = false  
}