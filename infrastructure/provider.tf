terraform {
  #required_version = ">=1.4.5"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      #version = "~>3.53.0"
    }
    random = {
      source  = "hashicorp/random"
      #version = "~>3.0"
    }
  }

}

provider "azurerm" {
  features {
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }
}
