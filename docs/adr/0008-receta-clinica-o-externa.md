# ADR 0008: Receta puede ser clínica o externa/manual

## Estado
Aceptada (implementada)

## Contexto
No toda venta de lentes graduados se origina en una consulta clínica registrada en el sistema — un cliente puede traer una receta emitida por otra óptica u oftalmólogo externo, y el negocio necesita poder cargarla igual para generar el trabajo a pedido.

## Decisión
`Receta.ConsultaClinicaId` es nullable (antes era obligatorio, ligado 1:1 a una `ConsultaClinica`). Se agregó `Receta.PersonId` (nullable, FK a `Person`) para representar una receta "externa" cargada manualmente sin consulta previa. Ambos campos son nullable y mutuamente informativos: si `ConsultaClinicaId` es null, la receta es externa. El endpoint `POST /api/recetas` permite el alta manual (`CreateRecetaManualRequest`), separado del flujo clínico normal que sigue generando la receta desde `ConsultaClinicaService`.

## Consecuencias
**Gana:** el flujo de venta a pedido no depende de que el paciente haya tenido una consulta en el sistema; cubre el caso real de negocio de una óptica que también vende con receta de terceros.

**Pierde:** hay dos caminos distintos para crear una `Receta` (clínico vs. manual) que hay que mantener sincronizados en reglas de validación; una receta externa no tiene la trazabilidad de diagnóstico/observaciones clínicas que sí tiene una receta nacida de una `ConsultaClinica`.

## Referencias
- `../schema.md` — Grupo B (`Receta`)
- `../modules/05-clinica.md`, `../modules/08-ventas.md` (pendientes, Fase 4)
