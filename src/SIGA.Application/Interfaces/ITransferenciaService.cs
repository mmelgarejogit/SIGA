using SIGA.Application.Common;
using SIGA.Application.DTOs.Stock;

namespace SIGA.Application.Interfaces;

public interface ITransferenciaService
{
    Task<Result<PagedResult<TransferenciaResponse>>> GetAllAsync(
        int page, int pageSize, Guid? sucursalId, string? estado);
    Task<Result<TransferenciaResponse>> GetByIdAsync(Guid id);
    Task<Result<TransferenciaResponse>> CreateAsync(CreateTransferenciaRequest request, int usuarioId);
    Task<Result<TransferenciaResponse>> ResolverAsync(Guid id, ResolverTransferenciaRequest request, int usuarioId);
}
