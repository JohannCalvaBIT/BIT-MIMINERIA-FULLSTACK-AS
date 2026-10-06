# Estructura de Carpetas - BIT Developer Framework

Este documento define la estructura del **proyecto objetivo**: lo que genera `bit init` en el repositorio de un cliente. No existe un "repositorio plantilla" que se copie — el framework (este repo) es el CLI `bit`, que genera esta estructura mediante scaffolding programático, no copiando archivos de una plantilla estática.

## Estructura del proyecto objetivo

La siguiente estructura aplica al proyecto real del cliente, generado por `bit init`:

```text
<project_name>/
├── docs/
│   ├── diagramas/
│   ├── manuales/
│   ├── metricas/
│   │   └── <project_name>_metricas.csv
│   └── def/requisitosnegocio/
├── src/
│   └── apps/
│       ├── web/                          ← Monolito (Frontend + Backend)
│       │   ├── frontend/                 ← Angular 20 + TypeScript 5.8
│       │   │   └── (frontend structure)
│       │   │
│       │   ├── backend/                  ← .NET 10 + Arquitectura Hexagonal
│       │   │   ├── Domain/
│       │   │   ├── Application/
│       │   │   ├── Infrastructure/
│       │   │   └── Presentation/
│       │   │
│       │   └── database/                 ← SQL Server 2022 + EF Core 10
│       │       └── Migrations/
│       │
│       ├── mobile/                       ← App móvil cliente (SOLO frontend)
│       │   └── flutter/                  ← Flutter 3.47 + Dart 3.13
│       │       ├── lib/
│       │       │   ├── domain/
│       │       │   ├── application/
│       │       │   ├── infrastructure/
│       │       │   └── presentation/
│       │       └── test/
│       │
│       └── integracion/                  ← App separada (Arquitectura Hexagonal)
│           │                             ← MassTransit + Azure Service Bus
│           ├── Domain/                   ← Entidades + eventos de dominio
│           │   ├── Entities/
│           │   ├── Interfaces/
│           │   └── DomainEvents/
│           │
│           ├── Application/              ← Casos de uso + DTOs
│           │   ├── Commands/
│           │   ├── Queries/
│           │   ├── DTOs/
│           │   └── Services/
│           │
│           ├── Infrastructure/           ← Adaptadores (DB, Bus, HTTP, Cache)
│           │   ├── Repositories/
│           │   ├── MassTransitConsumers/
│           │   ├── HttpClients/          ← Refit + Polly
│           │   ├── CacheManagers/        ← Azure Cache for Redis
│           │   ├── HostedServices/       ← Background workers
│           │   └── DataSources/
│           │
│           ├── Presentation/             ← API endpoints + Health checks
│           │   ├── Controllers/
│           │   ├── Middleware/
│           │   └── HealthChecks/
│           │
│           └── Tests/
│
├── tests/
│   ├── e2e/                               ← Playwright (smoke test base + specs de negocio)
│   └── functional/
├── infrastructure/                       ← ⏸️ diferido, no requerido por ahora (ver G-CICD-01)
│   └── terraform/                        ← IaC: 1 TF para web, 1 para integracion (cuando se retome)
├── .github/
│   ├── governance/
│   │   └── guardrails/                   ← los 11 guardrails + estructura-carpetas.md
│   │                                        (NO en docs/ — ver nota abajo)
│   ├── copilot-instructions.md
│   ├── skills/
│   └── workflows/ci.yml
├── .claude/
└── readme.md
```

> **Por qué `.github/governance/guardrails/` y no `docs/guardrails/`**: `.github/` (junto con `.claude/`) ya es territorio reservado para archivos que las herramientas (GitHub Actions, Copilot, Claude Code) exigen en rutas fijas — no es una carpeta donde alguien del equipo espere encontrar y "limpiar" documentación de proyecto casualmente, a diferencia de `docs/` (diagramas, manuales, métricas, requisitos de negocio). Los guardrails viven ahí para reducir el riesgo de que se borren por accidente durante una limpieza de `docs/`.

## Separación clara de componentes

