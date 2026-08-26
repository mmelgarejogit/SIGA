# ADR 0010: Centro de notificaciones interno antes que integración externa

## Estado
Aceptada (implementada — Fase 1 de 5 del módulo)

## Contexto
La especificación completa del módulo de notificaciones contemplaba notificaciones externas a pacientes (email/WhatsApp/SMS) y un sistema de plantillas — alcance demasiado grande para una sola entrega. Había que elegir con qué empezar para entregar valor real cuanto antes.

## Decisión
La Fase 1 del módulo se limitó a un centro de notificaciones **interno** al staff (`NotificacionInterna`), sin ningún canal externo todavía. Scoping por destinatario individual (`DestinatarioUsuarioId`), broadcast por sucursal (`DestinatarioSucursalId`) o broadcast global (ambos null). Tres triggers reales conectados desde el día uno: bajo stock (poller `BackgroundService` cada 30 min, no hook inline — porque los movimientos "Aprobado" se originan desde al menos 5 servicios distintos), transferencia de stock pendiente, y pedido de laboratorio recibido.

## Consecuencias
**Gana:** valor inmediato para el staff (alertas de bajo stock, transferencias, laboratorio) sin la complejidad de integrar un proveedor de email/WhatsApp todavía; el diseño de destinatario (usuario/sucursal/global) es reutilizable tal cual para las fases externas futuras.

**Pierde:** `Leido` es un único flag compartido para notificaciones de broadcast (no por-usuario), así que un usuario puede marcar como leída una notificación que otro compañero de la misma sucursal todavía no vio — decisión explícita aceptada, no un descuido. Un bug real derivado de un diseño relacionado (uso de `EsGlobal` para gatear visibilidad, que también es `true` para pacientes) ya fue encontrado y corregido durante la Fase 1.

## Referencias
- `../schema.md` — § Entidades transversales (`NotificacionInterna`)
- `../modules/13-notificaciones.md` (pendiente, Fase 4)
