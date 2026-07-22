# Casos de Prueba y Vulnerabilidades — SIGA

> Documento de testing basado en revisión de código de los servicios núcleo
> (`VentaService`, `CajaService`, `ComprasService`, `LaboratorioService`, `TurnoService`)
> y entidades de dominio. Fecha de revisión: 2026-07-15.
>
> **Cómo usar:** cada caso ataca un hueco concreto detectado en el código. Reproducir,
> confirmar el comportamiento, y marcar `[x]` cuando esté **arreglado y re-verificado**.

## Leyenda de estado

- `[ ]` Pendiente de probar / sin arreglar
- `[~]` Confirmado (el bug se reproduce, falta arreglar)
- `[x]` Arreglado y re-verificado
- **Severidad:** 🔴 Crítica (plata/fiscal) · 🟠 Alta (datos/stock) · 🟡 Media (operativa) · 🔵 Baja (UX)

---

## Tabla resumen de vulnerabilidades

| ID | Severidad | Título | Archivo / Línea | Estado |
|----|-----------|--------|-----------------|--------|
| V1 | 🔴 | Precio de devolución sale del catálogo, no de la venta | `VentaService.cs:903` | ☑ (065) |
| V2 | 🔴 | Devolución de venta a crédito devuelve plata que nunca entró | `VentaService.cs:899-925` | ◐ — caja topada en lo cobrado; falta cancelar saldo a crédito (ver V7) |
| V3 | 🟠 | No se valida que lo devuelto haya sido vendido | `VentaService.cs:772-780` | ☑ (065) |
| V4 | 🔴 | Egreso de caja huérfano al devolver con caja cerrada | `VentaService.cs:909-924` | ☑ — se exige caja abierta antes de confirmar; cubierto por `DevolucionYCajaTests` |
| V5 | 🟠 | No hay validación de stock en toda la venta (stock negativo) | `VentaService.cs:641-654` | ☑ — `ValidarStockDisponibleAsync` en ambas emisiones; cubierto por `StockEnLaVentaTests` |
| V6 | 🟠 | El "Cambio" no cobra la diferencia ni valida stock del nuevo | `VentaService.cs:898-899` | ☐ |
| V7 | 🔴 | No existe nota de crédito; factura y estado quedan intactos tras devolución | `VentaService.cs:860-928` | ☑ (065) — se emite NC; la factura sigue vigente a propósito |
| V8 | 🔴 | No se puede anular un cobro (campo `Anulado` sin escritura) | `VentaService.cs` (falta método) | ☐ |
| V9 | 🔴 | Race condition en numeración fiscal de facturas | `VentaService.cs:588` | ☐ |
| V10 | 🟠 | Emisión de factura ignora `NumeroDesde` del timbrado | `VentaService.cs:588` | ☐ |
| V11 | 🟡 | El arqueo esconde descuadres negativos (`Math.Max(0m, ...)`) | `CajaService.cs:189` | ☐ |
| V12 | 🟡 | No existe estado "Ausente" para turnos (no-show) | `TurnoEstado.cs` | ☐ |
| V13 | 🟡 | Sin FIFO/FEFO: la venta no consume lotes ni respeta vencimiento | `VentaService.cs:641-654` | ☐ |
| V14 | 🟡 | Cierre de caja no advierte ventas pendientes de cobro | `CajaService.cs:172` | ☐ |
| V15 | 🔵 | Sin límite de auto-agendamiento de turnos por paciente | `TurnoService.cs` (SelfBook) | ☐ |

---

## BLOQUE A — Devoluciones y arrepentimiento del cliente

### A1 · "El armazón que subió de precio" — 🔴 (V1)
- [ ] **Estado**
- **Setup:** Vender a *Rosa Benítez* un armazón Ray-Ban a **450.000** (precio promo). Emitir comprobante. Luego editar el catálogo y subir ese armazón a **600.000**.
- **Acción:** Rosa devuelve el armazón. Registrar y confirmar la devolución.
- **Resultado esperado:** salen **450.000** de caja (lo que pagó).
- **Resultado actual (bug):** salen **600.000** — el precio se toma de `ProductoDevuelto.PrecioVenta` (catálogo actual), no de la `VentaLinea` original.
- **Fix propuesto:** usar el precio unitario guardado en la `VentaLinea` de la venta original.

