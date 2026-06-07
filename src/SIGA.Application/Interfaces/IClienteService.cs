using SIGA.Application.Common;
using SIGA.Application.DTOs.Clientes;

namespace SIGA.Application.Interfaces;

public interface IClienteService
{
    Task<Result<PagedResult<ClienteResponse>>> GetAllAsync(int page, int pageSize, string? search, string? status, string? tipo);
    Task<Result<ClienteResponse>> GetByIdAsync(int id);
    Task<Result<PersonLookupResponse?>> BuscarPersonaPorCiAsync(string ci);
    Task<Result<ClienteResponse>> CreateAsync(CreateClienteRequest request);
    Task<Result<ClienteResponse>> UpdateAsync(int id, UpdateClienteRequest request);
    Task<Result<bool>> DesactivarAsync(int id);
    Task<Result<bool>> ActivarAsync(int id);
}
