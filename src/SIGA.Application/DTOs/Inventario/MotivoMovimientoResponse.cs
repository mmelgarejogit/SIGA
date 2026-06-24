namespace SIGA.Application.DTOs.Inventario;

public class MotivoMovimientoResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Tipo { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateMotivoMovimientoRequest
{
    public string Nombre { get; set; } = "";
    public string Tipo { get; set; } = "Ambos";
}

public class UpdateMotivoMovimientoRequest
{
    public string Nombre { get; set; } = "";
    public string Tipo { get; set; } = "Ambos";
    public bool IsActive { get; set; }
}
