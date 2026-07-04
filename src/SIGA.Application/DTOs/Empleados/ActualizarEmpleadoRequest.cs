namespace SIGA.Application.DTOs.Empleados;

public class ActualizarEmpleadoRequest
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public int CargoId { get; set; }
    public string FechaIngreso { get; set; } = "";
    public string? FechaEgreso { get; set; }
    public decimal? SalarioBase { get; set; }
    public int? SucursalId { get; set; }
}
