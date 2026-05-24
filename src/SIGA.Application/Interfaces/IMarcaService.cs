using SIGA.Application.Common;
using SIGA.Application.DTOs.Inventario;

namespace SIGA.Application.Interfaces;

public interface IMarcaService
{
    Task<Result<IEnumerable<MarcaResponse>>> GetMarcasAsync();
    Task<Result<MarcaResponse>> CreateMarcaAsync(CreateMarcaRequest request);
    Task<Result<MarcaResponse>> UpdateMarcaAsync(int id, UpdateMarcaRequest request);
    Task<Result<bool>> DeactivateMarcaAsync(int id);

    Task<Result<IEnumerable<ModeloResponse>>> GetModelosAsync(int? marcaId);
    Task<Result<ModeloResponse>> CreateModeloAsync(CreateModeloRequest request);
    Task<Result<ModeloResponse>> UpdateModeloAsync(int id, UpdateModeloRequest request);
    Task<Result<bool>> DeactivateModeloAsync(int id);
}
