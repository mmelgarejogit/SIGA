# CLAUDE.md

## Proyecto
SIGA es un backend desarrollado en ASP.NET Core para gestionar autenticación, usuarios, roles, pacientes y profesionales de un sistema de gestión para óptica.

## Objetivo del repositorio
Este repositorio contiene la API backend. Su responsabilidad principal es exponer endpoints REST, aplicar reglas de negocio, persistir datos y gestionar autenticación/autorización.

## Stack técnico
- ASP.NET Core Web API (.NET 10)
- C#
- Entity Framework Core
- PostgreSQL (Npgsql)
- JWT Authentication
- Swagger

## Arquitectura
El proyecto está dividido en capas:
- **SIGA.Api** — Controllers, Program.cs
- **SIGA.Application** — Interfaces, DTOs, Result<T>
- **SIGA.Infrastructure** — Services, DbContext, Configurations, Migrations
- **SIGA.Domain** — Entities

## Modelo de dominio

```
Person (Id, DNI único, FirstName, LastName, BirthDate, PhoneNumber, Email único, CreatedAt, UpdatedAt)
  └── User (Id, PersonId FK 1:1, PasswordHash, IsActive, CreatedAt, UpdatedAt)
              ├── UserRole (UserId, RoleId) → Role (Id, Name)
              ├── Professional (Id, UserId FK 1:1, Specialty, LicenseNumber, CreatedAt, UpdatedAt)
              └── Patient      (Id, UserId FK 1:1, CreatedAt, UpdatedAt)
```

- `Person` centraliza los datos personales: documento, nombre, email, teléfono, fecha de nacimiento.
- `User` es la entidad de autenticación. El email para login vive en `Person`.
- `Professional` y `Patient` siempre tienen un `User` asociado (necesitan login).
- Roles definidos: `Admin`, `Professional`, `Patient`.
- Borrado lógico vía `User.IsActive = false`.

## Convenciones
- Controladores livianos — solo llaman al servicio y devuelven `ToHttpResponse(result)`
- Lógica de negocio en servicios de Infrastructure
- DTOs para requests y responses (nunca exponer entidades de dominio)
- Métodos async/await para operaciones I/O
- No acceder al DbContext desde controllers
- Separación clara entre capas: Api → Application (interfaces/DTOs) ← Infrastructure

## Autenticación
- Login mediante JWT: `POST /api/auth/login`
- El email para buscar el usuario se resuelve via `User.Person.Email` (join)
- `JwtTokenGenerator.GenerateToken(user, roles)` requiere que `user.Person` esté cargado
- La respuesta exitosa devuelve: `email`, `jwtToken`, `roleClaims`

## API Contract Standards

### Paginación uniforme

Todos los endpoints de **listado** deben soportar los siguientes query parameters:

| Parámetro | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `page` | int | `1` | Número de página (1-based) |
| `pageSize` | int | `20` | Cantidad de ítems por página (max `100`) |
| `search` | string | `null` | Búsqueda libre (case-insensitive, partial match) |
| `sortBy` | string | `createdAt` | Campo por el cual ordenar |
| `sortOrder` | string | `desc` | `asc` o `desc` |
| `isActive` | bool? | `null` | Filtro de borrado lógico (`true` / `false` / omitir para todos) |

### Response shape de listados

```json
{
  "items": [ /* DTOs */ ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8
}
```

> **Regla:** nunca devolver entidades de dominio en `items`. Usar DTOs con los campos necesarios para la UI.

### Búsqueda (`search`)

- Case-insensitive (`EF.Functions.ILike` en PostgreSQL).
- Partial match (envuelto en `%` por el backend).
- Campos buscables documentados por entidad:
  - **Patients**: `Person.FirstName`, `Person.LastName`, `Person.Email`, `Person.DNI`
  - **Professionals**: `Person.FirstName`, `Person.LastName`, `Person.Email`, `Specialty`
  - **Users**: `Person.FirstName`, `Person.LastName`, `Person.Email`
  - **Turnos**: `Patient.Person.FirstName`, `Patient.Person.LastName`, `Professional.Person.FirstName`
  - **Consultas**: `Patient.Person.FirstName`, `Patient.Person.LastName`

### Ordenamiento (`sortBy` / `sortOrder`)

- `sortBy` debe coincidir con una propiedad del DTO de respuesta o del modelo subyacente.
- Default: `createdAt desc` (más recientes primero).
- Si el campo no existe, fallback a `createdAt desc`.

### DTO Naming

| Operación | Nombre del DTO |
|-----------|----------------|
| Create Request | `Create{Entity}Request` |
| Update Request | `Update{Entity}Request` |
| List Response | `{Entity}Dto` o `{Entity}ListDto` |
| Detail Response | `{Entity}DetailDto` |

### Formato de errores

```json
{
  "message": "Descripción legible del error",
  "error": "Código opcional para debugging"
}
```

- `400 Bad Request` — validación de input fallida
- `401 Unauthorized` — token inválido o ausente
- `403 Forbidden` — autenticado pero sin permiso para la acción
- `404 Not Found` — recurso no existe
- `409 Conflict` — conflicto de negocio (ej: email duplicado)
- `500 Internal Server Error` — error inesperado del servidor

## Endpoints implementados

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | /api/auth/register | Registro genérico (crea Person + User sin rol) |
| POST | /api/auth/login | Login JWT |
| GET | /api/professionals | Listar profesionales |
| GET | /api/professionals/{id} | Obtener profesional por id |
| POST | /api/professionals | Crear profesional (Person + User + Professional + rol) |
| PUT | /api/professionals/{id} | Actualizar profesional |
| DELETE | /api/professionals/{id} | Desactivar profesional (soft delete) |
| GET | /api/patients | Listar pacientes |
| GET | /api/patients/{id} | Obtener paciente por id |
| POST | /api/patients | Crear paciente (Person + User + Patient + rol) |
| PUT | /api/patients/{id} | Actualizar paciente |
| DELETE | /api/patients/{id} | Desactivar paciente (soft delete) |

## Dependency Injection
Todos los servicios se registran en `SIGA.Infrastructure.DependencyInjection.AddInfrastructure()`.
No registrar servicios directamente en `Program.cs`.

## Instrucciones para el asistente
- Respetar la arquitectura existente
- No proponer refactors grandes sin necesidad
- Indicar siempre qué archivos modificar
- Priorizar soluciones limpias y mantenibles
- Reutilizar estructuras existentes antes de crear nuevas
- No inventar componentes o capas que no existan sin justificación

## Evitar
- No poner lógica de negocio en controladores
- No duplicar servicios ya existentes
- No cambiar nombres de carpetas o namespaces sin necesidad
- No proponer soluciones incompatibles con JWT actual
- No acceder a `User.Email` directamente — el email vive en `User.Person.Email`
