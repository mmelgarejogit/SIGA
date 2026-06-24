namespace SIGA.Application.DTOs.Estados;

public class UpdateEstadoConfigRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string Color { get; set; } = "#6B7280";
    public int Orden { get; set; }
}
