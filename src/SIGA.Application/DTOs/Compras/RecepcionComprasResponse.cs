namespace SIGA.Application.DTOs.Compras;

public class RecepcionComprasResponse
{
    public int Id { get; set; }
    public string? Observaciones { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<RecepcionComprasItemResponse> Items { get; set; } = [];
}

public class RecepcionComprasItemResponse
{
    public int PedidoItemId { get; set; }
    public string ProductoNombre { get; set; } = "";
    public int Cantidad { get; set; }
}
