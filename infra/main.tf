terraform {
  required_version = ">= 1.5.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.100"
    }
  }
  # backend "azurerm" {
  #   resource_group_name  = "docuguardai-rg"
  #   storage_account_name = "docuguardaitfstate"
  #   container_name       = "tfstate"
  #   key                  = "docuguardai.tfstate"
  # }
}

provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy    = true
      recover_soft_deleted_key_vaults = true
    }
    # cognitive_account {
    #   purge_soft_delete_on_destroy = true
    # }
  }
}

data "azurerm_client_config" "current" {}

# ─────────────────────────────────────────────
# Resource Group
# ─────────────────────────────────────────────
resource "azurerm_resource_group" "docuguardai" {
  name     = "docuguardai-rg"
  location = "France Central"
}

# ─────────────────────────────────────────────
# Azure AI Content Safety
# ─────────────────────────────────────────────
resource "azurerm_cognitive_account" "content_safety" {
  name                  = "docuguardai-contentsafety"
  location              = azurerm_resource_group.docuguardai.location
  resource_group_name   = azurerm_resource_group.docuguardai.name
  kind                  = "ContentSafety"
  sku_name              = "S0"
  custom_subdomain_name = "docuguardai-contentsafety"   # required for Entra ID / Managed Identity

  identity {
    type = "SystemAssigned"
  }

  public_network_access_enabled = true   # set to false + private endpoint later if needed

  tags = {
    environment = "development"
    project     = "docuguardai"
  }
}

# ─────────────────────────────────────────────
# Key Vault
# ─────────────────────────────────────────────
resource "azurerm_key_vault" "docuguardai" {
  name                       = "docuguardai-kv"
  location                   = azurerm_resource_group.docuguardai.location
  resource_group_name        = azurerm_resource_group.docuguardai.name
  tenant_id                  = data.azurerm_client_config.current.tenant_id
  sku_name                   = "standard"
  soft_delete_retention_days = 7
  purge_protection_enabled   = false

  # Access for the current Terraform identity (so we can create secrets)
  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = data.azurerm_client_config.current.object_id

    secret_permissions = [
      "Get", "List", "Set", "Delete", "Purge", "Recover"
    ]
  }

  # Access for the Content Safety system-assigned identity (optional – only if the service itself needs to read secrets)
  # access_policy {
  #   tenant_id = data.azurerm_client_config.current.tenant_id
  #   object_id = azurerm_cognitive_account.content_safety.identity[0].principal_id
  #
  #   secret_permissions = ["Get", "List"]
  # }

  # Access for the App Service identity (uncomment when you re-enable the Web App)
  # access_policy {
  #   tenant_id = data.azurerm_client_config.current.tenant_id
  #   object_id = azurerm_linux_web_app.docuguardai_api.identity[0].principal_id
  #
  #   secret_permissions = ["Get", "List"]
  # }

  tags = {
    environment = "development"
    project     = "docuguardai"
  }
}

# Store Content Safety endpoint & key in Key Vault
resource "azurerm_key_vault_secret" "content_safety_endpoint" {
  name         = "ContentSafety--Endpoint"
  value        = azurerm_cognitive_account.content_safety.endpoint
  key_vault_id = azurerm_key_vault.docuguardai.id

  depends_on = [azurerm_key_vault.docuguardai]
}

resource "azurerm_key_vault_secret" "content_safety_key" {
  name         = "ContentSafety--Key"
  value        = azurerm_cognitive_account.content_safety.primary_access_key
  key_vault_id = azurerm_key_vault.docuguardai.id

  depends_on = [azurerm_key_vault.docuguardai]
}

resource "azurerm_cognitive_account" "document_intelligence" {
  name                  = "docuguardai-docintel"
  location              = azurerm_resource_group.docuguardai.location
  resource_group_name   = azurerm_resource_group.docuguardai.name
  kind                  = "FormRecognizer"          # Document Intelligence
  sku_name              = "S0"
  custom_subdomain_name = "docuguardai-docintel"

  identity {
    type = "SystemAssigned"
  }

  tags = {
    environment = "development"
    project     = "docuguardai"
  }
}

