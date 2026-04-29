using SIGA.Application.Common;
using SIGA.Application.DTOs.Clinica;

namespace SIGA.Application.Interfaces;

public interface IConsultaClinicaService
{
    Task<Result<PagedResult<ConsultaClinicaResponse>>> GetAllAsync(int page, int pageSize, string? search, int? patientId, int? professionalId);
    Task<Result<IEnumerable<ConsultaClinicaResponse>>> GetByPatientAsync(int patientId);
    Task<Result<ConsultaClinicaResponse>> GetByIdAsync(int id);
    Task<Result<ConsultaClinicaResponse>> CreateAsync(CreateConsultaClinicaRequest request);
    Task<Result<ConsultaClinicaResponse>> UpdateAsync(int id, UpdateConsultaClinicaRequest request);
    Task<Result<bool>> DeleteAsync(int id);
    Task<Result<RecetaResponse>> CreateOrUpdateRecetaAsync(int consultaId, CreateRecetaRequest request);
}
