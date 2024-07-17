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
    }   
}