using SIGA.Application.Common;
using SIGA.Application.DTOs.Auditoria;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;

namespace SIGA.Tests.Infrastructure;

/// <summary>
/// Doble de <see cref="IAuditService"/> que anota en memoria lo que se le pide registrar,
/// en vez de escribir en la base. Permite verificar que una operación de negocio deja su
/// rastro de auditoría sin arrastrar la dependencia de IHttpContextAccessor.
/// </summary>
public sealed class RecordingAuditService : IAuditService
{
    public List<(AuditAccion Accion, string Descripcion, string? Entidad, int? EntidadId)> Registros { get; } = [];

    public Task LogAsync(
        AuditAccion accion,
        string descripcion,
        string? entidad = null,
        int? entidadId = null,
        int? userIdOverride = null,
        string? usuarioNombreOverride = null)
    {
        Registros.Add((accion, descripcion, entidad, entidadId));
        return Task.CompletedTask;
    }

    public bool Registro(AuditAccion accion) => Registros.Any(r => r.Accion == accion);

    public Task<Result<PagedResult<RegistroAuditoriaDto>>> GetRegistrosAsync(AuditoriaFiltros f) =>
        throw new NotSupportedException("El doble de auditoría solo registra; no se consulta desde los tests.");

    public IReadOnlyList<AuditAccionDto> GetAcciones() => [];
}
