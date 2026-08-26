# ADR 0003: Autorización basada en permisos individuales, no en roles con nombre fijo

## Estado
Aceptada (implementada)

## Contexto
Un sistema de roles fijos (ej. "Admin", "Recepcionista" codificados en el backend) acopla la lógica de autorización al nombre del rol, dificultando que el negocio cree roles ad-hoc (ej. "Recepcionista Senior" con un permiso extra) sin tocar código ni hacer un deploy.

## Decisión
La autorización real de cada endpoint se basa en permisos individuales (`Permission`), no en el nombre ni el tipo del `Role`. Un `Role` es solo una colección con nombre de permisos (`RolePermission`, many-to-many); el JWT emite un claim `"permission"` por cada permiso resuelto, no el nombre del rol. `Program.cs` define una policy por permiso (`RequireClaim("permission", perm)`), más dos policies compuestas con `RequireAssertion` para OR de permisos. `Role.Type` (`admin`/`professional`/`patient`, nullable e inmutable) es la única noción de "rol de sistema", usada solo para lógica interna que necesita distinguir un rol inmutable (ej. bootstrap del admin), nunca para autorizar endpoints.

## Consecuencias
**Gana:** el negocio puede crear/editar roles y su combinación de permisos desde `/roles` sin ningún cambio de código ni deploy; los permisos son la única fuente de verdad para "qué puede hacer este usuario", sin casos especiales por nombre de rol.

**Pierde:** el JWT puede volverse más pesado con muchos claims de permiso si un rol tiene muchos permisos asignados; el código nunca puede asumir "si es rol X entonces puede hacer Y" — tiene que verificar el permiso explícito incluso para casos que parecen obvios por el rol, lo que exige disciplina consistente en cada nuevo endpoint.

## Referencias
- `../architecture.md` — § Autorización: permisos, no roles
- `../schema.md` — Grupo A (`Role`, `Permission`, `RolePermission`)
- `../modules/01-identidad-y-acceso.md` (pendiente, Fase 4)
