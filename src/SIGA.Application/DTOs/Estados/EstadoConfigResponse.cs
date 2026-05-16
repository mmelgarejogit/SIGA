namespace SIGA.Application.DTOs.Estados;

public class EstadoConfigResponse
{
    public int Id { get; set; }
    public string Entidad { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Color { get; set; } = "#6B7280";
    public string? CodigoInterno { get; set; }
    public bool EsProtegido { get; set; }
    public int Orden { get; set; }
}
