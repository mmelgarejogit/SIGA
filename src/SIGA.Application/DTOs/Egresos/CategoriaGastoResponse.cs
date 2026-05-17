namespace SIGA.Application.DTOs.Egresos;

public class CategoriaGastoResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
}
