# Ventas — ABM de Timbrados + numeración automática de facturas

> **Plan de implementación.** Documento de trabajo para ejecutar por pasos; marcar
> `[ ]` → `[x]` y anotar en "Registro de avance". Pertenece al **módulo de Ventas**.

**Estado general:** 🟡 Planificado — pendiente de implementación.
**Última actualización:** 2026-06-09.

---

## 1. Contexto y objetivo

Hoy, al emitir una **Factura Timbrada** (`VentaComprobanteView.vue` → `EmitirFacturaAsync`,
`VentaService.cs`), el cajero carga **a mano** N° de factura, timbrado y establecimiento.
Es propenso a error y no controla el correlativo legal.

**Objetivo:** un ABM de **timbrados activos** (módulo Ventas). Cada timbrado guarda su
**establecimiento**, **punto de expedición** y el **último número emitido**. Al facturar,
el cajero **elige un timbrado activo** y el sistema **genera automáticamente** el N° de
factura (`establecimiento-puntoExpedición-correlativo`), completa el establecimiento y
**avanza el correlativo**.

Formato paraguayo del número: `EEE-PPP-NNNNNNN` (ej. `001-001-0000001`), donde
`EEE` = establecimiento (3 díg), `PPP` = punto de expedición (3 díg), `NNNNNNN` =
correlativo (7 díg).

## 2. Decisiones de diseño propuestas (ajustables)

1. **Reemplaza** la carga manual de factura por **selección de timbrado** (automático).
   No se mantiene el ingreso manual de número/timbrado/establecimiento.
2. Cada fila de timbrado representa una **serie** = combinación única
   (NumeroTimbrado + Establecimiento + PuntoExpedición) con su propio correlativo.
3. Se incluye **vigencia** (fecha inicio/fin) y **rango autorizado** (NumeroDesde/Hasta)
   para validar al emitir y alertar agotamiento. (Si no lo querés, se omite el rango.)
4. **Permisos:** se reutilizan `ver_ventas` (ver) y `gestionar_ventas` (alta/edición/baja).
   Sin permisos nuevos → sin cambios de seeding.
5. **Trazabilidad:** `FacturaVenta` gana FK opcional `TimbradoId`.

> Confirmá/ajustá estos 5 puntos antes de ejecutar; el resto del plan deriva de ellos.

## 3. Modelo de datos

### Nueva entidad `Timbrado` (`SIGA.Domain/Entities/Timbrado.cs`)
```
Id                  int PK
NumeroTimbrado      string (req)         -- p.ej. "12345678"
Establecimiento     string(3) (req)      -- "001"
PuntoExpedicion     string(3) (req)      -- "001"
UltimoNumero        int (default 0)      -- correlativo ya emitido; el próximo = +1
NumeroDesde         int (default 1)      -- inicio del rango autorizado (opcional)
NumeroHasta         int (nullable)       -- fin del rango (opcional, para alerta)
FechaInicioVigencia DateOnly (req)
FechaFinVigencia    DateOnly (req)
IsActive            bool (default true)
CreatedAt           DateTime
```
Patrón EF (igual a `TipoLenteConfiguration.cs`): tabla `timbrados` (snake_case),
columnas PascalCase; índice **único** sobre (`NumeroTimbrado`,`Establecimiento`,`PuntoExpedicion`).

### Cambio en `FacturaVenta` (`FacturaVenta.cs`)
- `+ public int? TimbradoId` y nav `Timbrado? Timbrado` (FK opcional, `OnDelete(Restrict)`).

### Migración (recipe estándar del repo)
- `046_Timbrados` (o el número que siga): crea tabla `timbrados` + agrega `TimbradoId` a
  `facturas_venta`. Validar con `dotnet ef migrations has-pending-model-changes`.

## 4. Backend (`SIGA`)

Sigue el patrón **TipoLente** (referencia exacta de cada archivo entre paréntesis).

- [ ] **B1.** Entidad `Timbrado.cs` (ref `TipoLente.cs`).
- [ ] **B2.** `TimbradoConfiguration.cs` (ref `TipoLenteConfiguration.cs`) + `+ public DbSet<Timbrado> Timbrados`
  en `AppDbContext.cs` (junto a `TiposLente`, línea ~62) + ajuste de `FacturaVenta` (FK).
- [ ] **B3.** DTOs en `SIGA.Application/DTOs/Ventas/TimbradoDto.cs` (ref `TipoLenteDto.cs`):
  `TimbradoDto` (incluye `ProximoNumero` calculado = `UltimoNumero+1` y `NumeroCompletoPreview`),
  `CreateTimbradoRequest`, `UpdateTimbradoRequest`.
- [ ] **B4.** `ITimbradoService` + `TimbradoService` (ref `ITipoLenteService`/`TipoLenteService`):
  `GetAllAsync`, `GetActivosAsync` (solo `IsActive` y vigentes), `CreateAsync`, `UpdateAsync`,
  `DeactivateAsync`. Validaciones: establecimiento/punto = 3 dígitos numéricos; vigencia
  coherente (inicio ≤ fin); unicidad de la serie.
