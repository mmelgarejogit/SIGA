using System.ComponentModel.DataAnnotations;

namespace SIGA.Application.DTOs.Egresos;

public class CrearFacturaCompraRequest : IValidatableObject
{
    public Guid? SucursalId { get; set; }

    [Required(ErrorMessage = "El proveedor es obligatorio.")]
    public int ProveedorId { get; set; }

    public int? PedidoProveedorId { get; set; }

    [RegularExpression(@"^\d{3}-\d{3}-\d{7}$",
        ErrorMessage = "Formato de factura inválido. Esperado: NNN-NNN-NNNNNNN")]
    public string? NroFactura { get; set; }

    [Required(ErrorMessage = "El concepto es obligatorio.")]
    public string Concepto { get; set; } = "";

    public string? Observaciones { get; set; }

    [Required(ErrorMessage = "La fecha de emisión es obligatoria.")]
    public string FechaEmision { get; set; } = "";

    public string? FechaVencimiento { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El monto exento no puede ser negativo.")]
    public decimal MontoExento { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El monto gravado al 5% no puede ser negativo.")]
    public decimal MontoGravado5 { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El monto gravado al 10% no puede ser negativo.")]
    public decimal MontoGravado10 { get; set; }

    [Required(ErrorMessage = "La condición de venta es obligatoria.")]
    public string CondicionVenta { get; set; } = "Contado";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MontoExento <= 0 && MontoGravado5 <= 0 && MontoGravado10 <= 0)
            yield return new ValidationResult(
                "Al menos uno de los montos (exento, gravado 5%, gravado 10%) debe ser mayor a 0.",
                [nameof(MontoExento), nameof(MontoGravado5), nameof(MontoGravado10)]);

        if (CondicionVenta?.Equals("Credito", StringComparison.OrdinalIgnoreCase) == true
            && string.IsNullOrWhiteSpace(FechaVencimiento))
            yield return new ValidationResult(
                "La fecha de vencimiento es obligatoria para facturas a crédito.",
                [nameof(FechaVencimiento)]);
    }
}
