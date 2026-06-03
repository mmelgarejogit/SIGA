namespace SIGA.Application.DTOs.Stock;

public class TipoAjusteResponse
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Impacto { get; set; } = "";
    public bool Activo { get; set; }
}

public class CreateTipoAjusteRequest
{
    public string Nombre { get; set; } = "";
    public string Impacto { get; set; } = "Ambos";
}

public class UpdateTipoAjusteRequest
{
    public string Nombre { get; set; } = "";
    public string Impacto { get; set; } = "Ambos";
    public bool Activo { get; set; } = true;
}
