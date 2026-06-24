using System.ComponentModel.DataAnnotations;

namespace SIGA.Application.DTOs.Compras;

public class RegistrarRecepcionRequest : IValidatableObject
{
    [Required(ErrorMessage = "La factura es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "La factura es obligatoria.")]
    public int FacturaCompraId { get; set; }

    [Required(ErrorMessage = "La fecha de recepción es obligatoria.")]
    public string FechaRecepcion { get; set; } = "";

    [MaxLength(500)]
    public string? Observaciones { get; set; }

    public List<RecepcionItemRequest> Items { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!DateOnly.TryParse(FechaRecepcion, out _))
            yield return new ValidationResult("Fecha de recepción inválida.", [nameof(FechaRecepcion)]);

        var validos = Items.Where(i => i.CantidadRecibida > 0).ToList();
        if (validos.Count == 0)
            yield return new ValidationResult(
                "Debe incluir al menos un ítem con cantidad mayor a cero.",
                [nameof(Items)]);

        foreach (var i in validos)
        {
            var loteCompleto = !string.IsNullOrWhiteSpace(i.Lote);
            var vencCompleto = !string.IsNullOrWhiteSpace(i.FechaVencimiento);

            if (loteCompleto != vencCompleto)
                yield return new ValidationResult(
                    "Si se completa lote o vencimiento, ambos campos son obligatorios.",
                    [nameof(Items)]);
        }
    }
}

public class RecepcionItemRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "ItemId inválido.")]
    public int ItemId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "La cantidad no puede ser negativa.")]
    public int CantidadRecibida { get; set; }

    [MaxLength(80)]
    public string? Lote { get; set; }

    /// <summary>Formato yyyy-MM-dd</summary>
    public string? FechaVencimiento { get; set; }

    [MaxLength(500)]
    public string? Observaciones { get; set; }
}
