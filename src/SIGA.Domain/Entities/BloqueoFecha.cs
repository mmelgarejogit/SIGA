namespace SIGA.Domain.Entities;

public class BloqueoFecha
{
    public int Id { get; set; }

    public int ProfessionalId { get; set; }
    public Professional Professional { get; set; } = null!;

    public DateOnly Fecha { get; set; }
    public string? Motivo { get; set; }
}
