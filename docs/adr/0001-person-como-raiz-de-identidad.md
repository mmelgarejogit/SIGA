# ADR 0001: Person como raíz única de identidad; Cliente cuelga de Person

## Estado
Aceptada (implementada)

## Contexto
El sistema necesita modelar distintos roles funcionales de una misma persona física (paciente, profesional, cliente, empleado, usuario administrativo) sin duplicar sus datos personales ni forzar relaciones directas entre esos roles entre sí.

## Decisión
`Person` es la entidad raíz única (CI, nombre, contacto). Cada rol funcional (`User`, `Patient`, `Professional`, `Cliente`, `Empleado`) cuelga de `Person` (o de `User`) vía FK propio, nunca al revés. En particular, `Cliente` no es una entidad de facturación separada de `Patient`: ambos referencian el mismo `Person` cuando corresponden a la misma persona física, sin FK directa entre `Cliente` y `Patient` — se relacionan solo vía `PersonId` compartido. Los datos de facturación (`TipoFacturacion`, `RazonSocial`, `RucCiFiscal`, `Direccion`, etc.) viven directamente en `Cliente`. Esto reemplazó un diseño anterior con una entidad `DatosFacturacion` colgando de `Patient`, migrada y eliminada (migración `035_Clientes`).

## Consecuencias
**Gana:** un paciente puede convertirse en cliente (o viceversa) sin duplicar sus datos personales ni migrar identidad; consultas cross-rol ("¿este cliente también es paciente?") se resuelven por `PersonId` compartido, sin joins forzados entre tablas de rol.

**Pierde:** cualquier query que necesite "todas las facetas de una persona" tiene que hacer join manual vía `PersonId` en vez de tener una sola tabla; no hay integridad referencial que fuerce que un `Cliente` y un `Patient` con el mismo `Person` sean intencionalmente la misma persona (un alta descuidada podría crear un `Person` duplicado con datos ligeramente distintos para la misma persona real).

## Referencias
- `../schema.md` — Grupo A (`Person`, `Cliente`, `Patient`)
- `../modules/06-clientes.md` (pendiente, Fase 4)
