# ADR 0005: TrabajoPedido nace en Borrador junto al presupuesto

## Estado
Aceptada (implementada)

## Contexto
El flujo de venta "a pedido" (armazón + cristal + tratamientos + laboratorio) necesita capturarse desde el momento del presupuesto, antes de que el cliente confirme la compra — pero no debía aparecer en la cola de trabajo del laboratorio hasta que la venta esté efectivamente confirmada.

## Decisión
`EstadoTrabajoPedido.Borrador` (agregado con valor 5, sin renumerar los estados existentes) es el estado inicial de todo `TrabajoPedido` creado junto a un presupuesto, en el mismo paso que `CrearVentaAsync`. `GetTrabajosPedidoAsync` excluye explícitamente `Borrador` de la cola visible del laboratorio. Al confirmar la venta (`ConfirmarVentaAsync`), si el TP sigue en `Borrador` se exige asignar un laboratorio y pasa a `PendienteAprobacion`, recién ahí entrando al ciclo real de laboratorio.

## Consecuencias
**Gana:** el vendedor puede armar la configuración óptica completa (diseño de lente, tratamientos, armazón, laboratorio tentativo) desde el presupuesto sin comprometer al laboratorio ni generar ruido en su cola de trabajo real; el presupuesto sigue siendo libremente editable mientras está en `Borrador`.

**Pierde:** cualquier query o reporte sobre `TrabajoPedido` tiene que recordar excluir `Borrador` explícitamente si no quiere contar presupuestos no confirmados como trabajo real de laboratorio — un filtro fácil de olvidar en código nuevo.

## Referencias
- `../schema.md` — Grupo C (`TrabajoPedido`, `EstadoTrabajoPedido`)
- `../modules/08-ventas.md`, `../modules/09-laboratorio.md` (pendientes, Fase 4)
