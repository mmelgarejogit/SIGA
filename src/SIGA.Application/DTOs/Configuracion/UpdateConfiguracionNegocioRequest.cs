namespace SIGA.Application.DTOs.Configuracion;

public class UpdateConfiguracionNegocioRequest
{
    public string NombreFantasia { get; set; } = string.Empty;
    public string? RazonSocial { get; set; }
    public string? CUIT { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? SitioWeb { get; set; }
}
