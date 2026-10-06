# GUARDRAILS GLOBALES - Aplica a TODO el equipo

**Aplicable a**: WEB + INTEGRATION + MOBILE  
**Última actualización**: 2026-07-16

---

## G-GLOBAL-01: Naming Conventions

### Paquetes / Namespaces
```
Angular:     feature-based routing (app/home/components)
.NET:        Company.Project.Layer.Feature (Acme.CPos.Application.Orders)
Flutter:     com.company.app (com.miclabs.cpos)
```

### Files & Folders
- kebab-case: `product-list.component.ts`, `order-detail.page.dart`
- PascalCase: class names only
- CONSTANT_CASE: enums, constants
- _private.dart: private/internal files with underscore prefix

---

## G-GLOBAL-02: Version Control & Commits

### Branching Strategy (GitFlow simplificado — sin rama `release/*`)

```
main (production) ← tags: v1.0.0, v1.1.0
  │
  ├── develop (integration)
  │    ├── feature/nombre-corto-descriptivo    (nace de develop, vuelve a develop)
  │    └── bugfix/nombre-corto-descriptivo     (nace de develop, vuelve a develop)
  │
  └── hotfix/nombre-corto-descriptivo          (nace de main, vuelve a main Y a develop)
```

- ✅ `feature/*` y `bugfix/*`: se ramifican desde `develop`, se mergean de vuelta a `develop`
- ✅ `hotfix/*`: se ramifica desde `main` (nunca desde `develop` — producción puede estar adelante de lo que hay en desarrollo), y se mergea a **ambos** `main` y `develop` para que el fix no se pierda en el siguiente release
- ✅ Nombre de rama: `{feature|bugfix|hotfix}/<descripcion-corta-en-kebab-case>` — no se asume ningún issue tracker específico (Jira, GitHub Issues, etc.); si el equipo adopta uno, se puede prefijar el identificador del ticket al nombre
- ❌ No existe rama `release/*` en esta variante — es un GitFlow simplificado a propósito, no un descuido. Si el equipo necesita estabilizar releases antes de `main`, esa es la primera pieza a agregar

### Commit Messages (Conventional Commits)
```
feat: add user authentication
fix: resolve payment rounding error
docs: update API documentation
refactor: simplify order repository
test: add integration tests for checkout
chore: update dependencies
```

### PR Requirements
- At least 1 code review approval
- All tests passing
- No secrets in commits (use .env, Key Vault, Secure Storage)
- Squash merge to develop, merge commit to main

---

## G-GLOBAL-03: Security (Non-Negotiable)

### Secrets Management
- ❌ NEVER hardcode API keys, tokens, passwords
- ✅ Use: Azure Key Vault (.NET), flutter_secure_storage (Flutter)
- ✅ Environment variables: dev/staging/prod separated
- ✅ Rotate credentials: every 90 days minimum

### Data Validation
- ✅ Validate ALL inputs (frontend + backend)
- ✅ Sanitize: prevent XSS, SQL injection, command injection
- ✅ Parameterized queries: ALWAYS in database
- ✅ CORS: configure per environment

### Authentication & Authorization
- ✅ JWT tokens (short-lived access + long-lived refresh) son los emitidos por el IdP de identidad delegada (Microsoft Entra ID) — no tokens propios de la aplicación (ver guardrails-03-web-backend.md, G-WEB-BE-04)
- ✅ RBAC: role-based access control
- ✅ OAuth: verify server-side, never trust frontend
- ✅ Multi-tenant: validate tenant in every operation (solo aplica si el proyecto es multi-tenant — ver G-DB-01 en guardrails-10-database-modeling.md)

---

## G-GLOBAL-04: Dependency Management

### Prohibited Direct Dependencies
- ❌ No circular dependencies between layers
- ❌ Presentation NEVER imports Infrastructure directly
- ❌ Domain NEVER imports Application/Infrastructure/Presentation
- ✅ Use dependency injection containers

### Version Pinning
- ✅ Lock major versions in pubspec.yaml (Flutter)
- ✅ Lock patch versions in package-lock.json (Angular)
- ✅ Use SemVer: `^1.2.3` for compatible versions
- ✅ Review dependency updates monthly

