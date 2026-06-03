namespace SIGA.Domain.Entities;

public enum EstadoInventario { Borrador, EnConteo, Cerrado, Aprobado, Cancelado }
public enum AlcanceInventario { Total, Parcial }

public class InventarioFisico
{
    public Guid Id { get; set; }
    public Guid SucursalId { get; set; }
    public Sucursal Sucursal { get; set; } = null!;
    public EstadoInventario Estado { get; set; } = EstadoInventario.Borrador;
    public AlcanceInventario Alcance { get; set; } = AlcanceInventario.Total;
    public int? FiltroCategoriaId { get; set; }
    public CategoriaProducto? FiltroCategoria { get; set; }
    public DateTime? FechaInicioConteo { get; set; }
    public int IniciadoPorId { get; set; }
    public User IniciadoPor { get; set; } = null!;
    public int? EjecutadoPorId { get; set; }
    public User? EjecutadoPor { get; set; }
    public int? AprobadoPorId { get; set; }
    public User? AprobadoPor { get; set; }
    public string? Observacion { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FechaResolucion { get; set; }

    public ICollection<InventarioFisicoLinea> Lineas { get; set; } = [];
}
