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