| Componente | Ubicación | Arquitectura | Conexión |
|-----------|-----------|--------------|----------|
| **Web Frontend** | `src/apps/web/frontend/` | N/A (Angular) | HTTP → Backend |
| **Web Backend** | `src/apps/web/backend/` | **Hexagonal** (Domain→App→Infra→Presentation) | API REST + Shared DB |
| **Mobile** | `src/apps/mobile/flutter/` | **Clean Architecture** (domain, application, infrastructure, presentation) | HTTP → APIs REST |
| **Integración** | `src/apps/integracion/` | **Hexagonal** (Domain→App→Infra→Presentation) | Service Bus + Shared DB + APIs |

---

## Runtime e infraestructura (Azure)

El cómputo de la aplicación corre **directamente sobre servicios PaaS de Azure**: App Service para Web e Integración, y Azure Functions solo si el proyecto usa cómputo event-driven (condicional, ver guardrails-08).

### ❌ Prohibido como topología de despliegue

- Docker/contenedores como unidad de despliegue (Container Apps, AKS, App Service for Containers)
- Kubernetes en cualquier forma
- Orquestadores de contenedores propios

Desplegar la aplicación empaquetada en una imagen requiere **aprobación explícita del owner**: cambia el modelo operativo, de costos y de seguridad de todo el proyecto.

### ✅ Permitido en la máquina del desarrollador y en CI

Docker **sí** se usa como herramienta de desarrollo y prueba — es una dependencia real del framework, no una excepción tolerada:

- Levantar dependencias de infraestructura en local (SQL Server, Redis, Azurite) para el loop de desarrollo
- **Testcontainers** para tests de integración — exigido por `G-TEST-06` en `guardrails-11-testing.md`
- **Azurite** como sustituto local de Blob Storage — exigido por `guardrails-08-azure-functions.md` y `AZF-VALIDATOR.md`
- Contenedores efímeros en CI para correr la suite de tests

La regla es sobre **dónde corre la aplicación en producción**, no sobre con qué herramientas trabaja el desarrollador.

### Límite: el emulador no reemplaza a Azure

Un sustituto local no valida el servicio real. Antes de un merge a `main`/release, los flujos que dependen de Entra ID, Service Bus, Azure SQL o Key Vault se validan contra un ambiente Azure de desarrollo/test. Los emuladores divergen en autenticación (Managed Identity vs. usuario/contraseña), permisos, cuotas y superficie de API.

---

## C) Backend Web (.NET 10 + Arquitectura Hexagonal)

```text
src/apps/web/backend/
├── Domain/                              ← Core de negocio (NO depende de nada)
│   ├── Entities/
│   │   ├── Order.cs
│   │   ├── Customer.cs
│   │   └── Muestra.cs
│   │
│   ├── Interfaces/                      ← Puertos (abstracciones)
│   │   ├── IOrderRepository.cs
│   │   ├── INotificationService.cs
│   │   └── IExternalApiClient.cs
│   │
│   └── Exceptions/
│       ├── OrderNotFoundException.cs
│       └── InvalidMuestraException.cs
│
├── Application/                         ← Orquestación (casos de uso)
│   ├── Commands/
│   │   ├── CreateOrderCommand.cs
│   │   └── CreateOrderCommandHandler.cs    (MediatR pattern)
│   │
│   ├── Queries/
│   │   ├── GetOrderByIdQuery.cs
│   │   └── GetOrderByIdQueryHandler.cs
│   │
│   ├── DTOs/
│   │   ├── OrderCreateDto.cs
│   │   └── OrderResponseDto.cs
│   │
│   └── Services/
│       ├── OrderApplicationService.cs
│       └── MuestraApplicationService.cs
│
├── Infrastructure/                      ← Adaptadores (implementaciones)
│   ├── Repositories/
│   │   ├── OrderRepository.cs           (implementa IOrderRepository)
│   │   ├── MuestraRepository.cs
│   │   └── BaseRepository.cs
│   │
│   ├── DataSources/
│   │   ├── SqlServerDataSource.cs
│   │   └── EFContext.cs                 (Entity Framework Core 10)
│   │
│   ├── ExternalApiClients/
│   │   └── NotificationApiClient.cs     (implementa INotificationService)
│   │
│   ├── Mappers/
│   │   ├── OrderMapper.cs               (Entity ↔ DTO)
│   │   └── AutoMapperProfile.cs
│   │
│   └── Persistence/
│       └── (config. de DbContext — SIN migraciones EF Core; el esquema vive en database/, Script-Only, ver guardrails-10)
│
├── Presentation/                        ← Web API (Controllers)
│   ├── Controllers/
│   │   ├── OrdersController.cs          (HTTP GET/POST/PUT/DELETE)
│   │   └── MuestrasController.cs
│   │
│   ├── Middleware/
│   │   ├── GlobalExceptionHandler.cs
│   │   └── AuthenticationMiddleware.cs
│   │
│   └── Program.cs                       ← DI + Service Registration
│
└── Tests/
    ├── Unit/
    │   ├── Domain/
    │   └── Application/
    └── Integration/
        └── Controllers/
```

