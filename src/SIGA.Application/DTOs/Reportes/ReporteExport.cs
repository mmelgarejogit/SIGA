namespace SIGA.Application.DTOs.Reportes;

/// <summary>
/// Forma tabular genérica lista para exportar (PDF/CSV). Las filas ya vienen formateadas como texto
/// (Gs., fechas es-PY). Un solo exportador sirve para los 4 reportes. Reutilizable por un job de email.
/// </summary>
public class ReporteExport
{
    public string Titulo { get; set; } = "";
    /// <summary>Rango + filtros aplicados, para el encabezado.</summary>
    public string Subtitulo { get; set; } = "";
    public string[] Columnas { get; set; } = [];
    public List<string[]> Filas { get; set; } = [];
    /// <summary>Fila de totales (misma cardinalidad que Columnas). Null = sin totales.</summary>
    public string[]? Totales { get; set; }
    /// <summary>Índices de columnas numéricas (alineadas a la derecha en el PDF).</summary>
    public int[] ColumnasNumericas { get; set; } = [];
    /// <summary>Base del nombre de archivo, sin extensión.</summary>
    public string FileBaseName { get; set; } = "reporte";
}
