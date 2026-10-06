# Web Application Specification

## Purpose

Define the Web Application component: Angular 20 frontend + .NET 10 backend with Hexagonal Architecture, CQRS, and event-driven capabilities. The web backend SHALL support both synchronous REST APIs and asynchronous event publishing.

## Requirements

### Requirement: Frontend technology stack

The frontend SHALL use Angular 20 with TypeScript 5.8 and Tailwind CSS 4.

#### Scenario: Frontend development

- **WHEN** building UI components
- **THEN** use Angular 20 with standalone components
- **AND** style with Tailwind CSS 4 utility classes
- **AND** use TypeScript 5.8 in strict mode
- **AND** use RxJS 7.8.2 for reactive state management

### Requirement: Backend technology stack

The backend SHALL use .NET 10 with C# 14, ASP.NET Core 10, and Entity Framework Core 10.

#### Scenario: Backend API development

- **WHEN** building REST endpoints
- **THEN** use ASP.NET Core 10 controllers
- **AND** use Entity Framework Core 10 for data access
- **AND** use AutoMapper 13 for entity-to-DTO mapping
- **AND** use FluentValidation 11 for input validation
- **AND** use Serilog 7 for structured logging

### Requirement: Architecture pattern

The backend SHALL follow Hexagonal Architecture with CQRS using MediatR.

#### Scenario: Hexagonal layer isolation

- **WHEN** organizing code
- **THEN** organize in four layers: Domain, Application, Infrastructure, Presentation
- **AND** Domain SHALL have zero external dependencies
- **AND** Application SHALL depend only on Domain
- **AND** Infrastructure SHALL implement Domain interfaces
- **AND** Presentation SHALL never import Infrastructure directly

#### Scenario: CQRS with MediatR

- **WHEN** handling a request
- **THEN** Commands SHALL handle mutations (Create, Update, Delete)
- **AND** Queries SHALL handle read operations
- **AND** use MediatR 12 for command/query dispatch
- **AND** queries SHALL use `AsNoTracking()` with pagination

### Requirement: Domain events

The domain layer SHALL define domain events for state changes that other services need to react to. The web backend SHALL publish domain events to Azure Service Bus for asynchronous processing.

#### Scenario: Defining domain events

- **WHEN** a significant domain state change occurs (e.g., entity created, status changed)
- **THEN** raise a domain event in the Domain layer
- **AND** the event SHALL be a plain object with a meaningful name (e.g., `OrderCreatedDomainEvent`)
- **AND** include the entity ID, timestamp, and relevant payload
- **AND** the event SHALL implement a marker interface `IDomainEvent`

#### Scenario: Publishing domain events

- **WHEN** a command handler completes successfully
- **THEN** collect all domain events raised during the operation
- **AND** publish them to Azure Service Bus via MassTransit
- **AND** use the Outbox pattern for reliable delivery
- **AND** the outbox SHALL use the same database transaction as the business operation
- **AND** a background job SHALL read from the Outbox table and publish pending events

#### Scenario: Outbox implementation

- **WHEN** saving business data
- **THEN** save domain events to an `OutboxMessage` table in the same EF Core transaction
- **AND** include event type, serialized payload, correlation ID, and creation timestamp
- **AND** a Hosted Service SHALL poll the Outbox table every second for unsent messages
- **AND** publish each message to Service Bus
- **AND** mark the message as sent after successful delivery
- **AND** implement a retry mechanism with exponential backoff for failed deliveries

### Requirement: Background jobs

The web backend SHALL support background job processing for operations that do not require immediate completion.

#### Scenario: Background job execution

- **WHEN** an operation needs to run in the background (e.g., report generation, notification dispatch)
- **THEN** use `IHostedService` or `BackgroundService` in .NET
- **AND** register the background service in the DI container
- **AND** use a shared database for job state tracking
- **AND** implement health checks for background services

### Requirement: Database

The application SHALL use Azure SQL (shared with Integration service).

#### Scenario: Database access

- **WHEN** accessing the database
- **THEN** use Entity Framework Core 10 with parameterized queries
- **AND** use EF Core migrations (versioned, reversible)
- **AND** prevent SQL injection via parameterized queries only
- **AND** use Row-Level Security (RLS) for multi-tenant isolation, only if the project is multi-tenant

### Requirement: Testing

The backend SHALL have unit tests (75%+ coverage) and integration tests.

#### Scenario: Test organization

- **WHEN** writing tests
- **THEN** place unit tests in `Tests/Unit/`
- **AND** place integration tests in `Tests/Integration/`
- **AND** use NUnit 4.2 + Fluent Assertions 6.12
- **AND** test domain events are raised in unit tests
- **AND** test outbox integration with a fake Service Bus in integration tests