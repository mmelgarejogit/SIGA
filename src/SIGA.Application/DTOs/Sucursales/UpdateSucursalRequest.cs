namespace SIGA.Application.DTOs.Sucursales;

public class UpdateSucursalRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public int? CiudadId { get; set; }
    public string? Establecimiento { get; set; }
    public bool IsActive { get; set; } = true;
}
