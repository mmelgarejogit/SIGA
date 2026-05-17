namespace SIGA.Domain.Entities;

public abstract class Egreso
{
    public int Id { get; set; }
    public TipoEgreso Tipo { get; set; }
    public EstadoEgreso Estado { get; set; } = EstadoEgreso.Borrador;
    public decimal Monto { get; set; }
    public string Concepto { get; set; } = "";
    public string? Observaciones { get; set; }
    public DateOnly FechaEmision { get; set; }
    public DateOnly? FechaVencimiento { get; set; }
    public DateOnly? FechaPago { get; set; }
    public MetodoPago? MetodoPago { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool EstaVencido() =>
        Estado != EstadoEgreso.Pagado &&
        Estado != EstadoEgreso.Anulado &&
        FechaVencimiento.HasValue &&
        FechaVencimiento.Value < DateOnly.FromDateTime(DateTime.UtcNow);

    public void RegistrarPago(MetodoPago metodo, DateOnly fechaPago)
    {
        if (Estado == EstadoEgreso.Anulado)
            throw new InvalidOperationException("No se puede pagar un egreso anulado.");
        if (Estado == EstadoEgreso.Pagado)
            throw new InvalidOperationException("El egreso ya fue pagado.");

        Estado = EstadoEgreso.Pagado;
        MetodoPago = metodo;
        FechaPago = fechaPago;
        UpdatedAt = DateTime.UtcNow;
    }
}
