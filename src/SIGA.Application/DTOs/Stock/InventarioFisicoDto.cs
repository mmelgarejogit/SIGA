namespace SIGA.Application.DTOs.Stock;

public class InventarioFisicoLineaResponse
{
    public Guid Id { get; set; }
    public Guid ProductoVarianteId { get; set; }
    public string ProductoNombre { get; set; } = "";
    public string? VarianteSku { get; set; }
    public string? VarianteColor { get; set; }
    public string? VarianteTalle { get; set; }
    public int? CantidadSistema { get; set; }   // null → no expuesto al Encargado
    public int? CantidadContada { get; set; }
    public int? Diferencia { get; set; }
}

public class InventarioFisicoResponse
{
    public Guid Id { get; set; }
    public Guid SucursalId { get; set; }
    public string SucursalNombre { get; set; } = "";
    public string Estado { get; set; } = "";
    public string Alcance { get; set; } = "";
    public int? FiltroCategoriaId { get; set; }
    public string? FiltroCategoriaNombre { get; set; }
    public DateTime? FechaInicioConteo { get; set; }
    public int IniciadoPorId { get; set; }
    public string IniciadoPorNombre { get; set; } = "";
    public int? EjecutadoPorId { get; set; }
    public string? EjecutadoPorNombre { get; set; }
    public int? AprobadoPorId { get; set; }
    public string? AprobadoPorNombre { get; set; }
    public string? Observacion { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? FechaResolucion { get; set; }
    public int TotalLineas { get; set; }
    public int LineasContadas { get; set; }
    public int LineasConDiferencia { get; set; }
    public List<InventarioFisicoLineaResponse> Lineas { get; set; } = [];
}

public class CreateInventarioFisicoRequest
{
    public Guid SucursalId { get; set; }
    public string Alcance { get; set; } = "Total";
    public int? FiltroCategoriaId { get; set; }
    public string? Observacion { get; set; }
}

public class GuardarConteosRequest
{
    public List<ConteoLineaRequest> Lineas { get; set; } = [];
}

public class ConteoLineaRequest
{
    public Guid LineaId { get; set; }
    public int? CantidadContada { get; set; }
}
