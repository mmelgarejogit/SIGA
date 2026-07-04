namespace SIGA.Application.DTOs.Reportes;

/// <summary>
/// Filtros combinables de los reportes operativos. Todos opcionales; cada reporte usa el subconjunto
/// que le aplica. Los enums llegan como string y se parsean en el servicio (boundary string).
/// <c>PageSize = 0</c> significa "todas las filas" (usado por la exportación).
/// </summary>
public class ReporteOperativoFiltros
{
    public DateOnly? Desde { get; set; }
    public DateOnly? Hasta { get; set; }
    public int? SucursalId { get; set; }
    /// <summary>"Efectivo" | "Tarjeta" | "Transferencia" | "Cheque".</summary>
    public string? MetodoPago { get; set; }
    /// <summary>Nombre de la categoría de producto.</summary>
    public string? Categoria { get; set; }
    /// <summary>Id del usuario operador (Vendedor / RegistradoPor / CreadoPor).</summary>
    public int? OperadorId { get; set; }
    /// <summary>Inventario: "Entrada" | "Salida". Caja: "Ingreso" | "Egreso".</summary>
    public string? Tipo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
