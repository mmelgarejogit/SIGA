using Microsoft.EntityFrameworkCore;
using SIGA.Application.Interfaces;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

/// <summary>
/// Resuelve la sucursal destino para operaciones que ESCRIBEN stock/operaciones.
/// Usa la sucursal del usuario; si es global (admin sin sucursal), cae a "Casa Central".
/// </summary>
internal static class SucursalResolver
{
    private static int? _casaCentralCache;

    public static async Task<int> WriteBranchAsync(AppDbContext db, ICurrentUserContext current)
    {
        if (current.SucursalId is int s) return s;

        _casaCentralCache ??= await db.Sucursales
            .Where(x => x.Codigo == "CC")
            .Select(x => x.Id)
            .FirstAsync();

        return _casaCentralCache.Value;
    }
}
