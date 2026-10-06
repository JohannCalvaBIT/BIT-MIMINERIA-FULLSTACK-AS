# Technology Stack Specification

## Purpose

Document the locked technology versions and dependencies for BIT-MIMINERIA-FULLSTACK.

## Requirements

### Requirement: Frontend versions

The frontend SHALL use locked versions as defined in the Technology Lock table in `CLAUDE.md` (or `.github/copilot-instructions.md` for Copilot).

#### Scenario: Frontend dependency versions

- **WHEN** configuring frontend dependencies
- **THEN** Angular SHALL be version 20.0.0
- **AND** TypeScript SHALL be version 5.7
- **AND** Tailwind CSS SHALL be version 4.1
- **AND** RxJS SHALL be version 7.8.1

### Requirement: Backend versions

The backend SHALL use locked versions as defined in the Technology Lock table in `CLAUDE.md` (or `.github/copilot-instructions.md` for Copilot).

#### Scenario: Backend dependency versions

- **WHEN** configuring backend dependencies
- **THEN** .NET SHALL be version 10.0
- **AND** C# SHALL be version 13
- **AND** ASP.NET Core SHALL be version 10.0
- **AND** Entity Framework Core SHALL be version 10.0
- **AND** MediatR SHALL be version 12.x
- **AND** AutoMapper SHALL be version 13.x
- **AND** FluentValidation SHALL be version 11.x
- **AND** Serilog SHALL be version 7.x

### Requirement: Mobile versions

The mobile app SHALL use locked versions as defined in the Technology Lock table in `CLAUDE.md` (or `.github/copilot-instructions.md` for Copilot).

#### Scenario: Mobile dependency versions

- **WHEN** configuring mobile dependencies
- **THEN** Flutter SHALL be version 3.27
- **AND** Dart SHALL be version 3.4
- **AND** BLoC SHALL be the latest compatible version
- **AND** GoRouter SHALL be the latest compatible version
- **AND** Retrofit SHALL be the latest compatible version
- **AND** `sqflite_async` SHALL be the latest compatible version
- **AND** `freezed` SHALL be used for code generation
- **AND** `json_serializable` SHALL be used for JSON serialization
- **AND** `injectable` SHALL be used for dependency injection

### Requirement: Integration versions

The integration service SHALL use locked versions as defined in the Technology Lock table in `CLAUDE.md` (or `.github/copilot-instructions.md` for Copilot).

#### Scenario: Integration dependency versions

- **WHEN** configuring integration dependencies
- **THEN** .NET SHALL be version 10.0
- **AND** MassTransit SHALL be version 8.x
- **AND** Azure Service Bus SHALL be the latest SDK
- **AND** Refit SHALL be version 7.x
- **AND** Polly SHALL be version 8.x
- **AND** StackExchange.Redis SHALL be version 2.7+
- **AND** Azure Cache for Redis SHALL be the latest managed service