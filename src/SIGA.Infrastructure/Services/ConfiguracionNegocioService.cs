using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Configuracion;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class ConfiguracionNegocioService : IConfiguracionNegocioService
{
    private readonly AppDbContext _db;

    public ConfiguracionNegocioService(AppDbContext db) => _db = db;

    public async Task<Result<ConfiguracionNegocioResponse>> GetAsync()
    {
        var config = await _db.ConfiguracionNegocio.FirstOrDefaultAsync();
        if (config is null)
        {
            config = new ConfiguracionNegocio { UpdatedAt = DateTime.UtcNow };
            _db.ConfiguracionNegocio.Add(config);
            await _db.SaveChangesAsync();
        }
        return Result<ConfiguracionNegocioResponse>.Success(ToResponse(config));
    }

    public async Task<Result<ConfiguracionNegocioResponse>> UpdateAsync(UpdateConfiguracionNegocioRequest request)
    {
        var config = await _db.ConfiguracionNegocio.FirstOrDefaultAsync();
        if (config is null)
        {
            config = new ConfiguracionNegocio();
            _db.ConfiguracionNegocio.Add(config);
        }

        config.NombreFantasia = request.NombreFantasia.Trim();
        config.RazonSocial    = request.RazonSocial?.Trim();
        config.CUIT           = request.CUIT?.Trim();
        config.Direccion      = request.Direccion?.Trim();
        config.Telefono       = request.Telefono?.Trim();
        config.Email          = request.Email?.Trim();
        config.SitioWeb       = request.SitioWeb?.Trim();
        config.UpdatedAt      = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Result<ConfiguracionNegocioResponse>.Success(ToResponse(config));
    }

    private static ConfiguracionNegocioResponse ToResponse(ConfiguracionNegocio c) => new()
    {
        NombreFantasia = c.NombreFantasia,
        RazonSocial    = c.RazonSocial,
        CUIT           = c.CUIT,
        Direccion      = c.Direccion,
        Telefono       = c.Telefono,
        Email          = c.Email,
        SitioWeb       = c.SitioWeb,
        UpdatedAt      = c.UpdatedAt,
    };
}
