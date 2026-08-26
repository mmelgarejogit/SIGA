namespace SIGA.Domain.Entities;

public class NotificacionInterna
{
    public int Id { get; set; }

    /// <summary>Si tiene valor, la notificación es solo para este usuario.</summary>
    public int? DestinatarioUsuarioId { get; set; }
    public User? DestinatarioUsuario { get; set; }

    /// <summary>Si tiene valor (y DestinatarioUsuarioId es null), es para todo el staff de esta sucursal.</summary>
    public int? DestinatarioSucursalId { get; set; }
    public Sucursal? DestinatarioSucursal { get; set; }

    public TipoNotificacion Tipo { get; set; }
    public string Mensaje { get; set; } = null!;

    public string? EntidadOrigenTipo { get; set; }
    public int? EntidadOrigenId { get; set; }

    public bool Leido { get; set; } = false;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaLectura { get; set; }
}
