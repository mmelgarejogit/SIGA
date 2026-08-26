# API Reference — SIGA

> Escrito 2026-07-08 (Fase 3 del plan de documentación técnica, ver `README.md`). Inventario completo de los 35 controllers en `SIGA.Api/Controllers/*.cs`, verificado leyendo cada archivo (no hay `Route` con path/verbo extraído automáticamente para C# — ver limitación anotada en el plan). Agrupado en los mismos 14 módulos de negocio que `architecture.md` § "Módulos de negocio → Controllers", para que cada `modules/*.md` (Fase 4) pueda linkear 1:1.

## Cómo leer este documento

- **Ruta base:** todas las rutas empiezan con `/api`. Se omite el prefijo en las tablas salvo la primera columna "Ruta" que ya lo incluye completo.
- **Policy/Permiso:** el nombre exacto del `[Authorize(Policy = "...")]`. Si la tabla dice `(heredado)`, el controller entero está protegido por una policy a nivel de clase y el método no agrega una propia. `[Authorize]` sin policy = requiere estar autenticado, cualquier permiso. `AllowAnonymous` = público.
- **Request/Response:** nombre del tipo C# tal cual en el código. Todas las respuestas exitosas devuelven `result.Value` envuelto por `Result<T>` (ver `architecture.md`); los listados paginados siguen el shape uniforme `{ items, totalCount, page, pageSize, totalPages }` descrito en `CLAUDE.md`, salvo que se indique lo contrario.
- Los controllers heredan de `BaseController` (`ToHttpResponse`) salvo `EmpleadosController`, que define su propio `ToResponse` equivalente (mismo mapeo de `ErrorType`, sin caso `Unauthorized` explícito).

---

## 1. Identidad y Acceso

### `AuthController` — `/api/auth`

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| POST | /api/auth/register | AllowAnonymous | `RegisterRequest` | — |
| POST | /api/auth/register/patient | AllowAnonymous | `RegisterPatientRequest` | — |
| GET | /api/auth/verify-email?token= | AllowAnonymous | — (query `token`) | — |
| POST | /api/auth/login | AllowAnonymous | `LoginRequest` | `LoginResponse` (incl. `jwtToken`, `roleClaims`, `sucursalId`/`sucursalNombre`) |
| POST | /api/auth/change-password | `[Authorize]` (cualquier autenticado) | `ChangePasswordRequest` | — |
| POST | /api/auth/forgot-password | AllowAnonymous | `ForgotPasswordRequest` | — |
| POST | /api/auth/reset-password | AllowAnonymous | `ResetPasswordWithTokenRequest` | — |

### `UsersController` — `/api/users` (`[Authorize]` a nivel de clase)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/users | `ver_usuarios` | — | listado de usuarios |
| DELETE | /api/users/{id} | `editar_usuario` | — | — (desactiva, soft delete) |
| PUT | /api/users/{id}/reset-password | `editar_usuario` | `ResetPasswordRequest` | — (reset admin, fuerza cambio en próximo login) |

### `RolesController` — `/api/roles` (`[Authorize]` a nivel de clase)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/roles | `ver_roles` | — | listado de roles |
| GET | /api/roles/{id} | `ver_roles` | — | detalle de rol |
| POST | /api/roles | `crear_rol` | `RoleRequest` | — |
| PUT | /api/roles/{id} | `editar_rol` | `RoleRequest` | — |
| DELETE | /api/roles/{id} | `eliminar_rol` | — | — |
| GET | /api/roles/{id}/users | `ver_roles` | — | ⚠️ nota: llama a `GetRolesByUserAsync(id)`, mismo método que el endpoint de abajo — sospecha de bug/copy-paste (debería listar usuarios del rol, no roles del usuario). Verificar con `IRoleService` al escribir `modules/01-identidad-y-acceso.md`. |

### `UserRolesController` — `/api/users/{userId}/roles` (mismo archivo `RolesController.cs`, `[Authorize]` a nivel de clase)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/users/{userId}/roles | `ver_usuarios` | — | roles del usuario |
| POST | /api/users/{userId}/roles | `editar_usuario` | `AssignRoleRequest` | — |
| DELETE | /api/users/{userId}/roles/{roleId} | `editar_usuario` | — | — |

---

## 2. Personas

### `PatientsController` — `/api/patients` (`[Authorize]`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/patients?page&pageSize&search&status | `ver_pacientes` | — | listado paginado (pageSize máx. 500) |
| GET | /api/patients/{id} | `ver_pacientes` | — | detalle |
| POST | /api/patients | `crear_paciente` | `CreatePatientRequest` | — |
| PUT | /api/patients/{id} | `editar_paciente` | `UpdatePatientRequest` | — |
| DELETE | /api/patients/{id} | `desactivar_paciente` | — | — |

### `ProfessionalsController` — `/api/professionals` (`[Authorize]`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/professionals | `ver_profesionales` | — | listado (sin paginar) |
| GET | /api/professionals/{id} | `ver_profesionales` | — | detalle |
| POST | /api/professionals | `crear_profesional` | `CreateProfessionalRequest` | — |
| PUT | /api/professionals/{id} | `editar_profesional` | `UpdateProfessionalRequest` | — |
| DELETE | /api/professionals/{id} | `editar_profesional` | — | — ⚠️ nota: desactivar profesional usa la policy `editar_profesional`, no una específica de desactivación (a diferencia de `Patient`/`Cliente` que sí la tienen) |

Horarios de profesional viven en un controller aparte, ver `HorariosController` (módulo Agenda).

### `ClientesController` — `/api/clientes` (`[Authorize]`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/clientes?page&pageSize&search&status&tipo | `ver_clientes` | — | listado paginado |
| GET | /api/clientes/{id} | `ver_clientes` | — | detalle |
| GET | /api/clientes/buscar-persona?ci= | `crear_cliente` | — (query `ci`) | `Person` existente o null — para reutilizar persona al dar de alta |
| POST | /api/clientes | `crear_cliente` | `CreateClienteRequest` | — |
| PUT | /api/clientes/{id} | `editar_cliente` | `UpdateClienteRequest` | — |
| DELETE | /api/clientes/{id} | `desactivar_cliente` | — | — (desactiva) |
| POST | /api/clientes/{id}/activar | `desactivar_cliente` | — | — |

### `EspecialidadesController` — `/api/especialidades` (`[Authorize]`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/especialidades | `[Authorize]` (heredado, sin policy propia) | — | listado |
| GET | /api/especialidades/{id} | `[Authorize]` (heredado) | — | detalle |
| POST | /api/especialidades | `gestionar_especialidades` | `CreateEspecialidadRequest` | — |
| PUT | /api/especialidades/{id} | `gestionar_especialidades` | `UpdateEspecialidadRequest` | — |
| DELETE | /api/especialidades/{id} | `gestionar_especialidades` | — | — |

---

## 3. Personal

### `EmpleadosController` — `/api/empleados` (policy de clase `ver_empleados`; usa su propio `ToResponse`, no `BaseController`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/empleados?soloActivos | `ver_empleados` (heredado) | — | listado |
| GET | /api/empleados/{id} | `ver_empleados` (heredado) | — | detalle |
| POST | /api/empleados | `gestionar_empleados` | `CrearEmpleadoRequest` | — |
| PUT | /api/empleados/{id} | `gestionar_empleados` | `ActualizarEmpleadoRequest` | — |
| DELETE | /api/empleados/{id} | `gestionar_empleados` | — | — (desactiva) |
| GET | /api/empleados/cargos | `ver_empleados` (heredado) | — | listado de `CargoEmpleado` — confirma que este catálogo vive acá, sin controller propio |
| POST | /api/empleados/cargos | `gestionar_empleados` | `CrearCargoEmpleadoRequest` | — |
| PUT | /api/empleados/cargos/{id} | `gestionar_empleados` | `ActualizarCargoEmpleadoRequest` | — |

---

## 4. Agenda

### `TurnosController` — `/api/turnos` (`[Authorize]`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/turnos?fecha&professionalId&estado&patientId | `ver_agenda` | — | listado filtrado |
| GET | /api/turnos/disponibles?professionalId&fecha&sucursalId | `ver_disponibles` (OR: `ver_agenda` staff u `ver_mis_turnos` paciente) | — | slots libres |
| GET | /api/turnos/profesionales-disponibles?fecha&sucursalId | `ver_mis_turnos` | — | profesionales con horario activo — usado en el flujo de reserva del paciente |
| POST | /api/turnos | `gestionar_agenda` | `CreateTurnoRequest` | — |
| PUT | /api/turnos/{id}/estado | `gestionar_agenda` | `UpdateTurnoEstadoRequest` | — |
| DELETE | /api/turnos/{id} | `gestionar_agenda` | — | — |
| GET | /api/turnos/mis-turnos | `ver_mis_turnos` | — (usa `ClaimTypes.NameIdentifier` del JWT) | turnos del paciente autenticado |
| POST | /api/turnos/self-book | `ver_mis_turnos` | `SelfBookTurnoRequest` (incluye `SucursalId`, obligatorio) | — |
| POST | /api/turnos/{id}/solicitar-cancelacion | `ver_mis_turnos` | — | — |
| POST | /api/turnos/{id}/gestionar-cancelacion | `gestionar_agenda` | `GestionarCancelacionRequest` | — |
| POST | /api/turnos/confirmar/{token} | AllowAnonymous | — (token en URL, del email de confirmación) | — |

### `HorariosController` — `/api/professionals/{professionalId}` (`[Authorize]`, anidado bajo Professionals)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/professionals/{professionalId}/horarios | `ver_profesionales` | — | horarios semanales |
| PUT | /api/professionals/{professionalId}/horarios | `editar_profesional` | `SetHorariosRequest` | — (reemplaza el set completo por sucursal del usuario) |
| GET | /api/professionals/{professionalId}/bloqueos | `ver_profesionales` | — | fechas bloqueadas |
| POST | /api/professionals/{professionalId}/bloqueos | `editar_profesional` | `BloqueoFechaRequest` | — |
| DELETE | /api/professionals/{professionalId}/bloqueos/{bloqueoId} | `editar_profesional` | — | — |

---

## 5. Clínica

### `ConsultasController` — `/api/consultas` (`[Authorize]`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/consultas?page&pageSize&search&patientId&professionalId | `ver_consultas` | — | listado paginado; si el caller es profesional (claim `professional_id`), se auto-filtra a sus propias consultas |
| GET | /api/consultas/patient/{patientId} | `ver_consultas` | — | consultas de un paciente |
| GET | /api/consultas/{id} | `ver_consultas` | — | detalle |
| GET | /api/consultas/profesional/stats | `ver_consultas` | — (requiere claim `professional_id`, si no → 403) | estadísticas del profesional autenticado |
| POST | /api/consultas | `registrar_consulta` | `CreateConsultaClinicaRequest` | — (si el caller es profesional, se fuerza `ProfessionalId` al propio) |
| PUT | /api/consultas/{id} | `editar_consulta` | `UpdateConsultaClinicaRequest` | — |
| DELETE | /api/consultas/{id} | `eliminar_consulta` | — | — |
| PATCH | /api/consultas/{id}/estado | `editar_consulta` | `CambiarEstadoConsultaRequest` | — |
| POST | /api/consultas/{id}/receta | `editar_consulta` | `CreateRecetaRequest` | — (crea o actualiza la receta 1:1 de la consulta) |
| GET | /api/consultas/{id}/receta/pdf | `ver_consultas` | — | PDF (QuestPDF) — 404 si la consulta no tiene receta |
| GET | /api/consultas/mis-consultas | `ver_mis_turnos` | — | portal del paciente — sus propias consultas |
| GET | /api/consultas/{id}/mi-receta/pdf | `ver_mis_turnos` | — | portal del paciente — PDF de su propia receta |

### `RecetasController` — `/api/recetas` (`[Authorize]`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/recetas?clienteId= | `ver_ventas` | — | recetas del cliente (clínicas + manuales) — usado por el flujo de venta a pedido |
| POST | /api/recetas | `registrar_venta` | `CreateRecetaManualRequest` | — (receta externa, sin `ConsultaClinicaId`, vinculada directo a un cliente) |

---

## 6. Catálogo / Inventario

### `ProductosController` — `/api/productos` (`[Authorize]`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/productos?page&pageSize&search&categoria&bajoStock&tipoCategoria | `ver_inventario` | — | listado paginado |
| GET | /api/productos/{id} | `ver_inventario` | — | detalle |
| POST | /api/productos | `gestionar_inventario` | `CreateProductoRequest` | — |
| PUT | /api/productos/{id} | `gestionar_inventario` | `UpdateProductoRequest` | — |
| DELETE | /api/productos/{id} | `gestionar_inventario` | — | — (desactiva) |
| DELETE | /api/productos/{id}/permanente | `gestionar_inventario` | — | — (borrado físico) |
| POST | /api/productos/{id}/movimientos | `gestionar_inventario` | `CreateMovimientoStockRequest` | — |
| GET | /api/productos/{id}/movimientos | `ver_inventario` | — | movimientos de un producto |
| GET | /api/productos/movimientos?page&pageSize&tipo&estado | `ver_inventario` | — | movimientos globales |
| PATCH | /api/productos/movimientos/{id}/estado | `gestionar_inventario` | `AprobarRechazarMovimientoRequest` | — |
| GET | /api/productos/movimientos/{id}/pdf | `ver_inventario` | — | PDF de comprobante de movimiento |
| PUT | /api/productos/{id}/stock-config | `gestionar_inventario` | `UpdateStockConfigRequest` | — (mín/máx, global — ver nota en `schema.md`) |
| POST | /api/productos/{id}/imagen | `gestionar_inventario` | `IFormFile` (máx. 5MB, multipart) | — |
| DELETE | /api/productos/{id}/imagen | `gestionar_inventario` | — | — |
| GET | /api/productos/categorias | `ver_inventario` | — | listado de `CategoriaProducto` — confirma que vive acá, sin controller propio |
| POST | /api/productos/categorias | `gestionar_inventario` | `CreateCategoriaProductoRequest` | — |
| PUT | /api/productos/categorias/{id} | `gestionar_inventario` | `UpdateCategoriaProductoRequest` | — |
| DELETE | /api/productos/categorias/{id} | `gestionar_inventario` | — | — (desactiva) |
| DELETE | /api/productos/categorias/{id}/permanente | `gestionar_inventario` | — | — (borrado físico) |

### `MarcasController` — `/api/marcas` (`[Authorize]`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/marcas | `ver_inventario` | — | listado |
| POST | /api/marcas | `gestionar_inventario` | `CreateMarcaRequest` | — |
| PUT | /api/marcas/{id} | `gestionar_inventario` | `UpdateMarcaRequest` | — |
| DELETE | /api/marcas/{id} | `gestionar_inventario` | — | — |
| GET | /api/marcas/modelos?marcaId | `ver_inventario` | — | listado de `Modelo` — confirma que vive acá, sin controller propio |
| POST | /api/marcas/modelos | `gestionar_inventario` | `CreateModeloRequest` | — |
| PUT | /api/marcas/modelos/{id} | `gestionar_inventario` | `UpdateModeloRequest` | — |
| DELETE | /api/marcas/modelos/{id} | `gestionar_inventario` | — | — |

### `TipoLentesController` — `/api/tipos-lente` (`[Authorize]`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/tipos-lente | `ver_inventario` | — | listado |
| POST | /api/tipos-lente | `gestionar_inventario` | `CreateTipoLenteRequest` | — |
| PUT | /api/tipos-lente/{id} | `gestionar_inventario` | `UpdateTipoLenteRequest` | — |
| DELETE | /api/tipos-lente/{id} | `gestionar_inventario` | — | — (desactiva) |
| DELETE | /api/tipos-lente/{id}/permanente | `gestionar_inventario` | — | — |

### `TratamientosController` — `/api/tratamientos` (`[Authorize]`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/tratamientos | `ver_inventario` | — | listado |
| POST | /api/tratamientos | `gestionar_inventario` | `CreateTratamientoRequest` | — |
| PUT | /api/tratamientos/{id} | `gestionar_inventario` | `UpdateTratamientoRequest` | — |
| DELETE | /api/tratamientos/{id} | `gestionar_inventario` | — | — |

### `ServiciosController` — `/api/servicios` (`[Authorize]`)

⚠️ Nota de agrupamiento: el archivo usa DTOs de `SIGA.Application.DTOs.Ventas` (no `Inventario`) y las policies son `ver_ventas`/`gestionar_ventas` — funcionalmente pertenece al dominio comercial (servicios como exámenes con tarifa por profesional), aunque `architecture.md` lo agrupó bajo Catálogo/Inventario. Confirmar el agrupamiento definitivo al escribir `modules/07-inventario-catalogo.md` vs. `modules/08-ventas.md`.

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/servicios | `ver_ventas` | — | listado |
| POST | /api/servicios | `gestionar_ventas` | `CreateServicioRequest` | — |
| PUT | /api/servicios/{id} | `gestionar_ventas` | `UpdateServicioRequest` | — |
| DELETE | /api/servicios/{id} | `gestionar_ventas` | — | — |
| POST | /api/servicios/{id}/tarifas | `gestionar_ventas` | `CreateServicioTarifaRequest` | — (precio por profesional/especialidad) |
| DELETE | /api/servicios/tarifas/{tarifaId} | `gestionar_ventas` | — | — |
| GET | /api/servicios/{id}/precio?professionalId | `ver_ventas` | — | resuelve el precio aplicable (tarifa específica o default) |

### `MotivosMovimientoController` — `/api/motivos-movimiento` (`[Authorize]`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/motivos-movimiento?tipo | `ver_inventario` | — | listado, filtrable por tipo de movimiento |
| POST | /api/motivos-movimiento | `gestionar_inventario` | `CreateMotivoMovimientoRequest` | — |
| PUT | /api/motivos-movimiento/{id} | `gestionar_inventario` | `UpdateMotivoMovimientoRequest` | — |
| DELETE | /api/motivos-movimiento/{id} | `gestionar_inventario` | — | — |

### `StockLotesController` — `/api/stock/lotes` (policy de clase `ver_inventario`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/stock/lotes?productoId&vencidos | `ver_inventario` (heredado) | — | lotes con trazabilidad de vencimiento |
| POST | /api/stock/lotes/conteo | `ver_inventario` (heredado) ⚠️ | `RegistrarConteoRequest` | registra un conteo físico — nota: crear un conteo **no** exige `gestionar_inventario`, solo `ver_inventario`; solo *gestionar* (aprobar) el conteo lo exige (ver abajo) |
| GET | /api/stock/lotes/conteos?estado | `ver_inventario` (heredado) | — | listado de conteos |
| GET | /api/stock/lotes/conteos/{id} | `ver_inventario` (heredado) | — | detalle de conteo (con diferencias sistema-vs-físico) |
| POST | /api/stock/lotes/conteos/{id}/gestionar | `gestionar_inventario` | `GestionarConteoRequest` | aprueba/rechaza el conteo (ajusta stock si corresponde) |

### `UbicacionesController` — `/api/ubicaciones` (`[Authorize]`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/ubicaciones/departamentos?isActive | `[Authorize]` (heredado) | — | listado de `Departamento` |
| POST | /api/ubicaciones/departamentos | `gestionar_configuracion` | `CreateDepartamentoRequest` | — |
| PUT | /api/ubicaciones/departamentos/{id} | `gestionar_configuracion` | `UpdateDepartamentoRequest` | — |
| GET | /api/ubicaciones/ciudades?departamentoId&isActive | `[Authorize]` (heredado) | — | listado de `Ciudad` |
| POST | /api/ubicaciones/ciudades | `gestionar_configuracion` | `CreateCiudadRequest` | — |
| PUT | /api/ubicaciones/ciudades/{id} | `gestionar_configuracion` | `UpdateCiudadRequest` | — |

---

## 7. Ventas

### `VentasController` — `/api/ventas` (`[Authorize]`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/ventas?estado&tipo&fechaDesde&fechaHasta&clienteId&page&pageSize | `ver_ventas` | — | listado paginado |
| GET | /api/ventas/{id} | `ver_ventas` | — | detalle |
| POST | /api/ventas | `registrar_venta` | `CrearVentaRequest` — lleva bloque opcional `TrabajoPedido` (tipoLenteId/tratamientoIds/armazonProductoId/armazonDelCliente/laboratorioProveedorId/observacion) para venta "a pedido" | — |
| PUT | /api/ventas/{id} | `registrar_venta` | `ActualizarVentaRequest` | — |
| PUT | /api/ventas/{id}/confirmar | `registrar_venta` | — (usa `CurrentUserId`) | — (pasa de presupuesto/borrador a venta confirmada; exige `RecetaId` si es a pedido) |
| DELETE | /api/ventas/{id} | `registrar_venta` | — | — (elimina presupuesto, no venta confirmada) |
| PUT | /api/ventas/{id}/cancelar | `registrar_venta` | `CancelarVentaRequest` | — |
| GET | /api/ventas/cobros-pendientes | `ver_ventas` | — | ventas a crédito con saldo pendiente |
| POST | /api/ventas/cobros | `registrar_venta` | `RegistrarCobroRequest` | — |
| POST | /api/ventas/{id}/comprobante | `registrar_venta` | — (usa `CurrentUserId`) | — (emite comprobante, agrega ingreso a caja si no hay cobros previos) |
| POST | /api/ventas/facturas | `registrar_venta` | `EmitirFacturaRequest` | — |
| POST | /api/ventas/{id}/devoluciones | `registrar_venta` | `SolicitarDevolucionRequest` | — |
| GET | /api/ventas/{id}/devoluciones | `ver_ventas` | — | devoluciones de una venta |
| POST | /api/ventas/devoluciones/{devolucionId}/gestionar | `gestionar_ventas` | `GestionarDevolucionRequest` | — (aprobar/rechazar) |

### `TimbradosController` — `/api/timbrados` (`[Authorize]`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/timbrados | `ver_ventas` | — | listado |
| GET | /api/timbrados/activos | `ver_ventas` | — | timbrados vigentes (por sucursal) |
| GET | /api/timbrados/{id} | `ver_ventas` | — | detalle |
| POST | /api/timbrados | `gestionar_ventas` | `CreateTimbradoRequest` | — |
| PUT | /api/timbrados/{id} | `gestionar_ventas` | `UpdateTimbradoRequest` | — |
| DELETE | /api/timbrados/{id} | `gestionar_ventas` | — | — |

---

## 8. Laboratorio

### `LaboratorioController` — `/api/laboratorio` (`[Authorize]`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/laboratorio/pedidos?estado | `ver_laboratorio` | — | listado de `TrabajoPedido` (excluye `Borrador`) |
| POST | /api/laboratorio/pedidos/{id}/gestionar | `gestionar_laboratorio` | `GestionarTrabajoPedidoRequest` | — (aprobar/rechazar, pasa de `PendienteAprobacion`) |
| PUT | /api/laboratorio/pedidos/{id}/enviar | `gestionar_laboratorio` | `RegistrarEnvioRequest` | — |
| PUT | /api/laboratorio/pedidos/{id}/recibir | `gestionar_laboratorio` | — | — (NO cambia el estado de la venta — cobro desacoplado, ver [ADR 0006](./adr/0006-cobro-desacoplado-del-laboratorio.md)) |
| POST | /api/laboratorio/pedidos/{id}/factura | `gestionar_laboratorio` | `EmitirFacturaLaboratorioRequest` | — |

---

## 9. Compras

### `ComprasController` — `/api/compras/pedidos` (policy de clase `gestionar_pedidos`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/compras/pedidos?proveedorId&estado&page&pageSize | `ver_inventario` | — | listado paginado de OC |
| GET | /api/compras/pedidos/{id} | `ver_inventario` | — | detalle |
| POST | /api/compras/pedidos | `gestionar_pedidos` (heredado) | `CrearPedidoRequest` | — |
| PUT | /api/compras/pedidos/{id} | `gestionar_pedidos` (heredado) | `CrearPedidoRequest` | — |
| PUT | /api/compras/pedidos/{id}/confirmar | `aprobar_pedidos` | — | — |
| POST | /api/compras/pedidos/{id}/factura | `gestionar_pedidos` (heredado) | `RegistrarFacturaPedidoRequest` | — |
| POST | /api/compras/pedidos/{id}/devolucion | `gestionar_pedidos` (heredado) | `RegistrarDevolucionRequest` | — |
| PUT | /api/compras/pedidos/{id}/cancelar | `cancelar_pedido` (OR: creador `gestionar_pedidos` / aprobador `aprobar_pedidos`) | — | — |

### `ProveedoresController` — `/api/proveedores` (`[Authorize]`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/proveedores?page&pageSize&search&isActive | `ver_inventario` | — | listado paginado |
| GET | /api/proveedores/laboratorios | `ver_inventario` | — | subconjunto de proveedores marcados como laboratorio óptico |
| POST | /api/proveedores | `gestionar_pedidos` | `CreateProveedorRequest` | — |
| PUT | /api/proveedores/{id} | `gestionar_pedidos` | `CreateProveedorRequest` (se reutiliza para update) | — |
| DELETE | /api/proveedores/{id} | `gestionar_pedidos` | — | — |

### `FacturasCompraController` — `/api/compras/facturas` (policy de clase `ver_inventario`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/compras/facturas?proveedorId&condicionVenta&estado&origen&fechaDesde&fechaHasta&search&page&pageSize | `ver_inventario` (heredado) | — | listado paginado |
| GET | /api/compras/facturas/{id} | `ver_inventario` (heredado) | — | detalle |
| POST | /api/compras/facturas | `gestionar_pedidos` | `RegistrarFacturaDirectaRequest` | — (factura sin OC previa) |
| PUT | /api/compras/facturas/{id}/anular | `gestionar_pedidos` | `AnularFacturaRequest` | — |

### `RecepcionesController` — `/api/compras/recepciones` (policy de clase `ver_inventario`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/compras/recepciones?proveedorId&estadoOC&fechaDesde&fechaHasta&page&pageSize | `ver_inventario` (heredado) | — | listado paginado |
| GET | /api/compras/recepciones/{id} | `ver_inventario` (heredado) | — | detalle |
| GET | /api/compras/recepciones/facturas-disponibles | `ver_inventario` (heredado) | — | facturas de compra sin recepción asociada |
| POST | /api/compras/recepciones | `gestionar_pedidos` | `RegistrarRecepcionRequest` | — (usa `CurrentUserId`; genera `MovimientoStock` de entrada + `StockLote`) |

---

## 10. Caja y Egresos

### `CajaController` — `/api/caja` (policy de clase `ver_ventas`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/caja/resumen?fecha | `ver_ventas` (heredado) | — | resumen del día |
| GET | /api/caja/movimientos?fechaDesde&fechaHasta&tipo&page&pageSize | `ver_ventas` (heredado) | — | listado paginado de `MovimientoCaja` |
| GET | /api/caja/sesion-actual | `ver_ventas` (heredado) | — | `SesionCaja` abierta del usuario/sucursal |
| GET | /api/caja/apertura-sugerida | `ver_ventas` (heredado) | — | monto de apertura sugerido |
| POST | /api/caja/sesiones | `gestionar_caja` | `AbrirSesionRequest` | — |
| GET | /api/caja/sesiones/{id} | `ver_ventas` (heredado) | — | detalle de sesión |
| POST | /api/caja/sesiones/{id}/cerrar | `gestionar_caja` | `CerrarSesionRequest` | — (pasa a `PendienteAprobacion` según `EstadoSesionCaja`) |
| GET | /api/caja/sesiones?page&pageSize&estado | `ver_ventas` (heredado) | — | historial de sesiones |
| POST | /api/caja/sesiones/{id}/aprobar-cierre | `aprobar_cierres_caja` | — | — |
| POST | /api/caja/sesiones/{id}/rechazar-cierre | `aprobar_cierres_caja` | `RechazarCierreRequest` | — |

### `EgresosController` — `/api/egresos` (policy de clase `gestionar_egresos`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/egresos?tipo&estado&fechaDesde&fechaHasta&soloVencidos&page&pageSize | `ver_egresos` | — | listado paginado (`Egreso`, jerarquía TPH) |
| GET | /api/egresos/{id} | `ver_egresos` | — | detalle |
| POST | /api/egresos/facturas | `gestionar_egresos` (heredado) | `CrearFacturaCompraRequest` | — (subtipo `FacturaCompra`) |
| POST | /api/egresos/honorarios | `gestionar_egresos` (heredado) | `CrearHonorarioRequest` | — (subtipo `Honorario`) |
| POST | /api/egresos/salarios | `gestionar_egresos` (heredado) | `CrearSalarioRequest` | — (subtipo `SalarioEmpleado`) |
| POST | /api/egresos/gastos | `gestionar_egresos` (heredado) | `CrearGastoGeneralRequest` | — (subtipo `GastoGeneral`) |
| PUT | /api/egresos/{id}/aprobar | `aprobar_egresos` | — | — |
| PUT | /api/egresos/{id}/rechazar | `aprobar_egresos` | `RechazarEgresoRequest` | — |
| PUT | /api/egresos/{id}/pago | `pagar_egresos` (override explícito a nivel de método, no hereda `gestionar_egresos` de la clase) — con chequeo extra: pago externo requiere además el claim `aprobar_egresos` y `MotivoExterno` obligatorio | `RegistrarPagoRequest` | — |
| PUT | /api/egresos/{id}/anular | `gestionar_egresos` (heredado) | `AnularEgresoRequest` | — |
| GET | /api/egresos/categorias | `ver_egresos` | — | listado de `CategoriaGasto` — confirma que vive acá, sin controller propio |
| POST | /api/egresos/categorias | `gestionar_egresos` (heredado) | `CrearCategoriaGastoRequest` | — |
| PUT | /api/egresos/categorias/{id} | `gestionar_egresos` (heredado) | `ActualizarCategoriaGastoRequest` | — |

---

## 11. Configuración

### `ConfiguracionController` — `/api/configuracion` (`[Authorize]`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/configuracion | `[Authorize]` (heredado) | — | `ConfiguracionNegocio` (singleton) |
| PUT | /api/configuracion | `gestionar_configuracion` | `UpdateConfiguracionNegocioRequest` | — |

### `EstadosConfigController` — `/api/estados-config` (`[Authorize]`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/estados-config?entidad | `[Authorize]` (heredado) | — | estados cosméticos configurables por entidad (ej. `EstadoConfig` de Turnos) |
| POST | /api/estados-config | `gestionar_configuracion` | `CreateEstadoConfigRequest` | — |
| PUT | /api/estados-config/{id} | `gestionar_configuracion` | `UpdateEstadoConfigRequest` | — |
| DELETE | /api/estados-config/{id} | `gestionar_configuracion` | — | — |

---

## 12. Sucursales

### `SucursalesController` — `/api/sucursales` (`[Authorize]`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/sucursales?soloActivas | `[Authorize]` (heredado — lectura abierta a cualquier autenticado, incl. pacientes, para el flujo de reserva de turno) | — | listado |
| GET | /api/sucursales/{id} | `[Authorize]` (heredado) | — | detalle |
| POST | /api/sucursales | `gestionar_sucursales` | `CreateSucursalRequest` | — |
| PUT | /api/sucursales/{id} | `gestionar_sucursales` | `UpdateSucursalRequest` | — |
| DELETE | /api/sucursales/{id} | `gestionar_sucursales` | — | — |

### `TransferenciasController` — `/api/transferencias` (policy de clase `transferir_stock`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/transferencias?estado | `transferir_stock` (heredado) | — | listado |
| POST | /api/transferencias | `transferir_stock` (heredado) | `CreateTransferenciaRequest` | — (Salida en origen, estado Pendiente) |
| POST | /api/transferencias/{id}/gestionar | `transferir_stock` (heredado) | `GestionarTransferenciaRequest` | — (aceptar = Entrada en destino; rechazar = Entrada de vuelta al origen; solo el destino gestiona) |

---

## 13. Notificaciones

### `NotificacionesController` — `/api/notificaciones` (policy de clase `ver_notificaciones`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/notificaciones?soloNoLeidas&page&pageSize | `ver_notificaciones` (heredado) | — | notificaciones del usuario autenticado |
| GET | /api/notificaciones/contador | `ver_notificaciones` (heredado) | — | cantidad de no leídas (para el badge) |
| PUT | /api/notificaciones/{id}/leer | `ver_notificaciones` (heredado) | — | — |
| PUT | /api/notificaciones/leer-todas | `ver_notificaciones` (heredado) | — | — |

---

## 14. Reportes

### `ReportesController` — `/api/reportes` (`[Authorize]`)

| Método | Ruta | Policy | Request | Response |
|---|---|---|---|---|
| GET | /api/reportes/ventas?desde&hasta&agrupacion | `ver_reportes` | — | serie temporal agregada (agrupación dia\|semana\|mes) |
| GET | /api/reportes/citas?desde&hasta&agrupacion | `ver_reportes` | — | turnos + consultas + recetas agregado |
| GET | /api/reportes/inventario?desde&hasta&agrupacion | `ver_reportes` | — | snapshot de stock (valorización, crítico, por categoría) + movimientos del rango |
| GET | /api/reportes/compras?desde&hasta&agrupacion | `ver_inventario` ⚠️ (distinto del resto, usa la policy del módulo Compras en vez de `ver_reportes`) | — | OC, facturas, recepciones, compras por proveedor |
| GET | /api/reportes/operativo/{tipo}?desde&hasta&sucursalId&metodoPago&categoria&operadorId&tipoMov&page&pageSize | `ver_reportes` | — (`tipo`: ventas\|compras\|inventario\|caja) | listado operativo paginado y filtrable |
| GET | /api/reportes/operativo/{tipo}/export?formato&... (mismos filtros, sin paginar) | `ver_reportes` | — | archivo `application/pdf` o `text/csv` (QuestPDF / CSV UTF-8+BOM) |

---

**Relacionado:** `architecture.md` (capas y pipeline de seguridad), `schema.md` (entidades referenciadas por cada DTO), `adr/` (decisiones detrás de reglas como "cobro desacoplado del laboratorio" o "TrabajoPedido en Borrador").
