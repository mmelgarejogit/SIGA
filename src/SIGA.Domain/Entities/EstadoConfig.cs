namespace SIGA.Domain.Entities;

public class EstadoConfig
{
    public int Id { get; set; }
    public string Entidad { get; set; } = null!;  // "Turno" | "Pedido" | "Consulta"
    public string Nombre { get; set; } = null!;
    public string Color { get; set; } = "#6B7280";
    public string? CodigoInterno { get; set; }
    public bool EsProtegido { get; set; }
    public int Orden { get; set; }
}
