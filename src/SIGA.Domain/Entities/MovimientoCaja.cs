namespace SIGA.Domain.Entities;

public class MovimientoCaja
{
    public int Id { get; set; }

    public Guid SucursalId { get; set; }
    public Sucursal Sucursal { get; set; } = null!;

    public TipoMovimientoCaja Tipo { get; set; }
    public decimal Monto { get; set; }
    public string Concepto { get; set; } = null!;
    public MetodoPago MetodoPago { get; set; }

    public int? VentaId { get; set; }
    public Venta? Venta { get; set; }

    public int? EgresoId { get; set; }
    public Egreso? Egreso { get; set; }

    public DateOnly Fecha { get; set; }
    public string? Referencia { get; set; }

    public DateTime CreatedAt { get; set; }
}