namespace SIGA.Application.DTOs.Professionals;

public class UpdateProfessionalRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    public string Specialty { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
