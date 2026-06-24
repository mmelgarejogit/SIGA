namespace SIGA.Application.DTOs.Egresos;

public class CrearCategoriaGastoRequest
{
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
}

public class ActualizarCategoriaGastoRequest
{
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;
}
