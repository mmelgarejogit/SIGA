namespace SIGA.Application.DTOs.Especialidades;

public class CreateEspecialidadRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}
