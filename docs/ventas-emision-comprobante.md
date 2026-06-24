# Ventas — Arreglo del flujo de emisión de comprobante/factura

> **Documento de progreso vivo.** Marcar cada paso (`[ ]` → `[x]`) a medida que se
> completa y anotar lo relevante en el "Registro de avance" al final, para que cualquier
> modelo/persona pueda retomar el trabajo sin perder contexto.

**Estado general:** 🟢 Implementado y compilando (backend build 0 errores, front type-check
en verde). Pendiente solo la verificación manual end-to-end (V1–V4) con el backend reiniciado.
**Última actualización:** 2026-06-09.

---

## 1. Contexto y problema

La emisión del documento fiscal de una venta está partida en dos conceptos separados y
confusos:

- **`Comprobante`** (`SIGA.Domain/Entities/Comprobante.cs`): su enum `TipoComprobante`
  solo tiene `ReciboSimple`. Se emite al cobrar (`VentaService.EmitirComprobanteAsync`,
  `VentaService.cs:391`), genera **EGRESO de stock + INGRESO de caja** y mueve la venta a
  `ComprobanteEmitido`.
- **`FacturaVenta`** (`SIGA.Domain/Entities/FacturaVenta.cs`): factura timbrada. Hoy es
  un paso **opcional posterior** (`VentaService.EmitirFacturaAsync`, `VentaService.cs:451`)
  que **no genera stock ni caja ni cambia estado** — solo guarda datos fiscales. Antes
  solo se emitía desde `/ventas/:id` (botón escondido).

Resultado: "factura" parece un agregado escondido e inerte sobre una venta que ya tiene
recibo. Ese es el flujo confuso a corregir.

## 2. Decisiones acordadas con el usuario

1. **No tocar el modelo de datos.** Se mantienen las dos entidades (`Comprobante` y
   `FacturaVenta`). **Sin migración de esquema.**
2. La óptica emite **ambos tipos** según el caso (Recibo Simple o Factura Timbrada).
3. La emisión es un **paso dedicado, separado del cobro**, accesible desde el detalle y
   desde Cobros Pendientes.
4. **Excluyente:** una venta tiene un único documento fiscal (recibo **o** factura).
5. **Efectos unificados:** emitir factura genera los mismos efectos que el recibo
   (EGRESO stock + INGRESO caja + estado `ComprobanteEmitido`), **sin duplicar** la caja
   si ya hubo cobros/recibo previo ("genera si no hay recibo previo").

> El modelo no tiene estado `ENTREGADA` (el cobro va antes del laboratorio). Se respeta:
> el estado terminal sigue siendo `ComprobanteEmitido` para ambos tipos.

## 3. Objetivo

Convertir la emisión del documento fiscal en **un paso dedicado** donde el cajero
**elige el tipo** (Recibo Simple / Factura Timbrada). Ambos tipos producen idénticos
efectos y son mutuamente excluyentes.

---

## 4. Pasos de implementación

### Backend (`SIGA`) — solo lógica, sin migración
Archivo: `src/SIGA.Infrastructure/Services/VentaService.cs`

- [x] **B1.** Helper privado `AplicarEgresosDeEmision(venta, now)` creado con EGRESO stock
  por `VentaLinea` tipo `Producto` + INGRESO caja contado sin cobros previos.
- [x] **B2.** `EmitirComprobanteAsync` (recibo): exclusión `venta.Factura != null` añadida;
  efectos reemplazados por el helper.
- [x] **B3.** `EmitirFacturaAsync` (factura): reescrito a emisión real → valida
  `PuedeEmitirComprobante()`; exclusión (`Comprobante == null && Factura == null`); inserta
  `FacturaVenta`; llama al helper; setea `ComprobanteEmitido` + `FechaComprobante` + `UpdatedAt`.
- [x] **B4.** `dotnet build SIGA\SIGA.sln` en verde (0 errores).

Endpoints/DTOs sin cambios: `POST /api/ventas/{id}/comprobante` y `POST /api/ventas/facturas`
(`EmitirFacturaRequest` ya trae numeroFactura, timbrado, establecimiento, fechaEmision,
observaciones). Policy `registrar_venta`.

### Frontend (`SIGA-Web`)

- [x] **F1.** Nueva pantalla `VentaComprobanteView.vue` creada en
  `/ventas/:id(\d+)/comprobante`. Selector Recibo Simple / Factura Timbrada; si factura,
  form con N° factura, timbrado, establecimiento, fecha, observaciones. Botón "Emitir" →
  `emitirComprobante(id)` o `emitirFactura({...})`. Al éxito → `/ventas/:id`. Operable solo
  si `ListaParaCobrar` y sin documento.
