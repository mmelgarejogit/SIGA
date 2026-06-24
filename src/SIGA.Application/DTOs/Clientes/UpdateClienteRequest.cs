namespace SIGA.Application.DTOs.Clientes;

public class UpdateClienteRequest
{
    // Datos de la persona (CI inmutable).
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string? Sexo { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PersonEmail { get; set; }

    // Datos de facturación.
    public string TipoFacturacion { get; set; } = "Fisica";
    public string? RazonSocial { get; set; }
    public string? RucCiFiscal { get; set; }
    public string? Direccion { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
}
