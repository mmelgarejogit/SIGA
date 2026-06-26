using System.ComponentModel.DataAnnotations;

namespace SIGA.Application.DTOs.Compras;

public class RegistrarFacturaPedidoRequest : IValidatableObject
{
    [Required(ErrorMessage = "El número de factura es obligatorio.")]
    [RegularExpression(@"^\d{3}-\d{3}-\d{7}$",
        ErrorMessage = "Formato inválido. Esperado: 001-001-0000001")]
    public string NroFactura { get; set; } = "";

    [Required(ErrorMessage = "La fecha de emisión es obligatoria.")]
    public string FechaEmision { get; set; } = "";

    public string? FechaVencimiento { get; set; }

    [Required(ErrorMessage = "La condición de venta es obligatoria.")]
    public string CondicionVenta { get; set; } = "Contado";

    public string? MetodoPago { get; set; }

    public string? Observaciones { get; set; }

    /// <summary>
    /// Ítems de la factura. Si se omite, se copian automáticamente desde la OC con IVA 10%.
    /// </summary>
    public List<FacturaCompraItemRequest>? Items { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CondicionVenta?.Equals("Credito", StringComparison.OrdinalIgnoreCase) == true
            && string.IsNullOrWhiteSpace(FechaVencimiento))
            yield return new ValidationResult(
                "La fecha de vencimiento es obligatoria para facturas a crédito.",
                [nameof(FechaVencimiento)]);

        if (CondicionVenta?.Equals("Contado", StringComparison.OrdinalIgnoreCase) == true
            && string.IsNullOrWhiteSpace(MetodoPago))
            yield return new ValidationResult(
                "El método de pago es obligatorio para facturas al contado.",
                [nameof(MetodoPago)]);
    }
}