### A2 · "La devolución del fantasma" — 🟠 (V3)
- [ ] **Estado**
- **Setup:** *Carlos Duarte* compró **1** par de lentes de contacto. Comprobante emitido.
- **Acción:** solicitar una devolución por **3** unidades de ese producto. Luego solicitar otra devolución de la misma venta por el mismo producto.
- **Resultado esperado:** ambas rechazadas (no se puede devolver más de lo vendido, ni acumular devoluciones que superen lo comprado).
- **Resultado actual (bug):** ambas aceptadas. `SolicitarDevolucionAsync` solo valida `CantidadDevuelta > 0`; no compara contra `venta.Lineas` ni contra devoluciones previas.
- **Fix propuesto:** validar que cada producto exista en la venta y que la cantidad acumulada devuelta ≤ cantidad vendida.

### A3 · "El que pagó la seña y se arrepintió" — 🔴 (V2) ⭐ prioridad
- [~] **Estado** — mitad hecha: el reintegro de caja ya está topado en lo cobrado (recibe 150.000, no 800.000). Falta "queda sin deuda": el saldo a crédito (650.000) todavía figura pendiente (estado de venta sin `Devuelta` — ligado a V7).
- **Setup:** *Mirta Rolón* compra a **crédito** por **800.000**, deja **150.000** de seña, se lleva la mercadería con factura emitida.
- **Acción:** a los 10 días devuelve todo. Confirmar la devolución.
- **Resultado esperado:** recibe **150.000** (lo efectivamente pagado) y queda sin deuda.
- **Resultado actual (bug):** recibe **800.000** en efectivo. El egreso de caja se calcula sobre el precio del producto, sin mirar `TotalCobrado`.
- **Fix propuesto:** el monto a devolver no puede superar lo efectivamente cobrado no anulado de la venta.

### A4 · "El cambio caro" — 🟠 (V6)
- [ ] **Estado**
- **Setup:** *Luis Ayala* tiene un armazón de **300.000** comprado (comprobante emitido).
- **Acción:** registrar un **Cambio** por un armazón de **900.000**.
- **Resultado esperado:** se genera un cobro por la diferencia (**600.000**); se valida stock del armazón nuevo.
- **Resultado actual (bug):** no se genera ningún cobro (comentario en código: *"precio pendiente de definir"*). Tampoco se valida stock del producto nuevo, que igual se descuenta.
- **Fix propuesto:** calcular diferencia de precios y registrar cobro/nota; validar stock del producto nuevo antes de confirmar.

### A5 · "La devolución a las 8 de la noche" — 🔴 (V4)
- [ ] **Estado**
- **Setup:** cerrar la caja de la sucursal. Dejar una devolución en estado Pendiente.
- **Acción:** con la caja cerrada, aprobar/confirmar la devolución (tipo Devolución, con monto > 0).
- **Resultado esperado:** bloqueado con "No hay caja abierta" (igual que `RegistrarCobroAsync`).
- **Resultado actual (bug):** se registra un `MovimientoCaja` con `SesionCajaId = null`. La plata sale pero no aparece en ningún arqueo.
- **Fix propuesto:** exigir sesión de caja abierta antes de confirmar una devolución con impacto en efectivo.

### A6 · "La factura que sigue viva" — 🔴 (V7)
- [ ] **Estado**
- **Setup:** venta con factura emitida por el total.
- **Acción:** devolver el 100% de la venta. Revisar reporte de ventas y reportes de IVA.
- **Resultado esperado:** la venta refleja la devolución (estado "Devuelta" o similar) y existe una nota de crédito que descuenta el IVA.
- **Resultado actual (bug):** la venta sigue `ComprobanteEmitido`, la `FacturaVenta` sigue computando el total, no hay nota de crédito. Fiscalmente inconsistente.
- **Fix propuesto:** generar nota de crédito y actualizar estado/reportes al confirmar la devolución.

