using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Reportes;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class ReporteService(AppDbContext db, ICurrentUserContext current) : IReporteService
{
    private static readonly string[] MesesCortos =
        { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };

    public async Task<Result<ReporteVentasDto>> GetReporteVentasAsync(DateOnly desde, DateOnly hasta, string agrupacion)
    {
        var agrup = agrupacion is "mes" or "semana" or "dia" ? agrupacion : "dia";
        var branch = current.SucursalId;

        // ── Ventas con comprobante emitido en el rango (Facturado, top, condición, fiscal, saldo) ──
        var ventasEmitidas = await db.Ventas
            .Include(v => v.Lineas)
            .Include(v => v.Cobros)
            .AsSplitQuery()
            .Where(v => v.Estado == EstadoVenta.ComprobanteEmitido
                     && v.FechaComprobante != null
                     && v.FechaComprobante >= desde
                     && v.FechaComprobante <= hasta
                     && (branch == null || v.SucursalId == branch))
            .ToListAsync();

        // ── Cobros no anulados en el rango (Cobrado, métodos de pago, cajeros) ──
        var cobros = await db.Cobros
            .Include(c => c.Lineas)
            .Include(c => c.RegistradoPor).ThenInclude(u => u.Person)
            .AsSplitQuery()
            .Where(c => !c.Anulado && c.Fecha >= desde && c.Fecha <= hasta
                     && (branch == null || c.Venta.SucursalId == branch))
            .ToListAsync();

        var totalFacturado = ventasEmitidas.Sum(v => v.Total);
        var totalCobrado   = cobros.Sum(c => c.MontoTotal);
        var cantidadVentas = ventasEmitidas.Count;

        // ── Presupuestos / conversión (ventas creadas en el rango, no canceladas) ──
        var estadosCreados = await db.Ventas
            .Where(v => v.FechaVenta >= desde && v.FechaVenta <= hasta && v.Estado != EstadoVenta.Cancelada
                     && (branch == null || v.SucursalId == branch))
            .Select(v => v.Estado)
            .ToListAsync();
        var cantidadPresupuestos = estadosCreados.Count;
        var convertidos          = estadosCreados.Count(e => e == EstadoVenta.ComprobanteEmitido);

        // ── Serie temporal (facturado vs cobrado por bucket) ──
        var buckets = EnumerateBuckets(desde, hasta, agrup);
        var facturadoPorBucket = ventasEmitidas
            .GroupBy(v => BucketKey(v.FechaComprobante!.Value, agrup))
            .ToDictionary(g => g.Key, g => g.Sum(v => v.Total));
        var cobradoPorBucket = cobros
            .GroupBy(c => BucketKey(c.Fecha, agrup))
            .ToDictionary(g => g.Key, g => g.Sum(c => c.MontoTotal));

        var serie = buckets.Select(b => new SeriePuntoDto
        {
            Periodo   = b.Label,
            Facturado = facturadoPorBucket.GetValueOrDefault(b.Key, 0m),
            Cobrado   = cobradoPorBucket.GetValueOrDefault(b.Key, 0m),
        }).ToList();

        // ── Por método de pago (de las líneas de cobro) ──
        var lineasCobro    = cobros.SelectMany(c => c.Lineas).ToList();
        var totalEnLineas  = lineasCobro.Sum(l => l.Monto);
        var porMetodo = lineasCobro
            .GroupBy(l => l.MetodoPago)
            .Select(g => new MetodoPagoMontoDto
            {
                Metodo     = g.Key.ToString(),
                Monto      = g.Sum(l => l.Monto),
                Porcentaje = totalEnLineas > 0 ? Math.Round(g.Sum(l => l.Monto) / totalEnLineas * 100, 1) : 0m,
            })
            .OrderByDescending(m => m.Monto)
            .ToList();

        // ── Por condición de venta ──
        var porCondicion = ventasEmitidas
            .GroupBy(v => v.CondicionVenta)
            .Select(g => new CondicionMontoDto
            {
                Condicion = g.Key.ToString(),
                Monto     = g.Sum(v => v.Total),
                Cantidad  = g.Count(),
            })
            .OrderByDescending(c => c.Monto)
            .ToList();

        // ── Por categoría fiscal ──
        var porCategoriaFiscal = new List<CategoriaFiscalMontoDto>
        {
            new() { Categoria = "Exento",    Monto = ventasEmitidas.Sum(v => v.MontoExento) },
            new() { Categoria = "Gravado 5%", Monto = ventasEmitidas.Sum(v => v.MontoGravado5) },
            new() { Categoria = "Gravado 10%", Monto = ventasEmitidas.Sum(v => v.MontoGravado10) },
        }.Where(c => c.Monto > 0).ToList();

        // ── Top productos (líneas Producto + Lente) y servicios ──
        var lineas = ventasEmitidas.SelectMany(v => v.Lineas).ToList();
        var topProductos = lineas
            .Where(l => l.Tipo is TipoLineaVenta.Producto or TipoLineaVenta.Lente)
            .GroupBy(l => l.Descripcion)
            .Select(g => new RankingItemDto
            {
                Nombre   = g.Key,
                Cantidad = g.Sum(l => l.Cantidad),
                Monto    = g.Sum(l => l.Subtotal),
            })
            .OrderByDescending(r => r.Monto)
            .Take(10)
            .ToList();

        var topServicios = lineas
            .Where(l => l.Tipo == TipoLineaVenta.Servicio)
            .GroupBy(l => l.Descripcion)
            .Select(g => new RankingItemDto
            {
                Nombre   = g.Key,
                Cantidad = g.Sum(l => l.Cantidad),
                Monto    = g.Sum(l => l.Subtotal),
            })
            .OrderByDescending(r => r.Monto)
            .Take(10)
            .ToList();

        // ── Por cajero (quien registró el cobro) ──
        var porCajero = cobros
            .GroupBy(c => new
            {
                Nombre = c.RegistradoPor.Person == null
                    ? "—"
                    : $"{c.RegistradoPor.Person.FirstName} {c.RegistradoPor.Person.LastName}".Trim(),
            })
            .Select(g => new CajeroMontoDto
            {
                Nombre   = g.Key.Nombre,
                Monto    = g.Sum(c => c.MontoTotal),
                Cantidad = g.Count(),
            })
            .OrderByDescending(c => c.Monto)
            .ToList();

        var dto = new ReporteVentasDto
        {
            Desde      = desde.ToString("yyyy-MM-dd"),
            Hasta      = hasta.ToString("yyyy-MM-dd"),
            Agrupacion = agrup,

            TotalFacturado       = totalFacturado,
            TotalCobrado         = totalCobrado,
            CantidadVentas       = cantidadVentas,
            TicketPromedio       = cantidadVentas > 0 ? Math.Round(totalFacturado / cantidadVentas, 0) : 0m,
            SaldoPendiente       = ventasEmitidas.Where(v => v.SaldoPendiente > 0).Sum(v => v.SaldoPendiente),
            CantidadPresupuestos = cantidadPresupuestos,
            TasaConversion       = cantidadPresupuestos > 0
                ? Math.Round((decimal)convertidos / cantidadPresupuestos * 100, 1)
                : 0m,

            SerieTemporal      = serie,
            PorMetodoPago      = porMetodo,
            PorCondicion       = porCondicion,
            PorCategoriaFiscal = porCategoriaFiscal,
            TopProductos       = topProductos,
            TopServicios       = topServicios,
            PorCajero          = porCajero,
        };

        return Result<ReporteVentasDto>.Success(dto);
    }

    public async Task<Result<ReporteCitasDto>> GetReporteCitasAsync(DateOnly desde, DateOnly hasta, string agrupacion)
    {
        var agrup = agrupacion is "mes" or "semana" or "dia" ? agrupacion : "dia";

        var desdeDt = DateTime.SpecifyKind(desde.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var hastaDt = DateTime.SpecifyKind(hasta.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);
        var now     = DateTime.UtcNow;

        // ── Turnos del rango (por FechaHora) ──
        var branch = current.SucursalId;
        var turnos = await db.Turnos
            .Include(t => t.Professional).ThenInclude(p => p.User).ThenInclude(u => u.Person)
            .Where(t => t.FechaHora >= desdeDt && t.FechaHora <= hastaDt
                     && (branch == null || t.SucursalId == branch))
            .ToListAsync();

        var totalTurnos = turnos.Count;
        var completados = turnos.Count(t => t.Estado == TurnoEstado.Completado);
        var cancelados  = turnos.Count(t => t.Estado == TurnoEstado.Cancelado);
        var ausentes    = turnos.Count(t => t.FechaHora < now
                                         && (t.Estado == TurnoEstado.Pendiente || t.Estado == TurnoEstado.Confirmado));

        // ── Consultas activas del rango (por FechaConsulta) ──
        var consultas = await db.ConsultasClinicas
            .CountAsync(c => c.IsActive && c.FechaConsulta >= desdeDt && c.FechaConsulta <= hastaDt);

        // ── Recetas emitidas del rango (por FechaEmision) ──
        var recetas = await db.Recetas
            .CountAsync(r => r.FechaEmision >= desde && r.FechaEmision <= hasta);

        // ── Serie temporal (turnos vs completados) ──
        var buckets = EnumerateBuckets(desde, hasta, agrup);
        var turnosPorBucket = turnos
            .GroupBy(t => BucketKey(DateOnly.FromDateTime(t.FechaHora), agrup))
            .ToDictionary(g => g.Key, g => g.Count());
        var completPorBucket = turnos
            .Where(t => t.Estado == TurnoEstado.Completado)
            .GroupBy(t => BucketKey(DateOnly.FromDateTime(t.FechaHora), agrup))
            .ToDictionary(g => g.Key, g => g.Count());

        var serie = buckets.Select(b => new SeriePuntoCitasDto
        {
            Periodo     = b.Label,
            Turnos      = turnosPorBucket.GetValueOrDefault(b.Key, 0),
            Completados = completPorBucket.GetValueOrDefault(b.Key, 0),
        }).ToList();

        // ── Por estado ──
        var porEstado = turnos
            .GroupBy(t => t.Estado)
            .Select(g => new EstadoCitasDto
            {
                Estado     = g.Key.ToString(),
                Cantidad   = g.Count(),
                Porcentaje = totalTurnos > 0 ? Math.Round((decimal)g.Count() / totalTurnos * 100, 1) : 0m,
            })
            .OrderByDescending(e => e.Cantidad)
            .ToList();

        // ── Por profesional ──
        var porProfesional = turnos
            .GroupBy(t => new
            {
                Nombre = t.Professional.User.Person == null
                    ? "—"
                    : $"{t.Professional.User.Person.FirstName} {t.Professional.User.Person.LastName}".Trim(),
            })
            .Select(g => new ProfesionalCitasDto
            {
                Nombre      = g.Key.Nombre,
                Turnos      = g.Count(),
                Completados = g.Count(t => t.Estado == TurnoEstado.Completado),
            })
            .OrderByDescending(p => p.Turnos)
            .ToList();

        var dto = new ReporteCitasDto
        {
            Desde      = desde.ToString("yyyy-MM-dd"),
            Hasta      = hasta.ToString("yyyy-MM-dd"),
            Agrupacion = agrup,

            TotalTurnos    = totalTurnos,
            Completados    = completados,
            Cancelados     = cancelados,
            Ausentes       = ausentes,
            TasaAsistencia = totalTurnos > 0 ? Math.Round((decimal)completados / totalTurnos * 100, 1) : 0m,
            Consultas      = consultas,
            Recetas        = recetas,

            SerieTemporal  = serie,
            PorEstado      = porEstado,
            PorProfesional = porProfesional,
        };

        return Result<ReporteCitasDto>.Success(dto);
    }

    public async Task<Result<ReporteInventarioDto>> GetReporteInventarioAsync(DateOnly desde, DateOnly hasta, string agrupacion)
    {
        var agrup = agrupacion is "mes" or "semana" or "dia" ? agrupacion : "dia";

        // ── Snapshot del stock actual (no depende del rango) ──
        var productos = await db.Productos
            .Include(p => p.StockConfig)
            .Where(p => p.IsActive)
            .ToListAsync();

        var branch = current.SucursalId;
        var stockMap = await db.StockActual
            .Where(s => branch == null || s.SucursalId == branch)
            .GroupBy(s => s.ProductoId)
            .Select(g => new { ProductoId = g.Key, Stock = g.Sum(x => x.StockActual) })
            .ToDictionaryAsync(x => x.ProductoId, x => x.Stock);
        int StockOf(Producto p) => stockMap.GetValueOrDefault(p.Id, 0);
        int MinOf(Producto p) => p.StockConfig?.StockMinimo ?? 0;

        var valorInventario  = productos.Sum(p => StockOf(p) * p.PrecioCosto);
        var unidadesEnStock  = productos.Sum(StockOf);
        var stockCritico     = productos.Count(p => MinOf(p) > 0 && StockOf(p) <= MinOf(p));
        var sinStock         = productos.Count(p => StockOf(p) <= 0);

        // ── Por categoría (valor del stock) ──
        var porCategoria = productos
            .GroupBy(p => string.IsNullOrWhiteSpace(p.Categoria) ? "Sin categoría" : p.Categoria)
            .Select(g => new CategoriaInventarioDto
            {
                Categoria = g.Key,
                Productos = g.Count(),
                Valor     = g.Sum(p => StockOf(p) * p.PrecioCosto),
            })
            .OrderByDescending(c => c.Valor)
            .ToList();

        // ── Productos en stock crítico ──
        var productosCriticos = productos
            .Where(p => MinOf(p) > 0 && StockOf(p) <= MinOf(p))
            .Select(p => new ProductoCriticoDto
            {
                Nombre      = p.Nombre,
                StockActual = StockOf(p),
                StockMinimo = MinOf(p),
                Faltante    = MinOf(p) - StockOf(p),
            })
            .OrderByDescending(p => p.Faltante)
            .Take(15)
            .ToList();

        // ── Top productos por valor de stock ──
        var topPorValor = productos
            .Select(p => new ProductoValorDto
            {
                Nombre      = p.Nombre,
                StockActual = StockOf(p),
                Valor       = StockOf(p) * p.PrecioCosto,
            })
            .Where(p => p.Valor > 0)
            .OrderByDescending(p => p.Valor)
            .Take(10)
            .ToList();

        // ── Movimientos aprobados del período ──
        var desdeDt = DateTime.SpecifyKind(desde.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var hastaDt = DateTime.SpecifyKind(hasta.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);

        var movimientos = await db.MovimientosStock
            .Where(m => m.Estado == "Aprobado" && m.FechaMovimiento >= desdeDt && m.FechaMovimiento <= hastaDt)
            .Select(m => new { m.Tipo, m.Cantidad, m.FechaMovimiento })
            .ToListAsync();

        var totalEntradas = movimientos.Where(m => m.Tipo == "Entrada").Sum(m => m.Cantidad);
        var totalSalidas  = movimientos.Where(m => m.Tipo == "Salida").Sum(m => m.Cantidad);

        var buckets = EnumerateBuckets(desde, hasta, agrup);
        var entradasPorBucket = movimientos
            .Where(m => m.Tipo == "Entrada")
            .GroupBy(m => BucketKey(DateOnly.FromDateTime(m.FechaMovimiento), agrup))
            .ToDictionary(g => g.Key, g => g.Sum(m => m.Cantidad));
        var salidasPorBucket = movimientos
            .Where(m => m.Tipo == "Salida")
            .GroupBy(m => BucketKey(DateOnly.FromDateTime(m.FechaMovimiento), agrup))
            .ToDictionary(g => g.Key, g => g.Sum(m => m.Cantidad));

        var serie = buckets.Select(b => new SeriePuntoInventarioDto
        {
            Periodo  = b.Label,
            Entradas = entradasPorBucket.GetValueOrDefault(b.Key, 0),
            Salidas  = salidasPorBucket.GetValueOrDefault(b.Key, 0),
        }).ToList();

        var dto = new ReporteInventarioDto
        {
            Desde      = desde.ToString("yyyy-MM-dd"),
            Hasta      = hasta.ToString("yyyy-MM-dd"),
            Agrupacion = agrup,

            ProductosActivos = productos.Count,
            ValorInventario  = valorInventario,
            StockCritico     = stockCritico,
            SinStock         = sinStock,
            UnidadesEnStock  = unidadesEnStock,

            TotalEntradas = totalEntradas,
            TotalSalidas  = totalSalidas,

            SerieTemporal     = serie,
            PorCategoria      = porCategoria,
            ProductosCriticos = productosCriticos,
            TopPorValor       = topPorValor,
        };

        return Result<ReporteInventarioDto>.Success(dto);
    }

    public async Task<Result<ReporteComprasDto>> GetReporteComprasAsync(DateOnly desde, DateOnly hasta, string agrupacion)
    {
        var agrup = agrupacion is "mes" or "semana" or "dia" ? agrupacion : "dia";

        var branch = current.SucursalId;

        // ── Facturas de compra del rango (por FechaEmision, excluye anuladas/rechazadas) ──
        var facturas = await db.FacturasCompra
            .Include(f => f.Proveedor)
            .Where(f => f.FechaEmision >= desde && f.FechaEmision <= hasta
                     && f.Estado != EstadoEgreso.Anulado && f.Estado != EstadoEgreso.Rechazado
                     && (branch == null || f.SucursalId == branch))
            .ToListAsync();

        var montoFacturado = facturas.Sum(f => f.MontoTotal);
        var iva            = facturas.Sum(f => f.Iva5 + f.Iva10);
        var pendientePago  = facturas
            .Where(f => f.Estado == EstadoEgreso.Pendiente || f.Estado == EstadoEgreso.Aprobado)
            .Sum(f => f.MontoTotal);

        var ordenesCompra = await db.PedidosProveedor
            .CountAsync(p => p.FechaOrden != null && p.FechaOrden >= desde && p.FechaOrden <= hasta
                     && (branch == null || p.SucursalId == branch));

        var recepciones = await db.RecepcionesMercaderia
            .CountAsync(r => r.FechaRecepcion >= desde && r.FechaRecepcion <= hasta
                     && (branch == null || r.SucursalId == branch));

        // ── Serie temporal (monto facturado) ──
        var buckets = EnumerateBuckets(desde, hasta, agrup);
        var montoPorBucket = facturas
            .GroupBy(f => BucketKey(f.FechaEmision, agrup))
            .ToDictionary(g => g.Key, g => g.Sum(f => f.MontoTotal));

        var serie = buckets.Select(b => new SeriePuntoComprasDto
        {
            Periodo = b.Label,
            Monto   = montoPorBucket.GetValueOrDefault(b.Key, 0m),
        }).ToList();

        // ── Por estado de OC (creadas en el rango) ──
        var estadosOc = await db.PedidosProveedor
            .Where(p => p.FechaOrden != null && p.FechaOrden >= desde && p.FechaOrden <= hasta)
            .Select(p => p.Estado)
            .ToListAsync();
        var porEstadoOc = estadosOc
            .GroupBy(e => e)
            .Select(g => new EstadoOcDto
            {
                Estado     = g.Key.ToString(),
                Cantidad   = g.Count(),
                Porcentaje = ordenesCompra > 0 ? Math.Round((decimal)g.Count() / ordenesCompra * 100, 1) : 0m,
            })
            .OrderByDescending(e => e.Cantidad)
            .ToList();

        // ── Por proveedor (monto facturado) ──
        var porProveedor = facturas
            .GroupBy(f => f.Proveedor.Nombre)
            .Select(g => new ProveedorComprasDto
            {
                Nombre   = g.Key,
                Facturas = g.Count(),
                Monto    = g.Sum(f => f.MontoTotal),
            })
            .OrderByDescending(p => p.Monto)
            .Take(10)
            .ToList();

        var dto = new ReporteComprasDto
        {
            Desde      = desde.ToString("yyyy-MM-dd"),
            Hasta      = hasta.ToString("yyyy-MM-dd"),
            Agrupacion = agrup,

            OrdenesCompra  = ordenesCompra,
            MontoFacturado = montoFacturado,
            Iva            = iva,
            PendientePago  = pendientePago,
            Recepciones    = recepciones,

            SerieTemporal = serie,
            PorEstadoOc   = porEstadoOc,
            PorProveedor  = porProveedor,
        };

        return Result<ReporteComprasDto>.Success(dto);
    }

    // ── Helpers de bucketing ──────────────────────────────────────────────────

    private static DateOnly StartOfWeek(DateOnly d)
    {
        var diff = ((int)d.DayOfWeek + 6) % 7; // lunes = 0
        return d.AddDays(-diff);
    }

    private static string BucketKey(DateOnly d, string agrup) => agrup switch
    {
        "mes"    => $"{d.Year:D4}-{d.Month:D2}",
        "semana" => StartOfWeek(d).ToString("yyyy-MM-dd"),
        _        => d.ToString("yyyy-MM-dd"),
    };

    private static string BucketLabel(DateOnly d, string agrup) => agrup switch
    {
        "mes"    => $"{MesesCortos[d.Month - 1]} {d.Year % 100:D2}",
        "semana" => StartOfWeek(d).ToString("dd/MM"),
        _        => d.ToString("dd/MM"),
    };

    private static List<(string Key, string Label)> EnumerateBuckets(DateOnly desde, DateOnly hasta, string agrup)
    {
        var buckets = new List<(string, string)>();
        var seen    = new HashSet<string>();
        for (var cursor = desde; cursor <= hasta; cursor = cursor.AddDays(1))
        {
            var key = BucketKey(cursor, agrup);
            if (seen.Add(key))
                buckets.Add((key, BucketLabel(cursor, agrup)));
        }
        return buckets;
    }
}
