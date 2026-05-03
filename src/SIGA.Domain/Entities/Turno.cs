namespace SIGA.Domain.Entities;

public class Turno
{
    public int Id { get; set; }

    public int ProfessionalId { get; set; }
    public Professional Professional { get; set; } = null!;

    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public DateTime FechaHora { get; set; }

    public TurnoEstado Estado { get; set; } = TurnoEstado.Pendiente;

    public string? Motivo { get; set; }
    public string? Notas { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
