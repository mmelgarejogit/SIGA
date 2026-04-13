using SIGA.Application.Common;
using SIGA.Application.DTOs.Professionals;

namespace SIGA.Application.Interfaces;

public interface IProfessionalService
{
    Task<Result<IEnumerable<ProfessionalResponse>>> GetAllAsync();
    Task<Result<ProfessionalResponse>> GetByIdAsync(int id);
    Task<Result<ProfessionalResponse>> CreateAsync(CreateProfessionalRequest request);
    Task<Result<ProfessionalResponse>> UpdateAsync(int id, UpdateProfessionalRequest request);
    Task<Result<bool>> DeleteAsync(int id);
}
