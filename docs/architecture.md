# Arquitectura del Sistema — SIGA Óptica

> Reescrito completo el 2026-07-08. La versión anterior de este documento describía una versión muy temprana del sistema (~7 tablas: `persons`, `users`, `patients`, `professionals`, `appointments`, `clinical_records`, `sales`). El sistema actual tiene 35 controllers, 70 entidades/tablas reales (más ~22 enums) y 90 vistas de frontend — este documento describe el sistema real, verificado contra el código.

## Visión general

SIGA es un sistema de gestión integral para una óptica: identidad y acceso, pacientes/profesionales/clientes/empleados, agenda de turnos, historia clínica, catálogo e inventario (incl. catálogo óptico de armazones/tratamientos), ventas (venta directa y "a pedido" con receta + laboratorio externo), compras a proveedores, caja, egresos, notificaciones internas y reportes operativos — todo con soporte **multi-sucursal** y control de acceso basado en permisos.

Es un monorepo de dos repositorios separados:
- **Backend** (`SIGA`): ASP.NET Core Web API (.NET 10), C#, Entity Framework Core, PostgreSQL (hosteado en Neon).
- **Frontend** (`SIGA-Web`): Vue 3 SPA, TypeScript, Vite (ver `SIGA-Web/docs/frontend-architecture.md`, Fase 5 de este plan de documentación).

El modelo de datos gira en torno a la entidad `Person` (cualquier individuo físico del sistema — CI, nombre, contacto). A partir de ahí, roles funcionales (`User`, `Patient`, `Professional`, `Cliente`, `Empleado`) se cuelgan de `Person` según qué necesite hacer esa persona en el sistema. Ver `schema.md` para el detalle completo.

## Diagrama de componentes

```mermaid
graph TB
    subgraph Frontend["SIGA-Web (Vue 3 SPA)"]
        WEB[Vistas / Composables / Services]
    end

    subgraph Backend["SIGA (ASP.NET Core Web API .NET 10)"]
        API["SIGA.Api<br/>Controllers · Program.cs · JWT Auth · Policies de permisos · Swagger"]
        APP["SIGA.Application<br/>Interfaces de servicio · DTOs · Result&lt;T&gt;"]
        INFRA["SIGA.Infrastructure<br/>Services (impl.) · AppDbContext · EF Configurations · Migrations · BackgroundServices · Security"]
        DOM["SIGA.Domain<br/>Entities · abstracciones de Security (IPasswordHasher)"]
    end

    DB[(PostgreSQL — Neon)]
    RESEND[[Resend — envío de email]]
    HCAPTCHA[[hCaptcha — antibot]]

    WEB -- "HTTPS REST + JWT Bearer" --> API
    API --> APP
    API --> DOM
    INFRA -. implementa .-> APP
    INFRA --> DOM
    INFRA --> DB
    INFRA -- HttpClient "resend" --> RESEND
    INFRA -- HttpClient "hcaptcha" --> HCAPTCHA
```

## Capas del backend

### `SIGA.Domain`
Entidades de dominio puras (`Entities/`, 70 clases + ~22 enums, todas en un único namespace plano `SIGA.Domain.Entities`) y abstracciones de seguridad (`Security/IPasswordHasher`). Sin dependencias hacia afuera — es la capa más interna.

### `SIGA.Application`
Contratos que consume `SIGA.Api` e implementa `SIGA.Infrastructure`: interfaces de servicio (`Interfaces/IXxxService`), DTOs de request/response (`DTOs/`) y el wrapper `Common/Result<T>` para modelar éxito/error sin excepciones de control de flujo. No contiene lógica de negocio ni acceso a datos.

### `SIGA.Infrastructure`
La capa más grande — contiene toda la lógica de negocio y persistencia:
- **`Services/`** — implementación de cada `IXxxService` (≈40 servicios registrados, ver `DependencyInjection.AddInfrastructure`). Un servicio por módulo de negocio (`VentaService`, `LaboratorioService`, `EgresoService`, `TransferenciaStockService`, etc.), más 2 `BackgroundService` (`TurnoReminderService`, `StockBajoNotificadorService`, ver más abajo).
- **`Persistence/`** — `AppDbContext`, `Configurations/` (una clase `IEntityTypeConfiguration<T>` por entidad, Fluent API — fuente de verdad de constraints/índices/relaciones reales, más completa que las propiedades de la entidad sola) y `Migrations/` (EF Core, ~55 migraciones a la fecha).
- **`Security/`** — `Pbkdf2PasswordHasher`, `JwtTokenGenerator`, `CurrentUserContext` (implementa `ICurrentUserContext`: expone `UserId`/`SucursalId`/`EsGlobal`/`TienePermiso` vía `IHttpContextAccessor`, usado por los servicios transaccionales para filtrar/estampar por sucursal).
- **`Options/`** — POCOs de configuración (`ResendOptions`, `HCaptchaOptions`, `AppOptions`).

