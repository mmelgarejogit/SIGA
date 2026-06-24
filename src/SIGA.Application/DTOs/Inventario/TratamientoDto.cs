namespace SIGA.Application.DTOs.Inventario;

public class TratamientoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public decimal Precio { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTratamientoRequest
{
    public string Nombre { get; set; } = "";
    public decimal Precio { get; set; } = 0;
}

public class UpdateTratamientoRequest
{
    public string Nombre { get; set; } = "";
    public decimal Precio { get; set; } = 0;
    public bool IsActive { get; set; }
}
