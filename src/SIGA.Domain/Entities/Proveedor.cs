namespace SIGA.Domain.Entities;

public class Proveedor
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Contacto { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string Ruc { get; set; } = "";
    public string Timbrado { get; set; } = "";
    public DateOnly? VigenciaTimbrado { get; set; }
    public string? Establecimiento { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PedidoProveedor> Pedidos { get; set; } = [];
}
