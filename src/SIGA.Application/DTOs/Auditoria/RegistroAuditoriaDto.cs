namespace SIGA.Application.DTOs.Auditoria;

public class RegistroAuditoriaDto
{
    public int Id { get; set; }
    public string FechaHora { get; set; } = string.Empty;   // ISO 8601 (UTC)
    public string Categoria { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public string? Entidad { get; set; }
    public int? EntidadId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int? SucursalId { get; set; }
    public string? SucursalNombre { get; set; }
    public string? IpAddress { get; set; }
}

/// <summary>Una acción del catálogo con su categoría — alimenta el filtro de la vista.</summary>
public class AuditAccionDto
{
    public string Accion { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
}
