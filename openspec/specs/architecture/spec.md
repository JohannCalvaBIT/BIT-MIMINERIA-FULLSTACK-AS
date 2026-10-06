# Architecture Specification

## Purpose

Define the target Azure infrastructure and architecture patterns for BIT-MIMINERIA-FULLSTACK. The system SHALL support both synchronous (request-response) and asynchronous (event-driven) communication patterns.

## Requirements

### Requirement: Azure-native infrastructure

All infrastructure SHALL run on Microsoft Azure PaaS. Containers and Kubernetes SHALL NOT be used as a deployment topology. Docker on developer machines and in CI is permitted for local dependencies and Testcontainers-based integration tests.

#### Scenario: Infrastructure provisioning (deferred — not required for now)

- **WHEN** the project resumes IaC work
- **THEN** use Terraform in `infrastructure/terraform/`
- **AND** organize modules per component: web, integration, shared
- **AND** use parameter files per environment (dev, staging, prod)

### Requirement: API Gateway

Azure API Management SHALL be the single entry point for all HTTP requests.

#### Scenario: API Gateway behavior

- **WHEN** a client sends an HTTP request
- **THEN** Azure API Management validates the JWT token
- **AND** applies rate limiting
- **AND** enforces CORS policy
- **AND** forwards to the appropriate backend service

### Requirement: Synchronous communication

Synchronous communication SHALL follow the request-response pattern for operations that require immediate confirmation.

#### Scenario: REST API flow

- **WHEN** a client needs an immediate response
- **THEN** use REST over HTTPS through Azure API Management
- **AND** the flow is: Client → APIM → App Service (.NET 10) → Azure SQL / Redis
- **AND** APIM validates JWT, applies rate limiting, and enforces CORS

#### Scenario: Sync operation rules

- **WHEN** designing a synchronous endpoint
- **THEN** keep response times under 500ms (P95)
- **AND** use caching (Redis) for repeated reads
- **AND** use `AsNoTracking()` for read-only queries
- **AND** paginate all list endpoints
- **AND** return standardized API responses (envelope pattern)

### Requirement: Asynchronous event-driven communication

Asynchronous communication SHALL use Azure Service Bus with MassTransit for operations that do not require immediate confirmation, enabling loose coupling and scalability.

#### Scenario: Event publishing

- **WHEN** a domain event occurs that other services need to know about
- **THEN** publish the event to Azure Service Bus via MassTransit
- **AND** use the Outbox pattern to guarantee delivery (write event to DB + publish to bus atomically)
- **AND** assign a unique event ID for idempotency
- **AND** include a correlation ID for distributed tracing

#### Scenario: Event consumption

- **WHEN** consuming an event from Service Bus
- **THEN** implement idempotent consumers (same event can be delivered multiple times)
- **AND** use dead-letter queues for failed messages after max retries
- **AND** log the event processing result with correlation ID
- **AND** never perform synchronous HTTP calls inside event consumers (use async patterns)

#### Scenario: Event-driven flow

- **WHEN** the system processes an event asynchronously
- **THEN** the flow is: Service → Service Bus → Consumer → Processor → Database
- **AND** use Azure Functions or Hosted Services for background processing
- **AND** use Azure Cache for Redis for distributed cache invalidation

### Requirement: Hybrid sync/async operations

Operations SHALL use synchronous APIs for the initial request and asynchronous events for side effects.

#### Scenario: Command with side effects

- **WHEN** a client creates a resource via REST API
- **THEN** the API SHALL respond synchronously with the created resource
- **AND** the API SHALL publish a domain event (e.g., `EntityCreated`) to Service Bus
- **AND** downstream services SHALL consume the event asynchronously
- **AND** the client SHALL NOT wait for side effects to complete

#### Scenario: Eventual consistency

- **WHEN** a command triggers asynchronous side effects
- **THEN** the system SHALL accept eventual consistency
- **AND** the API response SHALL indicate that the operation was accepted
- **AND** clients SHALL poll or use WebSocket notifications for completion status

### Requirement: Event-driven design patterns

The system SHALL follow proven event-driven patterns for reliability and maintainability.

#### Scenario: Outbox pattern

- **WHEN** publishing events reliably
- **THEN** save events to an Outbox table in the same database transaction as the business operation
- **AND** a background process SHALL read from the Outbox and publish to Service Bus
- **AND** delete published events from the Outbox after confirmation

#### Scenario: Saga pattern (orchestration)

- **WHEN** a workflow spans multiple services with compensation needs
- **THEN** use MassTransit Saga for orchestration
- **AND** define compensating actions for each step
- **AND** persist saga state in the saga repository

#### Scenario: Event versioning

- **WHEN** an event schema evolves
- **THEN** use schema versioning in the event payload (e.g., `eventVersion: 1`)
- **AND** consumers SHALL handle multiple versions
- **AND** use polymorphic event deserialization
- **AND** never remove fields from existing events (only add optional fields)

### Requirement: Authentication and authorization

Microsoft Entra ID SHALL provide authentication and RBAC for all services.

#### Scenario: Service-to-service auth

- **WHEN** an Azure service needs to access another Azure resource
- **THEN** use Managed Identities (no connection strings)
- **AND** never hardcode credentials

#### Scenario: User authentication

- **WHEN** a user authenticates
- **THEN** the client obtains the JWT access and refresh tokens directly from Entra ID (Authorization Code + PKCE) — the backend never issues or persists its own user tokens
- **AND** every endpoint re-validates the token (signature, expiration, audience) via standard JWT Bearer middleware configured against Entra ID's OIDC metadata

### Requirement: Secrets management

Azure Key Vault SHALL store all secrets, certificates, and connection strings.

#### Scenario: Secret retrieval

- **WHEN** an application needs a secret
- **THEN** retrieve it from Azure Key Vault at runtime
- **AND** never store secrets in code, config files, or environment variables

### Requirement: Monitoring and observability

Application Insights and Log Analytics SHALL provide APM, distributed tracing, and centralized logging.

#### Scenario: Request monitoring

- **WHEN** a request enters the system
- **THEN** assign a correlation ID
- **AND** log all operations with structured logging (Serilog)
- **AND** trace across service boundaries
- **AND** propagate correlation IDs through async messages

### Requirement: Network security

Private Endpoints and NSGs SHALL isolate PaaS services within the VNet.

#### Scenario: Network isolation

- **WHEN** deploying PaaS services (SQL, Service Bus, Redis)
- **THEN** use Private Endpoints to restrict access to the VNet
- **AND** configure NSGs for network segmentation

### Requirement: Decision guide — sync vs async

Architects SHALL use the following criteria to choose between sync and async communication.

#### Scenario: Choose synchronous when

- **WHEN** the client needs an immediate response (e.g., read data, create resource)
- **AND** the operation completes in under 500ms
- **AND** consistency is required (no eventual consistency)
- **AND** the operation is a simple CRUD without side effects
- **THEN** use synchronous REST API

#### Scenario: Choose asynchronous when

- **WHEN** the operation involves multiple services
- **AND** the operation takes longer than 500ms
- **AND** the client does not need an immediate response
- **AND** eventual consistency is acceptable
- **AND** the operation needs to be retried on failure
- **AND** you need to decouple producers from consumers
- **THEN** use asynchronous event-driven communication via Service Bus