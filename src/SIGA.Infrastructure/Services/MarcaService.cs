using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Inventario;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class MarcaService(AppDbContext db) : IMarcaService
{
    // ── Marcas ─────────────────────────────────────────────────────────────────

    public async Task<Result<IEnumerable<MarcaResponse>>> GetMarcasAsync()
    {
        var modelos = await db.Modelos
            .GroupBy(m => m.MarcaId)
            .Select(g => new { MarcaId = g.Key, Total = g.Count() })
            .ToListAsync();

        var totalMap = modelos.ToDictionary(x => x.MarcaId, x => x.Total);

        var marcas = await db.Marcas.OrderBy(m => m.Nombre).ToListAsync();

        return Result<IEnumerable<MarcaResponse>>.Success(
            marcas.Select(m => ToMarcaResponse(m, totalMap.GetValueOrDefault(m.Id, 0))));
    }

    public async Task<Result<MarcaResponse>> CreateMarcaAsync(CreateMarcaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<MarcaResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        var nombre = request.Nombre.Trim();
        if (await db.Marcas.AnyAsync(m => m.Nombre == nombre))
            return Result<MarcaResponse>.Failure("Ya existe una marca con ese nombre.", ErrorType.Conflict);

        var marca = new Marca { Nombre = nombre };
        db.Marcas.Add(marca);
        await db.SaveChangesAsync();

        return Result<MarcaResponse>.Success(ToMarcaResponse(marca, 0));
    }

    public async Task<Result<MarcaResponse>> UpdateMarcaAsync(int id, UpdateMarcaRequest request)
    {
        var marca = await db.Marcas.FindAsync(id);
        if (marca is null)
            return Result<MarcaResponse>.Failure("Marca no encontrada.", ErrorType.NotFound);

        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<MarcaResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        var nombre = request.Nombre.Trim();
        if (await db.Marcas.AnyAsync(m => m.Nombre == nombre && m.Id != id))
            return Result<MarcaResponse>.Failure("Ya existe una marca con ese nombre.", ErrorType.Conflict);

        marca.Nombre   = nombre;
        marca.IsActive = request.IsActive;
        await db.SaveChangesAsync();

        var totalModelos = await db.Modelos.CountAsync(m => m.MarcaId == id);
        return Result<MarcaResponse>.Success(ToMarcaResponse(marca, totalModelos));
    }

    public async Task<Result<bool>> DeactivateMarcaAsync(int id)
    {
        var marca = await db.Marcas.FindAsync(id);
        if (marca is null)
            return Result<bool>.Failure("Marca no encontrada.", ErrorType.NotFound);

        marca.IsActive = false;
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    // ── Modelos ────────────────────────────────────────────────────────────────

    public async Task<Result<IEnumerable<ModeloResponse>>> GetModelosAsync(int? marcaId)
    {
        var query = db.Modelos.Include(m => m.Marca).AsQueryable();

        if (marcaId.HasValue)
            query = query.Where(m => m.MarcaId == marcaId.Value);

        var modelos = await query.OrderBy(m => m.Marca.Nombre).ThenBy(m => m.Nombre).ToListAsync();

        return Result<IEnumerable<ModeloResponse>>.Success(modelos.Select(ToModeloResponse));
    }

    public async Task<Result<ModeloResponse>> CreateModeloAsync(CreateModeloRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<ModeloResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        var marca = await db.Marcas.FindAsync(request.MarcaId);
        if (marca is null)
            return Result<ModeloResponse>.Failure("La marca no existe.", ErrorType.NotFound);

        var nombre = request.Nombre.Trim();
        if (await db.Modelos.AnyAsync(m => m.MarcaId == request.MarcaId && m.Nombre == nombre))
            return Result<ModeloResponse>.Failure("Ya existe un modelo con ese nombre para esta marca.", ErrorType.Conflict);

        var modelo = new Modelo { Nombre = nombre, MarcaId = request.MarcaId };
        db.Modelos.Add(modelo);
        await db.SaveChangesAsync();

        modelo.Marca = marca;
        return Result<ModeloResponse>.Success(ToModeloResponse(modelo));
    }

    public async Task<Result<ModeloResponse>> UpdateModeloAsync(int id, UpdateModeloRequest request)
    {
        var modelo = await db.Modelos.Include(m => m.Marca).FirstOrDefaultAsync(m => m.Id == id);
        if (modelo is null)
            return Result<ModeloResponse>.Failure("Modelo no encontrado.", ErrorType.NotFound);

        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<ModeloResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        var marca = await db.Marcas.FindAsync(request.MarcaId);
        if (marca is null)
            return Result<ModeloResponse>.Failure("La marca no existe.", ErrorType.NotFound);

        var nombre = request.Nombre.Trim();
        if (await db.Modelos.AnyAsync(m => m.MarcaId == request.MarcaId && m.Nombre == nombre && m.Id != id))
            return Result<ModeloResponse>.Failure("Ya existe un modelo con ese nombre para esta marca.", ErrorType.Conflict);

        modelo.Nombre   = nombre;
        modelo.MarcaId  = request.MarcaId;
        modelo.IsActive = request.IsActive;
        await db.SaveChangesAsync();

        modelo.Marca = marca;
        return Result<ModeloResponse>.Success(ToModeloResponse(modelo));
    }

    public async Task<Result<bool>> DeactivateModeloAsync(int id)
    {
        var modelo = await db.Modelos.FindAsync(id);
        if (modelo is null)
            return Result<bool>.Failure("Modelo no encontrado.", ErrorType.NotFound);

        modelo.IsActive = false;
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static MarcaResponse ToMarcaResponse(Marca m, int totalModelos) => new()
    {
        Id           = m.Id,
        Nombre       = m.Nombre,
        IsActive     = m.IsActive,
        TotalModelos = totalModelos,
    };

    private static ModeloResponse ToModeloResponse(Modelo m) => new()
    {
        Id          = m.Id,
        Nombre      = m.Nombre,
        MarcaId     = m.MarcaId,
        MarcaNombre = m.Marca.Nombre,
        IsActive    = m.IsActive,
    };
}
