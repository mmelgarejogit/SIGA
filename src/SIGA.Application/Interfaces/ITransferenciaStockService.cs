using SIGA.Application.Common;
using SIGA.Application.DTOs.Sucursales;

namespace SIGA.Application.Interfaces;

public interface ITransferenciaStockService
{
    Task<Result<IEnumerable<TransferenciaResponse>>> GetAllAsync(string? estado = null);
    Task<Result<TransferenciaResponse>> CreateAsync(CreateTransferenciaRequest request, int userId, string userName);
    Task<Result<TransferenciaResponse>> GestionarAsync(int id, GestionarTransferenciaRequest request, string userName);
}
