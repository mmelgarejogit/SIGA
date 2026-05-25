namespace SIGA.Domain.Entities;

public class Proveedor
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? RazonSocial { get; set; }
    public string Ruc { get; set; } = "";
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? SitioWeb { get; set; }
    public string? Facebook { get; set; }
    public string? Instagram { get; set; }
    public string? WhatsApp { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PedidoProveedor> Pedidos { get; set; } = [];
    public ICollection<ProveedorContacto> Contactos { get; set; } = [];
}