---

## G-GLOBAL-05: Code Review Checklist

### Architecture
- [ ] Layers correctly isolated (no cross-layer imports)
- [ ] Dependencies point inward (unidirectional)
- [ ] No domain logic in presentation/infrastructure

### Code Quality
- [ ] Unit tests: 70%+ coverage (frontend), 75%+ (backend)
- [ ] No console.log / print statements in production code
- [ ] No TODO/FIXME comments without tickets
- [ ] Naming follows conventions

### Security
- [ ] No hardcoded secrets
- [ ] Input validation present
- [ ] Error messages don't leak sensitive info
- [ ] HTTPS enforced
- [ ] CORS properly configured

---

## G-GLOBAL-06: Metrics & Observability

### Mandatory Metrics (ALL projects)
Every project MUST track the following metrics from day one, stored in `docs/metricas/<project_name>_metricas.csv`:

| # | Metric | Target | Azure Source | Collection |
|---|--------|--------|-------------|------------|
| 1 | API Response Time (P95) | < 500ms | Application Insights | Automated |
| 2 | API Error Rate | < 1% | Application Insights | Automated |
| 3 | Availability (Uptime) | ≥ 99.9% | Azure Monitor | Automated |
| 4 | Test Coverage | ≥ 75% backend, ≥ 70% frontend | CI/CD Pipeline | Per build |
| 5 | DB Query Performance (P95) | < 100ms | Azure SQL Insights | Automated |
| 6 | Service Bus Message Latency | < 5s end-to-end | Service Bus Metrics | Automated |
| 7 | Cache Hit Ratio (Redis) | ≥ 80% | Azure Cache for Redis | Automated |
| 8 | Deployment Frequency | ≥ 1x / week | GitHub Actions / Azure DevOps | Automated |
| 9 | Mean Time to Resolve (MTTR) | < 4 hours | Azure Monitor + Alerts | Tracked per incident |
| 10 | Active Users (DAU/MAU) | Per SLA | App Insights + DB | Automated |

### Metric Collection Rules
- ✅ ALL metrics populated from real Azure Monitor / App Insights data (no manual estimates)
- ✅ CSV updated at least weekly with current values
- ✅ Alert rules configured in Azure Monitor for metrics 1, 2, 3, 5, 6
- ✅ Dashboard created in Azure Portal (Metrics + Workbooks)
- ✅ Correlation ID tracked across ALL services (sync + async)
- ❌ Manual estimates or "TBD" values after first month of production
- ❌ Metrics without alert thresholds
- ❌ Missing correlation IDs in logs

### CSV Format (mandatory)
```csv
Metric,Target,Current,Date,Notes
API Response Time (P95),500ms,120ms,2026-07-16,Within target
API Error Rate,1%,0.5%,2026-07-16,Within target
Availability,99.9%,99.95%,2026-07-16,Within target
Test Coverage,75%,68%,2026-07-16,Below target - add tests
DB Query Performance (P95),100ms,45ms,2026-07-16,Within target
Service Bus Message Latency,5s,1.2s,2026-07-16,Within target
Cache Hit Ratio,80%,92%,2026-07-16,Within target
Deployment Frequency,1x/week,3x,2026-07-16,Exceeding target
MTTR,4h,2.5h,2026-07-16,Within target
Active Users (DAU),Per SLA,1250,2026-07-16,Within target
```

### Alert Thresholds
| Metric | Warning | Critical | Action |
|--------|---------|----------|--------|
| API Response Time | > 500ms P95 | > 2s P95 | Investigate query perf |
| API Error Rate | > 1% | > 5% | Rollback / hotfix |
| Availability | < 99.9% | < 99.5% | Incident response |
| DB Query Performance | > 100ms | > 500ms | Optimize indexes |
| Service Bus Latency | > 5s | > 30s | Check consumer health |

### Observability Stack (Azure)
- ✅ Application Insights: APM, distributed tracing, dependency mapping
- ✅ Log Analytics Workspace: centralized log storage, KQL queries
- ✅ Azure Monitor: Metrics, Alerts, Dashboards, Workbooks
- ✅ Serilog: structured logging with correlation ID enrichment
- ✅ Health checks: `/health` and `/health/live` on every service

