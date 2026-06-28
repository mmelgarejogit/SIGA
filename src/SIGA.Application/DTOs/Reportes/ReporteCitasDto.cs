namespace SIGA.Application.DTOs.Reportes;

public class ReporteCitasDto
{
    public string Desde { get; set; } = "";
    public string Hasta { get; set; } = "";
    public string Agrupacion { get; set; } = "";

    // ── KPIs ──────────────────────────────────────────────────────────────────
    public int TotalTurnos { get; set; }
    public int Completados { get; set; }
    public int Cancelados { get; set; }
    /// <summary>Turnos pasados que quedaron en Pendiente/Confirmado (no-show derivado).</summary>
    public int Ausentes { get; set; }
    /// <summary>Porcentaje (0–100) de turnos completados sobre el total del rango.</summary>
    public decimal TasaAsistencia { get; set; }
    public int Consultas { get; set; }
    public int Recetas { get; set; }

    // ── Desgloses ─────────────────────────────────────────────────────────────
    public List<SeriePuntoCitasDto> SerieTemporal { get; set; } = new();
    public List<EstadoCitasDto> PorEstado { get; set; } = new();
    public List<ProfesionalCitasDto> PorProfesional { get; set; } = new();
}

public class SeriePuntoCitasDto
{
    public string Periodo { get; set; } = "";
    public int Turnos { get; set; }
    public int Completados { get; set; }
}

public class EstadoCitasDto
{
    public string Estado { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal Porcentaje { get; set; }
}

public class ProfesionalCitasDto
{
    public string Nombre { get; set; } = "";
    public int Turnos { get; set; }
    public int Completados { get; set; }
}
