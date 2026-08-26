using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Auditoria;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class AuditService(AppDbContext db, ICurrentUserContext current, IHttpContextAccessor http)
    : IAuditService
{
    public async Task LogAsync(
        AuditAccion accion,
        string descripcion,
        string? entidad = null,
        int? entidadId = null,
        int? userIdOverride = null,
        string? usuarioNombreOverride = null)
    {
        var actorId = userIdOverride ?? current.UserId;

        var nombre = usuarioNombreOverride;
        if (string.IsNullOrWhiteSpace(nombre) && actorId.HasValue)
        {
            nombre = await db.Users
                .Where(u => u.Id == actorId.Value)
                .Select(u => u.Person.FirstName + " " + u.Person.LastName)
                .FirstOrDefaultAsync();
        }

        db.Set<RegistroAuditoria>().Add(new RegistroAuditoria
        {
            FechaHora     = DateTime.UtcNow,
            Categoria     = AuditCatalog.CategoriaDe(accion),
            Accion        = accion,
            UserId        = actorId,
            UsuarioNombre = string.IsNullOrWhiteSpace(nombre) ? "—" : nombre.Trim(),
            Entidad       = entidad,
            EntidadId     = entidadId,
            Descripcion   = descripcion,
            SucursalId    = current.SucursalId,
            IpAddress     = http.HttpContext?.Connection.RemoteIpAddress?.ToString(),
        });

        await db.SaveChangesAsync();
    }

    public async Task<Result<PagedResult<RegistroAuditoriaDto>>> GetRegistrosAsync(AuditoriaFiltros f)
    {
        var query = db.Set<RegistroAuditoria>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(f.Categoria) && Enum.TryParse<AuditCategoria>(f.Categoria, out var cat))
            query = query.Where(r => r.Categoria == cat);

        if (!string.IsNullOrWhiteSpace(f.Accion) && Enum.TryParse<AuditAccion>(f.Accion, out var acc))
            query = query.Where(r => r.Accion == acc);

        if (f.UserId.HasValue)
            query = query.Where(r => r.UserId == f.UserId.Value);

        if (DateOnly.TryParse(f.FechaDesde, out var desde))
        {
            var d = DateTime.SpecifyKind(desde.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            query = query.Where(r => r.FechaHora >= d);
        }

        if (DateOnly.TryParse(f.FechaHasta, out var hasta))
        {
            var h = DateTime.SpecifyKind(hasta.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            query = query.Where(r => r.FechaHora < h);
        }

        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            var term = $"%{f.Search.Trim()}%";
            query = query.Where(r =>
                EF.Functions.ILike(r.Descripcion, term) ||
                EF.Functions.ILike(r.UsuarioNombre, term));
        }

        var page = f.Page < 1 ? 1 : f.Page;
        var pageSize = f.PageSize is < 1 or > 100 ? 20 : f.PageSize;

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.FechaHora)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RegistroAuditoriaDto
            {
                Id             = r.Id,
                FechaHora      = r.FechaHora.ToString("o"),
                Categoria      = r.Categoria.ToString(),
                Accion         = r.Accion.ToString(),
                UserId         = r.UserId,
                UsuarioNombre  = r.UsuarioNombre,
                Entidad        = r.Entidad,
                EntidadId      = r.EntidadId,
                Descripcion    = r.Descripcion,
                SucursalId     = r.SucursalId,
                SucursalNombre = r.SucursalId == null
                    ? null
                    : db.Sucursales.Where(s => s.Id == r.SucursalId).Select(s => s.Nombre).FirstOrDefault(),
                IpAddress      = r.IpAddress,
            })
            .ToListAsync();

        return Result<PagedResult<RegistroAuditoriaDto>>.Success(new PagedResult<RegistroAuditoriaDto>
        {
            Items      = items,
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
        });
    }

    public IReadOnlyList<AuditAccionDto> GetAcciones() =>
        Enum.GetValues<AuditAccion>()
            .Select(a => new AuditAccionDto
            {
                Accion    = a.ToString(),
                Categoria = AuditCatalog.CategoriaDe(a).ToString(),
            })
            .ToList();
}
