namespace SIGA.Application.DTOs.Ubicacion;

public class CreateCiudadRequest
{
    public string Nombre { get; set; } = "";
    public int DepartamentoId { get; set; }
}

public class UpdateCiudadRequest
{
    public string Nombre { get; set; } = "";
    public int DepartamentoId { get; set; }
    public bool IsActive { get; set; } = true;
}
