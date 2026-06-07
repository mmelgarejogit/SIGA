namespace SIGA.Domain.Entities;

public class TrabajoPedido
{
    public int Id { get; set; }

    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;

    public int? RecetaId { get; set; }
    public Receta? Receta { get; set; }

    public int TipoLenteId { get; set; }
    public TipoLente TipoLente { get; set; } = null!;

    public ICollection<Tratamiento> Tratamientos { get; set; } = new List<Tratamiento>();

    public int? ArmazonProductoId { get; set; }
    public Producto? ArmazonProducto { get; set; }

    public int LaboratorioProveedorId { get; set; }
    public Proveedor LaboratorioProveedor { get; set; } = null!;

    public EstadoTrabajoPedido Estado { get; set; } = EstadoTrabajoPedido.PendienteEnvio;

    public DateOnly? FechaEnvio { get; set; }
    public DateOnly? FechaRecepcion { get; set; }

    public string? Observacion { get; set; }

    public string? ObservacionAprobacion { get; set; }
    public int? AprobadoPorId { get; set; }
    public User? AprobadoPor { get; set; }

    public FacturaLaboratorio? Factura { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
