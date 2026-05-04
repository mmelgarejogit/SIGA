namespace SIGA.Application.DTOs.Horarios;

public class BloqueoFechaRequest
{
    public DateOnly Fecha { get; set; }
    public string? Motivo { get; set; }
}