### A7 · "El que pagó seña y se arrepintió antes de retirar" — 🔴 (gap nuevo)
- [x] **Estado** — implementado 2026-07-17.
- **Setup:** venta a **crédito** de un trabajo a pedido; el cliente paga solo la **seña**; la venta queda `ListaParaCobrar` (sin comprobante, sin producto entregado).
- **Problema detectado:** no había circuito. La *devolución* pide seleccionar productos (no tiene ninguno) y `PuedeCancelarse()` **no incluía** `ListaParaCobrar`, así que tampoco se podía cancelar. Cliente en limbo. Además `CancelarVentaAsync` no tocaba la seña.
- **Regla de negocio acordada:** cancelar ≠ devolver. La disposición de la seña depende del estado del trabajo:
  - Trabajo **ya enviado/recibido** del laboratorio → seña **NO reembolsable** (cristal a pedido ya fabricado; la seña cubre ese costo). Regla dura.
  - Trabajo **no enviado** (`PendienteAprobacion`/`PendienteEnvio`) o producto de **stock** → **reembolso total** (egreso de caja, requiere caja abierta; los cobros se anulan).
- **Implementado:** `PuedeCancelarse()` ahora acepta `ListaParaCobrar`; `CancelarVentaAsync` resuelve la seña según el estado del trabajo y deja rastro en `Observaciones`. Front: `VentaDetalleView` muestra la disposición en el modal de cancelar (aviso de reembolso vs. retención).
- **Complemento (mismo día):** para no caer en el limbo "comprobante emitido antes de entregar", un trabajo a pedido **no puede emitir comprobante/factura hasta estar `Recibido`** del laboratorio (`ValidarTrabajoListoParaEmitir` en `EmitirComprobanteAsync`/`EmitirFacturaAsync`). Así la venta sigue cancelable durante toda la fabricación. Productos de stock no se afectan. Front: botón "Emitir" oculto hasta que el trabajo esté Recibido.

---

## BLOQUE B — Stock inexistente

### B1 · "El último armazón, dos veces" — 🟠 (V5) ⭐ prioridad
- [~] **Estado** — **sigue abierto**: la validación agregada en V5 (`ValidarStockDisponibleAsync`) corta el caso secuencial (B2), pero no éste. Es una condición de carrera: dos transacciones concurrentes leen el stock, ambas ven 1 disponible, y ambas escriben su egreso. Cerrarlo requiere bloqueo pesimista sobre el stock del producto (`SELECT ... FOR UPDATE`) o una restricción en la base que impida el negativo — no alcanza con validar antes de escribir.
- **Setup:** dejar **1** unidad en stock del modelo Vulk.
- **Acción:** dos operadores (o dos pestañas) crean, confirman y emiten comprobante sobre ese producto casi simultáneamente.
- **Resultado esperado:** uno de los dos falla por stock insuficiente.
- **Resultado actual (bug):** ambos pasan. Stock final: **-1**. No hay validación ni reserva de stock en la venta.
- **Fix propuesto:** validar stock disponible al emitir; idealmente reservar al confirmar.

### B2 · "La venta sin mercadería" — 🟠 (V5)
- [x] **Estado** — arreglado y cubierto por `StockEnLaVentaTests`
- **Setup:** producto con **2** unidades en stock.
- **Acción:** vender **10**. Confirmar. Emitir factura.
- **Resultado esperado:** bloqueo por stock insuficiente en algún paso.
- **Resultado actual (bug):** ningún paso frena; el negativo aparece recién en el reporte de inventario.

### B3 · "El lote vencido" — 🟡 (V13)
- [ ] **Estado**
- **Setup:** cargar lentes de contacto con dos lotes: uno que vence la próxima semana, otro el año que viene.
- **Acción:** vender una caja.
- **Resultado esperado:** el sistema consume el lote que vence antes (FEFO) y deja trazabilidad del lote entregado.
- **Resultado actual (bug):** la salida de stock (`MovimientoStock`) no referencia lote; no hay FIFO/FEFO en la venta. Riesgo de vender vencido sin saberlo.

---

## BLOQUE C — Caja que no cierra

