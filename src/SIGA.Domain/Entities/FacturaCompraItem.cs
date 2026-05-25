namespace SIGA.Domain.Entities;

public class FacturaCompraItem
{
    public int Id { get; set; }

    public int FacturaCompraId { get; set; }
    public FacturaCompra FacturaCompra { get; set; } = null!;

    public int? ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public string Descripcion { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public TipoIvaFactura TipoIva { get; set; } = TipoIvaFactura.Iva10;

    // Calculado en dominio
    public decimal Total => Cantidad * PrecioUnitario;
}
