namespace SIGA.Domain.Entities;

public class Cobro
{
    public int Id { get; set; }

    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;

    public decimal Monto { get; set; }
    public MetodoPago MetodoPago { get; set; }
    public DateOnly FechaCobro { get; set; }
    public string? Referencia { get; set; }
    public bool Anulado { get; set; } = false;

    public DateTime CreatedAt { get; set; }
}