---

## G-GLOBAL-07: Guardrail Activity Log

Every implementation task SHALL be logged in `docs/metricas/guardrail_activity_log.csv` to track guardrail compliance, tool effectiveness, and time savings.

### Log Format (mandatory)
```csv
fecha,proyecto,template_id,rol,actividad,codigo_guardrail,herramienta,tiempo_estimado_min,tiempo_real_min,ahorro_min,resultado,observaciones
2026-07-16,demo,web,Developer Backend,Implementar endpoint de casos,G-WEB-BE-03,Copilot,120,75,45,OK,Sin bloqueadores
```

### Field Rules

| Field | Formato | Obligatorio | Descripción |
|-------|---------|-------------|-------------|
| `fecha` | YYYY-MM-DD | ✅ | Día de la actividad |
| `proyecto` | kebab-case | ✅ | Nombre del proyecto |
| `template_id` | stack id de `stacks/catalog.yaml` (`web`, `mobile`, `integracion`, `all`) | ✅ | Stack usado |
| `rol` | Developer Backend / Frontend / Integration / Mobile | ✅ | Rol del ejecutor |
| `actividad` | texto descriptivo corto | ✅ | Qué se implementó |
| `codigo_guardrail` | G-{AREA}-{NN} | ✅ | Guardrail aplicado (debe existir en guardrails-*.md) |
| `herramienta` | Copilot / bit CLI / manual | ✅ | Herramienta usada |
| `tiempo_estimado_min` | entero | ✅ | Minutos estimados sin herramientas |
| `tiempo_real_min` | entero | ✅ | Minutos reales con herramientas |
| `ahorro_min` | entero (estimado - real) | ✅ | Diferencia (puede ser negativo si tomó más) |
| `resultado` | OK / FAIL / BLOCKED | ✅ | Si el guardrail se aplicó correctamente |
| `observaciones` | texto libre | ❌ | Bloqueadores, notas, contexto |

### Logging Rules

- ✅ Every implemented task SHALL have at least one log entry
- ✅ Each log entry references exactly one `codigo_guardrail`
- ✅ `ahorro_min` SHALL be calculated as `tiempo_estimado_min - tiempo_real_min`
- ✅ Log appended to CSV (never overwrite — historical record)
- ✅ `resultado = FAIL` if guardrail rule was violated and had to be corrected
- ✅ `resultado = BLOCKED` if guardrail exposed a missing dependency or blocker
- ❌ Log entries without `codigo_guardrail` (each entry must tie to a specific rule)
- ❌ Deleting or modifying historical entries (append-only)
- ❌ `ahorro_min` negativo sin observación explicando por qué tomó más tiempo

### Aggregate Metrics (feeds G-GLOBAL-06)

From the activity log, the following SHALL be computed weekly:

| Aggregate Metric | Calculation | Target |
|-----------------|-------------|--------|
| Guardrail Compliance Rate | `count(resultado=OK) / total` | ≥ 90% |
| Avg Time Savings per Task | `avg(ahorro_min)` | ≥ 20 min |
| Total Time Saved (week) | `sum(ahorro_min)` | ≥ 200 min |
| Most Applied Guardrail | `mode(codigo_guardrail)` | — |
| Tool Usage Distribution | `count(herramienta) / total` | — |
| Blockers Detected | `count(resultado=BLOCKED)` | ≤ 2/week |

---

## G-GLOBAL-08: Desarrollo local emulado y paridad con Azure

**Contexto — decisión del owner (2026-09-23):** el desarrollador **no tiene acceso a recursos de Azure**. El desarrollo es on-premise y las dependencias cloud se sustituyen con emuladores locales (SQL Server en contenedor, Azurite, Redis, emulador de identidad, captura de correo). No es transitorio: es el modelo de trabajo.

Consecuencia directa que hay que aceptar explícitamente: **el camino de autenticación que corre en producción (Managed Identity) no se puede ejercitar en la máquina del desarrollador.** Los emuladores autentican con cadena de conexión o clave compartida, no con Entra ID. Escribir la regla como "usá la misma credencial en los dos lados" sería una regla incumplible.

