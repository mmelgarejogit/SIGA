namespace SIGA.Application.DTOs.Inventario;

public class CreatePedidoItemRequest
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}

public class CreatePedidoProveedorRequest
{
    public int ProveedorId { get; set; }
    public string? Observaciones { get; set; }
    public IEnumerable<CreatePedidoItemRequest> Items { get; set; } = [];
}
