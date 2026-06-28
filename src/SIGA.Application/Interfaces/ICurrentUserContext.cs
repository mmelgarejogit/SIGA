namespace SIGA.Application.Interfaces;

/// <summary>
/// Acceso al usuario y la sucursal del request actual, resueltos desde los claims del JWT.
/// La sucursal es fija por usuario (claim "sucursal_id"); un usuario global (admin) no la tiene
/// y ve/opera sobre todas las sucursales.
/// </summary>
public interface ICurrentUserContext
{
    int? UserId { get; }
    int? SucursalId { get; }
    /// <summary>true cuando el usuario no está atado a una sucursal (ve todas).</summary>
    bool EsGlobal { get; }
    bool TienePermiso(string permiso);
}
