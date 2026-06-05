namespace SIGA.Application.DTOs.Ubicacion;

public class DepartamentoResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public bool IsActive { get; set; }
    public int TotalCiudades { get; set; }
}
