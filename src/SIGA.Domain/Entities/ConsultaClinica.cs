namespace SIGA.Domain.Entities;

public class ConsultaClinica
{
    public int Id { get; set; }

    public int SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }

    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public int ProfessionalId { get; set; }
    public Professional Professional { get; set; } = null!;

    public int? CitaId { get; set; }

    public DateTime FechaConsulta { get; set; }
    public string Motivo { get; set; } = null!;
    public string? Anamnesis { get; set; }
    public string? ExamenFisico { get; set; }
    public string DiagnosticoPrincipal { get; set; } = null!;
    public string? DiagnosticoSecundario { get; set; }
    public string? PlanTratamiento { get; set; }
    public string? Observaciones { get; set; }

    public int? EstadoConfigId { get; set; }
    public EstadoConfig? EstadoConfig { get; set; }

    public Receta? Receta { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
