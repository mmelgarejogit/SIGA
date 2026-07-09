# ADR 0007: El cristal graduado ya no se modela como Producto con stock

## Estado
Aceptada (implementada) — supersede un diseño anterior

## Contexto
El diseño original del catálogo óptico modelaba el cristal graduado como un `Producto` más, distinguido por `CategoriaProducto.Tipo = Cristal`, con `TrabajoPedido.CristalProductoId` como FK. En la práctica, un cristal a pedido no tiene stock ni SKU fijo: se fabrica según receta, diseño de lente y tratamientos elegidos por el cliente, por lo que forzarlo a la misma tabla que armazones con stock real generaba fricción de modelado (¿qué significa "stock" de un cristal que no existe hasta fabricarse?).

## Decisión
El cristal se especifica hoy directamente en `TrabajoPedido` mediante `TipoLente` (clasificación de diseño: monofocal/bifocal/progresivo/ocupacional) + una colección de `Tratamiento`s + precio, **sin FK a `Producto`**. `TrabajoPedido.CristalProductoId` fue removido del modelo. `TipoCategoriaProducto.Cristal` se mantiene en el enum pero está marcado `[Obsoleto]` explícitamente en el propio código (se conserva por compatibilidad de datos existentes, no se ofrece al dar de alta categorías nuevas). En `VentaLinea`, `TipoLineaVenta.Lente` identifica la línea del cristal a pedido, que no descuenta stock.

## Consecuencias
**Gana:** el cristal a pedido se modela con su semántica real (especificación de fabricación, no inventario), sin forzar un `Producto`/SKU ficticio por cada combinación de diseño + tratamiento.

**Pierde:** cualquier dato ya cargado bajo el diseño viejo (`CategoriaProducto.Tipo = Cristal`) queda como dato legado sin ofrecerse más en altas nuevas; quien documente o mantenga este módulo tiene que saber explícitamente que documentación/memoria de proyecto anterior a 2026-07-08 describe el diseño viejo (`CristalProductoId`) y ya no es vigente.

## Referencias
- `../schema.md` — Grupo B, nota "Catálogo óptico"
- `../modules/08-ventas.md`, `../modules/09-laboratorio.md` (pendientes, Fase 4)
