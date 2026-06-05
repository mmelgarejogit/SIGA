namespace SIGA.Domain.Entities;

public class Venta
{
    public int Id { get; set; }
    public string NumeroComprobante { get; set; } = null!;

    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public int? RecetaId { get; set; }
    public Receta? Receta { get; set; }

    public EstadoVenta Estado { get; set; } = EstadoVenta.Borrador;
    public TipoVenta Tipo { get; set; } = TipoVenta.Directa;
    public CondicionVenta CondicionVenta { get; set; } = CondicionVenta.Contado;

    public DateOnly FechaVenta { get; set; }
    public DateOnly? FechaConfirmacion { get; set; }
    public DateOnly? FechaComprobante { get; set; }

    public string? Observaciones { get; set; }

    public ICollection<VentaLinea> Lineas { get; set; } = new List<VentaLinea>();
    public ICollection<Cobro> Cobros { get; set; } = new List<Cobro>();
    public FacturaVenta? Factura { get; set; }
    public Comprobante? Comprobante { get; set; }
    public TrabajoPedido? TrabajoPedido { get; set; }
    public ICollection<Devolucion> Devoluciones { get; set; } = new List<Devolucion>();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // EF-ignored computed properties
    public decimal MontoExento    => Lineas.Where(l => l.CategoriaFiscal == CategoriaFiscal.Exento).Sum(l => l.Subtotal);
    public decimal MontoGravado5  => Lineas.Where(l => l.CategoriaFiscal == CategoriaFiscal.Gravado5).Sum(l => l.Subtotal);
    public decimal MontoGravado10 => Lineas.Where(l => l.CategoriaFiscal == CategoriaFiscal.Gravado10).Sum(l => l.Subtotal);
    public decimal Total          => MontoExento + MontoGravado5 + MontoGravado10;
    public decimal MontoSeña      => Cobros.Where(c => c.Tipo == TipoCobro.Seña && !c.Anulado).Sum(c => c.MontoTotal);
    public decimal TotalCobrado   => Cobros.Where(c => !c.Anulado).Sum(c => c.MontoTotal);
    public decimal SaldoPendiente => Total - TotalCobrado;

    public bool PuedeCancelarse()        => Estado is EstadoVenta.Borrador or EstadoVenta.Confirmada or EstadoVenta.EnProceso;
    public bool PuedeConfirmarse()       => Estado == EstadoVenta.Borrador;
    public bool PuedeEmitirComprobante() => Estado == EstadoVenta.ListaParaCobrar;
    public bool PuedeDevolver()          => Estado == EstadoVenta.ComprobanteEmitido;
}
