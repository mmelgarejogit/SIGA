namespace SIGA.Application.DTOs.Users;

public class AssignSucursalRequest
{
    /// <summary>Sucursal a asignar. null = usuario global (ve todas).</summary>
    public int? SucursalId { get; set; }
}