---

## D) Integración (.NET 10 + Arquitectura Hexagonal Mínima)

```text
src/apps/integracion/
├── Domain/                              ← Core (eventos + invariantes)
│   ├── Entities/
│   │   ├── SyncTask.cs
│   │   └── IntegrationLog.cs
│   │
│   ├── Interfaces/
│   │   ├── IEventPublisher.cs           (puerto: publiación eventos)
│   │   ├── IExternalApiClient.cs        (puerto: llamadas HTTP)
│   │   └── ISyncRepository.cs           (puerto: persistencia)
│   │
│   └── DomainEvents/
│       ├── MuestraCreatedEvent.cs
│       └── ResultadoSyncedEvent.cs
│
├── Application/                         ← Casos de uso de integración
│   ├── Commands/
│   │   ├── SyncMuestraCommand.cs
│   │   └── PublishResultadoCommand.cs
│   │
│   ├── Queries/
│   │   └── GetSyncStatusQuery.cs
│   │
│   ├── DTOs/
│   │   ├── MuestraSyncDto.cs
│   │   └── ResultadoSyncDto.cs
│   │
│   └── Services/
│       ├── MuestraSyncService.cs
│       └── EventOrchestrationService.cs
│
├── Infrastructure/                      ← Adaptadores reales
│   ├── Repositories/
│   │   ├── SyncRepository.cs
│   │   └── IntegrationLogRepository.cs
│   │
│   ├── MassTransitConsumers/            ← Handlers de eventos
│   │   ├── MuestraCreatedConsumer.cs
│   │   └── ResultadoSyncedConsumer.cs
│   │
│   ├── HttpClients/                     ← Refit + Polly
│   │   ├── IExternalApiClient.cs        (Refit interface)
│   │   └── ExternalApiClientConfig.cs
│   │
│   ├── CacheManagers/                   ← Azure Cache for Redis
│   │   ├── RedisCacheManager.cs
│   │   └── CacheConfiguration.cs
│   │
│   ├── HostedServices/                  ← Background workers
│   │   ├── MuestraSyncWorker.cs
│   │   └── ResultadoSyncWorker.cs
│   │
│   ├── EventPublishers/
│   │   └── ServiceBusEventPublisher.cs  (Azure Service Bus)
│   │
│   └── DataSources/
│       ├── IntegrationContext.cs        (shared DB: misma SQL Server que Web)
│       └── Migrations/
│
├── Presentation/                        ← API + Health checks
│   ├── Controllers/
│   │   ├── SyncController.cs
│   │   └── StatusController.cs
│   │
│   ├── Middleware/
│   │   └── CorrelationIdMiddleware.cs
│   │
│   └── HealthChecks/
│       ├── ServiceBusHealthCheck.cs
│       ├── ExternalApiHealthCheck.cs
│       └── DatabaseHealthCheck.cs
│
└── Tests/
    └── Integration/
        ├── MassTransitConsumers/
        └── SyncServices/
```

---

## E) Mobile Flutter (Clean Architecture - SOLO FRONTEND)

