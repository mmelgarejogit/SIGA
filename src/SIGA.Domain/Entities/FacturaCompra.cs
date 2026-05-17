namespace SIGA.Domain.Entities;

public class FacturaCompra : Egreso
{
    public string? NroFactura { get; set; }
    public int? PedidoProveedorId { get; set; }
    public PedidoProveedor? PedidoProveedor { get; set; }
    public int ProveedorId { get; set; }
    public Proveedor Proveedor { get; set; } = null!;

    public FacturaCompra()
    {
        Tipo = TipoEgreso.FacturaCompra;
    }
}