### C1 · "El cajero con dedos gordos" — 🔴 (V8) ⭐ prioridad
- [ ] **Estado**
- **Setup:** *Andrea* registra un cobro en efectivo de **1.500.000** cuando eran **150.000**.
- **Acción:** intentar anular/corregir el cobro.
- **Resultado esperado:** poder anular el cobro; la caja se recalcula.
- **Resultado actual (bug):** no existe método que ponga `Cobro.Anulado = true`. El campo se usa en todos los cálculos pero nada lo escribe. La única salida es tocar la base a mano.
- **Fix propuesto:** implementar `AnularCobroAsync` que marque el cobro y genere el `MovimientoCaja` reverso.

### C2 · "El cheque que rebotó" — 🔴 (V8)
- [ ] **Estado**
- **Setup:** cobrar una cuota con cheque.
- **Acción:** 3 días después el banco rechaza el cheque; intentar revertir.
- **Resultado esperado:** poder anular el cobro; la venta vuelve a mostrar saldo pendiente.
- **Resultado actual (bug):** mismo problema que C1; la venta figura como pagada y la caja como acreditada.

### C3 · "El cierre con el mostrador lleno" — 🟡 (V14)
- [ ] **Estado**
- **Setup:** 4 ventas en estado `ListaParaCobrar`, clientes esperando.
- **Acción:** cerrar la caja.
- **Resultado esperado:** advertencia de ventas pendientes de cobro antes de cerrar.
- **Resultado actual (bug):** `CerrarSesionAsync` no chequea ventas pendientes; tras cerrar nadie puede cobrar hasta reabrir.

### C4 · "El arqueo rechazado dos veces" — 🟡 (V11 relacionado)
- [ ] **Estado**
- **Setup:** cajero cierra con **200.000** de faltante → admin rechaza → sesión vuelve a Abierta.
- **Acción:** cajero vuelve a cerrar con el mismo faltante.
- **Verificar:** que el `MotivoRechazo` anterior no quede pegado indebidamente, y que el historial deje rastro de ambos intentos (que el segundo cierre no pise silenciosamente al primero).

### C5 · "El descuadre negativo escondido" — 🟡 (V11)
- [ ] **Estado**
- **Setup:** provocar que `EfectivoEsperado` dé negativo (más egresos en efectivo que ingresos + monto inicial).
- **Acción:** cerrar la caja.
- **Resultado esperado:** el descuadre real se refleja.
- **Resultado actual (bug):** `diferencia = EfectivoContado - Math.Max(0m, efectivoEsperado)` trata el esperado negativo como 0 y disimula el descuadre.

---

## BLOQUE D — Facturación fiscal

### D1 · "Las gemelas" — 🔴 (V9)
- [ ] **Estado**
- **Setup:** un timbrado activo.
- **Acción:** dos usuarios emiten factura simultáneamente sobre el mismo timbrado.
- **Resultado esperado:** números correlativos distintos.
- **Resultado actual (bug):** `UltimoNumero + 1` sin lock → dos facturas con el mismo número. Problema con Hacienda.
- **Fix propuesto:** lock/transacción atómica o secuencia a nivel base de datos.

### D2 · "El talonario que arranca en 1000" — 🟠 (V10)
- [ ] **Estado**
- **Setup:** timbrado con `NumeroDesde = 1000` y `UltimoNumero = 0`.
- **Acción:** emitir la primera factura.
- **Resultado esperado:** numerada **0001000**.
- **Resultado actual (bug):** sale **0000001** (ignora `NumeroDesde`).
- **Fix propuesto:** el primer número = `max(UltimoNumero + 1, NumeroDesde)`.

### D3 · "El timbrado que se vence el martes" — 🔵 (control, ya validado)
- [ ] **Estado**
- **Setup:** timbrado que vence hoy.
- **Acción:** emitir factura con fecha de mañana.
- **Resultado esperado:** bloqueado (línea 583 ya lo valida).
- **Verificar:** que el mensaje sea claro y el vendedor sepa qué hacer. Caso de confirmación, no de bug.

---

## BLOQUE E — Laboratorio y trabajo a pedido

