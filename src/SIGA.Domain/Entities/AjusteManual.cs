namespace SIGA.Domain.Entities;

public enum EstadoAjuste { Pendiente, Aprobado, Rechazado }

public class AjusteManual
{
    public Guid Id { get; set; }
    public Guid SucursalId { get; set; }
    public Sucursal Sucursal { get; set; } = null!;
    public Guid TipoAjusteId { get; set; }
    public TipoAjuste TipoAjuste { get; set; } = null!;
    public Guid ProductoVarianteId { get; set; }
    public ProductoVariante ProductoVariante { get; set; } = null!;
    public int Cantidad { get; set; }
    public string Observacion { get; set; } = "";
    public EstadoAjuste Estado { get; set; } = EstadoAjuste.Pendiente;
    public int CreadoPorId { get; set; }
    public User CreadoPor { get; set; } = null!;
    public int? AprobadoPorId { get; set; }
    public User? AprobadoPor { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaResolucion { get; set; }
    public string? ObservacionResolucion { get; set; }
}
