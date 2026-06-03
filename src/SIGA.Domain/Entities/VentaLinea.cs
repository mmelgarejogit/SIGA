namespace SIGA.Domain.Entities;

public class VentaLinea
{
    public int Id { get; set; }

    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;

    public TipoLineaVenta Tipo { get; set; }

    public Guid? ProductoVarianteId { get; set; }
    public ProductoVariante? ProductoVariante { get; set; }

    public int? ServicioId { get; set; }
    public Servicio? Servicio { get; set; }

    public string Descripcion { get; set; } = null!;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; } = 0;
    public CategoriaFiscal CategoriaFiscal { get; set; } = CategoriaFiscal.Gravado10;

    // EF-ignored computed property
    public decimal Subtotal => (PrecioUnitario * Cantidad) - Descuento;
}