### `SIGA.Api`
Controllers (35, uno por recurso — livianos, delegan a los servicios) y `Program.cs`: pipeline HTTP, autenticación JWT, autorización basada en permisos, Swagger y arranque de datos (ver abajo).

## Pipeline HTTP y seguridad (`Program.cs`)

1. **Autenticación JWT Bearer** — `TokenValidationParameters` valida issuer/audience/lifetime/signing key (`Jwt:Issuer`/`Jwt:Audience`/`Jwt:Secret` en configuración).
2. **Autorización por permisos, no por rol** — el JWT lleva un claim `"permission"` por cada permiso concedido (no un claim de rol). `Program.cs` define ~45 policies (una por permiso: `ver_pacientes`, `crear_venta`, `gestionar_laboratorio`, etc.) que simplemente exigen `RequireClaim("permission", perm)`. Dos policies compuestas usan `RequireAssertion` para OR de permisos: `cancelar_pedido` (creador `gestionar_pedidos` O aprobador `aprobar_pedidos`) y `ver_disponibles` (staff `ver_agenda` O paciente autogestionando `ver_mis_turnos`).
3. **`app.UseAuthentication()` → `app.UseAuthorization()` → `app.MapControllers()`.**
4. **Arranque (dentro de un scope, antes de `app.Run()`):**
   - `db.Database.MigrateAsync()` — aplica migraciones pendientes automáticamente al arrancar (no hay paso manual de `dotnet ef database update` en el flujo normal).
   - `DbSeeder.SeedAsync(db)` — siembra catálogos base (roles, permisos, sucursal "Casa Central", etc.), idempotente.
   - `DbSeeder.SeedAdminAsync(...)` — bootstrap de un usuario admin en **todos** los entornos (incl. producción), credenciales configurables por `Seed:AdminEmail`/`Seed:AdminPassword`.
   - `DevDataSeeder.SeedAsync(...)` — solo en `Development`, datos de prueba adicionales.

## Autorización: permisos, no roles

El sistema es explícitamente **permission-based**: un `Role` es solo una colección con nombre de `Permission`s (tabla pivote `RolePermission`), y el JWT emite los permisos resueltos, no el nombre del rol. Los controllers protegen cada acción con `[Authorize(Policy = "permiso_especifico")]`. Esto permite crear roles ad-hoc desde `/roles` sin tocar código. `Role.Type` (`admin`/`professional`/`patient`, nullable) es la única noción de "rol de sistema" y se usa solo para lógica que necesita distinguir un rol inmutable (ej. bootstrap del admin), no para autorización de endpoints.

## Multi-sucursal

Desde 2026-06-27 (proyecto transversal, todas las fases 0–6 completas en código y **commiteadas y pusheadas** a `matias-gaona` — commits `b289615`/`6d4a72b`, confirmado 2026-07-08): casi todo lo transaccional (stock, ventas, caja, timbrado, compras/recepciones, egresos, turnos/agenda, consultas clínicas) está scopeado por `SucursalId`. Quedan **globales**: catálogos (productos, marcas, categorías, servicios, etc.) y personas (`Patient`, `Cliente`, `Professional`, `User` de rol admin). El scoping se resuelve centralmente en `ICurrentUserContext`, no repitiendo la lógica en cada servicio. Detalle completo en `schema.md` (grupo Comercial) y en [`modules/15-sucursales.md`](./modules/15-sucursales.md).

## Módulos de negocio → Controllers

