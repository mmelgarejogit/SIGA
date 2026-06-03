namespace SIGA.Application.DTOs.Productos;

public class UpdateProductoRequest
{
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public int? CategoriaProductoId { get; set; }
    public int? MarcaId { get; set; }
    public int? ModeloId { get; set; }
    public bool IsActive { get; set; } = true;
}
