namespace SIGA.Domain.Entities;

public class Venta
{
    public int Id { get; set; }
    public string NumeroComprobante { get; set; } = null!;

    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public int? RecetaId { get; set; }
    public Receta? Receta { get; set; }

    public EstadoVenta Estado { get; set; } = EstadoVenta.Abierta;
    public CondicionVenta CondicionVenta { get; set; } = CondicionVenta.Contado;

    public DateOnly FechaVenta { get; set; }
    public DateOnly? FechaVencimiento { get; set; }

    public string? Observaciones { get; set; }

    public ICollection<VentaLinea> Lineas { get; set; } = new List<VentaLinea>();
    public ICollection<Cobro> Cobros { get; set; } = new List<Cobro>();
    public FacturaVenta? Factura { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // EF-ignored computed properties
    public decimal MontoExento    => Lineas.Where(l => l.CategoriaFiscal == CategoriaFiscal.Exento).Sum(l => l.Subtotal);
    public decimal MontoGravado5  => Lineas.Where(l => l.CategoriaFiscal == CategoriaFiscal.Gravado5).Sum(l => l.Subtotal);
    public decimal MontoGravado10 => Lineas.Where(l => l.CategoriaFiscal == CategoriaFiscal.Gravado10).Sum(l => l.Subtotal);
    public decimal Total          => MontoExento + MontoGravado5 + MontoGravado10;
    public decimal TotalCobrado   => Cobros.Where(c => !c.Anulado).Sum(c => c.Monto);
    public decimal SaldoPendiente => Total - TotalCobrado;

    public bool EstaPagada()    => SaldoPendiente <= 0;
    public bool PuedeAnularse() => Estado != EstadoVenta.Pagada
                                && Estado != EstadoVenta.Cobrada
                                && Estado != EstadoVenta.Anulada;
}
