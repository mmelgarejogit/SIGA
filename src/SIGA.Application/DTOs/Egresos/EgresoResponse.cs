namespace SIGA.Application.DTOs.Egresos;

public class EgresoResponse
{
    public int Id { get; set; }
    public string Tipo { get; set; } = "";
    public string Estado { get; set; } = "";
    public decimal Monto { get; set; }
    public string Concepto { get; set; } = "";
    public string? Observaciones { get; set; }
    public string FechaEmision { get; set; } = "";
    public string? FechaVencimiento { get; set; }
    public string? FechaPago { get; set; }
    public string? MetodoPago { get; set; }
    public bool EstaVencido { get; set; }
    public string? MotivoRechazo { get; set; }
    public string? FechaAprobacion { get; set; }
    public string? NroComprobante { get; set; }
    public DateTime CreatedAt { get; set; }

    // FacturaCompra — datos de referencia
    public string? NroFactura { get; set; }
    public int? ProveedorId { get; set; }
    public string? ProveedorNombre { get; set; }
    public int? PedidoProveedorId { get; set; }

    // FacturaCompra — desglose fiscal
    public decimal? MontoExento { get; set; }
    public decimal? MontoGravado5 { get; set; }
    public decimal? MontoGravado10 { get; set; }
    public decimal? Iva5 { get; set; }
    public decimal? Iva10 { get; set; }
    public decimal? MontoTotal { get; set; }
    public string? CondicionVenta { get; set; }

    // Honorario
    public int? ProfessionalId { get; set; }
    public string? ProfessionalNombre { get; set; }
    public string? Periodo { get; set; }

    // GastoGeneral
    public int? CategoriaGastoId { get; set; }
    public string? CategoriaGastoNombre { get; set; }

    // SalarioEmpleado
    public int? EmpleadoId { get; set; }
    public string? EmpleadoNombre { get; set; }
}
