namespace SIGA.Application.DTOs.Inventario;

public class TipoLenteDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTipoLenteRequest
{
    public string Nombre { get; set; } = "";
}

public class UpdateTipoLenteRequest
{
    public string Nombre { get; set; } = "";
    public bool IsActive { get; set; }
}
