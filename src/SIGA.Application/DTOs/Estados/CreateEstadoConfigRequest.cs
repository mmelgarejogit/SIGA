namespace SIGA.Application.DTOs.Estados;

public class CreateEstadoConfigRequest
{
    public string Entidad { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Color { get; set; } = "#6B7280";
    public int Orden { get; set; }
}
