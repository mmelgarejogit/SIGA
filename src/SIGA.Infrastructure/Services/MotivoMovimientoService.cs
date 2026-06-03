using SIGA.Application.Common;
using SIGA.Application.DTOs.Productos;
using SIGA.Application.Interfaces;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class MotivoMovimientoService(AppDbContext db) : IMotivoMovimientoService
{
    public Task<Result<IEnumerable<MotivoMovimientoResponse>>> GetAllAsync(string? tipo)
        => Task.FromResult(Result<IEnumerable<MotivoMovimientoResponse>>.Success([]));

    public Task<Result<MotivoMovimientoResponse>> CreateAsync(CreateMotivoMovimientoRequest request)
        => Task.FromResult(Result<MotivoMovimientoResponse>.Failure("Use /api/tipos-ajuste en su lugar.", ErrorType.Validation));

    public Task<Result<MotivoMovimientoResponse>> UpdateAsync(int id, UpdateMotivoMovimientoRequest request)
        => Task.FromResult(Result<MotivoMovimientoResponse>.Failure("Use /api/tipos-ajuste en su lugar.", ErrorType.Validation));

    public Task<Result<bool>> DeactivateAsync(int id)
        => Task.FromResult(Result<bool>.Failure("Use /api/tipos-ajuste en su lugar.", ErrorType.Validation));
}
