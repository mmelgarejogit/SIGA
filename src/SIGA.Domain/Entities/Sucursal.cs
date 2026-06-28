namespace SIGA.Domain.Entities;

public class Sucursal
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    /// <summary>Código corto único (para display y numeración interna).</summary>
    public string Codigo { get; set; } = null!;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }

    public int? CiudadId { get; set; }
    public Ciudad? Ciudad { get; set; }

    /// <summary>Código de establecimiento fiscal (Paraguay), p/ timbrado.</summary>
    public string? Establecimiento { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