resource "azurerm_key_vault_secret" "doc_intel_endpoint" {
  name         = "DocumentIntelligence--Endpoint"
  value        = azurerm_cognitive_account.document_intelligence.endpoint
  key_vault_id = azurerm_key_vault.docuguardai.id
}

# ─────────────────────────────────────────────
# Cosmos DB (Long-Term Memory)
# ─────────────────────────────────────────────
resource "azurerm_cosmosdb_account" "docuguardai" {
  name                = "docuguardai-cosmos"
  location            = azurerm_resource_group.docuguardai.location
  resource_group_name = azurerm_resource_group.docuguardai.name
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"

  # Good for development / low cost
  enable_automatic_failover = false
  enable_free_tier          = false       

  consistency_policy {
    consistency_level = "Session"
  }

  geo_location {
    location          = azurerm_resource_group.docuguardai.location
    failover_priority = 0
  }

  # Optional but useful for local development
  capabilities {
    name = "EnableServerless"   # remove this block if you prefer provisioned throughput
  }

  tags = {
    environment = "development"
    project     = "docuguardai"
  }
}

resource "azurerm_cosmosdb_sql_database" "memory" {
  name                = "DocuGuardAI"
  resource_group_name = azurerm_cosmosdb_account.docuguardai.resource_group_name
  account_name        = azurerm_cosmosdb_account.docuguardai.name
}

resource "azurerm_cosmosdb_sql_container" "memory" {
  name                  = "Memory"
  resource_group_name   = azurerm_cosmosdb_account.docuguardai.resource_group_name
  account_name          = azurerm_cosmosdb_account.docuguardai.name
  database_name         = azurerm_cosmosdb_sql_database.memory.name
  partition_key_paths   = ["/userId"]
  partition_key_version = 1

  # Only needed if you are NOT using Serverless
  # throughput = 400
}

# Store Cosmos DB connection details in Key Vault
resource "azurerm_key_vault_secret" "cosmos_endpoint" {
  name         = "CosmosDb--Endpoint"
  value        = azurerm_cosmosdb_account.docuguardai.endpoint
  key_vault_id = azurerm_key_vault.docuguardai.id
}

resource "azurerm_key_vault_secret" "cosmos_key" {
  name         = "CosmosDb--Key"
  value        = azurerm_cosmosdb_account.docuguardai.primary_key
  key_vault_id = azurerm_key_vault.docuguardai.id
}

resource "azurerm_key_vault_secret" "cosmos_connection_string" {
  name         = "CosmosDb--ConnectionString"
  value        = azurerm_cosmosdb_account.docuguardai.primary_sql_connection_string
  key_vault_id = azurerm_key_vault.docuguardai.id
}

# ─────────────────────────────────────────────
# Future resources (commented – re-enable when needed)
# ─────────────────────────────────────────────

# resource "azurerm_container_registry" "acr" {
#   name                = "docuguardairegistry2025"
#   resource_group_name = azurerm_resource_group.docuguardai.name
#   location            = azurerm_resource_group.docuguardai.location
#   sku                 = "Basic"
#   admin_enabled       = true
#
#   tags = {
#     environment = "development"
#     project     = "docuguardai"
#   }
# }

# resource "azurerm_service_plan" "docuguardai" {
#   name                = "docuguardai-plan"
#   resource_group_name = azurerm_resource_group.docuguardai.name
#   location            = azurerm_resource_group.docuguardai.location
#   os_type             = "Linux"
#   sku_name            = "B1"
# }

