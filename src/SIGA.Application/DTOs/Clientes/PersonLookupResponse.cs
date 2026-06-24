namespace SIGA.Application.DTOs.Clientes;

public class PersonLookupResponse
{
    public int PersonId { get; set; }
    public string Ci { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string? Sexo { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public bool YaEsCliente { get; set; }
}
