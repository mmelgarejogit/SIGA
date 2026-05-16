using SIGA.Application.Common;
using SIGA.Application.DTOs.Configuracion;

namespace SIGA.Application.Interfaces;

public interface IConfiguracionNegocioService
{
    Task<Result<ConfiguracionNegocioResponse>> GetAsync();
    Task<Result<ConfiguracionNegocioResponse>> UpdateAsync(UpdateConfiguracionNegocioRequest request);
}
