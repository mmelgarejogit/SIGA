using System.ComponentModel.DataAnnotations;

namespace SIGA.Application.DTOs.Inventario;

public class CreateProveedorRequest
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = "";

    public string? RazonSocial { get; set; }

    [Required(ErrorMessage = "El RUC es obligatorio.")]
    [RegularExpression(@"^\d{1,8}-\d$", ErrorMessage = "RUC inválido. Formato esperado: 80012345-6")]
    public string Ruc { get; set; } = "";

    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? SitioWeb { get; set; }
    public string? Facebook { get; set; }
    public string? Instagram { get; set; }
    public string? WhatsApp { get; set; }
    public bool EsLaboratorio { get; set; } = false;

    public List<CreateProveedorContactoDto> Contactos { get; set; } = [];
}
