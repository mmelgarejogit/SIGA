namespace SIGA.Application.DTOs.Notificaciones;

public class NotificacionPreferenciaResponse
{
    public int PersonId { get; set; }
    public bool RecibirEmail { get; set; }
    public TimeOnly? VentanaSilencioInicio { get; set; }
    public TimeOnly? VentanaSilencioFin { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
