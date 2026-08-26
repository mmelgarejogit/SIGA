# Módulo: Personal

## Propósito

Gestiona al personal administrativo/operativo de la óptica (recepción, cajeros, vendedores, etc.) que necesita acceso al sistema pero no es un profesional de salud — es la contraparte de RR.HH. de `Professional`. Alimenta al módulo de Egresos (`SalarioEmpleado`) para el registro de nóminas.

## Entidades principales

Ver [`schema.md` Grupo A](../schema.md#grupo-a--identidad-personas-y-personal).

| Entidad | Rol |
|---|---|
| `Empleado` | Marca que un `User` es empleado. `CargoId` FK, `FechaIngreso`/`FechaEgreso`, `SalarioBase` nullable |
| `CargoEmpleado` | Catálogo de cargos/puestos (ej. Recepcionista, Cajero) |

## Reglas de negocio clave

- **Estructura idéntica a `Professional`:** `Empleado` siempre requiere `User` (1:1), se crea con `IsEmailVerified=true` y `MustChangePassword=true` en la misma transacción (`Person`+`User`+`Empleado`+asignación de rol) — mismo patrón que el alta de profesional, ver [`02-pacientes-y-profesionales.md`](./02-pacientes-y-profesionales.md).
- **`CargoEmpleado` es un catálogo simple sin controller propio** — se gestiona como sub-recurso de `EmpleadosController` (`/api/empleados/cargos`), no tiene su ruta base independiente. Confirmado en `api-reference.md`.
- **La sucursal del empleado se asigna en este mismo formulario** (`EmpleadosView`), igual que para profesionales — no desde `UsuariosView`. Ver [`project_sucursales`, memoria de proyecto].
- **`SalarioBase` es nullable y opcional** — no todo empleado tiene un salario fijo cargado acá; cuando existe, es la base que consume el módulo de Egresos para generar `SalarioEmpleado` (un `Egreso` de tipo salario). Ver [`11-caja-y-egresos.md`](./11-caja-y-egresos.md).
- **`FechaEgreso` nullable** — permite registrar la baja de un empleado sin necesariamente desactivar el `User` en el mismo momento (son dos campos independientes: la fecha de egreso es dato de RR.HH., `IsActive` del `User` es control de acceso).
- **Borrado lógico** vía desactivación del `User` asociado, igual que el resto de las cuentas de staff.

## Endpoints

Detalle completo en [`api-reference.md` § 3](../api-reference.md#3-personal) (`EmpleadosController`, que usa su propio `ToResponse` en vez de `BaseController` — única excepción en los 35 controllers).

| Método | Ruta | Descripción |
|---|---|---|
| GET | /api/empleados?soloActivos | Listado |
| POST/PUT/DELETE | /api/empleados | Alta (transacción multi-entidad), edición, desactivación |
| GET/POST/PUT | /api/empleados/cargos | Catálogo de cargos, anidado bajo este mismo controller |

## Flujo típico

Idéntico en estructura al alta de profesional (ver diagrama de secuencia en [`02-pacientes-y-profesionales.md`](./02-pacientes-y-profesionales.md#alta-de-profesional-transacción-multi-entidad)), reemplazando `Professional`/`LicenseNumber`/`ProfesionalEspecialidad` por `Empleado`/`CargoId`/`FechaIngreso`. No se repite el diagrama acá para no duplicar contenido — la única diferencia estructural es que no hay equivalente a "especialidades" (many-to-many), la relación con `CargoEmpleado` es N:1 simple.

## Vistas de frontend

- `EmpleadosView.vue`
- `CargosEmpleadoView.vue`

## Estado

✅ Implementado. Sin limitaciones conocidas específicas de este módulo más allá de las ya generales de gestión de cuentas (ver [`01-identidad-y-acceso.md`](./01-identidad-y-acceso.md) § Estado).
