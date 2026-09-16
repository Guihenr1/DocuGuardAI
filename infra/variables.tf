variable "db_password" {
  description = "Administrator password for the PostgreSQL Flexible Server"
  type        = string
  sensitive   = true
}

variable "openai_principal_id" {
  description = "Principal ID that needs Cognitive Services OpenAI User role"
  type        = string
  sensitive   = true
}