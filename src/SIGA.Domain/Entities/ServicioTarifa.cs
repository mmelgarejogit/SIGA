namespace SIGA.Domain.Entities;

/// <summary>
/// Override de precio para un Servicio según dimensiones opcionales
/// (profesional y/o especialidad). La tarifa más específica gana;
/// si no hay ninguna que aplique se usa el precio base del Servicio.
/// </summary>
public class ServicioTarifa
{
    public int Id { get; set; }

    public int ServicioId { get; set; }
    public Servicio Servicio { get; set; } = null!;

    public int? ProfessionalId { get; set; }
    public Professional? Professional { get; set; }

    public int? EspecialidadId { get; set; }
    public Especialidad? Especialidad { get; set; }

    public decimal Precio { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
