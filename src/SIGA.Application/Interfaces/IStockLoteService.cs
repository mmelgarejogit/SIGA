using SIGA.Application.Common;
using SIGA.Application.DTOs.Inventario;

namespace SIGA.Application.Interfaces;

public interface IStockLoteService
{
    Task<Result<List<StockLoteDto>>> GetLotesAsync(int? productoId, bool? vencidos);
    Task<Result<ConteoInventarioDto>> RegistrarConteoAsync(int userId, string userName, RegistrarConteoRequest request);
    Task<Result<List<ConteoInventarioDto>>> GetConteosAsync(string? estado);
    Task<Result<ConteoInventarioDetalleDto>> GetConteoByIdAsync(int id);
    Task<Result<ConteoInventarioDto>> GestionarConteoAsync(int id, int userId, string userName, GestionarConteoRequest request);
}