| Módulo | Controllers |
|---|---|
| Identidad y Acceso | `AuthController`, `UsersController`, `RolesController` |
| Personas | `PatientsController`, `ProfessionalsController`, `ClientesController`, `EspecialidadesController` |
| Personal | `EmpleadosController` |
| Agenda | `TurnosController`, `HorariosController` |
| Clínica | `ConsultasController`, `RecetasController` |
| Catálogo / Inventario | `ProductosController`, `MarcasController`, `TipoLentesController`, `TratamientosController`, `ServiciosController`, `MotivosMovimientoController`, `StockLotesController`, `UbicacionesController` |
| Ventas | `VentasController`, `TimbradosController` |
| Laboratorio | `LaboratorioController` |
| Compras | `ComprasController`, `ProveedoresController`, `FacturasCompraController`, `RecepcionesController` |
| Caja y Egresos | `CajaController`, `EgresosController` |
| Configuración | `ConfiguracionController`, `EstadosConfigController` |
| Sucursales | `SucursalesController`, `TransferenciasController` |
| Notificaciones | `NotificacionesController` |
| Reportes | `ReportesController` |

El detalle de cada endpoint (verbo, ruta, policy, request/response) está en [`api-reference.md`](./api-reference.md).

**Nota:** algunos catálogos menores del dominio (`CargoEmpleado`, `CategoriaGasto`, `CategoriaProducto`, `Modelo`, `Ciudad`, `Departamento`) no tienen un controller propio visible en la lista de 35 — probablemente se gestionan como sub-recursos de otro controller (ej. bajo `ConfiguracionController` o `ProductosController`). Confirmar el mapeo exacto al escribir `api-reference.md`.

## Trabajos en segundo plano

Dos `BackgroundService` registrados en `DependencyInjection`:
- **`TurnoReminderService`** — recordatorios de turnos por email.
- **`StockBajoNotificadorService`** — poller cada 30 min que compara `vw_stock_actual` contra `ProductoStockConfig` por producto+sucursal y genera `NotificacionInterna` de bajo stock (evita duplicar si ya hay una sin leer). Se eligió poller sobre hook inline porque los movimientos de stock "Aprobado" se originan desde ≥5 servicios distintos.

## Generación de documentos y PDFs

**QuestPDF** (licencia Community, seteada en `Program.cs` antes del `builder`) para: receta óptica (`RecetaPdfGenerator`), movimientos de stock (`MovimientoStockPdfGenerator`) y reportes operativos (`ReporteExporter`, que también genera CSV con writer propio UTF-8+BOM). Los exportadores reciben el DTO, no el HTTP request, para poder reutilizarse desde un futuro job de email.

## Integraciones externas

- **PostgreSQL / Neon** — `Npgsql` con retry automático (`EnableRetryOnFailure`, 5 intentos). En desarrollo, `dotnet ef` corre en entorno `Production` por defecto y no toma la connection string de Neon — hay que forzar `ASPNETCORE_ENVIRONMENT=Development`.
- **Resend** — envío de email transaccional (verificación de cuenta, notificaciones externas de turnos). `HttpClient` nombrado `"resend"`.
- **hCaptcha** — protección del endpoint público de auto-registro de pacientes. `HttpClient` nombrado `"hcaptcha"`.

## Convenciones (de `CLAUDE.md`, vigentes)

- Controllers livianos: reciben el request, llaman al servicio, devuelven `ToHttpResponse(result)`. Sin lógica de negocio ni acceso a `DbContext`.
- DTOs nunca exponen entidades de dominio directamente.
- Paginación uniforme en listados (`page`/`pageSize`/`search`/`sortBy`/`sortOrder`/`isActive`) con response shape `{ items, totalCount, page, pageSize, totalPages }`.
- Todos los servicios se registran en `SIGA.Infrastructure.DependencyInjection.AddInfrastructure()` — nunca directamente en `Program.cs`.

## Frontend como cliente

`SIGA-Web` consume la API vía HTTP/JSON con el JWT en `Authorization: Bearer`. El token trae los claims `permission` (uno por permiso), `professional_id` (si aplica) y `sucursal_id` (si el usuario tiene sucursal fija) — el frontend los usa para mostrar/ocultar UI, pero la autorización real siempre se re-valida en el backend vía policies. Ver `SIGA-Web/docs/frontend-architecture.md` (pendiente, Fase 5).

---

**Relacionado:** `schema.md` (modelo de datos completo), `api-reference.md` (pendiente), `modules/*.md` (pendiente) — ver `README.md` para el índice completo.
