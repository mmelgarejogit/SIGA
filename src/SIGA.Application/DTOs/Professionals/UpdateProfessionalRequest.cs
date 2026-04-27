namespace SIGA.Application.DTOs.Professionals;

public class UpdateProfessionalRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }

    public string LicenseNumber { get; set; } = string.Empty;
    public List<int> EspecialidadIds { get; set; } = [];

    public bool IsActive { get; set; }
}
