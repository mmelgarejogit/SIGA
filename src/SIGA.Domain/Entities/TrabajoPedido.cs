namespace SIGA.Domain.Entities;

public class TrabajoPedido
{
    public int Id { get; set; }

    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;

    public int? RecetaId { get; set; }
    public Receta? Receta { get; set; }

    // Clasificación óptica opcional (monofocal/progresivo). El cristal real es CristalProducto.
    public int? TipoLenteId { get; set; }
    public TipoLente? TipoLente { get; set; }

    // Cristal como producto del catálogo (categoría tipo Cristal).
    public int? CristalProductoId { get; set; }
    public Producto? CristalProducto { get; set; }

    public ICollection<Tratamiento> Tratamientos { get; set; } = new List<Tratamiento>();

    public int? ArmazonProductoId { get; set; }
    public Producto? ArmazonProducto { get; set; }

    // El cliente trae su propio armazón (sin producto ni descuento de stock).
    public bool ArmazonDelCliente { get; set; }

    // Opcional en borrador; se asigna a más tardar al confirmar la venta a pedido.
    public int? LaboratorioProveedorId { get; set; }
    public Proveedor? LaboratorioProveedor { get; set; }

    public EstadoTrabajoPedido Estado { get; set; } = EstadoTrabajoPedido.Borrador;

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