### E1 · "El cristal que llegó mal graduado" — 🟠
- [ ] **Estado**
- **Setup:** *Pedro Giménez* pagó su multifocal completo; venta facturada. El laboratorio devuelve el cristal con graduación equivocada.
- **Verificar:** cómo se rehace el trabajo. `EstadoTrabajoPedido.Rechazado` existe pero la venta ya está cobrada/facturada. ¿Hay re-envío? ¿Se re-factura al laboratorio? ¿Quién asume el costo?
- **Gap:** no hay circuito de re-trabajo definido.

### E2 · "El armazón del cliente roto en el laboratorio" — 🟠
- [ ] **Estado**
- **Setup:** trabajo con `ArmazonDelCliente = true`. El laboratorio quiebra el armazón al montar.
- **Verificar:** no existe circuito para registrar la rotura ni la responsabilidad. Pasivo real de la óptica sin cobertura en el sistema.

### E3 · "El que nunca vino a retirar" — 🟡
- [ ] **Estado**
- **Setup:** trabajo recibido del laboratorio hace 4 meses; el cliente no aparece.
- **Verificar:** ¿existe reporte de trabajos sin retirar o alguna alerta? Gap probable.

---

## BLOQUE F — Turnos

### F1 · "El que no vino" — 🟡 (V12)
- [ ] **Estado**
- **Setup:** *Silvia* tiene turno a las 10:00 y no aparece.
- **Verificar:** no hay estado "Ausente"; el turno queda Pendiente indefinidamente. Sin métrica de no-show. Revisar impacto en la agenda del día siguiente.
- **Fix propuesto:** agregar `TurnoEstado.Ausente`.

### F2 · "El profesional que se enfermó" — 🟡
- [ ] **Estado**
- **Setup:** el Dr. Rojas avisa a las 7 AM que no viene; tiene 12 turnos ese día.
- **Verificar:** ¿hay cancelación masiva o son 12 acciones con 12 aprobaciones? Gap operativo probable.

### F3 · "El paciente que reserva 5 turnos" — 🔵 (V15)
- [ ] **Estado**
- **Setup:** desde `MisTurnosView`, el mismo paciente se auto-agenda 5 veces en la misma semana.
- **Verificar:** ¿hay límite o validación de duplicados en `SelfBookAsync`? Gap probable.

---

## Orden de ataque recomendado

1. **A3** (V2) — devolución de venta a crédito devuelve de más. Plata real, fix acotado.
2. **C1** (V8) — cobro mal cargado sin anulación posible. Bloqueante operativo.
3. **B1** (V5) — stock negativo por falta de validación/reserva.
4. **A1** (V1) — precio de devolución del catálogo.
5. **D1/D2** (V9/V10) — integridad fiscal de la numeración.

---

## Mejoras de flujo implementadas (no eran bugs)

- **[x] Cola global de aprobación de devoluciones** (2026-07-15). Antes solo se podían
  gestionar devoluciones abriendo la venta específica; no había una vista centralizada.
  - Backend: `GetDevolucionesPendientesAsync()` en `VentaService` + endpoint
    `GET /api/ventas/devoluciones/pendientes` (policy `gestionar_ventas`).
    `DevolucionDto` extendido con `ClienteNombre`.
  - Frontend: vista `DevolucionesPendientesView.vue`, ruta `/ventas/devoluciones`,
    item de menú **Ventas → Transacciones → Devoluciones**.
  - Nota: el flujo de *solicitud* y *gestión por venta* ya existía en `VentaDetalleView.vue`
    (botón "Devolver / Cambiar" visible cuando la venta está en `ComprobanteEmitido`).

## Deuda técnica detectada de paso

- **[x] Catch inefectivo en `VentaDetalleView.vue`** (resuelto 2026-07-16): los bloques de
  devolución/gestión usaban `e?.response?.data?.message` y siempre caían al fallback genérico.
  Migrados a `e instanceof Error ? e.message : "..."`, así ahora se muestra el mensaje explícito
  del backend (ej. "No se pueden devolver 5 unidades: la venta solo incluye 3").

---

## Registro de avance

