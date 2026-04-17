namespace SIGA.Application.DTOs.Auth;

public class LoginResponse
{
    public string JwtToken { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Specialty { get; set; }
    public List<string> RoleClaims { get; set; } = [];
    public List<string> Permissions { get; set; } = [];
}
