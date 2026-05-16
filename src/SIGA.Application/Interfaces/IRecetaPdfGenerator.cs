using SIGA.Application.DTOs.Clinica;
using SIGA.Application.DTOs.Configuracion;

namespace SIGA.Application.Interfaces;

public interface IRecetaPdfGenerator
{
    byte[] Generate(ConsultaClinicaResponse consulta, ConfiguracionNegocioResponse? config = null);
}