- [ ] **B5.** `TimbradosController` (`api/timbrados`, ref `TipoLentesController.cs`) con policies
  `ver_ventas` (GET) y `gestionar_ventas` (POST/PUT/DELETE).
- [ ] **B6.** Registrar `ITimbradoService` en `DependencyInjection.cs` (junto a la línea ~62
  de `TipoLenteService`).
- [ ] **B7.** Numeración automática en la emisión (`VentaService.cs`):
  - `EmitirFacturaRequest` → `{ ventaId, timbradoId, fechaEmision, observaciones }`
    (quita numeroFactura/timbrado/establecimiento manuales).
  - `EmitirFacturaAsync`: cargar timbrado; validar `IsActive`, vigencia (FechaEmision dentro
    de rango de fechas) y, si hay `NumeroHasta`, que `UltimoNumero+1 <= NumeroHasta`;
    `numero = UltimoNumero+1`; `NumeroFactura = $"{Establecimiento}-{PuntoExpedicion}-{numero:D7}"`;
    poblar `FacturaVenta` (NumeroFactura, Timbrado=NumeroTimbrado, Establecimiento, TimbradoId);
    `timbrado.UltimoNumero = numero`; resto igual (helper `AplicarEgresosDeEmision`, estado
    `ComprobanteEmitido`). Todo en el mismo `SaveChangesAsync` (atómico).
  > Nota concurrencia: para varias cajas sobre el mismo timbrado, agregar `RowVersion`
  > (concurrency token) al `Timbrado` y reintentar ante conflicto. Bajo riesgo con 1 punto.
- [ ] **B8.** `dotnet build` en verde (backend detenido por el lock) + `dotnet ef database update`.

## 5. Frontend (`SIGA-Web`)

- [ ] **F1.** `services/timbradoService.ts` (ref bloque TipoLente en `inventarioService.ts`):
  interfaces + `getTimbrados`, `getTimbradosActivos`, `createTimbrado`, `updateTimbrado`,
  `deactivateTimbrado`.
- [ ] **F2.** Vista ABM `views/TimbradosView.vue` (ref `TipoLentesView.vue`: tabla +
  FilterChips activo/inactivo + paginación + modales crear/editar/desactivar + RowContextMenu).
  Columnas: Timbrado · Establecimiento · Pto. Exped. · Próximo N° · Vigencia · Estado · acciones.
  Acciones gateadas por `gestionar_ventas`.
- [ ] **F3.** Ruta `/ventas/timbrados` en `router/index.ts` (`meta.permission = "ver_ventas"`).
- [ ] **F4.** Item "Timbrados" en el grupo **Ventas** de `menuConfig.ts` (icono `verified`/
  `confirmation_number`, permission `ver_ventas`).
- [ ] **F5.** `VentaComprobanteView.vue` (al elegir **Factura Timbrada**):
  - Reemplazar los inputs manuales por un **selector de timbrado activo** (`getTimbradosActivos`).
  - Mostrar **preview**: establecimiento, punto de expedición y **próximo N°**
    (`EEE-PPP-{ultimoNumero+1:D7}`).
  - Mantener fecha de emisión y observaciones.
  - Emitir con `{ ventaId, timbradoId, fechaEmision, observaciones }`.
  - Si no hay timbrados activos: mensaje + link a `/ventas/timbrados`.
- [ ] **F6.** `npm run type-check` en verde.

## 6. Verificación end-to-end

- [ ] **V1.** Crear timbrado (est `001`, punto `001`, últimoN° `0`, vigencia válida) → aparece
  en la lista con Próximo N° `001-001-0000001`.
- [ ] **V2.** Emitir Factura Timbrada eligiendo ese timbrado → `FacturaVenta.NumeroFactura =
  001-001-0000001`, venta en `ComprobanteEmitido`, stock+caja generados, y el timbrado pasa a
  `UltimoNumero = 1`.
- [ ] **V3.** Emitir otra factura con el mismo timbrado → `0000002` (correlativo avanza).
- [ ] **V4.** Validaciones: timbrado inactivo / fuera de vigencia / rango agotado → rechazo;
  establecimiento o punto no numéricos de 3 díg → error en el ABM.

---

## 7. Registro de avance

- 2026-06-09 — Plan creado. Pendiente confirmar las 5 decisiones de diseño (§2) y comenzar
  por el backend (B1).

> **Auditado 2026-07-08:** el encabezado de este documento dice "🟡 Planificado — pendiente de implementación", pero está **desactualizado**: confirmado contra código (`VentaService.cs`, `TimbradosController`, `api-reference.md`) que este plan **ya está implementado en su totalidad** — numeración automática por timbrado incluida. Fusionado en [modules/08-ventas.md](./modules/08-ventas.md). Este archivo se mantiene por referencia histórica.
