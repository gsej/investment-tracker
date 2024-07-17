resource "azurerm_linux_web_app" linuxapp {
    name                = "app-${var.service_name}"
    resource_group_name = azurerm_resource_group.group.name
    service_plan_id     = azurerm_service_plan.plan.id
    location            = azurerm_resource_group.group.location

    site_config {
        linux_fx_version = "DOTNETCORE|8.0" # This value might need adjustment based on Azure's current naming conventions for .NET runtimes
        always_on        = true  # Recommended for production environments
        ftps_state       = "Disabled" # Optional: Disable FTP access

    }

    app_settings = {
        APPINSIGHTS_INSTRUMENTATIONKEY = azurerm_application_insights.ai.instrumentation_key
    }   
}