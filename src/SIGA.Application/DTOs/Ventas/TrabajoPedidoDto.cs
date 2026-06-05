namespace SIGA.Application.DTOs.Ventas;

public class TrabajoPedidoTratamientoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
}

public class TrabajoPedidoDto
{
    public int Id { get; set; }
    public int? RecetaId { get; set; }
    public int TipoLenteId { get; set; }
    public string TipoLenteNombre { get; set; } = "";
    public List<TrabajoPedidoTratamientoDto> Tratamientos { get; set; } = new();
    public int? ArmazonProductoId { get; set; }
    public string? ArmazonProductoNombre { get; set; }
    public int LaboratorioProveedorId { get; set; }
    public string LaboratorioNombre { get; set; } = "";
    public string Estado { get; set; } = "";
    public string? FechaEnvio { get; set; }
    public string? FechaRecepcion { get; set; }
    public string? Observacion { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CrearTrabajoPedidoRequest
{
    public int? RecetaId { get; set; }
    public int TipoLenteId { get; set; }
    public List<int> TratamientoIds { get; set; } = new();
    public int? ArmazonProductoId { get; set; }
    public int LaboratorioProveedorId { get; set; }
    public string? Observacion { get; set; }
}

public class RegistrarEnvioLabRequest
{
    public string? Observacion { get; set; }
}

public class RegistrarRecepcionLabRequest
{
    public string? Observacion { get; set; }
}
