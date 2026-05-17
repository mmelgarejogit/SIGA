using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Egresos;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class EgresoService(AppDbContext db) : IEgresoService
{
    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static EgresoResponse Map(Egreso e) => new()
    {
        Id             = e.Id,
        Tipo           = e.Tipo.ToString(),
        Estado         = e.Estado.ToString(),
        Monto          = e.Monto,
        Concepto       = e.Concepto,
        Observaciones  = e.Observaciones,
        FechaEmision   = e.FechaEmision.ToString("yyyy-MM-dd"),
        FechaVencimiento = e.FechaVencimiento?.ToString("yyyy-MM-dd"),
        FechaPago      = e.FechaPago?.ToString("yyyy-MM-dd"),
        MetodoPago     = e.MetodoPago?.ToString(),
        EstaVencido    = e.EstaVencido(),
        CreatedAt      = e.CreatedAt,

        NroFactura         = e is FacturaCompra fc ? fc.NroFactura : null,
        ProveedorId        = e is FacturaCompra fc2 ? fc2.ProveedorId : null,
        ProveedorNombre    = e is FacturaCompra fc3 ? fc3.Proveedor?.Nombre : null,
        PedidoProveedorId  = e is FacturaCompra fc4 ? fc4.PedidoProveedorId : null,

        ProfessionalId    = e is Honorario h ? h.ProfessionalId : null,
        ProfessionalNombre = e is Honorario h2 ? $"{h2.Professional?.User?.Person?.FirstName} {h2.Professional?.User?.Person?.LastName}".Trim() : null,
        Periodo           = e is Honorario h3 ? h3.Periodo : null,

        CategoriaGastoId     = e is GastoGeneral g ? g.CategoriaGastoId : null,
        CategoriaGastoNombre = e is GastoGeneral g2 ? g2.CategoriaGasto?.Nombre : null,
    };

    private static CategoriaGastoResponse MapCategoria(CategoriaGasto c) => new()
    {
        Id          = c.Id,
        Nombre      = c.Nombre,
        Descripcion = c.Descripcion,
        Activo      = c.Activo,
    };

    private IQueryable<Egreso> BaseQuery() =>
        db.Egresos
            .Include(e => (e as FacturaCompra)!.Proveedor)
            .Include(e => (e as Honorario)!.Professional)
                .ThenInclude(p => p!.User)
                    .ThenInclude(u => u!.Person)
            .Include(e => (e as GastoGeneral)!.CategoriaGasto);

    // ── Categorías ───────────────────────────────────────────────────────────────

    public async Task<Result<IEnumerable<CategoriaGastoResponse>>> GetCategoriasAsync()
    {
        var cats = await db.CategoriasGasto.OrderBy(c => c.Nombre).ToListAsync();
        return Result<IEnumerable<CategoriaGastoResponse>>.Success(cats.Select(MapCategoria));
    }

    public async Task<Result<CategoriaGastoResponse>> CrearCategoriaAsync(CrearCategoriaGastoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<CategoriaGastoResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        var categoria = new CategoriaGasto
        {
            Nombre      = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim(),
        };
        db.CategoriasGasto.Add(categoria);
        await db.SaveChangesAsync();
        return Result<CategoriaGastoResponse>.Success(MapCategoria(categoria));
    }

    public async Task<Result<CategoriaGastoResponse>> ActualizarCategoriaAsync(int id, ActualizarCategoriaGastoRequest request)
    {
        var categoria = await db.CategoriasGasto.FindAsync(id);
        if (categoria is null)
            return Result<CategoriaGastoResponse>.Failure("Categoría no encontrada.", ErrorType.NotFound);

        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<CategoriaGastoResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        categoria.Nombre      = request.Nombre.Trim();
        categoria.Descripcion = request.Descripcion?.Trim();
        categoria.Activo      = request.Activo;
        await db.SaveChangesAsync();
        return Result<CategoriaGastoResponse>.Success(MapCategoria(categoria));
    }

    // ── Crear Egresos ─────────────────────────────────────────────────────────────

    public async Task<Result<EgresoResponse>> CrearFacturaCompraAsync(CrearFacturaCompraRequest request)
    {
        if (!DateOnly.TryParse(request.FechaEmision, out var fechaEmision))
            return Result<EgresoResponse>.Failure("Fecha de emisión inválida.", ErrorType.Validation);

        DateOnly? fechaVencimiento = null;
        if (!string.IsNullOrWhiteSpace(request.FechaVencimiento))
        {
            if (!DateOnly.TryParse(request.FechaVencimiento, out var fv))
                return Result<EgresoResponse>.Failure("Fecha de vencimiento inválida.", ErrorType.Validation);
            fechaVencimiento = fv;
        }

        var proveedorExists = await db.Proveedores.AnyAsync(p => p.Id == request.ProveedorId);
        if (!proveedorExists)
            return Result<EgresoResponse>.Failure("Proveedor no encontrado.", ErrorType.NotFound);

        var factura = new FacturaCompra
        {
            ProveedorId       = request.ProveedorId,
            PedidoProveedorId = request.PedidoProveedorId,
            NroFactura        = request.NroFactura?.Trim(),
            Monto             = request.Monto,
            Concepto          = request.Concepto.Trim(),
            Observaciones     = request.Observaciones?.Trim(),
            FechaEmision      = fechaEmision,
            FechaVencimiento  = fechaVencimiento,
            Estado            = EstadoEgreso.Pendiente,
        };
        db.FacturasCompra.Add(factura);
        await db.SaveChangesAsync();

        await db.Entry(factura).Reference(f => f.Proveedor).LoadAsync();
        return Result<EgresoResponse>.Success(Map(factura));
    }

    public async Task<Result<EgresoResponse>> CrearHonorarioAsync(CrearHonorarioRequest request)
    {
        if (!DateOnly.TryParse(request.FechaEmision, out var fechaEmision))
            return Result<EgresoResponse>.Failure("Fecha de emisión inválida.", ErrorType.Validation);

        DateOnly? fechaVencimiento = null;
        if (!string.IsNullOrWhiteSpace(request.FechaVencimiento))
        {
            if (!DateOnly.TryParse(request.FechaVencimiento, out var fv))
                return Result<EgresoResponse>.Failure("Fecha de vencimiento inválida.", ErrorType.Validation);
            fechaVencimiento = fv;
        }

        var professional = await db.Professionals
            .Include(p => p.User).ThenInclude(u => u.Person)
            .FirstOrDefaultAsync(p => p.Id == request.ProfessionalId);
        if (professional is null)
            return Result<EgresoResponse>.Failure("Profesional no encontrado.", ErrorType.NotFound);

        var honorario = new Honorario
        {
            ProfessionalId   = request.ProfessionalId,
            Monto            = request.Monto,
            Concepto         = request.Concepto.Trim(),
            Periodo          = request.Periodo?.Trim(),
            Observaciones    = request.Observaciones?.Trim(),
            FechaEmision     = fechaEmision,
            FechaVencimiento = fechaVencimiento,
            Estado           = EstadoEgreso.Pendiente,
        };
        db.Honorarios.Add(honorario);
        await db.SaveChangesAsync();

        honorario.Professional = professional;
        return Result<EgresoResponse>.Success(Map(honorario));
    }

    public async Task<Result<EgresoResponse>> CrearGastoGeneralAsync(CrearGastoGeneralRequest request)
    {
        if (!DateOnly.TryParse(request.FechaEmision, out var fechaEmision))
            return Result<EgresoResponse>.Failure("Fecha de emisión inválida.", ErrorType.Validation);

        DateOnly? fechaVencimiento = null;
        if (!string.IsNullOrWhiteSpace(request.FechaVencimiento))
        {
            if (!DateOnly.TryParse(request.FechaVencimiento, out var fv))
                return Result<EgresoResponse>.Failure("Fecha de vencimiento inválida.", ErrorType.Validation);
            fechaVencimiento = fv;
        }

        var categoria = await db.CategoriasGasto.FindAsync(request.CategoriaGastoId);
        if (categoria is null)
            return Result<EgresoResponse>.Failure("Categoría no encontrada.", ErrorType.NotFound);

        var gasto = new GastoGeneral
        {
            CategoriaGastoId = request.CategoriaGastoId,
            Monto            = request.Monto,
            Concepto         = request.Concepto.Trim(),
            Observaciones    = request.Observaciones?.Trim(),
            FechaEmision     = fechaEmision,
            FechaVencimiento = fechaVencimiento,
            Estado           = EstadoEgreso.Pendiente,
        };
        db.GastosGenerales.Add(gasto);
        await db.SaveChangesAsync();

        gasto.CategoriaGasto = categoria;
        return Result<EgresoResponse>.Success(Map(gasto));
    }

    // ── Transiciones de estado ────────────────────────────────────────────────────

    public async Task<Result<EgresoResponse>> RegistrarPagoAsync(int id, RegistrarPagoRequest request)
    {
        var egreso = await BaseQuery().FirstOrDefaultAsync(e => e.Id == id);
        if (egreso is null)
            return Result<EgresoResponse>.Failure("Egreso no encontrado.", ErrorType.NotFound);

        if (!Enum.TryParse<MetodoPago>(request.MetodoPago, ignoreCase: true, out var metodo))
            return Result<EgresoResponse>.Failure("Método de pago inválido.", ErrorType.Validation);

        if (!DateOnly.TryParse(request.FechaPago, out var fechaPago))
            return Result<EgresoResponse>.Failure("Fecha de pago inválida.", ErrorType.Validation);

        try
        {
            egreso.RegistrarPago(metodo, fechaPago);
        }
        catch (InvalidOperationException ex)
        {
            return Result<EgresoResponse>.Failure(ex.Message, ErrorType.Conflict);
        }

        await db.SaveChangesAsync();
        return Result<EgresoResponse>.Success(Map(egreso));
    }

    public async Task<Result<EgresoResponse>> AnularEgresoAsync(int id, AnularEgresoRequest request)
    {
        var egreso = await BaseQuery().FirstOrDefaultAsync(e => e.Id == id);
        if (egreso is null)
            return Result<EgresoResponse>.Failure("Egreso no encontrado.", ErrorType.NotFound);

        if (egreso.Estado == EstadoEgreso.Anulado)
            return Result<EgresoResponse>.Failure("El egreso ya está anulado.", ErrorType.Conflict);

        if (egreso.Estado == EstadoEgreso.Pagado)
            return Result<EgresoResponse>.Failure("No se puede anular un egreso pagado.", ErrorType.Conflict);

        egreso.Estado     = EstadoEgreso.Anulado;
        egreso.UpdatedAt  = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.Motivo))
            egreso.Observaciones = (egreso.Observaciones is not null
                ? egreso.Observaciones + " | " : "") + $"Anulado: {request.Motivo.Trim()}";

        await db.SaveChangesAsync();
        return Result<EgresoResponse>.Success(Map(egreso));
    }

    // ── Consultas ─────────────────────────────────────────────────────────────────

    public async Task<Result<EgresoResponse>> GetEgresoByIdAsync(int id)
    {
        var egreso = await BaseQuery().FirstOrDefaultAsync(e => e.Id == id);
        if (egreso is null)
            return Result<EgresoResponse>.Failure("Egreso no encontrado.", ErrorType.NotFound);
        return Result<EgresoResponse>.Success(Map(egreso));
    }

    public async Task<Result<PagedResult<EgresoResponse>>> GetEgresosAsync(
        string? tipo, string? estado, string? fechaDesde, string? fechaHasta,
        bool? soloVencidos, int page, int pageSize)
    {
        var query = BaseQuery();

        if (!string.IsNullOrWhiteSpace(tipo) && Enum.TryParse<TipoEgreso>(tipo, ignoreCase: true, out var t))
            query = query.Where(e => e.Tipo == t);

        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoEgreso>(estado, ignoreCase: true, out var s))
            query = query.Where(e => e.Estado == s);

        if (DateOnly.TryParse(fechaDesde, out var desde))
            query = query.Where(e => e.FechaEmision >= desde);

        if (DateOnly.TryParse(fechaHasta, out var hasta))
            query = query.Where(e => e.FechaEmision <= hasta);

        if (soloVencidos == true)
        {
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
            query = query.Where(e =>
                e.Estado != EstadoEgreso.Pagado &&
                e.Estado != EstadoEgreso.Anulado &&
                e.FechaVencimiento.HasValue &&
                e.FechaVencimiento.Value < hoy);
        }

        var total  = await query.CountAsync();
        var items  = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<PagedResult<EgresoResponse>>.Success(new PagedResult<EgresoResponse>
        {
            Items      = items.Select(Map),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize,
        });
    }
}
