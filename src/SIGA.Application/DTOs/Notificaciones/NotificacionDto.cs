namespace SIGA.Application.DTOs.Notificaciones;

public class NotificacionDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = null!;
    public string Mensaje { get; set; } = null!;
    public string? EntidadOrigenTipo { get; set; }
    public int? EntidadOrigenId { get; set; }
    public bool Leido { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaLectura { get; set; }
}
