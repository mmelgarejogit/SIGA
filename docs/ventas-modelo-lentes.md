# Ventas — Replanteo del modelo de lentes (cristales a pedido)

> **Documento de definición / progreso vivo.** Marcar cada paso (`[ ]` → `[x]`) a
> medida que se completa y anotar en el "Registro de avance" al final, para que
> cualquier modelo/persona pueda retomar el trabajo sin perder contexto.

**Estado general:** 🟢 Implementado — backend + frontend + migración listos y en verde. Falta verificación end-to-end (V1–V4) al reiniciar la API.
**Última actualización:** 2026-06-13.

---

## 1. Contexto y problema

Un lente graduado (trabajo a pedido) se arma hoy con **cuatro ejes que se solapan**:

1. **`CategoriaProducto` con `Tipo = Cristal`** (`TipoCategoriaProducto.cs`,
   `CategoriaProducto.cs`) — el cristal es un `Producto` del catálogo. Acá viven
   categorías reales inconsistentes como "lentes fotocromáticos" y "lentes oftálmicos".
2. **`TipoLente`** — entidad ABM aparte (monofocal/bifocal/progresivo). Es
   `TrabajoPedido.TipoLenteId`, etiquetado como "Clasificación (opcional)".
3. **`Tratamiento`** — entidad ABM aparte (antirreflejo, fotocromático…). N:N con el
   trabajo a pedido; se cobran como líneas de servicio.
4. **`Producto`** base — nombre, SKU, precio y **stock**.

Problemas concretos:

- **Ejes mezclados.** "Fotocromático" existe como **tratamiento** y también como nombre
  de **categoría de cristal**. "Oftálmico/bifocal" pertenecen a **TipoLente** pero se usan
  como categorías. El usuario termina metiendo en "categoría" lo que el modelo ya separa
  en *diseño* (TipoLente) + *tratamiento*.
- **Stock fantasma.** El cristal es un `Producto` y al emitir el comprobante se le hace
  **EGRESO de stock** (`VentaService.AplicarEgresosDeEmision` descuenta por cada línea de
  producto; el cristal se materializa como línea de producto en `optica.ts`). Pero los
  cristales **no se stockean** — se piden al laboratorio. Se descuenta inventario inexistente.

## 2. Decisiones tomadas (2026-06-13)

1. **El cristal deja de ser un `Producto`.** Pasa a ser una **especificación** dentro del
   trabajo a pedido: **diseño** (`TipoLente`) + **tratamientos** (`Tratamiento`) + **precio**,
   sin tocar inventario.
2. **Sin eje "Material".** Solo dos ejes: diseño (TipoLente) y tratamientos. El material,
   si hace falta, va en el nombre/observación.
3. **Lentes de contacto: fuera de alcance por ahora** (se definirán en otra iteración como
   producto de mostrador con stock).

### Modelo objetivo

```
Trabajo a pedido
  Armazón:     Producto con stock   (categoría tipo Armazón)  · opcional (o "del cliente")
  Lente:
    Diseño:       Bifocal            (TipoLente, ahora obligatorio en a pedido)
    Tratamientos: Fotocromático, Antirreflejo   (Tratamiento N:N)
    Precio:       450.000            (línea de venta, sin stock)
  → el lente NO descuenta stock; se pide al laboratorio (FacturaLaboratorio ya existe)
```

Resultado: desaparece `CategoriaProducto` tipo Cristal, "fotocromático" queda solo como
tratamiento, "bifocal/progresivo" queda solo como TipoLente, y las categorías de producto
se reservan para lo que tiene stock (armazones, accesorios, y a futuro lentes de contacto).

## 3. Decisiones derivadas

- **D1. Precio del lente. ✅ Confirmado:** `TipoLente` lleva un `PrecioBase` (decimal) que
  **autocompleta** el precio del lente al elegir el diseño en la venta, **editable** caso
  por caso.
- **D2. Representación de la línea. ✅ Confirmado:** se agrega `TipoLineaVenta.Lente = 2`.
  La línea del lente se crea **sin `ProductoId`** (no descuenta stock) y queda distinguible
  de productos y servicios clínicos en reportes.
- **D3. Datos existentes. ✅ Confirmado (dev):** entorno de desarrollo, datos descartables.
  No hace falta migración de datos: la migración EF dropea `CristalProductoId` directamente
  y los datos de prueba de cristales se limpian/ignoran.

## 4. Backend (`SIGA`) — pasos

> Requiere **migración de esquema** y **migración de datos**. Recordar resolver
> `AppDbContextModelSnapshot.cs` si se mergea con otras ramas con migraciones.

- [x] **B1.** `TipoLente.cs`: agregado `decimal PrecioBase` (default 0); expuesto en
  `TipoLenteDto` + Create/Update requests y en `TipoLenteService` (ToDto/Create/Update).
- [x] **B2.** `TipoLineaVenta.cs`: agregado `Lente = 2`.
- [x] **B3.** `TrabajoPedido.cs`: eliminados `CristalProductoId` + `CristalProducto`. El
  diseño queda en `TipoLenteId` (obligatorio al confirmar a pedido). `TrabajoPedidoConfiguration.cs`
  sin la FK al producto cristal.
- [x] **B4.** `CrearVentaRequest.cs` (`CrearVentaTrabajoPedidoRequest`): quitado
  `CristalProductoId`. El lente llega como línea tipo `Lente` con precio; el `TipoLenteId`
  viaja en el trabajo a pedido.
