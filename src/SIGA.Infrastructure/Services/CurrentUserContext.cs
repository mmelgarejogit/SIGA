using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SIGA.Application.Interfaces;

namespace SIGA.Infrastructure.Services;

public class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserContext(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private ClaimsPrincipal? User => _accessor.HttpContext?.User;

    public int? UserId =>
        int.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

    public int? SucursalId =>
        int.TryParse(User?.FindFirst("sucursal_id")?.Value, out var id) ? id : null;

    public bool EsGlobal => User?.Identity?.IsAuthenticated == true && !SucursalId.HasValue;

    public bool TienePermiso(string permiso) =>
        User?.HasClaim("permission", permiso) ?? false;
}
