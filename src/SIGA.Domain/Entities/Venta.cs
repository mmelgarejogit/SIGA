namespace SIGA.Domain.Entities;

public class Venta
{
    public int Id { get; set; }
    public string NumeroComprobante { get; set; } = null!;

    public int SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }

    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    /// <summary>Usuario que registró la venta (operador). Nullable: ventas históricas sin operador.</summary>
    public int? VendedorId { get; set; }
    public User? Vendedor { get; set; }

    public int? RecetaId { get; set; }
    public Receta? Receta { get; set; }

    public EstadoVenta Estado { get; set; } = EstadoVenta.Borrador;
    public TipoVenta Tipo { get; set; } = TipoVenta.Directa;
    public CondicionVenta CondicionVenta { get; set; } = CondicionVenta.Contado;

    public DateOnly FechaVenta { get; set; }
    public DateOnly? FechaConfirmacion { get; set; }
    public DateOnly? FechaComprobante { get; set; }

    /// <summary>Días de validez del presupuesto, contados desde FechaVenta.</summary>
    public int ValidezDias { get; set; } = 15;

    /// <summary>
    /// Plan de cuotas (opcional, solo tiene sentido en ventas a Crédito). Si es null, la venta
    /// a crédito funciona "libre": se registran cobros parciales sin cronograma ni vencimientos,
    /// igual que antes de que existiera este campo.
    /// </summary>
    public int? CantidadCuotas { get; set; }

    /// <summary>Días entre cuota y cuota (30 = mensual, 15 = quincenal). Requiere CantidadCuotas.</summary>
    public int? FrecuenciaCuotasDias { get; set; }

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

    // ── Plan de cuotas (solo si CantidadCuotas está definido) ──────────────────────

    /// <summary>Monto financiado (Total menos la seña) dividido en partes iguales.</summary>
    public decimal? MontoCuota =>
        CantidadCuotas is > 0 ? (Total - MontoSeña) / CantidadCuotas.Value : null;

    /// <summary>Cuánto de lo cobrado (excluyendo la seña) corresponde a cuotas ya cubiertas.</summary>
    public decimal TotalCobradoEnCuotas =>
        Cobros.Where(c => c.Tipo == TipoCobro.Cuota && !c.Anulado).Sum(c => c.MontoTotal);

    /// <summary>Cantidad de cuotas ya cubiertas por los cobros registrados, sin superar CantidadCuotas.</summary>
    public int? CuotasPagadas =>
        MontoCuota is > 0
            ? Math.Min((int)Math.Floor(TotalCobradoEnCuotas / MontoCuota.Value), CantidadCuotas!.Value)
            : null;

    /// <summary>Fecha de vencimiento de la próxima cuota sin cubrir (null si no hay plan o ya está todo pagado).</summary>
    public DateOnly? ProximaCuotaVencimiento =>
        CantidadCuotas is > 0 && FrecuenciaCuotasDias is > 0 && FechaConfirmacion is not null
            && CuotasPagadas is int pagadas && pagadas < CantidadCuotas.Value
            ? FechaConfirmacion.Value.AddDays(FrecuenciaCuotasDias.Value * (pagadas + 1))
            : null;

    /// <summary>True si la próxima cuota ya venció y todavía queda saldo pendiente.</summary>
    public bool CuotaVencida =>
        ProximaCuotaVencimiento is DateOnly f && f < DateOnly.FromDateTime(DateTime.UtcNow) && SaldoPendiente > 0;

    public bool PuedeCancelarse()        => Estado is EstadoVenta.Borrador or EstadoVenta.Confirmada or EstadoVenta.EnProceso;
    public bool PuedeConfirmarse()       => Estado == EstadoVenta.Borrador;
    public bool PuedeEmitirComprobante() => Estado == EstadoVenta.ListaParaCobrar;
    public bool PuedeDevolver()          => Estado == EstadoVenta.ComprobanteEmitido;
}
