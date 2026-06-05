namespace SIGA.Application.DTOs.Ubicacion;

public class CreateDepartamentoRequest
{
    public string Nombre { get; set; } = "";
}

public class UpdateDepartamentoRequest
{
    public string Nombre { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
