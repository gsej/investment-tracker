resource "azurerm_linux_web_app" linuxapp {
    name                = "app-${var.service_name}"
    resource_group_name = azurerm_resource_group.group.name
    service_plan_id     = azurerm_service_plan.plan.id
    location            = azurerm_resource_group.group.location

    site_config {
        application_stack {
       
        dotnet_version              = "8.0"
    }

    }

    app_settings = {
        APPINSIGHTS_INSTRUMENTATIONKEY = azurerm_application_insights.ai.instrumentation_key
        SqlConnectionString = "Server=tcp:${azurerm_mssql_server.sqlserver.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.database.name};Persist Security Info=False;User ID=${azurerm_mssql_server.sqlserver.administrator_login};Password=${azurerm_mssql_server.sqlserver.administrator_login_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
}
}