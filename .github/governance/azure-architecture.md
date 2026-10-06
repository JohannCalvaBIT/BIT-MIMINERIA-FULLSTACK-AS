# Azure Architecture

## Overview

All infrastructure runs on **Azure PaaS**. Containers/Kubernetes are not allowed as a deployment topology (no Container Apps, no AKS, no App Service for Containers). Docker on the developer machine and in CI is allowed and expected — local dependencies, Testcontainers (G-TEST-06) and Azurite.

The system supports two communication patterns:
- **Synchronous**: Request-response via REST API (immediate response)
- **Asynchronous**: Event-driven via Azure Service Bus (loose coupling, eventual consistency)

## Components

| Component | Azure Service | Pattern | Purpose |
|-----------|--------------|---------|---------|
| API Gateway | API Management | Sync | Single entry point, JWT validation, rate limiting, CORS |
| Web App | App Service | Sync + Async | REST API + background jobs + domain event publishing |
| Integration | App Service | Async | MassTransit consumers, sagas, external API orchestration |
| Async Compute | Azure Functions | Async | Event-driven processing, background jobs (optional) |
| Message Broker | Service Bus | Async | Topics (pub/sub), queues (point-to-point), dead-letter |
| Cache | Redis | Both | Distributed cache, session state, saga state |
| Database | Azure SQL | Both | Relational data (EF Core), shared across services |
| Storage | Blob Storage | Both | Files, images, documents |
| Auth | Entra ID | Both | Authentication, RBAC, Managed Identities |
| Secrets | Key Vault | Both | Connection strings, certificates, API keys |
| Monitoring | App Insights | Both | APM, distributed tracing, metrics |
| Logging | Log Analytics | Both | Centralized logs, KQL queries |
| Alerts | Azure Monitor | Both | Dashboards, alert rules |

## Synchronous Flow (Request-Response)

Used for CRUD operations and queries requiring immediate response.

```
Client → Azure APIM → App Service (.NET 10) → Azure SQL / Redis
         ├── JWT validation (Entra ID)
         ├── Rate limiting
         ├── IP whitelisting
         └── CORS enforcement

Response ← APIM ← App Service
         ├── Standard envelope (success/error)
         ├── Correlation ID header
         └── HTTP status codes
```

## Asynchronous Flow (Event-Driven)

Used for cross-service communication, background processing, and external API orchestration.

### Event Publishing (Outbox Pattern)

```
Web App (Command Handler)
    │
    ├── 1. Save business data + OutboxMessage in same EF transaction
    │
    ├── 2. BackgroundService reads Outbox table
    │
    ├── 3. Publish to Service Bus Topic
    │
    └── 4. Delete OutboxMessage after confirmation
```

### Event Consumption

```
Service Bus Topic
    │
    ├── Subscription A → Integration Consumer A → Azure SQL / Redis
    ├── Subscription B → Integration Consumer B → External API (Refit + Polly)
    └── Dead-Letter Queue → Alert → Manual replay
```

### Saga Orchestration (Long-Running Workflows)

```
Saga (MassTransit)
    │
    ├── Step 1: Send Command → Service A → Wait for Response Event
    ├── Step 2: Send Command → Service B → Wait for Response Event
    ├── Step 3: Send Command → Service C → Wait for Response Event
    │
    └── On Failure: Execute compensating actions (reverse order)
```

## Hybrid Flow (Sync Command + Async Side Effects)

```
Client → POST /api/orders (Sync)
    │
    ├── 201 Created (immediate response)
    │
    └── Web App publishes OrderCreatedDomainEvent to Service Bus (Async)
         │
         ├── Integration Service consumes → Update inventory
         ├── Integration Service consumes → Send notification
         └── Integration Service consumes → Sync with ERP
```

## Communication Decision Guide

| Criteria | Synchronous | Asynchronous |
|----------|-------------|--------------|
| Response time needed | Immediate (<500ms) | Deferred (seconds/minutes) |
| Consistency | Strong consistency | Eventual consistency |
| Client waiting | Yes | No (fire-and-forget) |
| Multiple services involved | No (or minimal) | Yes |
| Retry on failure | Client responsibility | Built-in (Service Bus + Polly) |
| Loose coupling | No | Yes |
| Example | GET /products, POST /order | Order fulfillment, data sync |

## Security

- **Managed Identities**: Service-to-service auth (no connection strings)
- **Private Endpoints**: PaaS services in VNet
- **NSGs**: Network isolation
- **Key Vault**: Centralized secrets management
- **Entra ID**: OAuth 2.0 + JWT validation at APIM level
- **Correlation ID**: Propagated across sync and async boundaries

## Infrastructure as Code

> ⏸️ Diferido por ahora — no requerido (ver G-CICD-01 en guardrails-09-devops-cicd.md). Cuando se retome, sigue siendo Terraform (solo — sin Bicep, sin ARM, sin Pulumi):

- **Terraform** en `infrastructure/terraform/`
- Separate modules for: web, integration, shared services
- Parameter files per environment (dev, staging, prod)
- Service Bus namespaces with topics, subscriptions, and dead-letter queues
