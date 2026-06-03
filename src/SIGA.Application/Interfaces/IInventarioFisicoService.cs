using SIGA.Application.Common;
using SIGA.Application.DTOs.Stock;

namespace SIGA.Application.Interfaces;

public interface IInventarioFisicoService
{
    Task<Result<PagedResult<InventarioFisicoResponse>>> GetAllAsync(int page, int pageSize, Guid? sucursalId, string? estado);
    Task<Result<InventarioFisicoResponse>> GetByIdAsync(Guid id, bool includeSnapshot);
    Task<Result<InventarioFisicoResponse>> CreateAsync(CreateInventarioFisicoRequest request, int adminUserId);
    Task<Result<InventarioFisicoResponse>> IniciarConteoAsync(Guid id, int adminUserId);
    Task<Result<InventarioFisicoResponse>> GuardarConteosAsync(Guid id, GuardarConteosRequest request, int encargadoUserId);
    Task<Result<InventarioFisicoResponse>> CerrarAsync(Guid id, int encargadoUserId);
    Task<Result<InventarioFisicoResponse>> AprobarAsync(Guid id, int adminUserId);
    Task<Result<InventarioFisicoResponse>> CancelarAsync(Guid id, int userId);
}
