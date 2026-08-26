# ADR 0002: Stock derivado de movimientos aprobados, no un campo mutable

## Estado
Aceptada (implementada)

## Contexto
El modelo original tenía `StockActual`/`StockMinimo`/`StockMaximo` como campos directos de `Producto`, lo que hacía imposible auditar de dónde salió cada cambio de stock ni soportar múltiples sucursales sin ambigüedad sobre a qué sucursal pertenecía ese número.

## Decisión
`Producto` ya no tiene campos de stock. El stock se deriva sumando `MovimientoStock` (`Entrada` +, `Salida` −) filtrados por `Estado = "Aprobado"`, expuesto vía la vista SQL `vw_stock_actual` (mapeada como entidad de solo lectura `StockActualView`, `HasNoKey()`), agrupada por producto + sucursal. Todo servicio que antes tocaba el stock directamente ahora crea un `MovimientoStock` en su lugar. Mín/máx de stock quedaron en una tabla separada 1:1 (`ProductoStockConfig`), hoy global (no por sucursal) por decisión explícita para no romper la relación 1:1 durante la migración multi-sucursal.

## Consecuencias
**Gana:** trazabilidad completa de cada cambio de stock (quién, cuándo, por qué motivo vía `MotivoMovimiento`); soporte natural para stock por sucursal sin duplicar `Producto`; movimientos pueden pasar por un estado `Pendiente`/`Rechazado` sin afectar el stock real hasta ser `Aprobado`.

**Pierde:** leer el stock actual siempre requiere una agregación (vista SQL) en vez de un simple `SELECT` de un campo; hay que mantener disciplina de que absolutamente ningún write-path modifique stock sin pasar por `MovimientoStock` — una excepción rompería la trazabilidad silenciosamente.

## Referencias
- `../schema.md` — Grupo B (`MovimientoStock`, `StockActualView`, `ProductoStockConfig`)
- `../modules/07-inventario-catalogo.md` (pendiente, Fase 4)
