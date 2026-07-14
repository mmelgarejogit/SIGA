namespace SIGA.Application.DTOs.Notificaciones;

public class UpdateNotificacionPreferenciaRequest
{
    public bool RecibirEmail { get; set; }
    public TimeOnly? VentanaSilencioInicio { get; set; }
    public TimeOnly? VentanaSilencioFin { get; set; }
}
