using SIGA.Application.Common;
using SIGA.Application.DTOs.Stock;

namespace SIGA.Application.Interfaces;

public interface IAjusteManualService
{
    Task<Result<PagedResult<AjusteManualResponse>>> GetAllAsync(
        int page, int pageSize, Guid? sucursalId, string? estado);
    Task<Result<AjusteManualResponse>> GetByIdAsync(Guid id);
    Task<Result<AjusteManualResponse>> CreateAsync(CreateAjusteManualRequest request, int usuarioId);
    Task<Result<AjusteManualResponse>> ResolverAsync(Guid id, ResolverAjusteRequest request, int usuarioId);
}