```text
src/apps/mobile/flutter/
├── lib/
│   ├── domain/                          ← Lógica pura (SIN dependencias)
│   │   ├── entities/
│   │   │   └── muestra_entity.dart
│   │   ├── repositories/                ← Puertos (abstracciones)
│   │   │   └── muestra_repository.dart  (abstract class)
│   │   └── usecases/
│   │       ├── get_muestras_usecase.dart
│   │       └── create_muestra_usecase.dart
│   │
│   ├── application/                     ← Orquestación (BLoC)
│   │   ├── blocs/
│   │   │   ├── muestras_bloc.dart
│   │   │   ├── muestras_event.dart      (@freezed sealed class)
│   │   │   └── muestras_state.dart      (@freezed sealed class)
│   │   │
│   │   └── services/
│   │       ├── auth_service.dart
│   │       └── connectivity_service.dart
│   │
│   ├── infrastructure/                  ← Adaptadores (HTTP, SQLite)
│   │   ├── datasources/
│   │   │   ├── muestra_remote_datasource.dart    (Retrofit)
│   │   │   └── muestra_local_datasource.dart     (SQLite)
│   │   │
│   │   ├── repositories/
│   │   │   └── muestra_repository_impl.dart      (implementa interface de domain)
│   │   │
│   │   ├── models/
│   │   │   └── muestra_model.dart               (@JsonSerializable)
│   │   │
│   │   └── local/
│   │       └── database_config.dart             (sqflite_async)
│   │
│   ├── presentation/                    ← SOLO UI (Widgets + Pages)
│   │   ├── pages/
│   │   │   └── muestras_page.dart
│   │   │
│   │   ├── widgets/
│   │   │   ├── muestra_card.dart
│   │   │   └── muestra_list_item.dart
│   │   │
│   │   ├── themes/
│   │   │   └── app_theme.dart           (Material Design 3)
│   │   │
│   │   └── routes/
│   │       └── app_router.dart          (GoRouter with guards)
│   │
│   ├── config/
│   │   ├── di/
│   │   │   └── service_locator.dart     (@injectable)
│   │   ├── environment/
│   │   └── constants/
│   │
│   └── main.dart
│
├── test/                                ← ≥75% cobertura domain + application
│   ├── domain/
│   │   └── usecases/
│   ├── application/
│   │   └── blocs/
│   └── presentation/
│       └── pages/
│
├── pubspec.yaml                         ← Versiones LOCKED
├── analysis_options.yaml
└── .gitignore
```

---

## Convenciones Generales (Hexagonal + Clean Architecture)

### Flujo de datos (unidireccional)

```
USUARIO INTERACTÚA
        ↓
   PRESENTATION (Controller/Page)
        ↓
   APPLICATION (Command/BLoC)
        ↓
   DOMAIN (Usecase/Entity)
        ↓
   INFRASTRUCTURE (Repository/Datasource)
        ↓
   PERSISTENCIA (DB/API/Cache)
```

**Regla crítica**: Nunca importar capa superior desde capa inferior. Las dependencias fluyen HACIA el Domain.

### Convención de nombres por capa

| Capa | Patrón | Ejemplo |
|------|--------|---------|
| Domain | `{Nombre}Entity` / `I{Nombre}Repository` | `OrderEntity`, `IOrderRepository` |
| Application | `{Action}{Entity}Command` / `{Entity}Bloc` | `CreateOrderCommand`, `OrdersBloc` |
| Infrastructure | `{Entity}Repository` / `{Entity}Datasource` | `OrderRepository`, `OrderRemoteDatasource` |
| Presentation | `{Entity}Page` / `{Entity}Card` | `OrdersPage`, `OrderCard` |


