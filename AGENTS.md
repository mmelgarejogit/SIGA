# AGENTS.md — SIGA Backend

## Stack

ASP.NET Core Web API (.NET 10), C#, Entity Framework Core, PostgreSQL (Npgsql), JWT Authentication, Swagger.

## Arquitectura en capas

```
SIGA.Api          → Controllers, Program.cs
SIGA.Application  → Interfaces, DTOs, Result<T>
SIGA.Infrastructure → Services, DbContext, Configurations, Migrations
SIGA.Domain        → Entities
```

- **Controladores livianos**: solo llaman al servicio y devuelven `ToHttpResponse(result)`. No lógica de negocio en controllers.
- **DTOs siempre**: nunca exponer entidades de dominio en responses.
- **Servicios en Infrastructure**: la lógica de negocio vive ahí, no en controllers ni en Application.
- **DI**: registrar servicios en `SIGA.Infrastructure.DependencyInjection.AddInfrastructure()`. No registrar directamente en `Program.cs`.

## Modelo de dominio

```
Person (Id, DNI único, FirstName, LastName, BirthDate, PhoneNumber, Email único, CreatedAt, UpdatedAt)
  └── User (Id, PersonId FK 1:1, PasswordHash, IsActive, CreatedAt, UpdatedAt)
              ├── UserRole (UserId, RoleId) → Role (Id, Name)
              ├── Professional (Id, UserId FK 1:1, Specialty, LicenseNumber, CreatedAt, UpdatedAt)
              └── Patient      (Id, UserId FK 1:1, CreatedAt, UpdatedAt)
```

- `Person` centraliza datos personales: documento, nombre, email, teléfono, fecha de nacimiento.
- `User` es la entidad de autenticación. El email para login vive en `Person`.
- `Professional` y `Patient` siempre tienen un `User` asociado (necesitan login).
- **Borrado lógico** vía `User.IsActive = false`. No usar `DELETE` SQL.

## Autenticación

- Login: `POST /api/auth/login` → JWT
- El email para buscar el usuario se resuelve via `User.Person.Email` (join).
- `JwtTokenGenerator.GenerateToken(user, roles)` requiere que `user.Person` esté cargado.
- La respuesta devuelve: `email`, `jwtToken`, `roleClaims`.

## Permisos

- **RBAC por permisos granulares** (ej: `crear_paciente`, `ver_calendario`, `registrar_venta`). Los roles son contenedores; los permisos son el chequeo real.
- No acceder a `User.Email` directamente — el email vive en `User.Person.Email`.

## API Contract Standards

Ver `CLAUDE.md` → *API Contract Standards* para especificación completa de:
- Paginación uniforme (`page`, `pageSize`, `search`, `sortBy`, `sortOrder`, `isActive`)
- Response shape de listados (`items`, `totalCount`, `page`, `pageSize`, `totalPages`)
- Búsqueda (`EF.Functions.ILike`, partial match, campos buscables por entidad)
- Ordenamiento y fallback
- DTO naming (`Create{Entity}Request`, `Update{Entity}Request`, `{Entity}Dto`)
- Formato de errores y status codes

## Comandos

```sh
# Migraciones
dotnet ef migrations add <Nombre> --project src/SIGA.Infrastructure --startup-project src/SIGA.Api

# Build / Run
dotnet build
dotnet run --project src/SIGA.Api
```

> **Tip:** si `dotnet build` falla por DLLs bloqueadas, matar el proceso `SIGA.Api` antes de recompilar.

Backend corre en `http://localhost:5038` (configurable en `launchSettings.json`).

## Endpoints implementados

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | /api/auth/register | Registro genérico (crea Person + User sin rol) |
| POST | /api/auth/login | Login JWT |
| GET/POST | /api/professionals | CRUD profesional |
| PUT/DELETE | /api/professionals/{id} | Actualizar / desactivar |
| GET/POST | /api/patients | CRUD paciente |
| PUT/DELETE | /api/patients/{id} | Actualizar / desactivar |
| GET | /api/turnos | Listar turnos (filtros: fecha, professionalId, estado) |
| POST | /api/turnos | Crear turno |
| PUT | /api/turnos/{id}/estado | Actualizar estado (Pendiente/Completado/Cancelado) |
| DELETE | /api/turnos/{id} | Cancelar turno |
| GET | /api/consultas | Listar consultas (filtros: patientId, professionalId, search) |
| GET | /api/consultas/patient/{patientId} | Historial clínico de un paciente |
| POST | /api/consultas | Crear consulta clínica |
| PUT | /api/consultas/{id} | Actualizar consulta |
| DELETE | /api/consultas/{id} | Eliminar consulta |
| POST | /api/consultas/{consultaId}/receta | Crear/actualizar receta |
| GET | /api/roles | Listar roles |
| GET/POST | /api/roles | CRUD rol |
| PUT/DELETE | /api/roles/{id} | Actualizar / eliminar rol |
| GET | /api/users/{userId}/roles | Roles de un usuario |
| POST | /api/users/{userId}/roles | Asignar rol |
| DELETE | /api/users/{userId}/roles/{roleId} | Quitar rol |