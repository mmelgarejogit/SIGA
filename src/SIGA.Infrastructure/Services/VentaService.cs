using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Ventas;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class VentaService(AppDbContext db) : IVentaService
{
    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static VentaDto Map(Venta v) => new()
    {
        Id                = v.Id,
        NumeroComprobante = v.NumeroComprobante,
        PatientId         = v.PatientId,
        PacienteNombre    = $"{v.Patient?.User?.Person?.FirstName} {v.Patient?.User?.Person?.LastName}".Trim(),
        RecetaId          = v.RecetaId,
        Estado            = v.Estado.ToString(),
        CondicionVenta    = v.CondicionVenta.ToString(),
        FechaVenta        = v.FechaVenta.ToString("yyyy-MM-dd"),
        FechaVencimiento  = v.FechaVencimiento?.ToString("yyyy-MM-dd"),
        MontoExento       = v.MontoExento,
        MontoGravado5     = v.MontoGravado5,
        MontoGravado10    = v.MontoGravado10,
        Total             = v.Total,
        TotalCobrado      = v.TotalCobrado,
        SaldoPendiente    = v.SaldoPendiente,
        Observaciones     = v.Observaciones,
        CreatedAt         = v.CreatedAt,
        Lineas = v.Lineas.Select(l => new VentaLineaDto
        {
            Id             = l.Id,
            Tipo           = l.Tipo.ToString(),
            ProductoVarianteId = l.ProductoVarianteId,
            ServicioId     = l.ServicioId,
            Descripcion    = l.Descripcion,
            Cantidad       = l.Cantidad,
            PrecioUnitario = l.PrecioUnitario,
            Descuento      = l.Descuento,
            CategoriaFiscal = l.CategoriaFiscal.ToString(),
            Subtotal       = l.Subtotal,
        }).ToList(),
        Cobros = v.Cobros.Select(c => new CobroDto
        {
            Id         = c.Id,
            Monto      = c.Monto,
            MetodoPago = c.MetodoPago.ToString(),
            FechaCobro = c.FechaCobro.ToString("yyyy-MM-dd"),
            Referencia = c.Referencia,
            Anulado    = c.Anulado,
            CreatedAt  = c.CreatedAt,
        }).ToList(),
        Factura = v.Factura == null ? null : new FacturaVentaDto
        {
            Id              = v.Factura.Id,
            NumeroFactura   = v.Factura.NumeroFactura,
            Timbrado        = v.Factura.Timbrado,
            Establecimiento = v.Factura.Establecimiento,
            MontoExento     = v.Factura.MontoExento,
            MontoGravado5   = v.Factura.MontoGravado5,
            MontoGravado10  = v.Factura.MontoGravado10,
            Iva5            = v.Factura.Iva5,
            Iva10           = v.Factura.Iva10,
            Total           = v.Factura.Total,
            FechaEmision    = v.Factura.FechaEmision.ToString("yyyy-MM-dd"),
            Observaciones   = v.Factura.Observaciones,
        },
    };

    private IQueryable<Venta> BaseQuery() =>
        db.Ventas
            .Include(v => v.Patient).ThenInclude(p => p.User).ThenInclude(u => u!.Person)
            .Include(v => v.Lineas).ThenInclude(l => l.ProductoVariante)
            .Include(v => v.Lineas).ThenInclude(l => l.Servicio)
            .Include(v => v.Cobros)
            .Include(v => v.Factura);

    // ── Queries ──────────────────────────────────────────────────────────────────

    public async Task<Result<VentaDto>> GetVentaByIdAsync(int id)
    {
        var venta = await BaseQuery().FirstOrDefaultAsync(v => v.Id == id);
        if (venta == null) return Result<VentaDto>.Failure("Venta no encontrada", ErrorType.NotFound);
        return Result<VentaDto>.Success(Map(venta));
    }

    public async Task<Result<PagedResult<VentaDto>>> GetVentasAsync(
        string? estado, string? fechaDesde, string? fechaHasta, int? patientId, int page, int pageSize)
    {
        var query = BaseQuery();

        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoVenta>(estado, out var e))
            query = query.Where(v => v.Estado == e);

        if (!string.IsNullOrWhiteSpace(fechaDesde) && DateOnly.TryParse(fechaDesde, out var desde))
            query = query.Where(v => v.FechaVenta >= desde);

        if (!string.IsNullOrWhiteSpace(fechaHasta) && DateOnly.TryParse(fechaHasta, out var hasta))
            query = query.Where(v => v.FechaVenta <= hasta);

        if (patientId.HasValue)
            query = query.Where(v => v.PatientId == patientId.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<PagedResult<VentaDto>>.Success(new PagedResult<VentaDto>
        {
            Items      = items.Select(Map).ToList(),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
        });
    }

    // ── Commands ─────────────────────────────────────────────────────────────────

    public async Task<Result<VentaDto>> CrearVentaAsync(CrearVentaRequest request)
    {
        if (!request.Lineas.Any())
            return Result<VentaDto>.Failure("La venta debe tener al menos una línea", ErrorType.Validation);

        var condicion = Enum.TryParse<CondicionVenta>(request.CondicionVenta, out var c) ? c : CondicionVenta.Contado;

        if (condicion == CondicionVenta.Credito && string.IsNullOrWhiteSpace(request.FechaVencimiento))
            return Result<VentaDto>.Failure("Las ventas a crédito requieren fecha de vencimiento", ErrorType.Validation);

        var venta = new Venta
        {
            NumeroComprobante = "",
            PatientId         = request.PatientId,
            RecetaId          = request.RecetaId,
            CondicionVenta    = condicion,
            FechaVenta        = DateOnly.Parse(request.FechaVenta),
            FechaVencimiento  = string.IsNullOrWhiteSpace(request.FechaVencimiento) ? null : DateOnly.Parse(request.FechaVencimiento),
            Observaciones     = request.Observaciones,
            Estado            = EstadoVenta.Abierta,
            CreatedAt         = DateTime.UtcNow,
            UpdatedAt         = DateTime.UtcNow,
        };

        foreach (var lr in request.Lineas)
        {
            var tipo = Enum.TryParse<TipoLineaVenta>(lr.Tipo, out var t) ? t : TipoLineaVenta.Producto;
            var cat  = Enum.TryParse<CategoriaFiscal>(lr.CategoriaFiscal, out var cf) ? cf : CategoriaFiscal.Gravado10;

            string descripcion = lr.Descripcion ?? "";
            if (string.IsNullOrWhiteSpace(descripcion))
            {
                if (tipo == TipoLineaVenta.Producto && lr.ProductoVarianteId.HasValue)
                {
                    var variante = await db.ProductoVariantes.Include(v => v.Producto).FirstOrDefaultAsync(v => v.Id == lr.ProductoVarianteId.Value);
                    descripcion = variante?.Producto?.Nombre ?? "Producto";
                }
                else if (tipo == TipoLineaVenta.Servicio && lr.ServicioId.HasValue)
                {
                    var serv = await db.Servicios.FindAsync(lr.ServicioId.Value);
                    descripcion = serv?.Nombre ?? "Servicio";
                }
            }

            venta.Lineas.Add(new VentaLinea
            {
                Tipo               = tipo,
                ProductoVarianteId = lr.ProductoVarianteId,
                ServicioId         = lr.ServicioId,
                Descripcion    = descripcion,
                Cantidad       = lr.Cantidad,
                PrecioUnitario = lr.PrecioUnitario,
                Descuento      = lr.Descuento,
                CategoriaFiscal = cat,
            });
        }

        db.Ventas.Add(venta);
        await db.SaveChangesAsync();

        venta.NumeroComprobante = $"REC-{venta.Id:D7}";
        await db.SaveChangesAsync();

        return await GetVentaByIdAsync(venta.Id);
    }

    public async Task<Result<VentaDto>> ConfirmarVentaAsync(int id)
    {
        var venta = await db.Ventas.FindAsync(id);
        if (venta == null) return Result<VentaDto>.Failure("Venta no encontrada", ErrorType.NotFound);
        if (venta.Estado != EstadoVenta.Abierta)
            return Result<VentaDto>.Failure("Solo se pueden confirmar ventas abiertas", ErrorType.Conflict);

        venta.Estado    = venta.CondicionVenta == CondicionVenta.Contado ? EstadoVenta.Confirmada : EstadoVenta.PendienteDePago;
        venta.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return await GetVentaByIdAsync(id);
    }

    public async Task<Result<VentaDto>> RegistrarCobroAsync(RegistrarCobroRequest request)
    {
        var venta = await BaseQuery().FirstOrDefaultAsync(v => v.Id == request.VentaId);
        if (venta == null) return Result<VentaDto>.Failure("Venta no encontrada", ErrorType.NotFound);

        if (venta.Estado == EstadoVenta.Anulada)
            return Result<VentaDto>.Failure("No se puede cobrar una venta anulada", ErrorType.Conflict);

        if (venta.Estado == EstadoVenta.Pagada || venta.Estado == EstadoVenta.Cobrada)
            return Result<VentaDto>.Failure("La venta ya fue cobrada completamente", ErrorType.Conflict);

        if (!Enum.TryParse<MetodoPago>(request.MetodoPago, out var metodo))
            return Result<VentaDto>.Failure("Método de pago inválido", ErrorType.Validation);

        var cobro = new Cobro
        {
            VentaId    = request.VentaId,
            Monto      = request.Monto,
            MetodoPago = metodo,
            FechaCobro = DateOnly.Parse(request.FechaCobro),
            Referencia = request.Referencia,
            Anulado    = false,
            CreatedAt  = DateTime.UtcNow,
        };

        db.Cobros.Add(cobro);

        // Determine if fully paid after this cobro
        var totalCobrado = venta.Cobros.Where(c => !c.Anulado).Sum(c => c.Monto) + request.Monto;
        if (totalCobrado >= venta.Total)
        {
            venta.Estado = venta.CondicionVenta == CondicionVenta.Contado ? EstadoVenta.Pagada : EstadoVenta.Cobrada;
        }
        else if (venta.Estado == EstadoVenta.Confirmada || venta.Estado == EstadoVenta.Abierta)
        {
            venta.Estado = EstadoVenta.PendienteDePago;
        }

        // Register cash movement
        db.MovimientosCaja.Add(new MovimientoCaja
        {
            Tipo       = TipoMovimientoCaja.Ingreso,
            Monto      = request.Monto,
            Concepto   = $"Cobro venta {venta.NumeroComprobante}",
            MetodoPago = metodo,
            VentaId    = venta.Id,
            Fecha      = cobro.FechaCobro,
            CreatedAt  = DateTime.UtcNow,
        });

        venta.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return await GetVentaByIdAsync(request.VentaId);
    }

    public async Task<Result<VentaDto>> EmitirFacturaAsync(EmitirFacturaRequest request)
    {
        var venta = await BaseQuery().FirstOrDefaultAsync(v => v.Id == request.VentaId);
        if (venta == null) return Result<VentaDto>.Failure("Venta no encontrada", ErrorType.NotFound);
        if (venta.Factura != null) return Result<VentaDto>.Failure("La venta ya tiene factura emitida", ErrorType.Conflict);

        var factura = new FacturaVenta
        {
            VentaId         = request.VentaId,
            NumeroFactura   = request.NumeroFactura,
            Timbrado        = request.Timbrado,
            Establecimiento = request.Establecimiento,
            MontoExento     = venta.MontoExento,
            MontoGravado5   = venta.MontoGravado5,
            MontoGravado10  = venta.MontoGravado10,
            FechaEmision    = DateOnly.Parse(request.FechaEmision),
            Observaciones   = request.Observaciones,
            CreatedAt       = DateTime.UtcNow,
        };

        db.FacturasVenta.Add(factura);
        await db.SaveChangesAsync();

        return await GetVentaByIdAsync(request.VentaId);
    }

    public async Task<Result<VentaDto>> AnularVentaAsync(AnularVentaRequest request)
    {
        var venta = await BaseQuery().FirstOrDefaultAsync(v => v.Id == request.VentaId);
        if (venta == null) return Result<VentaDto>.Failure("Venta no encontrada", ErrorType.NotFound);
        if (!venta.PuedeAnularse()) return Result<VentaDto>.Failure("La venta no puede anularse en su estado actual", ErrorType.Conflict);

        venta.Estado      = EstadoVenta.Anulada;
        venta.Observaciones = string.IsNullOrWhiteSpace(venta.Observaciones)
            ? $"Anulada: {request.Motivo}"
            : $"{venta.Observaciones} | Anulada: {request.Motivo}";
        venta.UpdatedAt   = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return await GetVentaByIdAsync(request.VentaId);
    }
}
