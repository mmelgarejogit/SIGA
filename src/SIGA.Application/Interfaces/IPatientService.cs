using SIGA.Application.Common;
using SIGA.Application.DTOs.Patients;

namespace SIGA.Application.Interfaces;

public interface IPatientService
{
    Task<Result<IEnumerable<PatientResponse>>> GetAllAsync();
    Task<Result<PatientResponse>> GetByIdAsync(int id);
    Task<Result<PatientResponse>> CreateAsync(CreatePatientRequest request);
    Task<Result<PatientResponse>> UpdateAsync(int id, UpdatePatientRequest request);
    Task<Result<bool>> DeleteAsync(int id);
}
