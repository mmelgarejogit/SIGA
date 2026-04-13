namespace SIGA.Application.DTOs.Professionals;

public class ProfessionalResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }

    public string DNI { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string? PhoneNumber { get; set; }
    public string Email { get; set; } = string.Empty;

    public string Specialty { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
