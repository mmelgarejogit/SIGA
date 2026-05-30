using SIGA.Application.Common;
using SIGA.Application.DTOs.Empleados;

namespace SIGA.Application.Interfaces;

public interface IEmpleadoService
{
    // Empleados
    Task<Result<IEnumerable<EmpleadoResponse>>> GetAllAsync(bool? soloActivos);
    Task<Result<EmpleadoResponse>> GetByIdAsync(int id);
    Task<Result<EmpleadoResponse>> CrearAsync(CrearEmpleadoRequest request);
    Task<Result<EmpleadoResponse>> ActualizarAsync(int id, ActualizarEmpleadoRequest request);
    Task<Result<EmpleadoResponse>> DesactivarAsync(int id);

    // Cargos
    Task<Result<IEnumerable<CargoEmpleadoResponse>>> GetCargosAsync();
    Task<Result<CargoEmpleadoResponse>> CrearCargoAsync(CrearCargoEmpleadoRequest request);
    Task<Result<CargoEmpleadoResponse>> ActualizarCargoAsync(int id, ActualizarCargoEmpleadoRequest request);
}
