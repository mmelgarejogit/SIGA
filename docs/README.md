# Documentación técnica — SIGA

Índice de la documentación técnica del backend (y, por referencia, del frontend `SIGA-Web`). Escrita para el trabajo final de grado — ver el plan completo de documentación en `C:\Users\Matias\.claude\plans\serialized-inventing-bubble.md`.

Jerarquía de fuentes al leer/escribir estos documentos: **código real** > memoria de proyecto (contexto histórico del "por qué") > docs previos. Cada documento indica su fecha de última reescritura completa.

## Fundaciones

- **[architecture.md](./architecture.md)** — capas del backend, pipeline HTTP/seguridad, autorización por permisos, multi-sucursal, integraciones externas, mapeo módulos↔controllers. *(reescrito 2026-07-08)*
- **[schema.md](./schema.md)** — modelo de datos completo: 70 entidades en 3 diagramas ER (Identidad/Personas/Personal, Catálogo/Clínica/Agenda, Comercial), diccionario de datos, tabla de enums, patrones transversales. *(reescrito 2026-07-08)*

## Referencia de API

- **[api-reference.md](./api-reference.md)** — inventario completo de los 35 controllers / ~140 endpoints (verbo, ruta, policy de permiso, DTOs de request/response), agrupado en los mismos 14 módulos de negocio. *(escrito 2026-07-08)* Incluye 3 hallazgos de código anotados inline (no de documentación, quedan para revisión aparte): posible bug copy-paste en `RolesController.GetUsersByRole`, `ServiciosController` agrupado bajo Catálogo pero funcionalmente comercial, policy inconsistente en `reportes/compras`.

## Decisiones de diseño (ADR)

- **[adr/](./adr/)** — 12 decisiones de arquitectura no triviales, formalizadas a partir de la memoria de proyecto acumulada. *(escrito 2026-07-08)*
  1. [0001 — Person como raíz de identidad](./adr/0001-person-como-raiz-de-identidad.md)
  2. [0002 — Stock derivado de movimientos](./adr/0002-stock-derivado-de-movimientos.md)
  3. [0003 — Autorización por permisos](./adr/0003-autorizacion-por-permisos.md)
  4. [0004 — Precio de venta derivado por margen](./adr/0004-precio-venta-derivado-por-margen.md)
  5. [0005 — TrabajoPedido nace en Borrador](./adr/0005-trabajopedido-nace-en-borrador.md)
  6. [0006 — Cobro desacoplado del laboratorio](./adr/0006-cobro-desacoplado-del-laboratorio.md)
  7. [0007 — El cristal ya no es Producto con stock](./adr/0007-cristal-ya-no-es-producto-con-stock.md)
  8. [0008 — Receta clínica o externa](./adr/0008-receta-clinica-o-externa.md)
  9. [0009 — Sucursal fija por usuario](./adr/0009-sucursal-fija-por-usuario.md)
  10. [0010 — Notificaciones internas antes que externas](./adr/0010-notificaciones-internas-antes-que-externas.md)
  11. [0011 — Egreso como jerarquía TPH](./adr/0011-egreso-como-jerarquia-tph.md)
  12. [0012 — Cambio de contraseña solo para cuentas nuevas](./adr/0012-cambio-contrasena-solo-cuentas-nuevas.md)

  También persistidas de forma resumida en el grafo indexado vía `manage_adr` (la tool maneja un documento único por proyecto, no ADRs individuales — el detalle completo vive en estos 12 archivos).

## Módulos de negocio

Cada uno sigue la misma plantilla: propósito de negocio, entidades principales (link a `schema.md`), reglas de negocio clave (link a ADR si aplica), endpoints relevantes (link a `api-reference.md`), flujo típico (diagrama de secuencia si aplica), vistas de frontend involucradas, estado (implementado/planificado). *(escritos 2026-07-08)*

1. [Identidad y acceso](./modules/01-identidad-y-acceso.md)
2. [Pacientes y profesionales](./modules/02-pacientes-y-profesionales.md)
3. [Personal](./modules/03-personal.md)
4. [Agenda y turnos](./modules/04-agenda-turnos.md)
5. [Clínica](./modules/05-clinica.md)
6. [Clientes](./modules/06-clientes.md)
7. [Inventario y catálogo](./modules/07-inventario-catalogo.md)
8. [Ventas](./modules/08-ventas.md)
9. [Laboratorio](./modules/09-laboratorio.md)
10. [Compras](./modules/10-compras.md)
11. [Caja y egresos](./modules/11-caja-y-egresos.md)
12. [Roles y permisos](./modules/12-roles-y-permisos.md)
13. [Notificaciones](./modules/13-notificaciones.md)
14. [Reportes](./modules/14-reportes.md)
15. [Sucursales](./modules/15-sucursales.md)

## Documentos existentes — auditados, no reescritos

Escritos en etapas anteriores del proyecto como documentos de trabajo/progreso. Cada uno tiene ahora un pie **"Auditado 2026-07-08"** que confirma su vigencia contra el código real (o señala el hallazgo puntual si algo estaba desactualizado). Se conservan como referencia histórica; el contenido relevante para la tesis vive fusionado en `modules/*.md`:

- [`despliegue-vps.md`](./despliegue-vps.md) — vigente; único hallazgo: referencia un `.env.example` que no existe en el repo.
- [`catalogo-datos-y-borrado.md`](./catalogo-datos-y-borrado.md) — backend confirmado vigente; checklist de ejecución/verificación manual no reconfirmable (no es estado de código). Ver [modules/07-inventario-catalogo.md](./modules/07-inventario-catalogo.md).
- [`ventas-emision-comprobante.md`](./ventas-emision-comprobante.md) — vigente. Fusionado en [modules/08-ventas.md](./modules/08-ventas.md).
- [`ventas-modelo-lentes.md`](./ventas-modelo-lentes.md) — vigente; es la fuente original de [ADR 0007](./adr/0007-cristal-ya-no-es-producto-con-stock.md). Fusionado en [modules/08-ventas.md](./modules/08-ventas.md).
- [`ventas-timbrados-abm.md`](./ventas-timbrados-abm.md) — su encabezado decía "planificado, pendiente" pero **ya está implementado en su totalidad** (numeración automática por timbrado incluida) — hallazgo corregido en el pie de auditoría, encabezado original conservado por trazabilidad. Fusionado en [modules/08-ventas.md](./modules/08-ventas.md).

## Frontend

- **[`../../SIGA-Web/docs/frontend-architecture.md`](../../SIGA-Web/docs/frontend-architecture.md)** — estructura de carpetas (24 servicios, 90 vistas, 9 composables), patrón vista/composable/servicio, stores Pinia, router y guards, sidebar dinámico por permisos, diagrama Mermaid de capas. *(escrito 2026-07-08)*
- `../../SIGA-Web/design-system.md` — convenciones de UI (tokens, componentes base). Auditado en la Fase 5: vigente.
- `../../SIGA-Web/CLAUDE.md` — convenciones generales de frontend. Auditado en la Fase 5: vigente en patrones, pero con 3 detalles desactualizados (tabla de ~14 endpoints obsoleta → ver `api-reference.md`; describe un array `navItems` hardcodeado que ya no existe, el sidebar real es dinámico vía `menuConfig.ts`; inventario de carpetas mínimo). No corregido en este pase — señalado para una futura pasada sobre ese archivo.