| Fecha | Caso/V | Acción | Resultado |
|-------|--------|--------|-----------|
| 2026-07-15 | — | Cola global de devoluciones (backend + front) | ✅ Implementado, compila y typecheck OK |
| 2026-07-16 | V3 | Validación: no devolver más unidades de las vendidas + producto debe pertenecer a la venta | ✅ `SolicitarDevolucionAsync` |
| 2026-07-16 | — | Regla: una sola devolución por venta (una rechazada no bloquea) | ✅ `SolicitarDevolucionAsync` |
| 2026-07-16 | V1 | Montos de devolución calculados desde la `VentaLinea` original, no del catálogo | ✅ `CalcularMontosDevueltos` |
| 2026-07-16 | V7 | Nota de Crédito fiscal al confirmar (migración 065): entidad `NotaCredito`, `Timbrado.Tipo` (Factura/NotaCredito), timbrado propio de NC. La factura NO se anula. Solo si la venta tenía factura. | ✅ Backend + front (timbrados y detalle de venta), compila y typecheck OK |
| 2026-07-16 | UX | Devoluciones: mensaje de error explícito (fix catch inefectivo), botón "Devolver" deshabilitado si ya hay devolución, stepper de cantidad topeado en lo vendido | ✅ `VentaDetalleView.vue`, typecheck OK |
| 2026-07-16 | UX | "Facturas de Venta" → **Comprobantes de Venta**: lista unificada factura + NC, filtro por tipo de documento, acción "Visualizar comprobante" (preview PDF en iframe) + descargar. PDF de NC nuevo. | ✅ `FacturasVentaView.vue` + `useFacturaVentaPdf.ts`, typecheck OK |
| 2026-07-16 | Flujo | Egresos de 3 a **2 pasos**: se elimina la aprobación (era sello de goma para un cajero solo). Todos nacen `Pendiente` → `Registrar pago` → `Pagado`. Seguridad movida al pago: modal de confirmación con concepto+monto+origen. Anular = deshacer. `RegistrarPago` acepta Pendiente (o Aprobado legacy). | ✅ Backend (Egreso, EgresoService, controller, interface) + front (borrada `AprobacionEgresosView`, ruta/menú, pago directo), typecheck OK, Infrastructure compila 0 err |
| 2026-07-17 | UX | Egresos: se quita el método de pago del **alta** (se descartaba, nunca se enviaba) — queda sólo en "Registrar pago", donde el dinero realmente sale. Caja: movimientos ordenados **más reciente primero** en caja abierta e historial. | ✅ Front (`NuevoEgresoView`, `VentasCierreView`, `CajaHistorialView`), typecheck OK |
| 2026-07-17 | V2 | Reintegro de devolución topado en `Venta.TotalCobrado` (cobros no anulados): `montoReintegro = min(valor mercadería, cobrado)`. La NC sigue emitiéndose por el valor de la mercadería (revierte la factura). Corrige la fuga de venta a crédito con seña. | ✅ Backend `GestionarDevolucionAsync` (+ include `Cobros`), Infrastructure compila 0 err. **Pendiente**: cancelar el saldo a crédito remanente (estado `Devuelta` / V7) |
| 2026-07-17 | UX | `VentaCobrarView`: arreglado el default muerto del tipo de cobro (dependía de `EnProceso`, que nunca se setea). Ahora primer cobro de venta a crédito = **Seña**, siguientes = Cuota. | ✅ Front, typecheck OK |
| 2026-07-17 | A7 | **Cancelar pedido con seña** (gap nuevo): `PuedeCancelarse()` acepta `ListaParaCobrar`; `CancelarVentaAsync` dispone la seña según el trabajo — retiene si ya se envió/recibió del lab (regla dura), reembolsa (egreso de caja + anula cobros) si no. Front muestra la disposición en el modal. | ✅ Backend (`Venta`, `CancelarVentaAsync`) + front (`VentaDetalleView`, catch arreglado), compila 0 err + typecheck OK |
| 2026-07-17 | A7+ | Trabajo a pedido: **no se puede emitir comprobante/factura hasta estar `Recibido`** del lab (`ValidarTrabajoListoParaEmitir`). Evita el limbo "comprobante emitido antes de entregar" (no cancelable + sin producto que devolver). Stock no afectado. | ✅ Backend (`EmitirComprobanteAsync`/`EmitirFacturaAsync`) + front (`puedeEmitirDoc` gateado), compila 0 err + typecheck OK |
