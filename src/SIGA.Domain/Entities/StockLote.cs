namespace SIGA.Domain.Entities;

public class StockLote
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;

    public int SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }

    public int RecepcionItemId { get; set; }
    public RecepcionMercaderiaItem RecepcionItem { get; set; } = null!;

    public string Lote { get; set; } = "";
    public DateOnly? FechaVencimiento { get; set; }
    public int CantidadInicial { get; set; }
    public DateOnly FechaIngreso { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
