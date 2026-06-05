namespace SIGA.Application.DTOs.Clientes;

public class CreateClienteRequest
{
    // Si se provee, se reutiliza la persona existente (ej. un paciente que también es cliente).
    public int? PersonId { get; set; }

    // Datos de la persona — requeridos solo cuando PersonId es null (alta de persona nueva).
    public string? Ci { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? BirthDate { get; set; }
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
