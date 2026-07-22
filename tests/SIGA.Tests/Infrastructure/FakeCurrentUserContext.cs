using SIGA.Application.Interfaces;

namespace SIGA.Tests.Infrastructure;

/// <summary>
/// Implementación de <see cref="ICurrentUserContext"/> para tests: reemplaza la lectura
/// de claims del JWT por valores fijos. Es la pieza que permite ejercitar el aislamiento
/// multi-sucursal sin levantar la API ni firmar tokens.
/// </summary>
public sealed class FakeCurrentUserContext : ICurrentUserContext
{
    private readonly HashSet<string> _permisos;

    private FakeCurrentUserContext(int? userId, int? sucursalId, IEnumerable<string>? permisos)
    {
        UserId = userId;
        SucursalId = sucursalId;
        _permisos = new HashSet<string>(permisos ?? []);
    }

    public int? UserId { get; }
    public int? SucursalId { get; }

    /// <summary>Global = sin sucursal fija; ve y opera sobre todas.</summary>
    public bool EsGlobal => SucursalId is null;

    public bool TienePermiso(string permiso) => _permisos.Contains(permiso);

    /// <summary>Usuario atado a una sucursal: solo ve lo de esa sucursal.</summary>
    public static FakeCurrentUserContext DeSucursal(int sucursalId, int userId = 1, params string[] permisos)
        => new(userId, sucursalId, permisos);

    /// <summary>Administrador sin sucursal fija: ve todo.</summary>
    public static FakeCurrentUserContext Global(int userId = 1, params string[] permisos)
        => new(userId, null, permisos);
}
