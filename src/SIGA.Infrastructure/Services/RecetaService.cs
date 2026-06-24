using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Clinica;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class RecetaService(AppDbContext db) : IRecetaService
{
    public async Task<Result<IEnumerable<RecetaResponse>>> GetByClienteAsync(int clienteId)
    {
        var personId = await db.Clientes
            .Where(c => c.Id == clienteId)
            .Select(c => (int?)c.PersonId)
            .FirstOrDefaultAsync();

        if (personId is null)
            return Result<IEnumerable<RecetaResponse>>.Failure("Cliente no encontrado.", ErrorType.NotFound);

        var recetas = await db.Recetas
            .Where(r => r.PersonId == personId)
            .OrderByDescending(r => r.FechaEmision)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Result<IEnumerable<RecetaResponse>>.Success(recetas.Select(ToResponse));
    }

    public async Task<Result<RecetaResponse>> CreateManualAsync(CreateRecetaManualRequest request)
    {
        var personId = await db.Clientes
            .Where(c => c.Id == request.ClienteId)
            .Select(c => (int?)c.PersonId)
            .FirstOrDefaultAsync();

        if (personId is null)
            return Result<RecetaResponse>.Failure("Cliente no encontrado.", ErrorType.Validation);

        var now = DateTime.UtcNow;
        var receta = new Receta
        {
            ConsultaClinicaId     = null,
            PersonId              = personId,
            FechaEmision          = request.FechaEmision,
            OdEsferico            = request.OdEsferico,
            OdCilindro            = request.OdCilindro,
            OdEje                 = request.OdEje,
            OdAdicion             = request.OdAdicion,
            OiEsferico            = request.OiEsferico,
            OiCilindro            = request.OiCilindro,
            OiEje                 = request.OiEje,
            OiAdicion             = request.OiAdicion,
            DistanciaInterpupilar = request.DistanciaInterpupilar,
            AvSinCorreccion       = request.AvSinCorreccion?.Trim(),
            AvConCorreccion       = request.AvConCorreccion?.Trim(),
            Observaciones         = request.Observaciones?.Trim(),
            CreatedAt             = now,
            UpdatedAt             = now,
        };

        db.Recetas.Add(receta);
        await db.SaveChangesAsync();

        return Result<RecetaResponse>.Success(ToResponse(receta));
    }

    private static RecetaResponse ToResponse(Receta r) => new()
    {
        Id                    = r.Id,
        ConsultaClinicaId     = r.ConsultaClinicaId,
        PersonId              = r.PersonId,
        EsExterna             = r.ConsultaClinicaId == null,
        FechaEmision          = r.FechaEmision,
        OdEsferico            = r.OdEsferico,
        OdCilindro            = r.OdCilindro,
        OdEje                 = r.OdEje,
        OdAdicion             = r.OdAdicion,
        OiEsferico            = r.OiEsferico,
        OiCilindro            = r.OiCilindro,
        OiEje                 = r.OiEje,
        OiAdicion             = r.OiAdicion,
        DistanciaInterpupilar = r.DistanciaInterpupilar,
        AvSinCorreccion       = r.AvSinCorreccion,
        AvConCorreccion       = r.AvConCorreccion,
        Observaciones         = r.Observaciones,
        CreatedAt             = r.CreatedAt,
        UpdatedAt             = r.UpdatedAt,
    };
}
