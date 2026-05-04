namespace SIGA.Application.DTOs.Turnos;

public class CreateTurnoRequest
{
    public int ProfessionalId { get; set; }
    public int PatientId { get; set; }
    public DateTime FechaHora { get; set; }
    public string? Motivo { get; set; }
    public string? Notas { get; set; }
}
