namespace SIGA.Application.DTOs.Clinica;

public class ProfessionalDashboardStatsResponse
{
    public int ConsultasHoy { get; set; }
    public int ConsultasEstaSemana { get; set; }
    public int ConsultasEsteMes { get; set; }
    public int PacientesUnicosEsteMes { get; set; }
    public int RecetasEmitidasEsteMes { get; set; }
    public List<ConsultaClinicaResponse> UltimasConsultas { get; set; } = [];
}
