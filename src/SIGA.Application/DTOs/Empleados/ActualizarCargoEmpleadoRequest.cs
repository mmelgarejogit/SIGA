namespace SIGA.Application.DTOs.Empleados;

public class ActualizarCargoEmpleadoRequest
{
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public bool Activo { get; set; }
}
