namespace SIGA.Application.DTOs.Roles;

public class RoleResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Type { get; set; }
    public List<string> Permissions { get; set; } = new();
}