La regla no es eliminar la diferencia — es **aislarla en un solo punto y compensarla con validación obligatoria antes del release**.

### Regla 1 — La diferencia vive en los archivos de configuración, no en el código

Lo único que cambia entre la máquina del desarrollador y los ambientes cloud son los **archivos de configuración por ambiente** (`appsettings.{Environment}.json`). El código compilado es idéntico.

- ✅ Los clientes de recursos externos (`BlobServiceClient`, `ServiceBusClient`, `SecretClient`, `DbContext`) se construyen **una sola vez, en el composition root** (`Presentation/Program.cs`).
- ✅ `Application` e `Infrastructure` reciben el cliente ya inyectado y son **idénticos** en local y en Azure.
- ✅ El interruptor es un valor **explícito** de configuración (`Cloud:Provider`), validado al arrancar.
- ❌ Prohibido `if (env.IsDevelopment())` fuera del composition root. Un `IHostEnvironment` decidiendo credenciales dentro de un repositorio, un handler o un consumer es violación.
- ❌ Prohibido inferir el ambiente por **ausencia** de un valor (“si no hay cadena de conexión, entonces es Azure”). Un valor que se filtre a un archivo cloud haría que la aplicación use el emulador en Azure **en silencio**, sin error.

```csharp
// appsettings.Local.json       → "Cloud": { "Provider": "Emulated" }
// appsettings.Production.json  → "Cloud": { "Provider": "Azure" }

var provider = config["Cloud:Provider"]
    ?? throw new InvalidOperationException("Falta 'Cloud:Provider' en la configuración.");

// ✅ CORRECTO — un único punto de decisión, explícito y que falla al arrancar
services.AddSingleton(provider switch
{
    "Emulated" => new BlobServiceClient(config.GetConnectionString("Blob")),
    "Azure"    => new BlobServiceClient(new Uri(config["Blob:Uri"]!), new DefaultAzureCredential()),
    _ => throw new InvalidOperationException($"'Cloud:Provider' no soportado: '{provider}'.")
});

// ❌ PROHIBIDO — decisión repartida por el código y atada al ambiente
if (env.IsDevelopment()) { /* ... */ } else { /* ... */ }
```

#### Qué va en cada archivo

| Archivo | Contenido | ¿Versionado? | ¿Puede contener secretos? |
|---|---|---|---|
| `appsettings.json` | Valores comunes a todos los ambientes | Sí | No |
| `appsettings.Local.json` | `Provider: Emulated` + endpoints y credenciales **de emulador** | Sí | No — solo credenciales públicas de emulador (Regla 2) |
| `appsettings.{Dev,QA,Prod}.json` | `Provider: Azure` + URIs, nombres de recursos y toggles | Sí | **No** — los secretos se resuelven en runtime desde Key Vault |
| `dotnet user-secrets` | Valores locales que no deben versionarse | No (fuera del repo) | Solo local |
| Key Vault | Secretos reales de cada ambiente Azure | N/A | Sí — es su único lugar |

El archivo de un ambiente cloud **se versiona igual que los demás** y por eso no puede llevar secretos: declara *qué* recurso usar (URI, nombre), nunca *con qué credencial* — eso lo resuelve Managed Identity contra Key Vault.

### Regla 2 — Las credenciales de emulador no son secretos

Las credenciales fijas y públicas de los emuladores (`UseDevelopmentStorage=true` de Azurite, la cadena del emulador de Service Bus, el usuario/contraseña del SQL Server local) **no** caen bajo la prohibición de `G-GLOBAL-03`: son valores locales, públicos y sin acceso a nada real.

- ✅ Pueden vivir versionados en un archivo claramente identificado como local (`appsettings.Local.json`, `.env.example`).
- ❌ Ese archivo no puede contener **ningún** valor que sirva contra un recurso real, ni siquiera de un ambiente de desarrollo en Azure.
- ❌ Una contraseña "local" reutilizada en cualquier ambiente compartido deja de ser local y vuelve a caer bajo `G-GLOBAL-03`.

### Regla 3 — Inventario explícito de divergencias

