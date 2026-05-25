namespace SIGA.Domain.Entities;

public class FacturaCompra : Egreso
{
    public string? NroFactura { get; set; }
    public int? PedidoProveedorId { get; set; }
    public PedidoProveedor? PedidoProveedor { get; set; }
    public int ProveedorId { get; set; }
    public Proveedor Proveedor { get; set; } = null!;

    public decimal MontoExento { get; set; }
    public decimal MontoGravado5 { get; set; }
    public decimal MontoGravado10 { get; set; }
    public CondicionVenta CondicionVenta { get; set; }

    // Propiedades calculadas — no persisten en DB
    public decimal Iva5 => Math.Round(MontoGravado5 / 21m, 0);
    public decimal Iva10 => Math.Round(MontoGravado10 / 11m, 0);
    public decimal MontoTotal => MontoExento + MontoGravado5 + MontoGravado10;

    public string? MotivoAnulacion { get; set; }

    public FacturaCompra()
    {
        Tipo = TipoEgreso.FacturaCompra;
    }
}
