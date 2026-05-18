using System.ComponentModel.DataAnnotations;

namespace SIGA.Application.DTOs.Inventario;

public class CreateProveedorRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = "";

    public string? Contacto { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "El RUC es obligatorio.")]
    [RegularExpression(@"^\d{1,8}-\d$", ErrorMessage = "RUC inválido. Formato esperado: 80012345-6")]
    public string Ruc { get; set; } = "";

    [Required(ErrorMessage = "El timbrado es obligatorio.")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "El timbrado debe tener exactamente 8 dígitos numéricos.")]
    public string Timbrado { get; set; } = "";

    // Fecha ISO (yyyy-MM-dd). Opcional; si se provee debe ser futura — validado en servicio.
    public string? VigenciaTimbrado { get; set; }

    [RegularExpression(@"^\d{3}-\d{3}$", ErrorMessage = "Establecimiento inválido. Formato esperado: 001-001")]
    public string? Establecimiento { get; set; }
}
