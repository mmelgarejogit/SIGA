namespace SIGA.Application.DTOs.Sucursales;

public class TransferenciaItemRequest
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}

public class CreateTransferenciaRequest
{
    /// <summary>Opcional: solo lo usa un usuario global (admin). Un usuario de sucursal transfiere desde la suya.</summary>
    public int? SucursalOrigenId { get; set; }
    public int SucursalDestinoId { get; set; }
    public string? Observaciones { get; set; }
    public List<TransferenciaItemRequest> Items { get; set; } = [];
}

public class GestionarTransferenciaRequest
{
    public bool Aceptar { get; set; }
    public string? Observaciones { get; set; }
}

public class TransferenciaItemResponse
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = "";
    public int Cantidad { get; set; }
}

public class TransferenciaResponse
{
    public int Id { get; set; }
    public int SucursalOrigenId { get; set; }
    public string SucursalOrigenNombre { get; set; } = "";
    public int SucursalDestinoId { get; set; }
    public string SucursalDestinoNombre { get; set; } = "";
    public string Fecha { get; set; } = "";
    public string Estado { get; set; } = "";
    public string? CreadoPorNombre { get; set; }
    public string? RecibidoPorNombre { get; set; }
    public string? Observaciones { get; set; }
    public List<TransferenciaItemResponse> Items { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}
