namespace SIGA.Application.DTOs.Professionals;

public class CreateProfessionalRequest
{
    public string CI { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string? PhoneNumber { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public string LicenseNumber { get; set; } = string.Empty;
    public List<int> EspecialidadIds { get; set; } = [];
}
