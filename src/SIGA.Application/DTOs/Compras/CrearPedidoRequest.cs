namespace SIGA.Application.DTOs.Compras;

public class CrearPedidoRequest
{
    public int ProveedorId { get; set; }
    public string? Observaciones { get; set; }
    public IEnumerable<ItemPedidoRequest> Items { get; set; } = [];
}

public class ItemPedidoRequest
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}
