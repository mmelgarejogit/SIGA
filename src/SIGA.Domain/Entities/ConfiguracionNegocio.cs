namespace SIGA.Domain.Entities;

public class ConfiguracionNegocio
{
    public int Id { get; set; }
    public string NombreFantasia { get; set; } = string.Empty;
    public string? RazonSocial { get; set; }
    public string? CUIT { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? SitioWeb { get; set; }
    public DateTime UpdatedAt { get; set; }
}
