namespace SIGA.Application.DTOs.Clinica;

public class CreateConsultaClinicaRequest
{
    public int PatientId { get; set; }
    public int ProfessionalId { get; set; }
    public int? CitaId { get; set; }
    public DateTime FechaConsulta { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? Anamnesis { get; set; }
    public string? ExamenFisico { get; set; }
    public string DiagnosticoPrincipal { get; set; } = string.Empty;
    public string? DiagnosticoSecundario { get; set; }
    public string? PlanTratamiento { get; set; }
    public string? Observaciones { get; set; }
    public CreateRecetaRequest? Receta { get; set; }
}
