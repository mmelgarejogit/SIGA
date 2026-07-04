using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Ventas;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class TimbradoService(AppDbContext db, ICurrentUserContext current) : ITimbradoService
{
    public async Task<Result<IEnumerable<TimbradoDto>>> GetAllAsync()
    {
        var query = db.Timbrados.Include(t => t.Sucursal).AsQueryable();
        if (current.SucursalId is int b)
            query = query.Where(t => t.SucursalId == b);
        var items = await query.OrderBy(t => t.NumeroTimbrado).ToListAsync();
        return Result<IEnumerable<TimbradoDto>>.Success(items.Select(ToDto));
    }

    public async Task<Result<IEnumerable<TimbradoDto>>> GetActivosAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var query = db.Timbrados
            .Include(t => t.Sucursal)
            .Where(t => t.IsActive && t.FechaInicioVigencia <= today && t.FechaFinVigencia >= today);
        if (current.SucursalId is int b)
            query = query.Where(t => t.SucursalId == b);
        var items = await query.OrderBy(t => t.NumeroTimbrado).ToListAsync();
        return Result<IEnumerable<TimbradoDto>>.Success(items.Select(ToDto));
    }

    public async Task<Result<TimbradoDto>> GetByIdAsync(int id)
    {
        var item = await db.Timbrados.FindAsync(id);
        if (item is null) return Result<TimbradoDto>.Failure("Timbrado no encontrado.", ErrorType.NotFound);
        return Result<TimbradoDto>.Success(ToDto(item));
    }

    public async Task<Result<TimbradoDto>> CreateAsync(CreateTimbradoRequest request)
    {
        var numTimbrado = request.NumeroTimbrado.Trim();
        var estable = request.Establecimiento.Trim();
        var punto = request.PuntoExpedicion.Trim();

        if (string.IsNullOrWhiteSpace(numTimbrado))
            return Result<TimbradoDto>.Failure("El número de timbrado es obligatorio.", ErrorType.Validation);
        if (!EsTresDigitos(estable))
            return Result<TimbradoDto>.Failure("El establecimiento debe ser exactamente 3 dígitos numéricos.", ErrorType.Validation);
        if (!EsTresDigitos(punto))
            return Result<TimbradoDto>.Failure("El punto de expedición debe ser exactamente 3 dígitos numéricos.", ErrorType.Validation);
        if (request.FechaFinVigencia < request.FechaInicioVigencia)
            return Result<TimbradoDto>.Failure("La fecha de fin de vigencia debe ser mayor o igual a la fecha de inicio.", ErrorType.Validation);

        if (await db.Timbrados.AnyAsync(t =>
            t.NumeroTimbrado == numTimbrado &&
            t.Establecimiento == estable &&
            t.PuntoExpedicion == punto))
            return Result<TimbradoDto>.Failure("Ya existe un timbrado con esa serie (número, establecimiento, punto).", ErrorType.Conflict);

        var sucursalId = request.SucursalId != 0
            ? request.SucursalId
            : await SucursalResolver.WriteBranchAsync(db, current);
        if (!await db.Sucursales.AnyAsync(s => s.Id == sucursalId))
            return Result<TimbradoDto>.Failure("Sucursal no encontrada.", ErrorType.Validation);

        var item = new Timbrado
        {
            SucursalId = sucursalId,
            NumeroTimbrado = numTimbrado,
            Establecimiento = estable,
            PuntoExpedicion = punto,
            UltimoNumero = 0,
            NumeroDesde = request.NumeroDesde,
            NumeroHasta = request.NumeroHasta,
            FechaInicioVigencia = request.FechaInicioVigencia,
            FechaFinVigencia = request.FechaFinVigencia,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        db.Timbrados.Add(item);
        await db.SaveChangesAsync();
        return Result<TimbradoDto>.Success(ToDto(item));
    }

    public async Task<Result<TimbradoDto>> UpdateAsync(int id, UpdateTimbradoRequest request)
    {
        var item = await db.Timbrados.FindAsync(id);
        if (item is null) return Result<TimbradoDto>.Failure("Timbrado no encontrado.", ErrorType.NotFound);

        var numTimbrado = request.NumeroTimbrado.Trim();
        var estable = request.Establecimiento.Trim();
        var punto = request.PuntoExpedicion.Trim();

        if (string.IsNullOrWhiteSpace(numTimbrado))
            return Result<TimbradoDto>.Failure("El número de timbrado es obligatorio.", ErrorType.Validation);
        if (!EsTresDigitos(estable))
            return Result<TimbradoDto>.Failure("El establecimiento debe ser exactamente 3 dígitos numéricos.", ErrorType.Validation);
        if (!EsTresDigitos(punto))
            return Result<TimbradoDto>.Failure("El punto de expedición debe ser exactamente 3 dígitos numéricos.", ErrorType.Validation);
        if (request.FechaFinVigencia < request.FechaInicioVigencia)
            return Result<TimbradoDto>.Failure("La fecha de fin de vigencia debe ser mayor o igual a la fecha de inicio.", ErrorType.Validation);

        if (await db.Timbrados.AnyAsync(t =>
            t.NumeroTimbrado == numTimbrado &&
            t.Establecimiento == estable &&
            t.PuntoExpedicion == punto &&
            t.Id != id))
            return Result<TimbradoDto>.Failure("Ya existe otro timbrado con esa serie (número, establecimiento, punto).", ErrorType.Conflict);

        if (request.SucursalId != 0 && request.SucursalId != item.SucursalId)
        {
            if (!await db.Sucursales.AnyAsync(s => s.Id == request.SucursalId))
                return Result<TimbradoDto>.Failure("Sucursal no encontrada.", ErrorType.Validation);
            item.SucursalId = request.SucursalId;
        }

        item.NumeroTimbrado = numTimbrado;
        item.Establecimiento = estable;
        item.PuntoExpedicion = punto;
        item.NumeroDesde = request.NumeroDesde;
        item.NumeroHasta = request.NumeroHasta;
        item.FechaInicioVigencia = request.FechaInicioVigencia;
        item.FechaFinVigencia = request.FechaFinVigencia;
        item.IsActive = request.IsActive;
        await db.SaveChangesAsync();
        return Result<TimbradoDto>.Success(ToDto(item));
    }

    public async Task<Result<bool>> DeactivateAsync(int id)
    {
        var item = await db.Timbrados.FindAsync(id);
        if (item is null) return Result<bool>.Failure("Timbrado no encontrado.", ErrorType.NotFound);
        item.IsActive = false;
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    private static TimbradoDto ToDto(Timbrado t) => new()
    {
        Id = t.Id,
        SucursalId = t.SucursalId,
        SucursalNombre = t.Sucursal?.Nombre,
        NumeroTimbrado = t.NumeroTimbrado,
        Establecimiento = t.Establecimiento,
        PuntoExpedicion = t.PuntoExpedicion,
        UltimoNumero = t.UltimoNumero,
        NumeroDesde = t.NumeroDesde,
        NumeroHasta = t.NumeroHasta,
        FechaInicioVigencia = t.FechaInicioVigencia,
        FechaFinVigencia = t.FechaFinVigencia,
        IsActive = t.IsActive,
        CreatedAt = t.CreatedAt,
    };

    private static bool EsTresDigitos(string s) =>
        s.Length == 3 && int.TryParse(s, out _);
}