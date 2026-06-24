using System.ComponentModel.DataAnnotations;

namespace SIGA.Application.DTOs.Compras;

public class FacturaCompraItemRequest
{
    public int? ProductoId { get; set; }

    [Required(ErrorMessage = "La descripción del ítem es obligatoria.")]
    [MaxLength(300)]
    public string Descripcion { get; set; } = "";

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public int Cantidad { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El precio unitario no puede ser negativo.")]
    public decimal PrecioUnitario { get; set; }

    /// <summary>Exento | Iva5 | Iva10</summary>
    [Required]
    public string TipoIva { get; set; } = "Iva10";
}
