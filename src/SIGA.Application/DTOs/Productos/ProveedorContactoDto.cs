namespace SIGA.Application.DTOs.Productos;

public class ProveedorContactoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Cargo { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
}

public class CreateProveedorContactoDto
{
    public string Nombre { get; set; } = "";
    public string? Cargo { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
}
