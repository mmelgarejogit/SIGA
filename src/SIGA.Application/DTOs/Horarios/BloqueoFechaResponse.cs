namespace SIGA.Application.DTOs.Horarios;

public class BloqueoFechaResponse
{
    public int Id { get; set; }
    public DateOnly Fecha { get; set; }
    public string? Motivo { get; set; }
}
