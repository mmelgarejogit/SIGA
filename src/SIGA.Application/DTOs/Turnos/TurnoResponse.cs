namespace SIGA.Application.DTOs.Turnos;

public class TurnoResponse
{
    public int Id { get; set; }
    public int ProfessionalId { get; set; }
    public string ProfessionalNombre { get; set; } = null!;
    public int PatientId { get; set; }
    public string PatientNombre { get; set; } = null!;
    public DateTime FechaHora { get; set; }
    public string Estado { get; set; } = null!;
    public bool SolicitudCancelacion { get; set; }
    public string? Motivo { get; set; }
    public string? Notas { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
