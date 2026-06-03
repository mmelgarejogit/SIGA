namespace SIGA.Application.DTOs.Stock;

public class SucursalResponse
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Codigo { get; set; } = "";
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSucursalRequest
{
    public string Nombre { get; set; } = "";
    public string Codigo { get; set; } = "";
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
}

public class UpdateSucursalRequest
{
    public string Nombre { get; set; } = "";
    public string Codigo { get; set; } = "";
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public bool IsActive { get; set; } = true;
}
