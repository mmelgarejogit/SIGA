# Catálogo — Datos nuevos + borrado real

> Documento de progreso. Trabajo derivado del replanteo del modelo de lentes
> ([ventas-modelo-lentes.md](ventas-modelo-lentes.md)).

**Estado general:** 🟢 Implementado (backend + frontend en verde). Falta ejecutar el
reset de datos y verificar.
**Última actualización:** 2026-06-13.

---

## 1. Objetivo

1. **Borrado real** (hard delete) de producto, categoría y tipo de lente, además del
   "Desactivar" existente.
2. **Datos nuevos** coherentes con el modelo actual (lentes = especificación, no producto).
3. **Resetear** los datos de catálogo de prueba para sembrar los nuevos.

## 2. Decisiones (2026-06-13)

- **Borrado:** seguro — rechaza si el registro está en uso (movimientos, ventas, trabajos a
  pedido, productos en la categoría) y sugiere desactivar. Convive con "Desactivar".
- **Reset:** script SQL acotado al catálogo + ventas/trabajos/movimientos de prueba
  (conserva usuarios, pacientes, egresos, caja general).
- **Stock:** productos de ejemplo con stock inicial (movimiento de ingreso aprobado).

## 3. Backend

- [x] **Borrado real con validación de uso:**
  - `ProductoService.DeleteAsync` (rechaza si hay movimientos / ventas / armazón en trabajos;
    red de seguridad `DbUpdateException`) + `DeleteCategoriaAsync` (rechaza si hay productos).
  - `TipoLenteService.DeleteAsync` (rechaza si está en trabajos a pedido).
  - Interfaces `IProductoService` / `ITipoLenteService` ampliadas.
  - Endpoints `DELETE /api/productos/{id}/permanente`,
    `DELETE /api/productos/categorias/{id}/permanente`,
    `DELETE /api/tipos-lente/{id}/permanente` (policy `gestionar_inventario`).
- [x] **Datos nuevos (seeders):**
  - `DbSeeder`: categorías con su `Tipo` (Marcos / Monturas Infantiles = Armazón; resto
    Genérico); tipos de lente como diseños con `PrecioBase` (Monofocal/Bifocal/Progresivo/
    Ocupacional); marcas/modelos sin cristales (Essilor, Hoya, Zeiss, Transitions).
  - `DevDataSeeder`: `ProductosSeed` sin "Lentes Oftálmicos"; agregados Monturas Infantiles
    y Lentes de Sol; `SeedInventarioAsync` ahora es idempotente (`if Productos.Any() return`)
    y corre siempre, así se repuebla tras el reset aunque ya haya pacientes.
- [x] **Build** de la solución en verde.

## 4. Frontend

- [x] `inventarioService`: `deleteProducto`, `deleteCategoria`, `deleteTipoLente`.
- [x] `ProductosView`, `CategoriasProductoView`, `TipoLentesView`: acción "Eliminar"
  (icono `delete_forever`, destructiva) + modal de confirmación que muestra el error del
  backend si el registro está en uso.
- [x] `type-check` en verde.

## 5. Reset de datos — pendiente de ejecutar

- [ ] **Con la API detenida**, ejecutar `scripts/reset-catalogo.sql` (DELETE ordenados en
  transacción; ver el encabezado del archivo).
- [ ] Reiniciar la API → los seeders repueblan el catálogo nuevo.

## 6. Verificación

- [ ] Catálogo → Categorías: aparecen las nuevas con su tipo; "Marcos"/"Monturas Infantiles"
  como Armazón.
- [ ] Catálogo → Productos: armazones, lentes de contacto, soluciones, accesorios, monturas
  infantiles y lentes de sol, con stock inicial.
- [ ] Catálogo → Tipos de Lente: Monofocal/Bifocal/Progresivo/Ocupacional con precio base.
- [ ] Venta a pedido: el armazón aparece en el buscador (tipo Armazón) y el lente se arma por
  diseño + precio.
- [ ] Botón "Eliminar": borra un registro sin uso; rechaza con mensaje uno en uso.

---

> **Auditado 2026-07-08:** el backend confirmado contra código — `DELETE
> /api/productos/{id}/permanente`, `DELETE /api/productos/categorias/{id}/permanente`
> (`ProductosController.cs`) y `DELETE /api/tipos-lente/{id}/permanente`
> (`TipoLentesController.cs`) existen tal cual se describe. El checklist de la sección 5
> (reset de datos) y 6 (verificación manual) no se pudo reconfirmar en esta pasada — son
> pasos de ejecución puntual en un entorno de dev, no estado de código. Contenido de fondo
> vigente; mencionado en [modules/07-inventario-catalogo.md](./modules/07-inventario-catalogo.md).
