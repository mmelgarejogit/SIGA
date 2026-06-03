namespace SIGA.Domain.Entities;

public class RecepcionMercaderiaItem
{
    public int Id { get; set; }
    public int RecepcionId { get; set; }
    public RecepcionMercaderia Recepcion { get; set; } = null!;
    public int PedidoItemId { get; set; }
    public PedidoProveedorItem PedidoItem { get; set; } = null!;
    public int Cantidad { get; set; }

    /// <summary>Lote del producto recibido (opcional). Si se completa, FechaVencimiento es obligatoria.</summary>
    public string? Lote { get; set; }

    /// <summary>Fecha de vencimiento del lote (opcional). Si se completa, Lote es obligatorio.</summary>
    public DateOnly? FechaVencimiento { get; set; }

    /// <summary>Observaciones particulares de esta línea recibida.</summary>
    public string? Observaciones { get; set; }

    public StockLote? StockLote { get; set; }
}
