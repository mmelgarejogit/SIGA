namespace SIGA.Application.DTOs.Turnos;

public class SelfBookTurnoRequest
{
    public int ProfessionalId { get; set; }
    public DateTime FechaHora { get; set; }
    public string? Motivo { get; set; }
}
