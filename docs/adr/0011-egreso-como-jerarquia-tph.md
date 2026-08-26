# ADR 0011: Egreso como jerarquía TPH unificando las salidas de dinero

## Estado
Aceptada (implementada)

## Contexto
El negocio tiene varios tipos de salida de dinero con semántica distinta (factura de proveedor, honorario a profesional, gasto general, salario de empleado, factura de laboratorio) pero que comparten casi todos sus campos (monto, estado, fechas, método de pago) y necesitan aparecer juntos en reportes de caja/egresos sin duplicar esa lógica común cinco veces.

## Decisión
`Egreso` es la clase base abstracta de la que heredan `FacturaCompra`, `Honorario`, `GastoGeneral`, `SalarioEmpleado` y `EgresoFacturaLaboratorio`, mapeada por EF Core como **Table-Per-Hierarchy (TPH)**: una única tabla física `egresos` con columna discriminadora `Tipo` (`TipoEgreso` enum). Cada subtipo solo agrega 1-3 columnas específicas propias (ej. `Honorario.ProfessionalId`, `SalarioEmpleado.EmpleadoId`, `GastoGeneral.CategoriaGastoId`).

## Consecuencias
**Gana:** reportes y consultas de caja/egresos tratan cualquier tipo de salida de dinero de forma uniforme (mismo `Estado`, mismo flujo de aprobación/pago) sin necesidad de `UNION` entre tablas separadas; agregar un sexto tipo de egreso no requiere tocar la lógica de caja existente, solo un nuevo subtipo + valor de enum.

**Pierde:** la tabla física `egresos` acumula columnas nullable de todos los subtipos (patrón típico y esperado de TPH); cualquier índice o constraint a nivel de un subtipo específico es más difícil de expresar que en tablas separadas (TPT).

## Referencias
- `../schema.md` — Grupo C (`Egreso`)
- `../modules/11-caja-y-egresos.md` (pendiente, Fase 4)
