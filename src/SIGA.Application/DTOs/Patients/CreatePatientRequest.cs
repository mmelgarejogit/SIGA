namespace SIGA.Application.DTOs.Patients;

public class CreatePatientRequest
{
    public string CI { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}
