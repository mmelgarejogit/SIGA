# ADR 0004: Precio de venta siempre derivado del margen de categoría

## Estado
Aceptada (implementada)

## Contexto
El formulario de producto no exponía el precio de venta y el campo quedaba en 0, forzando a tipear el precio a mano en cada línea de venta — inconsistente entre vendedores y propenso a error.

## Decisión
`Producto.PrecioVenta` se calcula siempre como `round(PrecioCosto × (1 + CategoriaProducto.Margen/100))`, centralizado en el método de dominio `Producto.AplicarCosto(costo, margen)`. Se llama desde todo write-path que toca el costo: alta/edición de producto, recepción de mercadería, y actualización del margen de una categoría (que recalcula todos los productos de esa categoría). El campo `PrecioVenta` de los DTOs de request quedó sin uso — el backend es la única fuente de verdad. En la venta, el precio autocompleta desde `producto.PrecioVenta` pero queda editable por línea para un ajuste puntual.

## Consecuencias
**Gana:** consistencia de márgenes garantizada por diseño; un solo lugar (el margen de la categoría) para ajustar la rentabilidad de toda una línea de productos a la vez.

**Pierde:** no hay forma de fijar un precio de venta "manual" de catálogo sin editar el margen de la categoría o el costo del producto — decisión explícita, no un descuido, pero limita casos de pricing por producto individual fuera del margen estándar salvo el ajuste manual por línea de venta. Deuda técnica conocida: renombrar una categoría no re-vincula `Producto.Categoria` (campo string legado) con el nuevo nombre, dejando productos huérfanos de su margen.

## Referencias
- `../schema.md` — Grupo B (`Producto`, `CategoriaProducto`)
- `../modules/07-inventario-catalogo.md` (pendiente, Fase 4)
