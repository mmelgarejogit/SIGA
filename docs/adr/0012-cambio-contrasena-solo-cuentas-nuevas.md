# ADR 0012: Cambio de contraseña obligatorio solo para cuentas nuevas

## Estado
Aceptada (implementada)

## Contexto
Al implementar el flujo de cambio de contraseña obligatorio para cuentas creadas por un admin (profesionales/empleados dados de alta con una contraseña que ellos no eligieron), había que decidir si aplicar el requisito retroactivamente a las cuentas ya existentes o solo hacia adelante.

## Decisión
`User.MustChangePassword` (default `false`) solo se setea en `true` para cuentas nuevas creadas desde ese momento en adelante (`ProfessionalService.CreateAsync` / `EmpleadoService.CrearAsync`); no hay migración retroactiva sobre cuentas existentes. El admin puede forzar el cambio en cualquier cuenta puntual vía `UserService.ResetPasswordAsync`. El flag no bloquea el login (a diferencia de `IsEmailVerified`): el frontend redirige después de autenticar, vía guard de router que cubre también navegación directa y refresh, no solo el login inicial.

## Consecuencias
**Gana:** cambio simple y sin fricción para las cuentas existentes, que no se ven interrumpidas por un requisito retroactivo; el admin conserva la herramienta puntual (reset) para forzar el cambio cuando haga falta caso por caso.

**Pierde:** cuentas viejas creadas con contraseña temporal antes de este feature nunca son forzadas a cambiarla automáticamente — depende de que el admin lo haga manualmente si le preocupa un caso puntual. No hay invalidación de JWTs ya emitidos al cambiar/resetear una contraseña (limitación conocida, sin infraestructura de revocación).

## Referencias
- `../architecture.md` (pendiente ampliar sección de seguridad con este flujo)
- `../modules/01-identidad-y-acceso.md` (pendiente, Fase 4)
