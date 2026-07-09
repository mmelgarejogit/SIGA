# ADR 0009: Una sucursal fija por usuario, sin selector

## Estado
Aceptada (implementada)

## Contexto
Al volver el sistema multi-sucursal había que decidir cómo un usuario del staff determina en qué sucursal está operando en cada momento. Un selector manual en el header es flexible pero propenso a error humano (olvidarse de cambiarlo y cargar una venta en la sucursal equivocada).

## Decisión
Cada `User` de staff tiene una sucursal fija (`User.SucursalId`), asignada desde el form de Profesional o de Empleado — no desde la pantalla de Usuarios, que solo gestiona roles y activar/desactivar. No hay selector de sucursal en el header: la sucursal del usuario logueado determina automáticamente el scoping de todo lo que crea, resuelto centralmente vía `ICurrentUserContext`. `SucursalId = null` significa usuario global: el admin (ve/filtra todas las sucursales) y los pacientes (intrínsecamente globales, eligen sucursal solo al reservar un turno). Catálogos (productos, marcas, servicios, etc.) y personas (pacientes, clientes, profesionales) quedan explícitamente globales, no scopeados por sucursal.

## Consecuencias
**Gana:** elimina una clase entera de errores humanos (cargar una operación en la sucursal equivocada por olvidar cambiar un selector); el scoping se resuelve una sola vez de forma centralizada en vez de repetirse en cada pantalla o servicio.

**Pierde:** un empleado que reparte su tiempo entre dos sucursales físicas no puede operar en la segunda sin que un admin le reasigne la sucursal — no hay soporte de "sucursal del día" ni multi-sucursal por usuario. Ya se identificó como refinamiento futuro un selector cross-sucursal solo para el admin en algunos listados (stock/productos).

## Referencias
- `../architecture.md` — § Multi-sucursal
- `../schema.md` — § Scoping por sucursal
- `../modules/15-sucursales.md` (pendiente, Fase 4 — verificar estado de commits antes de escribirlo)
