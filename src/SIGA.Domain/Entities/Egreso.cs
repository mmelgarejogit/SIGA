namespace SIGA.Domain.Entities;

public abstract class Egreso
{
    public int Id { get; set; }
    public int SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }
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

    public void Aprobar()
    {
        if (Estado != EstadoEgreso.Pendiente)
            throw new InvalidOperationException("Solo se puede aprobar un egreso pendiente.");

        Estado = EstadoEgreso.Aprobado;
        FechaAprobacion = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Rechazar(string motivo)
    {
        if (Estado != EstadoEgreso.Pendiente)
            throw new InvalidOperationException("Solo se puede rechazar un egreso pendiente.");

        Estado = EstadoEgreso.Rechazado;
        MotivoRechazo = motivo;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RegistrarPago(MetodoPago metodo, DateOnly fechaPago, string? nroComprobante)
    {
        if (Estado == EstadoEgreso.Anulado)
            throw new InvalidOperationException("No se puede pagar un egreso anulado.");
        if (Estado == EstadoEgreso.Pagado)
            throw new InvalidOperationException("El egreso ya fue pagado.");
        if (Estado != EstadoEgreso.Aprobado)
            throw new InvalidOperationException("Solo se puede pagar un egreso aprobado.");

        Estado = EstadoEgreso.Pagado;
        MetodoPago = metodo;
        FechaPago = fechaPago;
        NroComprobante = nroComprobante?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
