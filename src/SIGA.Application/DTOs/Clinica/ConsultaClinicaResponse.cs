namespace SIGA.Application.DTOs.Clinica;

public class ConsultaClinicaResponse
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientFirstName { get; set; } = string.Empty;
    public string PatientLastName { get; set; } = string.Empty;
    public string PatientCI { get; set; } = string.Empty;
    public int ProfessionalId { get; set; }
    public string ProfessionalFirstName { get; set; } = string.Empty;
    public string ProfessionalLastName { get; set; } = string.Empty;
    public int? CitaId { get; set; }
    public DateTime FechaConsulta { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? Anamnesis { get; set; }
    public string? ExamenFisico { get; set; }
    public string DiagnosticoPrincipal { get; set; } = string.Empty;
    public string? DiagnosticoSecundario { get; set; }
    public string? PlanTratamiento { get; set; }
    public string? Observaciones { get; set; }
    public RecetaResponse? Receta { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
