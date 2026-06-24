namespace SIGA.Application.DTOs.Ventas;

public class CrearVentaRequest
{
    public int? ClienteId { get; set; }
    public int? RecetaId { get; set; }
    public string Tipo { get; set; } = "Directa";
    public string CondicionVenta { get; set; } = "Contado";
    public string FechaVenta { get; set; } = null!;
    public int ValidezDias { get; set; } = 15;
    public string? Observaciones { get; set; }
    public List<AgregarLineaRequest> Lineas { get; set; } = new();

    // Config óptica del trabajo a pedido (solo Tipo = TrabajoAPedido). Se crea en estado Borrador.
    public CrearVentaTrabajoPedidoRequest? TrabajoPedido { get; set; }
}

public class CrearVentaTrabajoPedidoRequest
{
    // Diseño del lente (monofocal/bifocal/progresivo). El precio del lente viaja como línea
    // de venta tipo Lente; acá solo se guarda la especificación para la orden al laboratorio.
    public int? TipoLenteId { get; set; }
    public List<int> TratamientoIds { get; set; } = new();
    public int? ArmazonProductoId { get; set; }
    public bool ArmazonDelCliente { get; set; }
    public int? LaboratorioProveedorId { get; set; }
    public string? Observacion { get; set; }
}
