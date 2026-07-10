# Módulo: Roles y Permisos

## Propósito

ABM del catálogo de `Role`/`Permission` y su asignación: define qué combinaciones de permisos existen como "rol" seleccionable y qué permisos tiene cada uno. Es el panel de configuración detrás de la autorización descrita en [`01-identidad-y-acceso.md`](./01-identidad-y-acceso.md) — acá se define *qué puede hacer cada rol*, allá se gestiona *qué rol tiene cada usuario*.

## Entidades principales

Ver [`schema.md` Grupo A](../schema.md#grupo-a--identidad-personas-y-personal).

| Entidad | Rol |
|---|---|
| `Role` | Rol configurable con nombre. `Type` (`admin`\|`professional`\|`patient`, nullable e **inmutable**) marca los 3 roles de sistema |
| `Permission` | Catálogo fijo de 55 permisos granulares sembrados por `DbSeeder.AllPermissions` (`ver_pacientes`, `crear_venta`, `gestionar_laboratorio`, etc.). 54 tienen policy homónima en `Program.cs`; la excepción es `ver_recepcion`, que está en el seed pero **no** tiene policy declarada (verificado 2026-07-09) — no se puede usar hoy en un `[Authorize(Policy=...)]` aunque exista en la base. |
| `RolePermission` | Pivote many-to-many — la asignación real de permisos a un rol |

## Reglas de negocio clave

- **Un `Role` es solo un nombre + una colección de `Permission`s** — no hay lógica de negocio codificada por nombre de rol en ningún endpoint. Ver [ADR 0003](../adr/0003-autorizacion-por-permisos.md).
- **Se pueden crear roles ad-hoc desde `/roles` sin tocar código ni deployar** — ej. "Recepcionista Senior" con un permiso extra sobre "Recepcionista" — porque la autorización nunca consulta `Role.Name`, solo los permisos resueltos en el JWT.
- **`Role.Type` es inmutable y no es un permiso** — identifica los 3 roles de sistema (`admin`, `professional`, `patient`) para lógica interna puntual (ej. bootstrap del usuario admin, o que el seeder sepa a qué rol darle todos los permisos). No participa en ninguna policy de autorización de endpoint.
- **El admin tiene todos los permisos excepto `ver_mis_turnos`** (ese es específico del portal de autogestión del paciente) — sembrado por el `DbSeeder`, no hardcodeado en cada policy.
- **Vista propia separada de la gestión de usuarios:** `RolesView`/`RolFormView` (cards por módulo de negocio, para elegir permisos agrupados visualmente) es una pantalla distinta de `UsuariosView` — esta última solo asigna *qué rol* tiene cada usuario, no edita el contenido de un rol. Decisión de diseño explícita registrada en memoria de proyecto para no mezclar ambas responsabilidades en una sola pantalla.
- **⚠️ Posible bug de copy-paste detectado (Fase 3, sin corregir):** `GET /api/roles/{id}/users` en `RolesController` llama internamente al mismo método (`GetRolesByUserAsync`) que se usa para "roles de un usuario" — debería listar los usuarios que tienen ese rol, no roles de un usuario. Ver `api-reference.md` § 1. Queda pendiente de verificar contra `IRoleService` y corregir; no se tocó código en esta fase de documentación.

## Endpoints

Detalle completo en [`api-reference.md` § 1](../api-reference.md#1-identidad-y-acceso) (`RolesController`, `UserRolesController` — ambos viven en el mismo archivo `RolesController.cs`).

| Método | Ruta | Descripción |
|---|---|---|
| GET | /api/roles | Listado de roles |
| GET | /api/roles/{id} | Detalle (incluye sus permisos) |
| POST/PUT/DELETE | /api/roles | ABM del rol y su combinación de permisos |
| GET | /api/roles/{id}/users | ⚠️ ver nota de bug arriba |
| GET/POST/DELETE | /api/users/{userId}/roles | Asignación de roles a un usuario (documentado también en `01-identidad-y-acceso.md`, vive en `UsuariosView`) |

## Flujo típico

CRUD estándar sin pasos multi-capa no triviales — no amerita diagrama de secuencia. La única mecánica particular es que el formulario de rol (`RolFormView`) presenta los 55 `Permission` agrupados por módulo de negocio (cards), y guarda la selección completa como el nuevo set de `RolePermission` del rol (reemplazo total, no diffing incremental).

## Vistas de frontend

- `RolesView.vue` (listado)
- `RolFormView.vue` (alta/edición, cards de permisos por módulo)

## Estado

✅ Implementado. ⚠️ Bug conocido sin corregir en `GET /api/roles/{id}/users` (ver arriba) — candidato a corrección de código fuera del alcance de esta documentación.
