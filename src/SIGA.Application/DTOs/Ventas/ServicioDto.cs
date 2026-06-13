namespace SIGA.Application.DTOs.Ventas;

public class ServicioDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ServicioTarifaDto> Tarifas { get; set; } = new();
}

public class ServicioTarifaDto
{
    public int Id { get; set; }
    public int? ProfessionalId { get; set; }
    public string? ProfessionalNombre { get; set; }
    public int? EspecialidadId { get; set; }
    public string? EspecialidadNombre { get; set; }
    public decimal Precio { get; set; }
}

public class CreateServicioRequest
{
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; } = 0;
}

public class UpdateServicioRequest
{
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; } = 0;
    public bool IsActive { get; set; }
}

public class CreateServicioTarifaRequest
{
    public int? ProfessionalId { get; set; }
    public int? EspecialidadId { get; set; }
    public decimal Precio { get; set; } = 0;
}

public class PrecioResueltoDto
{
    public decimal Precio { get; set; }
    public string Origen { get; set; } = "base"; // "profesional" | "especialidad" | "base"
}