- [x] **F2.** `router/index.ts`: ruta `venta-comprobante` registrada junto a `venta-cobrar`.
- [x] **F3.** `VentaCobrarView.vue`: emisión quitada. Botón único "Registrar cobro"
  (`guardarCobro`) + botón "Emitir comprobante" (`goEmitir`) → `/ventas/:id/comprobante`.
  Import `emitirComprobante` removido.
- [x] **F4.** `VentaDetalleView.vue`: botón "Emitir comprobante" condicional (`puedeEmitirDoc`
  = `ListaParaCobrar` sin documento) → navega a `/ventas/:id/comprobante`. Eliminados botón
  "Emitir factura", su modal, `submitFactura`/`factForm`/estado e import `emitirFactura`.
- [x] **F5.** `CobrosPendientesView.vue`: acción "Emitir comprobante" agregada a `menuItems`.
- [x] **F6.** `npm --prefix SIGA-Web run type-check` en verde (exit 0).

## 5. Flujo resultante

```
ListaParaCobrar
   ├─ (Crédito) Pantalla de cobro → registrar seña/cuotas (caja)
   └─ Paso dedicado /ventas/:id/comprobante
        elegir tipo → Recibo Simple | Factura Timbrada
        → EGRESO stock + INGRESO caja (si no hubo cobros) + ComprobanteEmitido
```
- **Contado:** registra el pago (desglose de métodos) y luego emite; o emite directo (la
  emisión genera la caja por el total).
- **Crédito:** registra cobros y luego emite (la caja ya viene de los cobros; la emisión
  solo baja stock y cierra estado).

## 6. Verificación end-to-end (requiere reiniciar backend)

- [ ] **V1. Recibo/Contado:** venta directa Contado → confirmar → Cobros Pendientes →
  emitir Recibo Simple → venta en `ComprobanteEmitido`, INGRESO caja (total) y EGRESO
  stock por producto.
- [ ] **V2. Factura/Contado:** ídem con Factura Timbrada (datos fiscales) → mismos efectos
  y aparece en `Historial de Facturas` (`/ventas/facturas`).
- [ ] **V3. Crédito:** registrar seña (caja) → emitir documento → la emisión **no duplica**
  caja (solo egreso stock) y cierra estado.
- [ ] **V4. Exclusión:** emitir factura sobre venta con recibo (y viceversa) → rechazado
  con conflicto.

---

## 7. Registro de avance

> Anotar aquí fecha, qué se completó y cualquier desvío/decisión nueva.

- 2026-06-09 — Plan creado y aprobado. Documento materializado en el repo.
- 2026-06-09 — Backend B1–B3 implementados (`VentaService.cs`): helper compartido +
  factura con efectos reales + exclusión recibo/factura. Frontend F1–F6 completos y
  type-check en verde.
- 2026-06-09 — B4 OK: `dotnet build` en verde (0 errores). **Todo el código completo.**
  **Pendiente:** reiniciar `SIGA.Api` y correr la verificación manual V1–V4.
- 2026-06-09 — Fixes post-implementación en la pantalla de cobro (`VentaCobrarView.vue`):
  (1) validación de que el monto a cobrar no supere el saldo pendiente (botón "Registrar
  cobro" deshabilitado y mensaje cuando saldo ≤ 0); (2) el botón "Volver" ahora va a
  `/ventas/cobros-pendientes` (antes iba a `/ventas`). Backend: misma validación de saldo
  agregada en `RegistrarCobroAsync` (`VentaService.cs`). Front type-check en verde;
  build backend en verde (0 errores). Reiniciar `SIGA.Api` para que tome la validación.
- 2026-06-09 — Ajustes de navegación: sidebar `Ventas` reordenado (Lista de Ventas debajo
  de Nueva Venta) en `menuConfig.ts`; botón "Volver" de `VentaComprobanteView.vue` cambiado
  a `router.back()` para respetar el origen (Cobros Pendientes / detalle / cobro), ya que la
  pantalla es accesible desde varios lugares.
- 2026-06-09 — Más navegación: "Volver" de `VentaDetalleView.vue` → `router.back()`. En
  `VentasView.vue` (Lista de Ventas) se eliminó el modal intermedio de detalle: la fila ahora
  navega directo a `/ventas/:id` (removidos `BaseModal`/`BaseButton`/`openDetalle`/
  `condicionLabel`). Front type-check en verde.
