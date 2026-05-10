namespace SIGA.Application.DTOs.Inventario;

public class CreateProveedorRequest
{
    public string Nombre { get; set; } = "";
    public string? Contacto { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
}
