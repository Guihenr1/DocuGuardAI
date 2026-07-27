terraform {
  required_version = ">= 1.5.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~>3.100"
    }
  }
  backend "azurerm" {
    resource_group_name  = "docuguardai-rg"
    storage_account_name = "docuguardaitfstate"
    container_name       = "tfstate"
    key                  = "docuguardai.tfstate"
  }
}

provider "azurerm" {
  features {}
}

data "azurerm_client_config" "current" {}

resource "azurerm_resource_group" "docuguardai" {
  name     = "docuguardai-rg"
  location = "France Central"
}

resource "azurerm_container_registry" "acr" {
  name                = "docuguardairegistry2025"
  resource_group_name = azurerm_resource_group.docuguardai.name
  location            = azurerm_resource_group.docuguardai.location
  sku                 = "Basic"
  admin_enabled       = true

  tags = {
    environment = "development"
    project     = "docuguardai"
  }
}

resource "azurerm_service_plan" "docuguardai" {
  name                = "docuguardai-plan"
  resource_group_name = azurerm_resource_group.docuguardai.name
  location            = azurerm_resource_group.docuguardai.location
  os_type             = "Linux"
  sku_name            = "B1"
}

resource "azurerm_linux_web_app" "docuguardai_api" {
  name                = "docuguardai-api-2025"
  resource_group_name = azurerm_resource_group.docuguardai.name
  location            = azurerm_resource_group.docuguardai.location
  service_plan_id     = azurerm_service_plan.docuguardai.id

  site_config {
    application_stack {
      docker_image_name        = "docuguardai-api:latest"
      docker_registry_url      = "https://${azurerm_container_registry.acr.login_server}"
      docker_registry_username = azurerm_container_registry.acr.admin_username
      docker_registry_password = azurerm_container_registry.acr.admin_password
    }
    always_on = true
  }

  app_settings = {
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE" = "false"
    "DOCKER_ENABLE_CI"                    = "true"
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_role_assignment" "acr_pull" {
  principal_id                     = azurerm_linux_web_app.docuguardai_api.identity[0].principal_id
  role_definition_name             = "AcrPull"
  scope                            = azurerm_container_registry.acr.id
  skip_service_principal_aad_check = true
}

resource "azurerm_postgresql_flexible_server" "docuguardai" {
  name                   = "docuguardai-db"
  resource_group_name    = azurerm_resource_group.docuguardai.name
  location               = azurerm_resource_group.docuguardai.location
  zone                   = "1"

  administrator_login    = "dbadmin"
  administrator_password = var.db_password  

  sku_name               = "B_Standard_B1ms" 
  storage_mb             = 32768        
  version                = "15"

  backup_retention_days  = 7
  geo_redundant_backup_enabled = false

  tags = {
    environment = "development"
    project     = "docuguardai"
  }
}

resource "azurerm_postgresql_flexible_server_firewall_rule" "app_service" {
  name             = "allow-app-service"
  server_id        = azurerm_postgresql_flexible_server.docuguardai.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "255.255.255.255"
}

resource "azurerm_key_vault" "docuguardai" {
  name                = "docuguardai-kv"
  location            = azurerm_resource_group.docuguardai.location
  resource_group_name = azurerm_resource_group.docuguardai.name
  tenant_id           = data.azurerm_client_config.current.tenant_id
  sku_name            = "standard"

  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = azurerm_linux_web_app.docuguardai_api.identity[0].principal_id

    secret_permissions = ["Get", "List"]
  }
}

output "acr_login_server" {
  value = azurerm_container_registry.acr.login_server
}

output "app_service_url" {
  description = "Full URL to your docuguardai App Service"
  value       = "https://${azurerm_linux_web_app.docuguardai_api.name}.azurewebsites.net"
}

output "app_service_hostname" {
  description = "Default hostname of the App Service"
  value       = "${azurerm_linux_web_app.docuguardai_api.name}.azurewebsites.net"
}

output "acr_admin_username" {
  value     = azurerm_container_registry.acr.admin_username
  sensitive = true
}

output "acr_admin_password" {
  value     = azurerm_container_registry.acr.admin_password
  sensitive = true
}