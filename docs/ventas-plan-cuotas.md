# Plan de cuotas en Ventas — Handoff

> Estado: implementado en código (backend + frontend). **Falta compilar y aplicar la
> migración en un entorno con `dotnet` disponible** — no se pudo verificar en este
> sandbox porque no tiene el SDK de .NET instalado.

## Qué se implementó

Se agregó un plan de cuotas **opcional** para ventas a `Credito`. Si no se define,
la venta a crédito sigue funcionando "libre" (cobros parciales sin cronograma),
exactamente igual que antes.

- El vendedor, al elegir "Crédito" en el editor de venta, puede opcionalmente elegir
  **cantidad de cuotas** (3/6/9/12) y **frecuencia** (Mensual/Quincenal). Si no toca
  nada, queda "Sin plan (libre)" — cero fricción para el flujo actual.
- El monto de cada cuota, cuántas están pagadas y la fecha de la próxima se calculan
  automáticamente (no hay carga manual de cronograma).
- En "Cobros Pendientes" se muestra debajo del cliente: `Próx. cuota 2/6: ₲150.000 ·
  vence 15/07`, en rojo con ícono de alerta si está vencida.
- No se tocó la pantalla de cobro (`VentaCobrarView.vue`) ni el flujo de emisión de
  comprobante — el plan es solo informativo/de seguimiento, el cobro sigue siendo
  libre (podés cobrar cualquier monto en cualquier momento).

## Pasos para continuar desde la terminal

Ejecutar en la raíz del repo backend (`SIGA/`):

```bash
# 1. Restaurar y compilar para detectar cualquier error de sintaxis/tipos
dotnet build

# 2. Si compila bien, revisar que la migración hand-written sea consistente
#    con el modelo actual (por si hay que regenerarla):
dotnet ef migrations list --project src/SIGA.Infrastructure --startup-project src/SIGA.Api

# 3. Si dotnet ef detecta algo raro (pending model changes, etc.), lo más seguro es
#    borrar los 2 archivos de la migración 059 y regenerarla desde cero:
#    rm src/SIGA.Infrastructure/Persistence/Migrations/20260709120000_059_VentaPlanCuotas*.cs
#    dotnet ef migrations add 059_VentaPlanCuotas --project src/SIGA.Infrastructure --startup-project src/SIGA.Api

# 4. Aplicar la migración a la base de datos local
dotnet ef database update --project src/SIGA.Infrastructure --startup-project src/SIGA.Api
```

Luego, en el repo frontend (`SIGA-Web/`):

```bash
npm run type-check   # ya corrido y sin errores en este sandbox, pero re-verificar tras el build del back
npm run dev          # probar manualmente: crear una venta a crédito con plan de cuotas
                      # y ver el indicador en /ventas/cobros-pendientes (o la ruta equivalente)
```

## Archivos modificados

### Backend (`SIGA/`)

| Archivo | Cambio |
|---|---|
| `src/SIGA.Domain/Entities/Venta.cs` | Campos `CantidadCuotas`, `FrecuenciaCuotasDias` + propiedades calculadas `MontoCuota`, `TotalCobradoEnCuotas`, `CuotasPagadas`, `ProximaCuotaVencimiento`, `CuotaVencida` |
| `src/SIGA.Infrastructure/Persistence/Configurations/VentaConfiguration.cs` | `.Property(...)` para las 2 columnas nuevas + `.Ignore(...)` para las 5 propiedades calculadas |
| `src/SIGA.Infrastructure/Persistence/Migrations/20260709120000_059_VentaPlanCuotas.cs` | Migración nueva (hand-written, **sin verificar con `dotnet ef`**) |
| `src/SIGA.Infrastructure/Persistence/Migrations/20260709120000_059_VentaPlanCuotas.Designer.cs` | Snapshot de la migración (hand-written) |
| `src/SIGA.Infrastructure/Persistence/Migrations/AppDbContextModelSnapshot.cs` | Actualizado con las 2 columnas nuevas |
| `src/SIGA.Application/DTOs/Ventas/CrearVentaRequest.cs` | + `CantidadCuotas`, `FrecuenciaCuotasDias` |
| `src/SIGA.Application/DTOs/Ventas/ActualizarVentaRequest.cs` | + `CantidadCuotas`, `FrecuenciaCuotasDias` |
| `src/SIGA.Application/DTOs/Ventas/VentaDto.cs` | + los 2 campos del plan + los 4 calculados |
| `src/SIGA.Infrastructure/Services/VentaService.cs` | `CrearVentaAsync`, `ActualizarVentaAsync` y el mapper `Map(Venta v)` propagan el plan (solo si `CondicionVenta == Credito`) |

### Frontend (`SIGA-Web/`)

| Archivo | Cambio |
|---|---|
| `src/services/ventasService.ts` | Tipos `Venta`, `CrearVentaRequest`, `ActualizarVentaRequest` con los campos nuevos |
| `src/components/VentaEditor.vue` | Selects "Plan de cuotas" / "Frecuencia" (solo visibles con `Crédito`), refs, `buildPayload()`, `actualizarVenta()` y `cargarPresupuesto()` actualizados |
| `src/views/CobrosPendientesView.vue` | Indicador "Próx. cuota X/Y · vence dd/mm" (rojo si vencida) debajo del nombre del cliente |

## Pendiente / próximos pasos sugeridos

- [ ] Correr `dotnet build` y `dotnet ef database update` (ver arriba)
- [ ] Probar de punta a punta: crear venta a crédito con plan de 3 cuotas mensuales,
      registrar cobros parciales, confirmar que `CuotasPagadas` y `ProximaCuotaVencimiento`
      avanzan correctamente
- [ ] (Futuro, mencionado por el usuario) Mejorar más a fondo el flujo de ventas a
      cuotas — este handoff cubre solo la primera integración simple