Cada proyecto mantiene en su documentación la tabla de qué difiere entre local y Azure. Sin esta tabla, nadie sabe dónde está el riesgo acumulado:

| Recurso | Local (emulado) | Azure | Camino que NO se ejercita en local |
|---|---|---|---|
| Base de datos | SQL Server contenedor, auth SQL | Azure SQL, `Authentication=Active Directory Default` | Managed Identity + RBAC de la base |
| Blob Storage | Azurite, clave de desarrollo | Blob Storage, Managed Identity | Managed Identity + RBAC del storage |
| Mensajería | Emulador o harness in-memory de MassTransit (`G-TEST-06`) | Service Bus, Managed Identity | Managed Identity, particiones, dead-letter real |
| Secretos | `dotnet user-secrets` | Key Vault | Acceso a Key Vault, rotación |
| Identidad | Emulador de Entra | Entra ID | Claims, roles y consentimiento reales |
| Correo | Captura SMTP local | Azure Communication Services | El SDK de ACS, si el código no envía por SMTP |

**Nota sobre la primera fila — por qué la divergencia de base de datos no se puede cerrar.** SQL Server 2022 sí soporta autenticación con Microsoft Entra ID, en Windows y Linux on-premises, por dos caminos: registrando la instancia en **Azure Arc**, o —únicamente en Windows— configurando a mano certificado, `adal.dll` y claves del registro. Ninguno sirve en este contexto:

- **Los dos exigen conectividad saliente a Azure** (`login.microsoftonline.com`, `graph.microsoft.com`, `database.windows.net`, entre otros) y un tenant de Entra ID real con app registration. El desarrollador no tiene acceso a Azure — es la premisa de este guardrail.
- **El camino sin Arc es específico de Windows** (registro de Windows, `adal.dll` en `system32`, almacén de certificados de la máquina). No aplica al contenedor Linux de desarrollo.
- **Aun funcionando, no reproduciría el camino real**: autenticaría contra SQL Server, no contra Azure SQL con Managed Identity, que es lo que corre en producción.

Esta divergencia es **permanente y aceptada**. Se compensa con la Regla 4; no se intenta cerrar.

Fuentes: [Microsoft Entra authentication for SQL Server overview](https://learn.microsoft.com/en-us/sql/relational-databases/security/authentication-access/azure-ad-authentication-sql-server-overview) · [Enable Microsoft Entra Authentication for SQL Server on Windows Without Azure Arc](https://learn.microsoft.com/en-us/sql/relational-databases/security/authentication-access/microsoft-entra-authentication-sql-server-enable-without-arc)

### Regla 4 — Validación en Azure antes del release (control compensatorio)

Como el camino de producción **nunca** corre en la máquina del desarrollador, no puede ser producción la primera vez que corre:

- ✅ El pipeline despliega a un ambiente Azure de desarrollo/test y ejecuta ahí un smoke test que ejercita, con Managed Identity real, cada fila de la tabla de la Regla 3.
- ✅ El acceso a Azure es del **pipeline** (identidad federada/service principal), no de las personas — coherente con que el desarrollador no tenga credenciales.
- ❌ Prohibido promover a producción un cambio que toque el acceso a un recurso de Azure sin que ese smoke test haya corrido en Azure.
- La responsabilidad es del pipeline, no del desarrollador (ver `guardrails-09-devops-cicd.md`).

### Validación rápida

- [ ] Los clientes de recursos externos se construyen solo en el composition root
- [ ] Ningún `if (env.IsDevelopment())` decide credenciales fuera de ahí
- [ ] El interruptor local/Azure es un valor explícito (`Cloud:Provider`) leído de configuración, no de `IHostEnvironment`
- [ ] La aplicación falla al arrancar si `Cloud:Provider` falta o no es reconocido
- [ ] Ningún `appsettings.{Dev,QA,Prod}.json` versionado contiene secretos — solo URIs, nombres y toggles
- [ ] Existe y está actualizada la tabla de divergencias del proyecto (Regla 3)
- [ ] Ningún archivo versionado contiene credenciales que sirvan contra un recurso real
- [ ] El smoke test en Azure con Managed Identity corrió antes del release
