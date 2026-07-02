using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Reportes;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class ReporteOperativoService(AppDbContext db, ICurrentUserContext current) : IReporteOperativoService
{
    // Un usuario atado a una sucursal solo ve la suya (seguridad); uno global usa el filtro (o todas).
    private int? ResolveBranch(ReporteOperativoFiltros f) => current.SucursalId ?? f.SucursalId;

    private static MetodoPago? ParseMetodo(string? s) =>
        Enum.TryParse<MetodoPago>(s, true, out var m) ? m : null;

    private static IQueryable<T> Paginate<T>(IQueryable<T> q, ReporteOperativoFiltros f) =>
        f.PageSize > 0 ? q.Skip((Math.Max(1, f.Page) - 1) * f.PageSize).Take(f.PageSize) : q;

    // ── Ventas ──────────────────────────────────────────────────────────────────────
    public async Task<Result<ReporteOperativoDto<ReporteVentaRow>>> GetVentasAsync(ReporteOperativoFiltros f)
    {
        var branch = ResolveBranch(f);
        var metodo = ParseMetodo(f.MetodoPago);

        var q = db.Ventas.Where(v => v.Estado != EstadoVenta.Borrador);
        if (branch is int b)        q = q.Where(v => v.SucursalId == b);
        if (f.Desde is DateOnly d)  q = q.Where(v => v.FechaVenta >= d);
        if (f.Hasta is DateOnly h)  q = q.Where(v => v.FechaVenta <= h);
        if (f.OperadorId is int op) q = q.Where(v => v.VendedorId == op);
        if (metodo is MetodoPago m) q = q.Where(v => v.Cobros.Any(c => !c.Anulado && c.Lineas.Any(l => l.MetodoPago == m)));
        if (!string.IsNullOrWhiteSpace(f.Categoria))
            q = q.Where(v => v.Lineas.Any(l => l.Producto != null && l.Producto.Categoria == f.Categoria));

        var totalCount = await q.CountAsync();
        var total   = await q.SumAsync(v => (decimal?)v.Lineas.Sum(l => l.PrecioUnitario * l.Cantidad - l.Descuento)) ?? 0m;
        var cobrado = await q.SumAsync(v => (decimal?)v.Cobros.Where(c => !c.Anulado).Sum(c => c.MontoTotal)) ?? 0m;

        var raw = await Paginate(q.OrderByDescending(v => v.FechaVenta).ThenByDescending(v => v.Id), f)
            .Select(v => new
            {
                v.FechaVenta,
                v.NumeroComprobante,
                ClienteRazon  = v.Cliente != null ? v.Cliente.RazonSocial : null,
                ClienteNombre = v.Cliente != null ? v.Cliente.Person.FirstName + " " + v.Cliente.Person.LastName : null,
                v.Tipo,
                v.CondicionVenta,
                v.Estado,
                Total    = v.Lineas.Sum(l => l.PrecioUnitario * l.Cantidad - l.Descuento),
                Cobrado  = v.Cobros.Where(c => !c.Anulado).Sum(c => c.MontoTotal),
                Sucursal = v.Sucursal != null ? v.Sucursal.Nombre : null,
                Vendedor = v.Vendedor != null ? v.Vendedor.Person.FirstName + " " + v.Vendedor.Person.LastName : null,
            })
            .ToListAsync();

        var rows = raw.Select(r => new ReporteVentaRow
        {
            Fecha             = r.FechaVenta,
            NumeroComprobante = r.NumeroComprobante,
            Cliente           = r.ClienteRazon ?? r.ClienteNombre ?? "—",
            Tipo              = r.Tipo.ToString(),
            Condicion         = r.CondicionVenta.ToString(),
            Estado            = r.Estado.ToString(),
            Total             = r.Total,
            Cobrado           = r.Cobrado,
            Sucursal          = r.Sucursal ?? "—",
            Vendedor          = r.Vendedor ?? "—",
        }).ToList();

        return Result<ReporteOperativoDto<ReporteVentaRow>>.Success(new ReporteOperativoDto<ReporteVentaRow>
        {
            Rows = rows, TotalCount = totalCount, Page = f.Page, PageSize = f.PageSize,
            Totales = new() { ["total"] = total, ["cobrado"] = cobrado, ["saldo"] = total - cobrado },
        });
    }

    // ── Compras (facturas de compra) ─────────────────────────────────────────────────
    public async Task<Result<ReporteOperativoDto<ReporteCompraRow>>> GetComprasAsync(ReporteOperativoFiltros f)
    {
        var branch = ResolveBranch(f);
        var metodo = ParseMetodo(f.MetodoPago);

        var q = db.FacturasCompra.AsQueryable();
        if (branch is int b)        q = q.Where(x => x.SucursalId == b);
        if (f.Desde is DateOnly d)  q = q.Where(x => x.FechaEmision >= d);
        if (f.Hasta is DateOnly h)  q = q.Where(x => x.FechaEmision <= h);
        if (f.OperadorId is int op) q = q.Where(x => x.RegistradoPorId == op);
        if (metodo is MetodoPago m) q = q.Where(x => x.MetodoPago == m);
        if (!string.IsNullOrWhiteSpace(f.Categoria))
            q = q.Where(x => x.Items.Any(i => i.Producto != null && i.Producto.Categoria == f.Categoria));

        var totalCount = await q.CountAsync();
        var totalMonto = await q.SumAsync(x => (decimal?)(x.MontoExento + x.MontoGravado5 + x.MontoGravado10)) ?? 0m;

        var raw = await Paginate(q.OrderByDescending(x => x.FechaEmision).ThenByDescending(x => x.Id), f)
            .Select(x => new
            {
                x.FechaEmision,
                x.NroFactura,
                Proveedor = x.Proveedor != null ? x.Proveedor.Nombre : null,
                x.CondicionVenta,
                x.Estado,
                x.MetodoPago,
                Monto    = x.MontoExento + x.MontoGravado5 + x.MontoGravado10,
                Sucursal = x.Sucursal != null ? x.Sucursal.Nombre : null,
                Registrado = x.RegistradoPor != null ? x.RegistradoPor.Person.FirstName + " " + x.RegistradoPor.Person.LastName : null,
            })
            .ToListAsync();

        var rows = raw.Select(r => new ReporteCompraRow
        {
            Fecha         = r.FechaEmision,
            NroFactura    = r.NroFactura ?? "—",
            Proveedor     = r.Proveedor ?? "—",
            Condicion     = r.CondicionVenta.ToString(),
            Estado        = r.Estado.ToString(),
            MetodoPago    = r.MetodoPago?.ToString() ?? "—",
            MontoTotal    = r.Monto,
            Sucursal      = r.Sucursal ?? "—",
            RegistradoPor = r.Registrado ?? "—",
        }).ToList();

        return Result<ReporteOperativoDto<ReporteCompraRow>>.Success(new ReporteOperativoDto<ReporteCompraRow>
        {
            Rows = rows, TotalCount = totalCount, Page = f.Page, PageSize = f.PageSize,
            Totales = new() { ["total"] = totalMonto },
        });
    }

    // ── Movimientos de inventario ────────────────────────────────────────────────────
    public async Task<Result<ReporteOperativoDto<ReporteMovInventarioRow>>> GetMovInventarioAsync(ReporteOperativoFiltros f)
    {
        var branch = ResolveBranch(f);

        var q = db.MovimientosStock.AsQueryable();
        if (branch is int b)        q = q.Where(m => m.SucursalId == b);
        if (f.Desde is DateOnly d)  q = q.Where(m => m.FechaMovimiento >= d.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        if (f.Hasta is DateOnly h)  q = q.Where(m => m.FechaMovimiento <= h.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc));
        if (f.OperadorId is int op) q = q.Where(m => m.CreadoPorId == op.ToString());
        if (!string.IsNullOrWhiteSpace(f.Tipo))      q = q.Where(m => m.Tipo == f.Tipo);
        if (!string.IsNullOrWhiteSpace(f.Categoria)) q = q.Where(m => m.Producto.Categoria == f.Categoria);

        var totalCount   = await q.CountAsync();
        var totalEntrada = await q.Where(m => m.Tipo == "Entrada").SumAsync(m => (int?)m.Cantidad) ?? 0;
        var totalSalida  = await q.Where(m => m.Tipo == "Salida").SumAsync(m => (int?)m.Cantidad) ?? 0;

        var rows = await Paginate(q.OrderByDescending(m => m.FechaMovimiento).ThenByDescending(m => m.Id), f)
            .Select(m => new ReporteMovInventarioRow
            {
                Fecha     = m.FechaMovimiento,
                Producto  = m.Producto.Nombre,
                Categoria = m.Producto.Categoria,
                Tipo      = m.Tipo,
                Cantidad  = m.Cantidad,
                Motivo    = m.Motivo ?? "—",
                Estado    = m.Estado,
                CreadoPor = m.CreadoPorNombre ?? "—",
                Sucursal  = m.Sucursal != null ? m.Sucursal.Nombre : "—",
            })
            .ToListAsync();

        return Result<ReporteOperativoDto<ReporteMovInventarioRow>>.Success(new ReporteOperativoDto<ReporteMovInventarioRow>
        {
            Rows = rows, TotalCount = totalCount, Page = f.Page, PageSize = f.PageSize,
            Totales = new() { ["entradas"] = totalEntrada, ["salidas"] = totalSalida },
        });
    }

    // ── Movimientos de caja ──────────────────────────────────────────────────────────
    public async Task<Result<ReporteOperativoDto<ReporteMovCajaRow>>> GetMovCajaAsync(ReporteOperativoFiltros f)
    {
        var branch = ResolveBranch(f);
        var metodo = ParseMetodo(f.MetodoPago);
        TipoMovimientoCaja? tipoCaja = Enum.TryParse<TipoMovimientoCaja>(f.Tipo, true, out var tc) ? tc : null;

        var q = db.MovimientosCaja.AsQueryable();
        if (branch is int b)                  q = q.Where(m => m.SucursalId == b);
        if (f.Desde is DateOnly d)            q = q.Where(m => m.Fecha >= d);
        if (f.Hasta is DateOnly h)            q = q.Where(m => m.Fecha <= h);
        if (f.OperadorId is int op)           q = q.Where(m => m.RegistradoPorId == op);
        if (metodo is MetodoPago m)           q = q.Where(m2 => m2.MetodoPago == m);
        if (tipoCaja is TipoMovimientoCaja t) q = q.Where(mc => mc.Tipo == t);

        var totalCount = await q.CountAsync();
        var ingresos   = await q.Where(m => m.Tipo == TipoMovimientoCaja.Ingreso).SumAsync(m => (decimal?)m.Monto) ?? 0m;
        var egresos    = await q.Where(m => m.Tipo == TipoMovimientoCaja.Egreso).SumAsync(m => (decimal?)m.Monto) ?? 0m;

        var raw = await Paginate(q.OrderByDescending(m => m.Fecha).ThenByDescending(m => m.Id), f)
            .Select(m => new
            {
                m.Fecha,
                m.Tipo,
                m.Concepto,
                m.MetodoPago,
                m.Monto,
                m.Referencia,
                Registrado = m.RegistradoPor != null ? m.RegistradoPor.Person.FirstName + " " + m.RegistradoPor.Person.LastName : null,
                Sucursal   = m.Sucursal != null ? m.Sucursal.Nombre : null,
            })
            .ToListAsync();

        var rows = raw.Select(r => new ReporteMovCajaRow
        {
            Fecha         = r.Fecha,
            Tipo          = r.Tipo.ToString(),
            Concepto      = r.Concepto,
            MetodoPago    = r.MetodoPago.ToString(),
            Monto         = r.Monto,
            RegistradoPor = r.Registrado ?? "—",
            Sucursal      = r.Sucursal ?? "—",
            Referencia    = r.Referencia ?? "—",
        }).ToList();

        return Result<ReporteOperativoDto<ReporteMovCajaRow>>.Success(new ReporteOperativoDto<ReporteMovCajaRow>
        {
            Rows = rows, TotalCount = totalCount, Page = f.Page, PageSize = f.PageSize,
            Totales = new() { ["ingresos"] = ingresos, ["egresos"] = egresos, ["neto"] = ingresos - egresos },
        });
    }

    // ── Exportación (todas las filas → tabla genérica lista para PDF/CSV) ─────────────
    public async Task<Result<ReporteExport>> GetExportAsync(string tipo, ReporteOperativoFiltros f)
    {
        f.PageSize = 0; // sin paginar
        switch (tipo?.ToLowerInvariant())
        {
            case "ventas":
            {
                var r = await GetVentasAsync(f);
                return r.IsSuccess ? Result<ReporteExport>.Success(BuildVentas(r.Value!, f)) : Fail(r);
            }
            case "compras":
            {
                var r = await GetComprasAsync(f);
                return r.IsSuccess ? Result<ReporteExport>.Success(BuildCompras(r.Value!, f)) : Fail(r);
            }
            case "inventario":
            {
                var r = await GetMovInventarioAsync(f);
                return r.IsSuccess ? Result<ReporteExport>.Success(BuildInventario(r.Value!, f)) : Fail(r);
            }
            case "caja":
            {
                var r = await GetMovCajaAsync(f);
                return r.IsSuccess ? Result<ReporteExport>.Success(BuildCaja(r.Value!, f)) : Fail(r);
            }
            default:
                return Result<ReporteExport>.Failure("Tipo de reporte inválido.", ErrorType.Validation);
        }
    }

    private static Result<ReporteExport> Fail<T>(Result<T> r) =>
        Result<ReporteExport>.Failure(r.Error ?? "Error al generar el reporte.", r.ErrorType);

    private static readonly NumberFormatInfo GsFmt =
        new() { NumberGroupSeparator = ".", NumberGroupSizes = [3], NumberDecimalDigits = 0 };
    private static string Gs(decimal n) => n.ToString("N0", GsFmt);
    private static string D(DateOnly d) => d.ToString("dd/MM/yyyy");
    private static string DT(DateTime d) => d.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

    private static string Subtitulo(ReporteOperativoFiltros f)
    {
        var p = new List<string>();
        if (f.Desde is DateOnly d && f.Hasta is DateOnly h) p.Add($"{D(d)} — {D(h)}");
        else if (f.Desde is DateOnly d2) p.Add($"Desde {D(d2)}");
        else if (f.Hasta is DateOnly h2) p.Add($"Hasta {D(h2)}");
        if (!string.IsNullOrWhiteSpace(f.MetodoPago)) p.Add($"Método: {f.MetodoPago}");
        if (!string.IsNullOrWhiteSpace(f.Categoria))  p.Add($"Categoría: {f.Categoria}");
        if (!string.IsNullOrWhiteSpace(f.Tipo))       p.Add($"Tipo: {f.Tipo}");
        return p.Count > 0 ? string.Join("   ·   ", p) : "Todos los registros";
    }

    private static ReporteExport BuildVentas(ReporteOperativoDto<ReporteVentaRow> d, ReporteOperativoFiltros f) => new()
    {
        Titulo = "Reporte de Ventas",
        Subtitulo = Subtitulo(f),
        Columnas = ["Fecha", "Comprobante", "Cliente", "Tipo", "Condición", "Estado", "Vendedor", "Sucursal", "Total", "Cobrado", "Saldo"],
        Filas = d.Rows.Select(r => new[]
        {
            D(r.Fecha), r.NumeroComprobante, r.Cliente, r.Tipo, r.Condicion, r.Estado, r.Vendedor, r.Sucursal,
            Gs(r.Total), Gs(r.Cobrado), Gs(r.Saldo),
        }).ToList(),
        Totales = ["TOTALES", "", "", "", "", "", "", "", Gs(d.Totales.GetValueOrDefault("total")), Gs(d.Totales.GetValueOrDefault("cobrado")), Gs(d.Totales.GetValueOrDefault("saldo"))],
        ColumnasNumericas = [8, 9, 10],
        FileBaseName = "reporte-ventas",
    };

    private static ReporteExport BuildCompras(ReporteOperativoDto<ReporteCompraRow> d, ReporteOperativoFiltros f) => new()
    {
        Titulo = "Reporte de Compras",
        Subtitulo = Subtitulo(f),
        Columnas = ["Fecha", "N° Factura", "Proveedor", "Condición", "Estado", "Método pago", "Registrado por", "Sucursal", "Monto"],
        Filas = d.Rows.Select(r => new[]
        {
            D(r.Fecha), r.NroFactura, r.Proveedor, r.Condicion, r.Estado, r.MetodoPago, r.RegistradoPor, r.Sucursal, Gs(r.MontoTotal),
        }).ToList(),
        Totales = ["TOTALES", "", "", "", "", "", "", "", Gs(d.Totales.GetValueOrDefault("total"))],
        ColumnasNumericas = [8],
        FileBaseName = "reporte-compras",
    };

    private static ReporteExport BuildInventario(ReporteOperativoDto<ReporteMovInventarioRow> d, ReporteOperativoFiltros f) => new()
    {
        Titulo = "Reporte de Movimientos de Inventario",
        Subtitulo = Subtitulo(f),
        Columnas = ["Fecha", "Producto", "Categoría", "Tipo", "Cantidad", "Motivo", "Estado", "Creado por", "Sucursal"],
        Filas = d.Rows.Select(r => new[]
        {
            DT(r.Fecha), r.Producto, r.Categoria, r.Tipo, r.Cantidad.ToString(), r.Motivo, r.Estado, r.CreadoPor, r.Sucursal,
        }).ToList(),
        Totales = ["TOTALES", "", "", "", $"+{Gs(d.Totales.GetValueOrDefault("entradas"))} / -{Gs(d.Totales.GetValueOrDefault("salidas"))}", "", "", "", ""],
        ColumnasNumericas = [4],
        FileBaseName = "reporte-inventario",
    };

    private static ReporteExport BuildCaja(ReporteOperativoDto<ReporteMovCajaRow> d, ReporteOperativoFiltros f) => new()
    {
        Titulo = "Reporte de Movimientos de Caja",
        Subtitulo = Subtitulo(f),
        Columnas = ["Fecha", "Tipo", "Concepto", "Método pago", "Registrado por", "Sucursal", "Referencia", "Monto"],
        Filas = d.Rows.Select(r => new[]
        {
            D(r.Fecha), r.Tipo, r.Concepto, r.MetodoPago, r.RegistradoPor, r.Sucursal, r.Referencia, Gs(r.Monto),
        }).ToList(),
        Totales = ["TOTALES", "", $"Ingresos: {Gs(d.Totales.GetValueOrDefault("ingresos"))}   ·   Egresos: {Gs(d.Totales.GetValueOrDefault("egresos"))}", "", "", "", "", Gs(d.Totales.GetValueOrDefault("neto"))],
        ColumnasNumericas = [7],
        FileBaseName = "reporte-caja",
    };
}
