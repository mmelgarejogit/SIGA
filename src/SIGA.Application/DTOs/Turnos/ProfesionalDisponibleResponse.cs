namespace SIGA.Application.DTOs.Turnos;

/// <summary>Profesional con disponibilidad para una fecha — vista liviana para la reserva del paciente.</summary>
public class ProfesionalDisponibleResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string? Especialidad { get; set; }
}
