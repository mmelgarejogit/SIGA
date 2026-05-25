namespace SIGA.Domain.Entities;

public class RecepcionMercaderiaItem
{
    public int Id { get; set; }
    public int RecepcionId { get; set; }
    public RecepcionMercaderia Recepcion { get; set; } = null!;
    public int PedidoItemId { get; set; }
    public PedidoProveedorItem PedidoItem { get; set; } = null!;
    public int Cantidad { get; set; }
}
