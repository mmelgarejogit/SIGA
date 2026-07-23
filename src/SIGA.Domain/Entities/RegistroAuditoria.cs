namespace SIGA.Domain.Entities;

/// <summary>Agrupación de alto nivel de un evento auditado (para filtrar en la vista).</summary>
public enum AuditCategoria
{
    Seguridad,
    Admin,
    Operativo,
}

/// <summary>
/// Catálogo de acciones auditadas. Se persiste como string, así que agregar un valor nuevo
/// NO requiere migración. Mantener el nombre estable una vez en producción.
/// </summary>
public enum AuditAccion
{
    // ── Seguridad ──
    LoginExitoso,
    LoginFallido,
    PasswordCambiado,
    PasswordReseteado,

    // ── Admin ──
    UsuarioDesactivado,
    RolAsignado,
    RolQuitado,
    RolCreado,
    RolActualizado,
    RolEliminado,

    // ── Operativo ──
    VentaAnulada,
    DevolucionAprobada,
    DevolucionRechazada,
    CierreCajaAprobado,
    CierreCajaRechazado,
}

/// <summary>
/// Registro append-only de un evento auditado. No tiene navegaciones de dominio a propósito:
/// guarda snapshots (UsuarioNombre) para que el historial quede estable aunque el usuario cambie.
/// </summary>
public class RegistroAuditoria
{
    public int Id { get; set; }
    public DateTime FechaHora { get; set; }          // UTC
    public AuditCategoria Categoria { get; set; }
    public AuditAccion Accion { get; set; }

    /// <summary>Autor del evento. Null en un login fallido de un usuario desconocido.</summary>
    public int? UserId { get; set; }
    /// <summary>Snapshot legible del autor (o el email intentado en un login fallido).</summary>
    public string UsuarioNombre { get; set; } = string.Empty;

    /// <summary>Recurso afectado, ej. "Venta", "User", "Role". Opcional.</summary>
    public string? Entidad { get; set; }
    public int? EntidadId { get; set; }

    /// <summary>Descripción legible en español, ej. "Anuló la venta 0001-45".</summary>
    public string Descripcion { get; set; } = string.Empty;

    public int? SucursalId { get; set; }
    public string? IpAddress { get; set; }
}

/// <summary>Mapa único acción → categoría. Fuente de verdad para clasificar y para el filtro de la vista.</summary>
public static class AuditCatalog
{
    public static readonly IReadOnlyDictionary<AuditAccion, AuditCategoria> Categorias =
        new Dictionary<AuditAccion, AuditCategoria>
        {
            [AuditAccion.LoginExitoso]            = AuditCategoria.Seguridad,
            [AuditAccion.LoginFallido]            = AuditCategoria.Seguridad,
            [AuditAccion.PasswordCambiado]        = AuditCategoria.Seguridad,
            [AuditAccion.PasswordReseteado]       = AuditCategoria.Seguridad,
            [AuditAccion.UsuarioDesactivado]      = AuditCategoria.Admin,
            [AuditAccion.RolAsignado]             = AuditCategoria.Admin,
            [AuditAccion.RolQuitado]              = AuditCategoria.Admin,
            [AuditAccion.RolCreado]               = AuditCategoria.Admin,
            [AuditAccion.RolActualizado]          = AuditCategoria.Admin,
            [AuditAccion.RolEliminado]            = AuditCategoria.Admin,
            [AuditAccion.VentaAnulada]            = AuditCategoria.Operativo,
            [AuditAccion.DevolucionAprobada]      = AuditCategoria.Operativo,
            [AuditAccion.DevolucionRechazada]     = AuditCategoria.Operativo,
            [AuditAccion.CierreCajaAprobado]      = AuditCategoria.Operativo,
            [AuditAccion.CierreCajaRechazado]     = AuditCategoria.Operativo,
        };

    public static AuditCategoria CategoriaDe(AuditAccion accion) =>
        Categorias.TryGetValue(accion, out var c) ? c : AuditCategoria.Operativo;
}
