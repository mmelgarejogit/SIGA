namespace SIGA.Application.DTOs.Stock;

public class AjusteManualResponse
{
    public Guid Id { get; set; }
    public Guid SucursalId { get; set; }
    public string SucursalNombre { get; set; } = "";
    public Guid TipoAjusteId { get; set; }
    public string TipoAjusteNombre { get; set; } = "";
    public string TipoAjusteImpacto { get; set; } = "";
    public Guid ProductoVarianteId { get; set; }
    public string ProductoNombre { get; set; } = "";
    public string? VarianteSku { get; set; }
    public string? VarianteColor { get; set; }
    public string? VarianteTalle { get; set; }
    public int Cantidad { get; set; }
    public string Observacion { get; set; } = "";
    public string Estado { get; set; } = "";
    public int CreadoPorId { get; set; }
    public string CreadoPorNombre { get; set; } = "";
    public int? AprobadoPorId { get; set; }
    public string? AprobadoPorNombre { get; set; }
    public string? ObservacionResolucion { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaResolucion { get; set; }
}

public class CreateAjusteManualRequest
{
    public Guid SucursalId { get; set; }
    public Guid TipoAjusteId { get; set; }
    public Guid ProductoVarianteId { get; set; }
    public int Cantidad { get; set; }
    public string Observacion { get; set; } = "";
}

public class ResolverAjusteRequest
{
    public string Accion { get; set; } = "";  // "Aprobar" | "Rechazar"
    public string? Observacion { get; set; }
}
