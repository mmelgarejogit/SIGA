namespace SIGA.Domain.Entities;

/// <summary>
/// Nota de Crédito fiscal emitida al confirmar una devolución. NO anula la factura original:
/// la referencia y compensa el valor de la mercadería devuelta. Numeración por timbrado propio
/// de tipo <see cref="TipoDocumentoFiscal.NotaCredito"/>.
/// </summary>
public class NotaCredito
{
    public int Id { get; set; }

    public int DevolucionId { get; set; }
    public Devolucion Devolucion { get; set; } = null!;

    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;

    /// <summary>Factura de venta que esta NC compensa.</summary>
    public int FacturaVentaId { get; set; }
    public FacturaVenta FacturaVenta { get; set; } = null!;

    public string NumeroNotaCredito { get; set; } = null!;
    public string Timbrado { get; set; } = null!;
    public string Establecimiento { get; set; } = null!;

    // Montos correspondientes a lo devuelto (no al total de la factura)
    public decimal MontoExento { get; set; }
    public decimal MontoGravado5 { get; set; }
    public decimal MontoGravado10 { get; set; }

    public DateOnly FechaEmision { get; set; }
    public string? Observaciones { get; set; }

    public int EmitidoPorId { get; set; }
    public User EmitidoPor { get; set; } = null!;

    public int? TimbradoId { get; set; }
    public Timbrado? TimbradoConfig { get; set; }

    public DateTime CreatedAt { get; set; }

    // EF-ignored computed properties (mismo criterio que FacturaVenta)
    public decimal Iva5  => Math.Round(MontoGravado5 / 21m, 2);
    public decimal Iva10 => Math.Round(MontoGravado10 / 11m, 2);
    public decimal Total => MontoExento + MontoGravado5 + MontoGravado10;
}
