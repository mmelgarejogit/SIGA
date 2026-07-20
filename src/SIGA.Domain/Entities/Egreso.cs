namespace SIGA.Domain.Entities;

public abstract class Egreso
{
    public int Id { get; set; }
    public int SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }

    /// <summary>Usuario que registró el egreso (operador). Nullable: egresos históricos sin operador.</summary>
    public int? RegistradoPorId { get; set; }
    public User? RegistradoPor { get; set; }

    public TipoEgreso Tipo { get; set; }
    public EstadoEgreso Estado { get; set; } = EstadoEgreso.Borrador;
    public decimal Monto { get; set; }
    public string Concepto { get; set; } = "";
    public string? Observaciones { get; set; }
    public DateOnly FechaEmision { get; set; }
    public DateOnly? FechaVencimiento { get; set; }
    public DateOnly? FechaPago { get; set; }
    public MetodoPago? MetodoPago { get; set; }
    public string? MotivoRechazo { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public string? NroComprobante { get; set; }
    public bool PagoExterno { get; set; }
    public string? MotivoPagoExterno { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool EstaVencido() =>
        Estado != EstadoEgreso.Pagado &&
        Estado != EstadoEgreso.Anulado &&
        Estado != EstadoEgreso.Rechazado &&
        FechaVencimiento.HasValue &&
        FechaVencimiento.Value < DateOnly.FromDateTime(DateTime.UtcNow);

    public void RegistrarPago(MetodoPago metodo, DateOnly fechaPago, string? nroComprobante)
    {
        if (Estado == EstadoEgreso.Anulado)
            throw new InvalidOperationException("No se puede pagar un egreso anulado.");
        if (Estado == EstadoEgreso.Pagado)
            throw new InvalidOperationException("El egreso ya fue pagado.");
        if (Estado == EstadoEgreso.Rechazado)
            throw new InvalidOperationException("No se puede pagar un egreso rechazado.");
        // Se puede pagar un egreso registrado (Pendiente) o uno ya aprobado en el flujo anterior (Aprobado).
        if (Estado != EstadoEgreso.Pendiente && Estado != EstadoEgreso.Aprobado)
            throw new InvalidOperationException("Solo se puede pagar un egreso pendiente de pago.");

        Estado = EstadoEgreso.Pagado;
        MetodoPago = metodo;
        FechaPago = fechaPago;
        NroComprobante = nroComprobante?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
