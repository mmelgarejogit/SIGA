using SIGA.Application.Common;
using SIGA.Application.DTOs.Auditoria;
using SIGA.Domain.Entities;

namespace SIGA.Application.Interfaces;

public interface IAuditService
{
    /// <summary>
    /// Registra un evento auditado. Llamar SIEMPRE después de persistir la acción de negocio
    /// (hace su propio SaveChanges). El autor y la sucursal se toman del usuario actual;
    /// para eventos anónimos (login) pasar userIdOverride / usuarioNombreOverride.
    /// </summary>
    Task LogAsync(
        AuditAccion accion,
        string descripcion,
        string? entidad = null,
        int? entidadId = null,
        int? userIdOverride = null,
        string? usuarioNombreOverride = null);

    Task<Result<PagedResult<RegistroAuditoriaDto>>> GetRegistrosAsync(AuditoriaFiltros f);

    /// <summary>Catálogo de acciones (con su categoría) para poblar el filtro de la vista.</summary>
    IReadOnlyList<AuditAccionDto> GetAcciones();
}
