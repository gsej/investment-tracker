# resource "azurerm_public_ip" "pip" {
#   name                = "pip-${var.service_name}"
#   resource_group_name = azurerm_resource_group.group.name
#   location            = azurerm_resource_group.group.location
#   allocation_method   = "Static"
#   sku                 = "Standard"
# }


# resource "azurerm_api_management" "apim" {
#     name = "apim-${var.service_name}"
#     location = azurerm_resource_group.group.location
#     resource_group_name = azurerm_resource_group.group.name
#     publisher_name = "gsej"
#     publisher_email = "gsej@erskinejones.com"
#     sku_name = "Developer_1"
# }