# resource "azurerm_linux_web_app" "docuguardai_api" {
#   name                = "docuguardai-api-2025"
#   resource_group_name = azurerm_resource_group.docuguardai.name
#   location            = azurerm_resource_group.docuguardai.location
#   service_plan_id     = azurerm_service_plan.docuguardai.id
#
#   site_config {
#     application_stack {
#       docker_image_name        = "docuguardai-api:latest"
#       docker_registry_url      = "https://${azurerm_container_registry.acr.login_server}"
#       docker_registry_username = azurerm_container_registry.acr.admin_username
#       docker_registry_password = azurerm_container_registry.acr.admin_password
#     }
#     always_on = true
#   }
#
#   app_settings = {
#     "WEBSITES_ENABLE_APP_SERVICE_STORAGE" = "false"
#     "DOCKER_ENABLE_CI"                    = "true"
#     # "ContentSafety__Endpoint" = "@Microsoft.KeyVault(SecretUri=...)"  # optional Key Vault reference
#   }
#
#   identity {
#     type = "SystemAssigned"
#   }
# }

# resource "azurerm_role_assignment" "acr_pull" {
#   principal_id                     = azurerm_linux_web_app.docuguardai_api.identity[0].principal_id
#   role_definition_name             = "AcrPull"
#   scope                            = azurerm_container_registry.acr.id
#   skip_service_principal_aad_check = true
# }

# resource "azurerm_postgresql_flexible_server" "docuguardai" {
#   name                   = "docuguardai-db"
#   resource_group_name    = azurerm_resource_group.docuguardai.name
#   location               = azurerm_resource_group.docuguardai.location
#   zone                   = "1"
#
#   administrator_login    = "dbadmin"
#   administrator_password = var.db_password
#
#   sku_name   = "B_Standard_B1ms"
#   storage_mb = 32768
#   version    = "15"
#
#   backup_retention_days        = 7
#   geo_redundant_backup_enabled = false
#
#   tags = {
#     environment = "development"
#     project     = "docuguardai"
#   }
# }

# resource "azurerm_postgresql_flexible_server_firewall_rule" "app_service" {
#   name             = "allow-app-service"
#   server_id        = azurerm_postgresql_flexible_server.docuguardai.id
#   start_ip_address = "0.0.0.0"
#   end_ip_address   = "255.255.255.255"
# }

# ─────────────────────────────────────────────
# Outputs
# ─────────────────────────────────────────────
output "content_safety_endpoint" {
  description = "Azure AI Content Safety endpoint"
  value       = azurerm_cognitive_account.content_safety.endpoint
}

output "content_safety_id" {
  description = "Azure AI Content Safety resource ID"
  value       = azurerm_cognitive_account.content_safety.id
}

output "key_vault_uri" {
  description = "Key Vault URI"
  value       = azurerm_key_vault.docuguardai.vault_uri
}

output "key_vault_name" {
  value = azurerm_key_vault.docuguardai.name
}

output "cosmos_endpoint" {
  description = "Cosmos DB endpoint"
  value       = azurerm_cosmosdb_account.docuguardai.endpoint
}

output "cosmos_account_name" {
  value = azurerm_cosmosdb_account.docuguardai.name
}

# Future outputs (commented)
# output "acr_login_server" {
#   value = azurerm_container_registry.acr.login_server
# }
#
# output "app_service_url" {
#   description = "Full URL to your docuguardai App Service"
#   value       = "https://${azurerm_linux_web_app.docuguardai_api.name}.azurewebsites.net"
# }
#
# output "app_service_hostname" {
#   description = "Default hostname of the App Service"
#   value       = "${azurerm_linux_web_app.docuguardai_api.name}.azurewebsites.net"
# }
#
# output "acr_admin_username" {
#   value     = azurerm_container_registry.acr.admin_username
#   sensitive = true
# }
#
# output "acr_admin_password" {
#   value     = azurerm_container_registry.acr.admin_password
#   sensitive = true
# }
#
# output "db_hostname" {
#   description = "Fully qualified domain name for PostgreSQL flexible server"
#   value       = azurerm_postgresql_flexible_server.docuguardai.fqdn
# }
#
# output "db_password" {
#   description = "PostgreSQL admin password (sensitive)"
#   value       = var.db_password
#   sensitive   = true
# }