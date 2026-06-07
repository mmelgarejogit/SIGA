namespace SIGA.Domain.Entities;

public class Cliente
{
    public int Id { get; set; }

    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public TipoFacturacion TipoFacturacion { get; set; } = TipoFacturacion.Fisica;

    public string? RazonSocial { get; set; }
    public string? RucCiFiscal { get; set; }
    public string? Direccion { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
