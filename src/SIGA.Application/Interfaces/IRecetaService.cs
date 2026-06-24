using SIGA.Application.Common;
using SIGA.Application.DTOs.Clinica;

namespace SIGA.Application.Interfaces;

public interface IRecetaService
{
    Task<Result<IEnumerable<RecetaResponse>>> GetByClienteAsync(int clienteId);
    Task<Result<RecetaResponse>> CreateManualAsync(CreateRecetaManualRequest request);
}
