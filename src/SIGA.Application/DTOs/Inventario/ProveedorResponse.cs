namespace SIGA.Application.DTOs.Inventario;

public class ProveedorResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Contacto { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
