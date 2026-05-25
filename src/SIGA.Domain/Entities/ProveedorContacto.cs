namespace SIGA.Domain.Entities;

public class ProveedorContacto
{
    public int Id { get; set; }
    public int ProveedorId { get; set; }
    public Proveedor Proveedor { get; set; } = null!;
    public string Nombre { get; set; } = "";
    public string? Cargo { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
}