- [x] **B5.** `VentaService.cs`: `BaseQuery` sin `Include(CristalProducto)`; `CrearVentaAsync`
  ya no setea `CristalProductoId`; `Map`/`MapTrabajoPedidoDto` sin cristal. `ConfirmarVentaAsync`
  valida `TipoLenteId` presente para a pedido. `AplicarEgresosDeEmision` no cambia (ignora
  líneas sin `ProductoId`, así el lente no descuenta stock).
- [x] **B6.** DTOs `TrabajoPedidoDto.cs` / `TrabajoPedidoListDto.cs`: sin `CristalProducto*`
  ni `CristalNombre`; el lente se describe por `TipoLenteNombre` + tratamientos.
- [x] **B7.** `LaboratorioService.cs`: sin `Include`/usos de `CristalProducto`; `TipoLenteNombre`
  ya no cae al nombre del cristal.
- [x] **B8.** `TipoCategoriaProducto.cs`: `Cristal` marcado obsoleto (valor conservado por
  compatibilidad). Sin migración de esquema (enum int).
- [x] **B9.** `DbSeeder.cs`: el seed de categorías ya no incluye categorías de lente
  (Oftálmicos/Progresivos/Bifocales/Antirreflejo/Filtros/Fotocromáticos).
- [x] **B10.** Migración EF `20260614025057_046_ModeloLentes`: drop de FK + índice + columna
  `CristalProductoId` en `trabajos_pedido` y add `PrecioBase` en `tipos_lente`. Se aplica
  sola al arrancar (`Program.cs` → `MigrateAsync()`).
- [x] **B11.** `dotnet build SIGA\SIGA.sln` en verde (0 errores).

## 5. Frontend (`SIGA-Web`) — pasos

- [x] **F1.** `composables/optica.ts`: `OpticaState` sin `cristal`/`cristalPrecio`; ahora
  `tipoLenteId` + `tipoLenteNombre` + `lentePrecio`. `opticaLineas` emite el lente como línea
  **tipo `Lente` sin `productoId`** (descripción "Lente {diseño}"). `opticaTrabajoPedido` sin
  `cristalProductoId`.
- [x] **F2.** `TrabajoOpticoCard.vue`: buscador de cristal reemplazado por selector de
  **diseño** (TipoLente) + input de **precio** autocompletado por `PrecioBase`. Bloque
  "Clasificación (opcional)" fusionado. Sin carga `getProductos({ tipoCategoria: "Cristal" })`.
- [x] **F3.** `VentaEditor.vue`: usa el nuevo `OpticaState` vía el composable; textos "cristal"
  → "lente".
- [x] **F4.** `services/ventasService.ts` + `inventarioService.ts`: sin `cristalProductoId`/
  `cristalNombre`; `TipoLinea` incluye `"Lente"`; `TipoLente` con `precioBase`. ("Cristal" se
  conserva en el union de `TipoCategoriaProducto` para datos legacy.)
- [x] **F5.** `TrabajosPedidoView` y `VentaDetalleView`: el lente se muestra por diseño
  (TipoLente). (`*Aprobacion`/`*Recepciones`/`*Facturas` no referenciaban el cristal.)
- [x] **F6.** `CategoriasProductoView.vue`: "Cristal" fuera de las opciones de alta/edición
  (badge legacy conservado).
- [x] **F7.** `TipoLentesView.vue`: campo `PrecioBase` agregado al ABM (columna + modales).
- [x] **F8.** `npm --prefix SIGA-Web run type-check` en verde.

## 6. Migración de datos

- [ ] **M1.** Desactivar (`IsActive = false`) los `Producto` de categorías tipo Cristal y
  esas `CategoriaProducto`.
- [ ] **M2.** Trabajos a pedido históricos con `CristalProductoId`: antes del drop (B10),
  derivar `TipoLenteId` (si está vacío) y crear/ajustar la línea de lente con el precio.
  En entorno de desarrollo se puede omitir y limpiar datos de prueba.

## 7. Verificación end-to-end

- [ ] **V1.** Crear presupuesto a pedido: elegir armazón (stock) + diseño + tratamientos +
  precio del lente. El lente aparece como línea sin afectar inventario.
- [ ] **V2.** Confirmar → emitir comprobante: EGRESO de stock **solo** del armazón; el lente
  no descuenta stock. Caja/total correctos.
- [ ] **V3.** El alta de categorías ya no ofrece tipo "Cristal"; las categorías cristal
  viejas quedan inactivas y no aparecen en los buscadores.
- [ ] **V4.** Orden al laboratorio / factura de lab describe el lente por diseño +
  tratamientos.

## 8. Registro de avance

> Anotar fecha, qué se completó y cualquier desvío/decisión nueva.

- 2026-06-13 — Definición creada. Decisiones tomadas: cristal = especificación sin stock,
  sin eje material, lentes de contacto fuera de alcance.
- 2026-06-13 — Confirmadas D1 (PrecioBase en TipoLente, editable) y D2 (TipoLineaVenta.Lente).
- 2026-06-13 — D3 = dev (datos descartables, sin migración de datos). Implementado el código
  backend (B1–B9) y el frontend (F1–F8, type-check en verde).
- 2026-06-13 — Generada la migración `046_ModeloLentes` y build de la solución en verde (B10/B11).
  La migración se auto-aplica al arrancar la API. **Pendiente:** reiniciar la API y correr la
  verificación end-to-end V1–V4.

> **Auditado 2026-07-08:** contenido vigente, confirmado contra código — es la fuente original de [ADR 0007](./adr/0007-cristal-ya-no-es-producto-con-stock.md). Fusionado en [modules/08-ventas.md](./modules/08-ventas.md). Este archivo se mantiene por referencia histórica.
