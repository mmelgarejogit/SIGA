namespace SIGA.Application.DTOs.Roles;

public class RoleRequest
{
    public string Name { get; set; } = null!;
    public List<string> Permissions { get; set; } = new();
}
