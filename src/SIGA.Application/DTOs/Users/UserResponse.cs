namespace SIGA.Application.DTOs.Users;

public class UserResponse
{
    public int UserId { get; set; }
    public string CI { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Profesional" | "Paciente" | "Usuario"
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Roles { get; set; } = [];
}
