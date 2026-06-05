namespace SIGA.Application.DTOs.Clientes;

public class ClienteResponse
{
    public int Id { get; set; }
    public int PersonId { get; set; }

    public string Ci { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string? Sexo { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PersonEmail { get; set; }

    public string TipoFacturacion { get; set; } = "Fisica";
    public string? RazonSocial { get; set; }
    public string? RucCiFiscal { get; set; }
    public string? Direccion { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
