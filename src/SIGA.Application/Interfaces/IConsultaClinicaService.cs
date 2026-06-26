using SIGA.Application.Common;
using SIGA.Application.DTOs.Clinica;

namespace SIGA.Application.Interfaces;

public interface IConsultaClinicaService
{
    Task<Result<PagedResult<ConsultaClinicaResponse>>> GetAllAsync(int page, int pageSize, string? search, int? patientId, int? professionalId);
    Task<Result<IEnumerable<ConsultaClinicaResponse>>> GetByPatientAsync(int patientId);

    // ── Vista del propio paciente (resuelve el patientId desde el userId del JWT) ──
    Task<Result<IEnumerable<ConsultaClinicaResponse>>> GetMisConsultasAsync(int userId);
    Task<Result<ConsultaClinicaResponse>> GetMiConsultaAsync(int userId, int consultaId);
    Task<Result<ConsultaClinicaResponse>> GetByIdAsync(int id);
    Task<Result<ConsultaClinicaResponse>> CreateAsync(CreateConsultaClinicaRequest request);
    Task<Result<ConsultaClinicaResponse>> UpdateAsync(int id, UpdateConsultaClinicaRequest request);
    Task<Result<bool>> DeleteAsync(int id);
    Task<Result<RecetaResponse>> CreateOrUpdateRecetaAsync(int consultaId, CreateRecetaRequest request);
    Task<Result<ConsultaClinicaResponse>> CambiarEstadoAsync(int id, int estadoConfigId);
    Task<Result<ProfessionalDashboardStatsResponse>> GetProfessionalStatsAsync(int professionalId);
}
