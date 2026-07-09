# ADR 0006: Cobro desacoplado del ciclo del laboratorio

## Estado
Aceptada (implementada)

## Contexto
Originalmente una venta "a pedido" quedaba en estado `EnProceso` hasta que el laboratorio recibía el trabajo, y solo entonces se habilitaba el cobro — un bloqueo de negocio real, porque impedía cobrarle al cliente (seña o total) mientras el pedido seguía en tránsito con un laboratorio externo, a veces por días o semanas.

## Decisión
`ConfirmarVentaAsync` deja **toda** venta (directa o a pedido) en estado `ListaParaCobrar` inmediatamente al confirmar, sin esperar al laboratorio. `LaboratorioService.RegistrarRecepcionAsync` ya no cambia el estado de la venta. El ciclo del laboratorio (envío → recepción → factura) corre en paralelo al cobro, sin bloquearse mutuamente.

## Consecuencias
**Gana:** el negocio puede cobrar (seña o total) apenas se confirma la venta, sin esperar la logística externa del laboratorio.

**Pierde:** `EstadoVenta.EnProceso` quedó sin uso para ventas nuevas — el enum se mantiene por compatibilidad con datos/reportes históricos, lo que es una fuente potencial de confusión para quien lea el código o el enum sin conocer este historial.

## Referencias
- `../schema.md` — Grupo C (`Venta.Estado`)
- `../modules/08-ventas.md` (pendiente, Fase 4)
