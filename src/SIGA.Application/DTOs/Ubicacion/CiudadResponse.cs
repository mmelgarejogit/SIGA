namespace SIGA.Application.DTOs.Ubicacion;

public class CiudadResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public int DepartamentoId { get; set; }
    public string DepartamentoNombre { get; set; } = "";
    public bool IsActive { get; set; }
}
