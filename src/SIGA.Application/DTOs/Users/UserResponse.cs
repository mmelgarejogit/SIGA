namespace SIGA.Application.DTOs.Users;

public class UserResponse
{
    public int UserId { get; set; }
    public int PersonId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DNI { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public string Type { get; set; } = string.Empty;
    public IEnumerable<string> Roles { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}
