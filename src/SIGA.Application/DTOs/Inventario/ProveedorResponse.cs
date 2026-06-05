namespace SIGA.Application.DTOs.Inventario;

public class ProveedorResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? RazonSocial { get; set; }
    public string Ruc { get; set; } = "";
    public string? Direccion { get; set; }
    public int? CiudadId { get; set; }
    public string? CiudadNombre { get; set; }
    public string? DepartamentoNombre { get; set; }
    public string? SitioWeb { get; set; }
    public string? Facebook { get; set; }
    public string? Instagram { get; set; }
    public string? WhatsApp { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ProveedorContactoDto> Contactos { get; set; } = [];
}
