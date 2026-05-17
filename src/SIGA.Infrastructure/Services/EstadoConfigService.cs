using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Estados;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class EstadoConfigService(AppDbContext db) : IEstadoConfigService
{
    private static readonly string[] EntidadesValidas = ["Turno", "Pedido", "Consulta"];

    public async Task<Result<IEnumerable<EstadoConfigResponse>>> GetByEntidadAsync(string? entidad)
    {
        var query = db.EstadosConfig.AsQueryable();

        if (!string.IsNullOrWhiteSpace(entidad))
            query = query.Where(e => e.Entidad == entidad);

        var estados = await query.OrderBy(e => e.Entidad).ThenBy(e => e.Orden).ToListAsync();
        return Result<IEnumerable<EstadoConfigResponse>>.Success(estados.Select(ToResponse));
    }

    public async Task<Result<EstadoConfigResponse>> CreateAsync(CreateEstadoConfigRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<EstadoConfigResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        if (!EntidadesValidas.Contains(request.Entidad))
            return Result<EstadoConfigResponse>.Failure("Entidad inválida. Valores posibles: Turno, Pedido, Consulta.", ErrorType.Validation);

        if (await db.EstadosConfig.AnyAsync(e => e.Entidad == request.Entidad && e.Nombre == request.Nombre.Trim()))
            return Result<EstadoConfigResponse>.Failure("Ya existe un estado con ese nombre para esta entidad.", ErrorType.Conflict);

        var estado = new EstadoConfig
        {
            Entidad      = request.Entidad,
            Nombre       = request.Nombre.Trim(),
            Color        = request.Color,
            EsProtegido  = false,
            Orden        = request.Orden,
        };

        db.EstadosConfig.Add(estado);
        await db.SaveChangesAsync();
        return Result<EstadoConfigResponse>.Success(ToResponse(estado));
    }

    public async Task<Result<EstadoConfigResponse>> UpdateAsync(int id, UpdateEstadoConfigRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<EstadoConfigResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        var estado = await db.EstadosConfig.FindAsync(id);
        if (estado is null)
            return Result<EstadoConfigResponse>.Failure("Estado no encontrado.", ErrorType.NotFound);

        if (estado.EsProtegido && estado.Nombre != request.Nombre.Trim())
            return Result<EstadoConfigResponse>.Failure("No se puede cambiar el nombre de un estado protegido.", ErrorType.Validation);

        if (await db.EstadosConfig.AnyAsync(e => e.Entidad == estado.Entidad && e.Nombre == request.Nombre.Trim() && e.Id != id))
            return Result<EstadoConfigResponse>.Failure("Ya existe un estado con ese nombre para esta entidad.", ErrorType.Conflict);

        estado.Nombre = request.Nombre.Trim();
        estado.Color  = request.Color;
        estado.Orden  = request.Orden;

        await db.SaveChangesAsync();
        return Result<EstadoConfigResponse>.Success(ToResponse(estado));
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var estado = await db.EstadosConfig.FindAsync(id);
        if (estado is null)
            return Result<bool>.Failure("Estado no encontrado.", ErrorType.NotFound);

        if (estado.EsProtegido)
            return Result<bool>.Failure("No se puede eliminar un estado protegido.", ErrorType.Validation);

        var enUso = estado.Entidad switch
        {
            "Turno"    => await db.Turnos.AnyAsync(t => t.EstadoCustomId == id),
            "Consulta" => await db.ConsultasClinicas.AnyAsync(c => c.EstadoConfigId == id),
            _          => false,
        };

        if (enUso)
            return Result<bool>.Failure("No se puede eliminar el estado porque está asignado a uno o más registros.", ErrorType.Conflict);

        db.EstadosConfig.Remove(estado);
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    private static EstadoConfigResponse ToResponse(EstadoConfig e) => new()
    {
        Id           = e.Id,
        Entidad      = e.Entidad,
        Nombre       = e.Nombre,
        Color        = e.Color,
        CodigoInterno = e.CodigoInterno,
        EsProtegido  = e.EsProtegido,
        Orden        = e.Orden,
    };
}
