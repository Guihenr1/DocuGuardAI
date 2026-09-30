# DocuGuardAI

**Status: Active development** (Core features completed up to multi-agent orchestration)

DocuGuardAI is a .NET 10 solution for intelligent document processing.  
It analyzes documents, extracts structured information, applies content safety checks, and supports multi-turn conversations through a Manager + Support agent architecture.

### What’s implemented

- Azure Key Vault + Managed Identity
- Document Intelligence (and multimodal AI clients)
- Content Safety
- NLP preprocessing pipeline
- Cosmos DB long-term memory
- Redis cache with local fallback
- Conversational loop
- Manager & Support Agents (orchestration)

### Tech Stack

- .NET 10
- Clean Architecture
- MediatR (CQRS)
- Azure AI Foundry
- Azure Document Intelligence
- Azure Content Safety
- Azure Cosmos DB
- Redis
- Azure Key Vault

### Project Structure

src/
├── DocuGuardAI.Domain
├── DocuGuardAI.Application
├── DocuGuardAI.Infrastructure
└── DocuGuardAI.WebApi

### Roadmap (next)

- SLM memory updates
- MCP server integrations

---

Still under active development. Documentation and additional features will be added progressively.