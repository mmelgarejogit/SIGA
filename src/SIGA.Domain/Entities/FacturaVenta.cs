namespace SIGA.Domain.Entities;

public class FacturaVenta
{
    public int Id { get; set; }

    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;

    public string NumeroFactura { get; set; } = null!;
    public string Timbrado { get; set; } = null!;
    public string Establecimiento { get; set; } = null!;

    public decimal MontoExento { get; set; }
    public decimal MontoGravado5 { get; set; }
    public decimal MontoGravado10 { get; set; }

    public DateOnly FechaEmision { get; set; }
    public string? Observaciones { get; set; }

    // EF-ignored computed properties
    public decimal Iva5    => Math.Round(MontoGravado5 / 21m, 2);
    public decimal Iva10   => Math.Round(MontoGravado10 / 11m, 2);
    public decimal Total   => MontoExento + MontoGravado5 + MontoGravado10;

    public DateTime CreatedAt { get; set; }

    public int? TimbradoId { get; set; }
    public Timbrado? TimbradoConfig { get; set; }
}