│   ├── e2e/
│   └── functional/
├── infrastructure/                       ← ⏸️ diferido, no requerido por ahora (ver G-CICD-01)
│   └── terraform/
├── .github/
├── .claude/
└── readme.md
```

## Stack de plantilla

- enterprise-angular-net-sqlserver: Angular 20 + .NET 10 + SQL Server

Regla:

- No mezclar esta plantilla con otros frameworks frontend en el mismo proyecto.
- Cualquier cambio de stack requiere aprobacion explicita.

## Convenciones Flutter (Clean Architecture)

**Stack**: Flutter 3.47 + Dart 3.13 + BLoC + SQLite + build_runner

### Capas obligatorias

| Capa | Responsabilidad | Dependencias |
|------|-----------------|--------------|
| **Domain** | Entities, Repositories (abstracts), Usecases | CERO (no depende de nada) |
| **Application** | BLoCs, Events, States, Services | Domain |
| **Infrastructure** | HTTP (Retrofit), SQLite, Repository implementations | Domain + Application |
| **Presentation** | Pages, Widgets, Navigation (GoRouter) | Application + Domain |

### Reglas críticas Flutter

- ✅ `pubspec.yaml` con versiones locked
- ✅ `build_runner` ejecutado antes de commit (`dart run build_runner build`)
- ✅ Generadores: `freezed`, `json_serializable`, `injectable`, `retrofit`
- ✅ BLoCs con eventos inmutables (`@freezed sealed class`)
- ✅ SQLite con `sqflite_async` (cross-platform encryption)
- ✅ Seguridad: `flutter_secure_storage` para tokens/API keys
- ✅ Testing: Unit (Domain/Application ≥75%), Widget, Integration
- ❌ Direct HTTP calls en BLoCs (usar Repository)
- ❌ SQLite queries en UI layer
- ❌ StatefulWidget para lógica de negocio (usar BLoC)
- ❌ Modificar archivos `.g.dart` o `.freezed.dart` (son generados)

### Naming Convention

```
Entities:        MuestraEntity, ResultadoEntity
Repositories:    MuestraRepository (abstract), MuestraRepositoryImpl (concrete)
Usecases:        GetMuestrasUsecase, CreateMuestraUsecase
BLoCs:           MuestrasBloc, MuestraDetailBloc
Events:          MuestrasLoadRequested, MuestrasFilterChanged
States:          MuestrasLoading, MuestrasLoaded, MuestrasError
Models:          MuestraModel, MuestraResponse
Datasources:     MuestraLocalDatasource, MuestraRemoteDatasource
Pages:           MuestrasPage, MuestraDetailPage
Widgets:         MuestraCard, MuestraListItem
Services:        AuthService, PreferencesService, ConnectivityService
```

---

## Convenciones Backend Web (Hexagonal Architecture)

**Stack**: .NET 10 + C# 13 + ASP.NET Core 10 + Entity Framework Core 10 + MediatR 12

### Capas (NO hay cruces directos)

| Capa | Responsabilidad | Importa desde |
|------|-----------------|----------------|
| **Domain** | Entidades, interfaces, excepciones | NADA |
| **Application** | Commands, Queries, DTOs, Servicios (MediatR) | Domain |
| **Infrastructure** | Repositories, Context, Mappers, Adaptadores | Domain + Application |
| **Presentation** | Controllers, Middleware, DI | Todas las anteriores |

### Reglas críticas Backend

- ✅ Domain define interfaces (puertos), Infrastructure implementa (adaptadores)
- ✅ MediatR para Command/Query dispatch
- ✅ Mappers (AutoMapper) para Entity ↔ DTO
- ✅ Migraciones vía scripts SQL versionados en `database/` (Script-Only — NO migraciones de EF Core, ver guardrails-10-database-modeling.md)
- ✅ Queries con `AsNoTracking()`, paginadas, con `Include()` (no N+1)
- ✅ Parámetros en SQL (prevenir inyección)
- ✅ Global Exception Handler Middleware
- ✅ Autenticación delegada al IdP (Microsoft Entra ID): el backend valida el token emitido externamente (firma, expiración, audience) en cada endpoint — no emite ni renueva tokens de usuario propios (ver guardrails-03-web-backend.md, G-WEB-BE-04)
- ✅ Autorización: validar token, usuario, rol/permiso en cada endpoint
- ✅ Logging estructurado con correlation ID
- ❌ Presentation importa Infrastructure directamente
- ❌ Domain importa Application/Infrastructure
- ❌ SELECT * sin paginación
- ❌ Credenciales en code (usar Key Vault)

### Naming Convention

```
Entities:        Order, Customer, Muestra
Commands:        CreateOrderCommand, UpdateOrderCommandHandler
Queries:         GetOrderByIdQuery, GetOrdersQuery
DTOs:            OrderCreateDto, OrderResponseDto
Repositories:    IOrderRepository, OrderRepository
Services:        OrderApplicationService, INotificationService
Exceptions:      OrderNotFoundException, InvalidMuestraException
```

---

## Convenciones Integración (Hexagonal Mínima + MassTransit)

**Stack**: .NET 10 + MassTransit 8 + Azure Service Bus + Refit 16 + Polly 8 + Redis

### Capas (igual a Backend Web, pero más pequeño)

| Capa | Responsabilidad | Conexión |
|------|-----------------|----------|
| **Domain** | Entidades de sync, interfaces de puertos | - |
| **Application** | Casos de uso (sync, eventos) | Domain |
| **Infrastructure** | MassTransit consumers, Refit clients, Redis cache | Domain + Application |
| **Presentation** | Sync API + Health checks | Todas |

### Responsabilidades de Integración

- ✅ Escuchar eventos de Domain (Service Bus)
- ✅ Orquestar workflows multi-paso (sagas)
- ✅ Llamar APIs externas (Refit + Polly)
- ✅ Cachear datos (Redis)
- ✅ Persistir sync logs en shared DB
- ✅ Background workers para tareas asincrónicas
- ✅ Health checks para dependencias (DB, Bus, APIs)
- ❌ Lógica de negocio de Web o Mobile
- ❌ Cachés sin invalidación
- ❌ Hardcode de secretos/endpoints

### Naming Convention

```
DomainEvents:    MuestraCreatedEvent, ResultadoSyncedEvent
Commands:        SyncMuestraCommand, PublishResultadoCommand
Consumers:       MuestraCreatedConsumer, ResultadoSyncedConsumer
Repositories:    SyncRepository, IntegrationLogRepository
HttpClients:     IExternalApiClient (Refit interface)
Workers:         MuestraSyncWorker, ResultadoSyncWorker
CacheManagers:   RedisCacheManager
```

## Reglas de validacion rapida

### Checklist General
- ✓ Proyecto objetivo con `src/apps/`
- ✓ `docs/metricas/<project_name>_metricas.csv` presente
- ✓ `docs/metricas/guardrail_activity_log.csv` presente

### Checklist Web (Frontend + Backend)
- ✓ `src/apps/web/frontend/` con estructura Angular
- ✓ `src/apps/web/backend/Domain/` (Hexagonal layer)
- ✓ `src/apps/web/backend/Application/` (Commands/Queries)
- ✓ `src/apps/web/backend/Infrastructure/` (Repositories)
- ✓ `src/apps/web/backend/Presentation/Controllers/`
- ✓ `src/apps/web/database/Migrations/` (scripts SQL versionados — Script-Only, ver guardrails-10-database-modeling.md)
- ✓ NO importaciones cruzadas entre capas

### Checklist Mobile (Flutter SOLO frontend)
- ✓ `src/apps/mobile/flutter/lib/domain/` (entities + repositories abstract)
- ✓ `src/apps/mobile/flutter/lib/application/` (blocs @freezed)
- ✓ `src/apps/mobile/flutter/lib/infrastructure/` (datasources + models)
- ✓ `src/apps/mobile/flutter/lib/presentation/` (pages + widgets)
- ✓ `src/apps/mobile/flutter/test/` con ≥75% cobertura domain+application
- ✓ `pubspec.yaml` con build_runner, freezed, injectable, retrofit (versiones locked)
- ✓ `.gitignore` incluye `*.g.dart`, `*.freezed.dart`, `.dart_tool`
- ✓ NO backend en mobile (backend es en Web)

### Checklist Integración (Aplicación separada)
- ✓ `src/apps/integracion/Domain/` (entidades + eventos)
- ✓ `src/apps/integracion/Application/` (casos de uso)
- ✓ `src/apps/integracion/Infrastructure/MassTransitConsumers/` (event handlers)
- ✓ `src/apps/integracion/Infrastructure/HttpClients/` (Refit + Polly)
- ✓ `src/apps/integracion/Infrastructure/HostedServices/` (background workers)
- ✓ `src/apps/integracion/Infrastructure/CacheManagers/` (Redis)
- ✓ `src/apps/integracion/Presentation/HealthChecks/` (service bus, DB, APIs)
- ✓ Usa shared DB (misma SQL Server que Web)
- ✓ Publica/suscribe eventos por Service Bus (no llamadas directas)

### Checklist Terraform (IaC) — ⏸️ diferido, no bloqueante por ahora (ver G-CICD-01)
- ✓ `infrastructure/terraform/web/` (backend web + DB)
- ✓ `infrastructure/terraform/integracion/` (app separada + service bus + cache)
- ✓ NO hardcode de secretos (usar Key Vault)
- ✓ Managed Identity para autenticación

Si falla cualquier punto de las secciones anteriores (no-Terraform), BLOQUEAR implementación y pedir ajustes. La sección Terraform no bloquea mientras esté diferida.
