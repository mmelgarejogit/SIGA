using SIGA.Application.DTOs.Clinica;

namespace SIGA.Application.Interfaces;

public interface IRecetaPdfGenerator
{
    byte[] Generate(ConsultaClinicaResponse consulta);
}
