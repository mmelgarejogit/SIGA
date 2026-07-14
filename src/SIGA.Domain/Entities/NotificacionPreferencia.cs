namespace SIGA.Domain.Entities;

public class NotificacionPreferencia
{
    public int Id { get; set; }

    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public bool RecibirEmail { get; set; } = true;

    public TimeOnly? VentanaSilencioInicio { get; set; }
    public TimeOnly? VentanaSilencioFin { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
