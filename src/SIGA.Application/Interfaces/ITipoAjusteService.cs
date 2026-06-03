using SIGA.Application.Common;
using SIGA.Application.DTOs.Stock;

namespace SIGA.Application.Interfaces;

public interface ITipoAjusteService
{
    Task<Result<IEnumerable<TipoAjusteResponse>>> GetAllAsync(string? impacto, bool? activo);
    Task<Result<TipoAjusteResponse>> GetByIdAsync(Guid id);
    Task<Result<TipoAjusteResponse>> CreateAsync(CreateTipoAjusteRequest request);
    Task<Result<TipoAjusteResponse>> UpdateAsync(Guid id, UpdateTipoAjusteRequest request);
    Task<Result<bool>> DeactivateAsync(Guid id);
}
