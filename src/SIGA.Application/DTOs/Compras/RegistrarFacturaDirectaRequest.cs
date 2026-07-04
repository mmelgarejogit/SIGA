using System.ComponentModel.DataAnnotations;

namespace SIGA.Application.DTOs.Compras;

public class RegistrarFacturaDirectaRequest : IValidatableObject
{
    [Required(ErrorMessage = "El proveedor es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El proveedor es obligatorio.")]
    public int ProveedorId { get; set; }

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

    [MinLength(1, ErrorMessage = "Debe incluir al menos un ítem en la factura.")]
    public List<FacturaCompraItemRequest> Items { get; set; } = [];

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

        var total = Items.Sum(i => i.Cantidad * i.PrecioUnitario);
        if (total <= 0)
            yield return new ValidationResult(
                "El total de los ítems debe ser mayor a 0.",
                [nameof(Items)]);
    }
}
